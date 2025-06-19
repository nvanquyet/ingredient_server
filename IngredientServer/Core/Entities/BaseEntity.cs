namespace IngredientServer.Core.Entities;
using System.ComponentModel.DataAnnotations;


public abstract class BaseEntity
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}