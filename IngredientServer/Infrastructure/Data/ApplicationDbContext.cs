using IngredientServer.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // DbSets for main entities only
    public DbSet<User> Users { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealFood> MealFoods { get; set; }
    public DbSet<FoodIngredient> FoodIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Height).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Weight).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TargetWeight).HasColumnType("decimal(5,2)");
            entity.Property(e => e.FoodAllergies).HasMaxLength(1000);
            entity.Property(e => e.FoodPreferences).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Enum conversions
            entity.Property(e => e.PrimaryNutritionGoal).HasConversion<string>();
            entity.Property(e => e.ActivityLevel).HasConversion<string>();
            entity.Property(e => e.gender).HasConversion<string>();
        });

        // Ingredient
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Unit).HasConversion<string>().IsRequired();
            entity.Property(e => e.Category).HasConversion<string>().IsRequired();
            entity.Property(e => e.ExpiryDate).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Ingredients)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.UserId, e.ExpiryDate });
        });

        // Food
        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Category).HasConversion<string>().IsRequired();
            entity.Property(e => e.CookingMethod).HasConversion<string>();
            entity.Property(e => e.Recipe).HasMaxLength(2000);
            entity.Property(e => e.PreparationTimeMinutes);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Foods)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Category);
        });

        // Meal
        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MealType).HasConversion<string>().IsRequired();
            entity.Property(e => e.MealDate).IsRequired();
            entity.Property(e => e.ConsumedAt).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Meals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.MealDate);
        });

        // MealFood
        modelBuilder.Entity<MealFood>(entity =>
        {
            entity.HasKey(e => new { e.MealId, e.FoodId });
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Meal)
                .WithMany(m => m.MealFoods)
                .HasForeignKey(e => e.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Food)
                .WithMany(f => f.MealFoods)
                .HasForeignKey(e => e.FoodId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FoodIngredient
        modelBuilder.Entity<FoodIngredient>(entity =>
        {
            entity.HasKey(e => new { e.FoodId, e.IngredientId });
            entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Unit).HasConversion<string>().IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Food)
                .WithMany(f => f.FoodIngredients)
                .HasForeignKey(e => e.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Ingredient)
                .WithMany()
                .HasForeignKey(e => e.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}