export default function StarRating({ rating, max = 5 }) {
  return (
    <div className="star-rating">
      {[...Array(max)].map((_, i) => {
        const full = i < Math.floor(rating)
        const half = !full && i < rating
        return (
          <span key={i} className={`star ${full ? 'full' : half ? 'half' : 'empty'}`}>
            ★
          </span>
        )
      })}
      <span className="star-value">{rating.toFixed(1)}</span>
    </div>
  )
}