using GameKeysStore.API.Data;
using GameKeysStore.API.Models.DTOs;
using GameKeysStore.API.Models.Entities;
using GameKeysStore.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace GameKeysStore.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Verificar email y username únicos
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("El email ya está registrado.");

        if (await _db.Users.AnyAsync(u => u.Username == dto.Username))
            throw new InvalidOperationException("El nombre de usuario ya está en uso.");

        // Validaciones específicas de vendedor
        if (dto.IsSeller)
        {
            if (string.IsNullOrWhiteSpace(dto.FirstName))
                throw new InvalidOperationException("El nombre es obligatorio para vendedores.");

            if (string.IsNullOrWhiteSpace(dto.LastName))
                throw new InvalidOperationException("El apellido es obligatorio para vendedores.");

            if (string.IsNullOrWhiteSpace(dto.DNI))
                throw new InvalidOperationException("El DNI es obligatorio para vendedores.");

            if (dto.BirthDate == null)
                throw new InvalidOperationException("La fecha de nacimiento es obligatoria para vendedores.");

            // Validar que sea mayor de edad
            var age = DateTime.UtcNow.Year - dto.BirthDate.Value.Year;
            if (dto.BirthDate.Value.Date > DateTime.UtcNow.AddYears(-age)) age--;
            if (age < 18)
                throw new InvalidOperationException("Debés ser mayor de 18 años para registrarte como vendedor.");

            // Validar DNI único
            if (await _db.SellerProfiles.AnyAsync(s => s.DNI == dto.DNI))
                throw new InvalidOperationException("El DNI ya está registrado.");
        }

        // Generar token de verificación
        var verificationToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.IsSeller ? UserRole.Seller : UserRole.User,
            CreatedAt = DateTime.UtcNow,
            IsEmailVerified = false,
            EmailVerificationToken = verificationToken
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (dto.IsSeller)
        {
            var sellerProfile = new SellerProfile
            {
                UserId = user.Id,
                DisplayName = dto.SellerDisplayName ?? dto.Username,
                FirstName = dto.FirstName!,
                LastName = dto.LastName!,
                DNI = dto.DNI!,
                BirthDate = dto.BirthDate.HasValue
                ? DateTime.SpecifyKind(dto.BirthDate.Value, DateTimeKind.Utc)
                : null,
                MercadoPagoAccount = dto.MercadoPagoAccount,
                CreatedAt = DateTime.UtcNow
            };
            _db.SellerProfiles.Add(sellerProfile);
            await _db.SaveChangesAsync();
        }

        await SendVerificationEmailAsync(user.Email, user.Username, verificationToken);

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id,
            IsEmailVerified = false
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _db.Users
            .Include(u => u.SellerProfile)
            .FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new InvalidOperationException("Email o contraseña incorrectos.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new InvalidOperationException("Email o contraseña incorrectos.");

        if (!user.IsEmailVerified)
            throw new InvalidOperationException("Debés verificar tu email antes de iniciar sesión.");

        if (user.IsBanned)
            throw new InvalidOperationException("Tu cuenta fue suspendida.");

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id,
            IsEmailVerified = true
        };
    }

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return;

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();

        await SendResetEmailAsync(user.Email, user.Username, token);
    }

    public async Task ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == dto.Token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow)
            ?? throw new InvalidOperationException("El link es inválido o ya expiró.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _db.SaveChangesAsync();
    }

    public async Task<AuthResponseDto> VerifyEmailAsync(string token)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.EmailVerificationToken != null &&
            u.EmailVerificationToken.ToLower() == token.ToLower())
            ?? throw new InvalidOperationException("Token de verificación inválido.");

        if (user.IsEmailVerified)
            throw new InvalidOperationException("El email ya fue verificado.");

        user.IsEmailVerified = true;
        user.EmailVerificationToken = null;
        await _db.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = GenerateToken(user),
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString(),
            UserId = user.Id,
            IsEmailVerified = true
        };
    }

    public async Task ResendVerificationAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || user.IsEmailVerified) return;

        // Verificar cooldown de 30 segundos
        if (user.LastVerificationEmailSent.HasValue)
        {
            var secondsSinceLastSent = (DateTime.UtcNow - user.LastVerificationEmailSent.Value).TotalSeconds;
            if (secondsSinceLastSent < 30)
            {
                var remaining = (int)(30 - secondsSinceLastSent);
                throw new InvalidOperationException($"Esperá {remaining} segundos antes de reenviar.");
            }
        }

        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        user.EmailVerificationToken = token;
        user.LastVerificationEmailSent = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await SendVerificationEmailAsync(user.Email, user.Username, token);
    }

    private string GenerateToken(User user)
    {
        var secret = _config["Jwt:Secret"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(_config["Jwt:ExpirationHours"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task SendVerificationEmailAsync(string email, string username, string token)
    {
        var frontendUrl = _config["Frontend:BaseUrl"]!;
        var verifyLink = $"{frontendUrl}/verify-email?token={token}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["Email:FromName"]!, _config["Email:Username"]!));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "✅ Verificá tu cuenta - GameKeys Store";

        var builder = new BodyBuilder
        {
            HtmlBody = $"""
            <div style="font-family:Arial,sans-serif;max-width:500px;margin:0 auto;background:#0e0e1c;color:#e8e8f0;border-radius:8px;overflow:hidden">
                <div style="padding:30px;text-align:center;border-bottom:1px solid #1e1e3a">
                    <h1 style="font-size:22px;margin:0">🎮 GameKeys<span style="color:#ff6a00">Store</span></h1>
                </div>
                <div style="padding:32px">
                    <h2 style="margin-bottom:12px">Hola, {username}!</h2>
                    <p style="color:#6b7280;margin-bottom:24px">Gracias por registrarte. Hacé click en el botón para verificar tu cuenta.</p>
                    <a href="{verifyLink}" style="display:inline-block;background:#ff6a00;color:#000;padding:14px 28px;border-radius:4px;font-weight:700;text-decoration:none;letter-spacing:1px;font-size:14px">VERIFICAR CUENTA</a>
                    <p style="color:#6b7280;margin-top:24px;font-size:13px">Si no te registraste en GameKeys Store, ignorá este email.</p>
                </div>
            </div>
            """
        };
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["Email:SmtpHost"]!, int.Parse(_config["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["Email:Username"]!, _config["Email:Password"]!);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    private async Task SendResetEmailAsync(string email, string username, string token)
    {
        var frontendUrl = _config["Frontend:BaseUrl"]!;
        var resetLink = $"{frontendUrl}/reset-password?token={token}";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config["Email:FromName"]!, _config["Email:Username"]!));
        message.To.Add(new MailboxAddress(username, email));
        message.Subject = "🔐 Recuperar contraseña - GameKeys Store";

        var builder = new BodyBuilder
        {
            HtmlBody = $"""
            <div style="font-family:Arial,sans-serif;max-width:500px;margin:0 auto;background:#0e0e1c;color:#e8e8f0;border-radius:8px;overflow:hidden">
                <div style="padding:30px;text-align:center;border-bottom:1px solid #1e1e3a">
                    <h1 style="font-size:22px;margin:0">🎮 GameKeys<span style="color:#ff6a00">Store</span></h1>
                </div>
                <div style="padding:32px">
                    <h2 style="margin-bottom:12px">Hola, {username}!</h2>
                    <p style="color:#6b7280;margin-bottom:24px">Recibimos una solicitud para restablecer tu contraseña. El link expira en <strong style="color:#ff6a00">1 hora</strong>.</p>
                    <a href="{resetLink}" style="display:inline-block;background:#ff6a00;color:#000;padding:14px 28px;border-radius:4px;font-weight:700;text-decoration:none;letter-spacing:1px;font-size:14px">RESTABLECER CONTRASEÑA</a>
                    <p style="color:#6b7280;margin-top:24px;font-size:13px">Si no solicitaste esto, ignorá este email.</p>
                </div>
            </div>
            """
        };
        message.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["Email:SmtpHost"]!, int.Parse(_config["Email:SmtpPort"]!), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["Email:Username"]!, _config["Email:Password"]!);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}