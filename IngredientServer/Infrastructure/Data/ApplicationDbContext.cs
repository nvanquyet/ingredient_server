using IngredientServer.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace IngredientServer.Infrastructure.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // DbSets for main entities
    public DbSet<User> Users { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Food> Foods { get; set; }
    public DbSet<Meal> Meals { get; set; }
    public DbSet<MealFood> MealFoods { get; set; }
    public DbSet<FoodIngredient> FoodIngredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
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

            // Navigation properties
            entity.HasMany(u => u.Ingredients)
                .WithOne(i => i.User)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Foods)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(u => u.Meals)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(u => u.NutritionTargets)
                .WithOne() // Không có navigation property ngược lại
                .HasForeignKey<UserNutritionTargets>(nt => nt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Ingredient configuration
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
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Ingredients)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiryDate);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => new { e.UserId, e.ExpiryDate });
            entity.HasIndex(e => new { e.UserId, e.Category });
        });
        
        // UserNutritionTargets configuration - DI CHUYỂN VÀO ĐÚNG VỊ TRÍ
        modelBuilder.Entity<UserNutritionTargets>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Index for performance
            entity.HasIndex(e => e.UserId)
                .IsUnique() // Mỗi user chỉ có một bản ghi nutrition targets
                .HasDatabaseName("IX_UserNutritionTargets_UserId");
            
            // Decimal precision
            entity.Property(e => e.TargetDailyCalories).HasPrecision(8, 2);
            entity.Property(e => e.TargetDailyProtein).HasPrecision(8, 2);
            entity.Property(e => e.TargetDailyCarbohydrates).HasPrecision(8, 2);
            entity.Property(e => e.TargetDailyFat).HasPrecision(8, 2);
            entity.Property(e => e.TargetDailyFiber).HasPrecision(8, 2);
            
            // Timestamps
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Food configuration
        modelBuilder.Entity<Food>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.PreparationTimeMinutes).IsRequired();
            entity.Property(e => e.CookingTimeMinutes).IsRequired();
            entity.Property(e => e.Calories).HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.Protein).HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.Carbohydrates).HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.Fat).HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.Fiber).HasColumnType("decimal(8,2)").IsRequired();
            entity.Property(e => e.Instructions).HasColumnType("json").IsRequired();
            entity.Property(e => e.Tips).HasColumnType("json").IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.DifficultyLevel).HasDefaultValue(1);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Foods)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(f => f.FoodIngredients)
                .WithOne(fi => fi.Food)
                .HasForeignKey(fi => fi.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(f => f.MealFoods)
                .WithOne(mf => mf.Food)
                .HasForeignKey(mf => mf.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.DifficultyLevel);
        });

        // Meal configuration
        modelBuilder.Entity<Meal>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.MealType).HasConversion<string>().IsRequired();
            entity.Property(e => e.MealDate).IsRequired();
            entity.Property(e => e.ConsumedAt).IsRequired(false); // Nullable
            entity.Property(e => e.TotalCalories).HasDefaultValue(0.0);
            entity.Property(e => e.TotalProtein).HasDefaultValue(0.0);
            entity.Property(e => e.TotalCarbs).HasDefaultValue(0.0);
            entity.Property(e => e.TotalFat).HasDefaultValue(0.0);
            entity.Property(e => e.TotalFiber).HasDefaultValue(0.0);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Meals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(m => m.MealFoods)
                .WithOne(mf => mf.Meal)
                .HasForeignKey(mf => mf.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.MealDate);
            entity.HasIndex(e => new { e.UserId, e.MealDate });
            entity.HasIndex(e => new { e.UserId, e.MealType });
        });

        // MealFood configuration (Junction table)
        modelBuilder.Entity<MealFood>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.MealId).IsRequired();
            entity.Property(e => e.FoodId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Meal)
                .WithMany(m => m.MealFoods)
                .HasForeignKey(e => e.MealId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Food)
                .WithMany(f => f.MealFoods)
                .HasForeignKey(e => e.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite unique index to prevent duplicate entries
            entity.HasIndex(e => new { e.MealId, e.FoodId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        // FoodIngredient configuration (Junction table)
        modelBuilder.Entity<FoodIngredient>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FoodId).IsRequired();
            entity.Property(e => e.IngredientId).IsRequired();
            entity.Property(e => e.Quantity).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.Unit).HasConversion<string>().IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(e => e.Food)
                .WithMany(f => f.FoodIngredients)
                .HasForeignKey(e => e.FoodId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Ingredient)
                .WithMany() // Ingredient không có navigation property ngược lại
                .HasForeignKey(e => e.IngredientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Composite unique index to prevent duplicate entries
            entity.HasIndex(e => new { e.FoodId, e.IngredientId }).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        // Seed data or additional configurations can be added here
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Add any seed data if needed
        // For example, default nutrition goals, activity levels, etc.
    }
}