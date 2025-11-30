using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IngredientServer.Core.Helpers;

namespace IngredientServer.Core.Entities
{
    public enum IngredientUnit
    {
        // Weight units (giữ nguyên giá trị cũ)
        Kilogram = 0,
        Liter = 1,         // giữ nguyên từ code cũ
        Piece = 2,         // giữ nguyên từ code cũ
        Box = 3,           // giữ nguyên từ code cũ
        Gram = 4,          // giữ nguyên từ code cũ
        Milliliter = 5,    // giữ nguyên từ code cũ
        Can = 6,           // giữ nguyên từ code cũ
        Cup = 7,           // giữ nguyên từ code cũ
        Tablespoon = 8,    // giữ nguyên từ code cũ
        Teaspoon = 9,      // giữ nguyên từ code cũ
        Package = 10,      // giữ nguyên từ code cũ
        Bottle = 11,       // giữ nguyên từ code cũ
        
        // Thêm các đơn vị mới
        Pound = 20,        // Pao (lb)
        Ounce = 21,        // Ao-xơ (oz)
        FluidOunce = 22,   // Fluid ounce (fl oz)
        Pint = 23,         // Pint
        Quart = 24,        // Quart
        Gallon = 25,       // Gallon
        
        // Countable units
        Slice = 30,        // Lát
        Clove = 31,        // tép (tỏi, hành)
        Head = 32,         // củ, đầu (bắp cải, tỏi)
        Bunch = 33,        // bó (rau, hành)
        Stalk = 34,        // cọng (cần tây, rau muống)
        Wedge = 35,        // miếng (cam, chanh)
        Sheet = 36,        // lá (lá bánh tráng, lá nho)
        Pod = 37,          // quả (đậu bắp, đậu đũa)
        
        // Container units
        Bag = 40,          // Túi
        Jar = 41,          // Lọ
        Tube = 42,         // Tuýp
        Carton = 43,       // Thùng carton
        
        // Small quantity units
        Pinch = 50,        // nhúm (muối, đường)
        Dash = 51,         // chút (nước mắm, giấm)
        Drop = 52,         // giọt
        
        // Other
        Serving = 60,      // phần
        Portion = 61,      // suất
        Other = 99
    }

    public enum IngredientCategory 
    {
        // Giữ nguyên các giá trị cũ để tương thích với database
        Dairy = 0,        // giữ nguyên từ code cũ
        Meat = 1,         // giữ nguyên từ code cũ
        Vegetables = 2,   // giữ nguyên từ code cũ
        Fruits = 3,       // giữ nguyên từ code cũ
        Grains = 4,       // giữ nguyên từ code cũ
        Seafood = 5,      // giữ nguyên từ code cũ
        Beverages = 6,    // giữ nguyên từ code cũ
        Condiments = 7,   // giữ nguyên từ code cũ
        Snacks = 8,       // giữ nguyên từ code cũ
        Frozen = 9,       // giữ nguyên từ code cũ
        Canned = 10,      // giữ nguyên từ code cũ
        Spices = 11,      // giữ nguyên từ code cũ
        
        // Thêm các category mới
        Poultry = 20,     // thịt gia cầm
        Eggs = 21,        // trứng
        Legumes = 22,     // đậu, đỗ
        Nuts = 23,        // hạt, quả hạch
        Tofu = 24,        // đậu phụ
        
        // Vegetables subcategories
        LeafyGreens = 30, // rau lá xanh
        RootVegetables = 31, // rau củ
        Herbs = 32,       // rau thơm
        
        // Fruits subcategories
        Berries = 40,     // quả mọng
        Citrus = 41,      // cam quýt
        
        // Grains & Starches subcategories
        Rice = 50,        // gạo
        Pasta = 51,       // mì, pasta
        Bread = 52,       // bánh mì
        Noodles = 53,     // mì, phở, bún
        
        // Cooking essentials
        Oils = 60,        // dầu ăn
        Vinegar = 61,     // giấm
        Sauces = 62,      // nước sốt
        Seasonings = 63,  // gia vị nêm
        
        // Baking
        Baking = 70,      // đồ làm bánh
        Flour = 71,       // bột
        Sugar = 72,       // đường
        Sweeteners = 73,  // chất tạo ngọt
        
        // Beverages subcategories
        Alcoholic = 80,   // đồ uống có cồn
        
        // Processed foods
        Processed = 90,   // thực phẩm chế biến sẵn
        
        // Other
        Other = 99
    }

    // Sửa từ abstract thành concrete class
    public sealed class Ingredient : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Quantity { get; set; }

        [Required]
        public IngredientUnit Unit { get; set; } = IngredientUnit.Gram;

        [Required]
        public IngredientCategory Category { get; set; } = IngredientCategory.Other;

        [Required]
        public DateTime ExpiryDate { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }
        
        // Computed properties
        public int DaysUntilExpiry => DateTimeHelper.DaysUntilExpiry(ExpiryDate);
        public bool IsExpired => DateTimeHelper.IsExpired(ExpiryDate);
        public bool IsExpiringSoon => DateTimeHelper.IsExpiringSoon(ExpiryDate);
        
        public string ExpiryDateUtc => DateTimeHelper.NormalizeToUtc(ExpiryDate)
            .ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}