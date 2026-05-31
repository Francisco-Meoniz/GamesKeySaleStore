# 🎮 GameKeys Store

Plataforma de compra y venta de keys de videojuegos. Los vendedores publican sus keys con precio propio y los usuarios pueden comprarlas de forma segura mediante MercadoPago.

## Stack

**Backend:** ASP.NET Core 8 · PostgreSQL · Entity Framework Core · JWT · MercadoPago SDK · MailKit · QuestPDF

**Frontend:** React 18 · Vite · React Router DOM · TanStack Query · Axios

---

## Requisitos previos

- Visual Studio 2022
- .NET 8 SDK
- Node.js 18+
- PostgreSQL 15+
- pgAdmin (recomendado)

---

## Configuración del Backend

### 1 �?Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/gamekeysstore.git
```

### 2 �?Abrir el proyecto
Abrí `KeyGamesSaleStore.sln` en Visual Studio 2022.

### 3 �?Configurar `appsettings.json`
Copiá `appsettings.example.json` y renombralo a `appsettings.json`. Completá los valores:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gamekeysdb;Username=postgres;Password=TU_PASSWORD"
  },
  "MercadoPago": {
    "AccessToken": "TEST-XXXXXXXXXXXXXXXX"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "tu@gmail.com",
    "Password": "tu_app_password",
    "FromName": "GameKeys Store"
  },
  "Jwt": {
    "Secret": "clave-secreta-minimo-32-caracteres!!",
    "Issuer": "GameKeysStore",
    "Audience": "GameKeysStoreUsers",
    "ExpirationHours": 24
  },
  "Frontend": {
    "BaseUrl": "http://localhost:5173"
  },
  "Backend": {
    "BaseUrl": "http://localhost:5086"
  }
}
```

### 4 �?Crear la base de datos
En pgAdmin creá una base de datos llamada `gamekeysdb`.

### 5 �?Aplicar migraciones
En la Consola del Administrador de Paquetes de Visual Studio:
```powershell
Update-Database
```

### 6 �?Cargar datos de prueba (opcional)
Con el proyecto corriendo, ejecutá desde Swagger:
```
POST /api/seed
```

### 7 �?Correr el backend
Presioná `F5` en Visual Studio. El backend corre en `http://localhost:5086` y Swagger en `http://localhost:5086/swagger`.

---

## Configuración del Frontend

### 1 �?Entrar a la carpeta
```bash
cd gamekeysstore-frontend
```

### 2 �?Instalar dependencias
```bash
npm install
```

### 3 �?Verificar el puerto del backend
En `src/api/client.js` asegurate que el puerto coincida con el del backend:
```js
const client = axios.create({
  baseURL: 'http://localhost:5086/api',
})
```

### 4 �?Correr el frontend
```bash
npm run dev
```

Abrí `http://localhost:5173` en el navegador.

---

## Credenciales de prueba

### MercadoPago
- Obtené el Access Token de prueba en [mercadopago.com.ar/developers](https://mercadopago.com.ar/developers)
- Usá la tarjeta de prueba para pagos:
  - **Número:** `5031 7557 3453 0604`
  - **Vencimiento:** `11/25`
  - **CVV:** `123`
  - **Nombre:** `APRO`

### Email
- Usá Gmail con una App Password generada desde [myaccount.google.com](https://myaccount.google.com)
- Activá la verificación en dos pasos antes de generar la App Password

### Usuario Admin
Luego de registrarte, asignate el rol Admin desde pgAdmin:
```sql
UPDATE "Users" SET "Role" = 2 WHERE "Email" = 'tu@email.com';
```

---

## Funcionalidades

| Módulo | Descripción |
|--------|-------------|
| 🔐 Auth | Registro, login, verificación de email, recuperar contraseña |
| 🎮 Catálogo | Listado de juegos con buscador y filtros |
| 🔑 Keys | Cada vendedor publica sus keys con precio propio |
| 🛒 Carrito | Hasta 3 keys de distintos juegos |
| 💳 Pagos | Integración con MercadoPago |
| 📧 Emails | Verificación, recupero de contraseña y comprobante de compra |
| 📄 PDF | Comprobante descargable tras la compra |
| �?Ratings | Sistema de calificación de vendedores |
| 👤 Perfil | Edición de datos, foto, historial de compras |
| 🏪 Panel Vendedor | Publicar y gestionar keys |
| ⚙️ Panel Admin | Gestionar usuarios, juegos y órdenes |

---

## Estructura del proyecto

```
GameKeysStore/
├── KeyGamesSaleStore/          # Backend ASP.NET Core
�?  ├── Controllers/            # Endpoints de la API
�?  ├── Models/
�?  �?  ├── Entities/           # Modelos de la BD
�?  �?  └── DTOs/               # Objetos de transferencia
�?  ├── Services/               # Lógica de negocio
�?  ├── Data/                   # DbContext
�?  └── appsettings.example.json
�?└── gamekeysstore-frontend/     # Frontend React
    └── src/
        ├── api/                # Llamadas al backend
        ├── components/         # Componentes reutilizables
        ├── context/            # AuthContext, CartContext
        └── pages/              # Páginas de la app
```

---

## Para producción

Cuando hagas el hosting, cambiá estas cosas:

1. **`appsettings.json`** �?URLs reales en vez de localhost
2. **`src/api/client.js`** �?URL del backend en producción
3. **`PaymentService.cs`** �?Agregar `AutoReturn = "approved"` y usar token de producción de MP
4. **Credenciales** �?Cambiar Access Token de prueba por el de producción

---

## Notas importantes

- El `appsettings.json` **no debe subirse al repositorio** (está en `.gitignore`)
- En modo desarrollo, MercadoPago no redirige automáticamente �?hay que navegar manualmente a `/payment/success?orderId=X`
- Las keys del seed son ficticias, solo para pruebas

Cuentas para usar para probar la pagina:
Admin:
juan@gmail.com
12345678
Seller:
undeadfeelthe1@gmail.com
12345678
User:
thecoffeeguy011@gmail.com
1234