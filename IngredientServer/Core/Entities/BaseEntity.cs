using IngredientServer.Core.Helpers;
using System.ComponentModel.DataAnnotations;

namespace IngredientServer.Core.Entities;

public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// Normalizes all DateTime properties to UTC
    /// </summary>
    public virtual void NormalizeDateTimes()
    {
        CreatedAt = DateTimeHelper.NormalizeToUtc(CreatedAt);
        UpdatedAt = DateTimeHelper.NormalizeToUtc(UpdatedAt);
    }
}