using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopVerse.Domain.Entities;
using ShopVerse.Domain.Enums;

namespace ShopVerse.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ShopVerseDbContext context, UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        await context.Database.MigrateAsync();

        await SeedRolesAsync(roleManager);
        await SeedUsersAsync(userManager);
        await SeedCategoriesAsync(context);
        await SeedProductsAsync(context);
        await SeedCouponsAsync(context);
        await SeedDemoOrdersAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole<Guid>> roleManager)
    {
        string[] roles = ["Admin", "Customer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<User> userManager)
    {
        if (await userManager.FindByEmailAsync("admin@shopverse.com") == null)
        {
            var admin = new User
            {
                UserName = "admin@shopverse.com",
                Email = "admin@shopverse.com",
                FirstName = "Shop",
                LastName = "Admin",
                EmailConfirmed = true,
                IsActive = true
            };
            await userManager.CreateAsync(admin, "Admin@123456");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        if (await userManager.FindByEmailAsync("john@example.com") == null)
        {
            var customer1 = new User
            {
                UserName = "john@example.com",
                Email = "john@example.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                PhoneNumber = "9876543210",
                IsActive = true
            };
            await userManager.CreateAsync(customer1, "Customer@123456");
            await userManager.AddToRoleAsync(customer1, "Customer");
        }

        if (await userManager.FindByEmailAsync("priya@example.com") == null)
        {
            var customer2 = new User
            {
                UserName = "priya@example.com",
                Email = "priya@example.com",
                FirstName = "Priya",
                LastName = "Sharma",
                EmailConfirmed = true,
                PhoneNumber = "9123456780",
                IsActive = true
            };
            await userManager.CreateAsync(customer2, "Customer@123456");
            await userManager.AddToRoleAsync(customer2, "Customer");
        }
    }

    private static async Task SeedCategoriesAsync(ShopVerseDbContext context)
    {
        if (await context.Categories.AnyAsync()) return;

        var electronics = new Category { Name = "Electronics", Slug = "electronics", Description = "Gadgets, devices and tech accessories", ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400", SortOrder = 1, IsActive = true };
        var fashion = new Category { Name = "Fashion", Slug = "fashion", Description = "Clothing, footwear and accessories", ImageUrl = "https://images.unsplash.com/photo-1445205170230-053b83016050?w=400", SortOrder = 2, IsActive = true };
        var home = new Category { Name = "Home & Living", Slug = "home-living", Description = "Furniture, decor and kitchen essentials", ImageUrl = "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=400", SortOrder = 3, IsActive = true };
        var sports = new Category { Name = "Sports & Fitness", Slug = "sports-fitness", Description = "Gym equipment, outdoor and sports gear", ImageUrl = "https://images.unsplash.com/photo-1517649763962-0c623066013b?w=400", SortOrder = 4, IsActive = true };
        var beauty = new Category { Name = "Beauty & Health", Slug = "beauty-health", Description = "Skincare, grooming and wellness products", ImageUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=400", SortOrder = 5, IsActive = true };
        var books = new Category { Name = "Books", Slug = "books", Description = "Fiction, non-fiction, textbooks and more", ImageUrl = "https://images.unsplash.com/photo-1507842217343-583bb7270b66?w=400", SortOrder = 6, IsActive = true };
        var toys = new Category { Name = "Toys & Games", Slug = "toys-games", Description = "Toys for all ages, board games and puzzles", ImageUrl = "https://images.unsplash.com/photo-1558060370-d644479cb6f7?w=400", SortOrder = 7, IsActive = true };
        var grocery = new Category { Name = "Grocery", Slug = "grocery", Description = "Fresh produce, packaged foods and beverages", ImageUrl = "https://images.unsplash.com/photo-1542838132-92c53300491e?w=400", SortOrder = 8, IsActive = true };
        var accessories = new Category { Name = "Accessories", Slug = "accessories", Description = "Bags, watches, sunglasses and leather goods", ImageUrl = "https://images.unsplash.com/photo-1523293182086-7651a899d37f?w=400", SortOrder = 9, IsActive = true };

        context.Categories.AddRange(electronics, fashion, home, sports, beauty, accessories, books, toys, grocery);
        await context.SaveChangesAsync();

        // Sub-categories
        var subCats = new List<Category>
        {
            new() { Name = "Smartphones", Slug = "smartphones", ParentId = electronics.Id, SortOrder = 1, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400" },
            new() { Name = "Laptops", Slug = "laptops", ParentId = electronics.Id, SortOrder = 2, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400" },
            new() { Name = "Headphones", Slug = "headphones", ParentId = electronics.Id, SortOrder = 3, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400" },
            new() { Name = "Cameras", Slug = "cameras", ParentId = electronics.Id, SortOrder = 4, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=400" },
            new() { Name = "Men's Clothing", Slug = "mens-clothing", ParentId = fashion.Id, SortOrder = 1, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1617137968427-85924c800a22?w=400" },
            new() { Name = "Women's Clothing", Slug = "womens-clothing", ParentId = fashion.Id, SortOrder = 2, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1551163943-3f6a855d1153?w=400" },
            new() { Name = "Footwear", Slug = "footwear", ParentId = fashion.Id, SortOrder = 3, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400" },
            new() { Name = "Kitchen", Slug = "kitchen", ParentId = home.Id, SortOrder = 1, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400" },
            new() { Name = "Furniture", Slug = "furniture", ParentId = home.Id, SortOrder = 2, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400" },
            new() { Name = "Gym Equipment", Slug = "gym-equipment", ParentId = sports.Id, SortOrder = 1, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=400" },
            new() { Name = "Sportswear", Slug = "sportswear", ParentId = sports.Id, SortOrder = 2, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1517649763962-0c623266010b?w=400" },
            new() { Name = "Skincare", Slug = "skincare", ParentId = beauty.Id, SortOrder = 1, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1608248597261-833258657b45?w=400" },
            new() { Name = "Grooming & Makeup", Slug = "grooming-makeup", ParentId = beauty.Id, SortOrder = 2, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=400" },
            new() { Name = "Bags & Luggage", Slug = "bags-luggage", ParentId = accessories.Id, SortOrder = 1, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=400" },
            new() { Name = "Watches & Eyewear", Slug = "watches-eyewear", ParentId = accessories.Id, SortOrder = 2, IsActive = true, ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400" },
        };

        context.Categories.AddRange(subCats);
        await context.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ShopVerseDbContext context)
    {
        if (await context.Products.CountAsync() >= 100) return;

        var categories = await context.Categories.ToListAsync();
        var smartphones = categories.First(c => c.Slug == "smartphones");
        var laptops = categories.First(c => c.Slug == "laptops");
        var headphones = categories.First(c => c.Slug == "headphones");
        var cameras = categories.First(c => c.Slug == "cameras");
        var menClothing = categories.First(c => c.Slug == "mens-clothing");
        var womenClothing = categories.First(c => c.Slug == "womens-clothing");
        var footwear = categories.First(c => c.Slug == "footwear");
        var kitchen = categories.First(c => c.Slug == "kitchen");
        var furniture = categories.First(c => c.Slug == "furniture");
        var gym = categories.First(c => c.Slug == "gym-equipment");
        var electronics = categories.First(c => c.Slug == "electronics");
        var beauty = categories.First(c => c.Slug == "beauty-health");
        var books = categories.First(c => c.Slug == "books");
        var toys = categories.First(c => c.Slug == "toys-games");

        var products = new List<Product>
        {
            // SMARTPHONES
            new()
            {
                Name = "Apple iPhone 15 Pro Max 256GB",
                Slug = "apple-iphone-15-pro-max-256gb",
                Brand = "Apple",
                Sku = "APPL-IP15PM-256",
                Price = 134900,
                CompareAtPrice = 159900,
                StockQuantity = 45,
                IsFeatured = true,
                CategoryId = smartphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=600",
                ShortDescription = "Titanium design, A17 Pro chip, 48MP main camera",
                Description = "The iPhone 15 Pro Max features a titanium design with the A17 Pro chip and an advanced 48MP camera system with 5x optical zoom. Includes USB-C with USB 3 speeds and Action button.",
                AverageRating = 4.8,
                TotalReviews = 1204,
                TotalSold = 3200,
                Tags = "iphone,apple,smartphone,5g",
                Images = new List<ProductImage>
                {
                    new() { Url = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=800", IsPrimary = true, SortOrder = 0, AltText = "iPhone 15 Pro Max front" },
                    new() { Url = "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?w=800", SortOrder = 1, AltText = "iPhone 15 Pro Max side" },
                }
            },
            new()
            {
                Name = "Samsung Galaxy S24 Ultra 512GB",
                Slug = "samsung-galaxy-s24-ultra-512gb",
                Brand = "Samsung",
                Sku = "SAM-S24U-512",
                Price = 129999,
                CompareAtPrice = 149999,
                StockQuantity = 38,
                IsFeatured = true,
                CategoryId = smartphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=600",
                ShortDescription = "200MP camera, S Pen, Galaxy AI, titanium frame",
                Description = "Samsung Galaxy S24 Ultra brings Galaxy AI with a built-in S Pen, 200MP main camera, and titanium frame for unmatched productivity and creativity.",
                AverageRating = 4.7,
                TotalReviews = 892,
                TotalSold = 2100,
                Tags = "samsung,galaxy,smartphone,spen,ai"
            },
            new()
            {
                Name = "OnePlus 12 256GB Silky Black",
                Slug = "oneplus-12-256gb-silky-black",
                Brand = "OnePlus",
                Sku = "OP12-256-BLK",
                Price = 64999,
                CompareAtPrice = 69999,
                StockQuantity = 62,
                CategoryId = smartphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=600",
                ShortDescription = "Snapdragon 8 Gen 3, Hasselblad cameras, 100W charging",
                Description = "OnePlus 12 features the Snapdragon 8 Gen 3 processor with Hasselblad-tuned cameras and industry-leading 100W SUPERVOOC charging that fills the phone in under 25 minutes.",
                AverageRating = 4.5,
                TotalReviews = 567,
                TotalSold = 1500
            },
            new()
            {
                Name = "Google Pixel 8 Pro 128GB",
                Slug = "google-pixel-8-pro-128gb",
                Brand = "Google",
                Sku = "GPX-8P-128",
                Price = 89999,
                CompareAtPrice = 99999,
                StockQuantity = 29,
                CategoryId = smartphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=600",
                ShortDescription = "Google Tensor G3, best-in-class camera AI, 7 years of updates",
                Description = "Google Pixel 8 Pro with Tensor G3 chip offers the smartest camera on Android with AI features like Magic Eraser, Best Take, and Photo Unblur.",
                AverageRating = 4.6,
                TotalReviews = 445,
                TotalSold = 980
            },
            new()
            {
                Name = "Xiaomi 14 Ultra 512GB",
                Slug = "xiaomi-14-ultra-512gb",
                Brand = "Xiaomi",
                Sku = "XI-14U-512",
                Price = 99999,
                CompareAtPrice = 109999,
                StockQuantity = 20,
                CategoryId = smartphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1567581935884-3349723552ca?w=600",
                ShortDescription = "Leica 1-inch sensor camera, Snapdragon 8 Gen 3",
                Description = "Xiaomi 14 Ultra co-engineered with Leica features a 1-inch main sensor and 4 cameras, delivering professional photography results in every shot.",
                AverageRating = 4.4,
                TotalReviews = 312,
                TotalSold = 750
            },

            // LAPTOPS
            new()
            {
                Name = "Apple MacBook Pro 14\" M3 Pro 18GB",
                Slug = "apple-macbook-pro-14-m3-pro-18gb",
                Brand = "Apple",
                Sku = "APPL-MBP14-M3P",
                Price = 199900,
                CompareAtPrice = 229900,
                StockQuantity = 18,
                IsFeatured = true,
                CategoryId = laptops.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=600",
                ShortDescription = "M3 Pro chip, 18GB RAM, 512GB SSD, Liquid Retina XDR",
                Description = "MacBook Pro 14 with M3 Pro chip delivers unprecedented performance for professionals. Features a stunning Liquid Retina XDR display, up to 18 hours battery life, and all the ports you need.",
                AverageRating = 4.9,
                TotalReviews = 2341,
                TotalSold = 4100,
                Tags = "apple,macbook,laptop,m3"
            },
            new()
            {
                Name = "Dell XPS 15 OLED Intel i9 32GB",
                Slug = "dell-xps-15-oled-i9-32gb",
                Brand = "Dell",
                Sku = "DELL-XPS15-I9",
                Price = 179900,
                CompareAtPrice = 199900,
                StockQuantity = 12,
                CategoryId = laptops.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=600",
                ShortDescription = "3.5K OLED touch, i9-13900H, RTX 4070, 32GB DDR5",
                Description = "Dell XPS 15 with 3.5K OLED display and RTX 4070 GPU is a powerhouse for creators. The stunning display with 100% DCI-P3 color gamut makes every image come alive.",
                AverageRating = 4.6,
                TotalReviews = 789,
                TotalSold = 1200
            },
            new()
            {
                Name = "ASUS ROG Zephyrus G14 AMD Ryzen 9",
                Slug = "asus-rog-zephyrus-g14-ryzen9",
                Brand = "ASUS",
                Sku = "ASUS-ROG-G14-R9",
                Price = 149900,
                CompareAtPrice = 169900,
                StockQuantity = 25,
                CategoryId = laptops.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=600",
                ShortDescription = "Ryzen 9 8945HS, RTX 4060, 14\" 165Hz, AniMe Matrix LED",
                Description = "The ROG Zephyrus G14 is the ultimate compact gaming laptop featuring the AMD Ryzen 9 8945HS and NVIDIA RTX 4060 in a sleek 14-inch form factor with iconic AniMe Matrix LED lid.",
                AverageRating = 4.7,
                TotalReviews = 634,
                TotalSold = 1890
            },
            new()
            {
                Name = "Lenovo ThinkPad X1 Carbon Gen 11",
                Slug = "lenovo-thinkpad-x1-carbon-gen11",
                Brand = "Lenovo",
                Sku = "LEN-X1C-G11",
                Price = 139900,
                CompareAtPrice = 159900,
                StockQuantity = 14,
                CategoryId = laptops.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=600",
                ShortDescription = "Intel i7-1365U, 16GB LPDDR5, 512GB SSD, 14\" IPS",
                Description = "Lenovo ThinkPad X1 Carbon Gen 11 is the ultimate business ultrabook. At just 1.12 kg, it offers legendary ThinkPad durability with Intel Evo certification.",
                AverageRating = 4.5,
                TotalReviews = 412,
                TotalSold = 800
            },
            new()
            {
                Name = "HP Spectre x360 14\" 2-in-1 OLED",
                Slug = "hp-spectre-x360-14-oled",
                Brand = "HP",
                Sku = "HP-SX360-14-OLED",
                Price = 159900,
                CompareAtPrice = 179900,
                StockQuantity = 9,
                CategoryId = laptops.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1525547719571-a2d4ac8945e2?w=600",
                ShortDescription = "Intel i7 Evo, 2.8K OLED touch, 360° convertible, stylus",
                Description = "HP Spectre x360 14 is a premium 2-in-1 convertible with a stunning 2.8K OLED display, Intel Core i7 Evo platform and an included rechargeable stylus for creative workflows.",
                AverageRating = 4.4,
                TotalReviews = 287,
                TotalSold = 560
            },

            // HEADPHONES
            new()
            {
                Name = "Sony WH-1000XM5 Wireless Noise Cancelling",
                Slug = "sony-wh-1000xm5",
                Brand = "Sony",
                Sku = "SONY-XM5-BLK",
                Price = 29990,
                CompareAtPrice = 34990,
                StockQuantity = 75,
                IsFeatured = true,
                CategoryId = headphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600",
                ShortDescription = "Industry-leading ANC, 30hr battery, Multipoint connection",
                Description = "Sony WH-1000XM5 headphones feature Sony's best-ever noise cancellation with Integrated Processor V1. Ultra-comfortable design, 30-hour battery, and crystal-clear hands-free calling.",
                AverageRating = 4.8,
                TotalReviews = 3456,
                TotalSold = 8900,
                Tags = "sony,headphones,noise-cancelling,wireless"
            },
            new()
            {
                Name = "Apple AirPods Pro 2nd Generation",
                Slug = "apple-airpods-pro-2nd-gen",
                Brand = "Apple",
                Sku = "APPL-APP2",
                Price = 24900,
                CompareAtPrice = 26900,
                StockQuantity = 120,
                IsFeatured = true,
                CategoryId = headphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1588423771073-b8903fead85b?w=600",
                ShortDescription = "H2 chip, Adaptive Transparency, Find My, USB-C case",
                Description = "AirPods Pro with H2 chip offer next-level Active Noise Cancellation, Adaptive Audio, and Personalised Spatial Audio for an immersive listening experience.",
                AverageRating = 4.7,
                TotalReviews = 5678,
                TotalSold = 15000
            },
            new()
            {
                Name = "Bose QuietComfort 45 Wireless",
                Slug = "bose-quietcomfort-45",
                Brand = "Bose",
                Sku = "BOSE-QC45",
                Price = 26900,
                CompareAtPrice = 32900,
                StockQuantity = 43,
                CategoryId = headphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1546435770-a3e736d5d67a?w=600",
                ShortDescription = "24hr battery, dual mic, comfortable over-ear design",
                Description = "Bose QuietComfort 45 delivers proprietary noise cancelling with a sleek new design, 24 hours of battery life, and the signature Bose sound quality audiophiles love.",
                AverageRating = 4.6,
                TotalReviews = 1890,
                TotalSold = 4200
            },

            // CAMERAS
            new()
            {
                Name = "Sony Alpha A7 IV Full Frame Mirrorless",
                Slug = "sony-alpha-a7-iv",
                Brand = "Sony",
                Sku = "SONY-A7IV",
                Price = 249990,
                CompareAtPrice = 279990,
                StockQuantity = 8,
                CategoryId = cameras.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600",
                ShortDescription = "33MP BSI CMOS, 4K 60p video, real-time tracking AF",
                Description = "Sony Alpha A7 IV is a versatile full-frame mirrorless camera with a new 33MP sensor, advanced AI-based autofocus, and professional 4K 60p video capabilities.",
                AverageRating = 4.8,
                TotalReviews = 567,
                TotalSold = 420
            },
            new()
            {
                Name = "Canon EOS R6 Mark II Body",
                Slug = "canon-eos-r6-mark-ii",
                Brand = "Canon",
                Sku = "CANON-R6M2",
                Price = 219900,
                CompareAtPrice = 249900,
                StockQuantity = 11,
                CategoryId = cameras.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=600",
                ShortDescription = "24.2MP, 40fps RAW burst, dual memory slots, IBIS",
                Description = "Canon EOS R6 Mark II delivers speed and reliability for action photography with 40 fps burst shooting, advanced subject detection AF, and 6.5-stop Image Stabilization.",
                AverageRating = 4.7,
                TotalReviews = 389,
                TotalSold = 320
            },

            // MEN'S CLOTHING
            new()
            {
                Name = "Allen Solly Men's Slim Fit Casual Shirt",
                Slug = "allen-solly-slim-fit-casual-shirt",
                Brand = "Allen Solly",
                Sku = "AS-MSHIRT-SLIM",
                Price = 1299,
                CompareAtPrice = 2499,
                StockQuantity = 200,
                CategoryId = menClothing.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1598033129183-c4f50c736f10?w=600",
                ShortDescription = "100% cotton, slim fit, formal casual wear",
                Description = "Allen Solly slim fit shirt crafted from premium 100% cotton. Perfect for office to evening transitions. Available in multiple solid and printed options.",
                AverageRating = 4.3,
                TotalReviews = 2341,
                TotalSold = 6500
            },
            new()
            {
                Name = "Levi's 501 Original Fit Jeans Dark Blue",
                Slug = "levis-501-original-fit-jeans",
                Brand = "Levi's",
                Sku = "LEVIS-501-DKBL",
                Price = 3499,
                CompareAtPrice = 4999,
                StockQuantity = 150,
                CategoryId = menClothing.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1542272604-787c3835535d?w=600",
                ShortDescription = "Original fit, button fly, premium denim",
                Description = "The original blue jean since 1873. The Levi's 501 Original Fit jeans are an iconic style with a straight leg and button fly made from premium denim that gets better with every wear.",
                AverageRating = 4.5,
                TotalReviews = 4567,
                TotalSold = 12000
            },
            new()
            {
                Name = "Nike Dri-FIT Men's Training T-Shirt",
                Slug = "nike-dri-fit-training-tshirt",
                Brand = "Nike",
                Sku = "NIKE-DRI-MTSH",
                Price = 1595,
                CompareAtPrice = 1995,
                StockQuantity = 300,
                CategoryId = menClothing.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1576566588028-4147f3842f27?w=600",
                ShortDescription = "Dri-FIT tech, 100% polyester, moisture wicking",
                Description = "Nike Dri-FIT technology moves sweat away from skin to keep you dry and comfortable during workouts. Standard fit with slightly looser construction for natural range of motion.",
                AverageRating = 4.4,
                TotalReviews = 3201,
                TotalSold = 9800
            },

            // WOMEN'S CLOTHING
            new()
            {
                Name = "Biba Women's A-Line Kurta Set",
                Slug = "biba-aline-kurta-set",
                Brand = "Biba",
                Sku = "BIBA-KURTA-AL",
                Price = 2299,
                CompareAtPrice = 3499,
                StockQuantity = 120,
                CategoryId = womenClothing.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1583391733956-6c78276477e1?w=600",
                ShortDescription = "Cotton blend, ethnic print, A-line silhouette",
                Description = "Biba A-Line Kurta Set in vibrant ethnic prints. Crafted from soft cotton blend, this kurta set includes kurta with matching palazzo and dupatta for a complete ethnic look.",
                AverageRating = 4.4,
                TotalReviews = 1823,
                TotalSold = 4500
            },
            new()
            {
                Name = "H&M Women's Relaxed Linen Blazer",
                Slug = "hm-women-linen-blazer",
                Brand = "H&M",
                Sku = "HM-WBZ-LIN",
                Price = 3999,
                CompareAtPrice = 5499,
                StockQuantity = 60,
                CategoryId = womenClothing.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1548123378-bde4abb81072?w=600",
                ShortDescription = "Linen blend, oversized fit, notch lapels",
                Description = "H&M relaxed-fit blazer in a woven linen blend. Features notch lapels, chest pocket and side pockets. A versatile wardrobe staple that pairs with everything.",
                AverageRating = 4.2,
                TotalReviews = 734,
                TotalSold = 2100
            },

            // FOOTWEAR
            new()
            {
                Name = "Nike Air Max 270 React Sneakers",
                Slug = "nike-air-max-270-react",
                Brand = "Nike",
                Sku = "NIKE-AM270-WHT",
                Price = 12995,
                CompareAtPrice = 14995,
                StockQuantity = 90,
                IsFeatured = true,
                CategoryId = footwear.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=600",
                ShortDescription = "Air Max unit + React foam, lifestyle sneaker",
                Description = "Nike Air Max 270 React combines two of Nike's most innovative cushioning technologies—Air Max and React foam—for an extremely responsive, comfortable ride that looks as good as it feels.",
                AverageRating = 4.6,
                TotalReviews = 3892,
                TotalSold = 9200
            },
            new()
            {
                Name = "Adidas Ultraboost 22 Running Shoes",
                Slug = "adidas-ultraboost-22",
                Brand = "Adidas",
                Sku = "ADI-UB22-BLK",
                Price = 15999,
                CompareAtPrice = 17999,
                StockQuantity = 55,
                CategoryId = footwear.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?w=600",
                ShortDescription = "Boost midsole, Primeknit upper, Continental rubber",
                Description = "Adidas Ultraboost 22 features an enhanced BOOST midsole and a new Primeknit upper for a more responsive, sock-like fit. Continental rubber outsole offers excellent grip on wet and dry surfaces.",
                AverageRating = 4.7,
                TotalReviews = 2134,
                TotalSold = 5600
            },
            new()
            {
                Name = "Red Chief Men's Genuine Leather Formal Shoes",
                Slug = "red-chief-leather-formal-shoes",
                Brand = "Red Chief",
                Sku = "RC-FORM-BR",
                Price = 2999,
                CompareAtPrice = 4999,
                StockQuantity = 80,
                CategoryId = footwear.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1549298916-b41d501d3772?w=600",
                ShortDescription = "Genuine leather upper, cushioned insole, derby style",
                Description = "Red Chief formal shoes in genuine leather with a classic derby design. Features cushioned footbed, anti-skid rubber sole and superior craftsmanship for all-day comfort.",
                AverageRating = 4.3,
                TotalReviews = 1234,
                TotalSold = 3400
            },

            // KITCHEN
            new()
            {
                Name = "Prestige Svachh 5L Pressure Cooker",
                Slug = "prestige-svachh-5l-pressure-cooker",
                Brand = "Prestige",
                Sku = "PRE-PC5L",
                Price = 2199,
                CompareAtPrice = 3199,
                StockQuantity = 145,
                CategoryId = kitchen.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=600",
                ShortDescription = "5 litre, stainless steel, induction base, ISI marked",
                Description = "Prestige Svachh 5L hard anodised pressure cooker with patented 'Snap Lock' system that prevents the lid from opening accidentally. Works on all cooktops including induction.",
                AverageRating = 4.4,
                TotalReviews = 4521,
                TotalSold = 12000
            },
            new()
            {
                Name = "Philips HL7756 750W Mixer Grinder",
                Slug = "philips-hl7756-mixer-grinder",
                Brand = "Philips",
                Sku = "PHI-MG750",
                Price = 3299,
                CompareAtPrice = 4899,
                StockQuantity = 89,
                CategoryId = kitchen.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1570222094114-d054a817e56b?w=600",
                ShortDescription = "750W motor, 3 stainless steel jars, 3-speed control",
                Description = "Philips HL7756 mixer grinder with powerful 750W motor handles even the toughest grinding tasks. Includes 3 multipurpose jars with flow breakers for smooth mixing and grinding.",
                AverageRating = 4.3,
                TotalReviews = 2134,
                TotalSold = 5800
            },
            new()
            {
                Name = "Instant Pot Duo 7-in-1 Electric Pressure Cooker 6Qt",
                Slug = "instant-pot-duo-7in1-6qt",
                Brand = "Instant Pot",
                Sku = "IP-DUO-6QT",
                Price = 8999,
                CompareAtPrice = 12999,
                StockQuantity = 40,
                CategoryId = kitchen.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1585664811087-47f65abbad64?w=600",
                ShortDescription = "7-in-1: pressure cooker, slow cooker, rice cooker, steamer, sauté, yogurt maker, warmer",
                Description = "Instant Pot Duo is the world's best-selling multi-cooker. Replace 7 kitchen appliances with one versatile pot. Cook up to 70% faster than traditional cooking methods.",
                AverageRating = 4.7,
                TotalReviews = 1890,
                TotalSold = 3400
            },

            // FURNITURE
            new()
            {
                Name = "Wakefit Orthopedic Memory Foam Mattress Queen",
                Slug = "wakefit-orthopedic-mattress-queen",
                Brand = "Wakefit",
                Sku = "WF-ORTH-QUE",
                Price = 14999,
                CompareAtPrice = 22999,
                StockQuantity = 25,
                IsFeatured = true,
                CategoryId = furniture.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600",
                ShortDescription = "6-inch, medium-firm, 3-zone orthopedic support, 5yr warranty",
                Description = "Wakefit Orthopedic Memory Foam Mattress provides targeted support for your head, back and legs. The medium-firm feel is ideal for all sleep positions and comes with a 100-night free trial.",
                AverageRating = 4.5,
                TotalReviews = 3456,
                TotalSold = 4200
            },
            new()
            {
                Name = "IKEA KALLAX Shelf Unit White",
                Slug = "ikea-kallax-shelf-unit-white",
                Brand = "IKEA",
                Sku = "IKEA-KALLAX-WHT",
                Price = 5999,
                CompareAtPrice = 7499,
                StockQuantity = 30,
                CategoryId = furniture.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=600",
                ShortDescription = "4-cube shelving, 77x77cm, can be freestanding or wall-mounted",
                Description = "IKEA KALLAX shelf unit is a versatile storage solution that can be placed either freestanding or wall-mounted. Perfect for books, records, or decorative items.",
                AverageRating = 4.4,
                TotalReviews = 2123,
                TotalSold = 3100
            },

            // GYM EQUIPMENT
            new()
            {
                Name = "Boldfit Adjustable Dumbbell Set 20kg",
                Slug = "boldfit-adjustable-dumbbell-set-20kg",
                Brand = "Boldfit",
                Sku = "BF-DUMB-20",
                Price = 3499,
                CompareAtPrice = 4999,
                StockQuantity = 65,
                CategoryId = gym.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?w=600",
                ShortDescription = "20kg total, adjustable weight, ergonomic grip",
                Description = "Boldfit adjustable dumbbell set with 20kg total weight. Features easy weight adjustment mechanism and ergonomic anti-slip grip. Perfect for home gym workouts.",
                AverageRating = 4.3,
                TotalReviews = 1234,
                TotalSold = 2890
            },
            new()
            {
                Name = "PowerMax Fitness TDA-125 Treadmill",
                Slug = "powermax-tda-125-treadmill",
                Brand = "PowerMax",
                Sku = "PM-TDA125",
                Price = 34999,
                CompareAtPrice = 49999,
                StockQuantity = 12,
                CategoryId = gym.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=600",
                ShortDescription = "2HP motor, 12 programs, 3-level manual incline, foldable",
                Description = "PowerMax TDA-125 motorized treadmill with 2HP motor supports speeds from 1-14 km/h. Features 12 pre-set workout programs, 3-level manual incline and foldable design for compact storage.",
                AverageRating = 4.2,
                TotalReviews = 567,
                TotalSold = 780
            },
            new()
            {
                Name = "Strauss Anti-Skid Yoga Mat 6mm",
                Slug = "strauss-yoga-mat-6mm",
                Brand = "Strauss",
                Sku = "STRS-YM-6",
                Price = 699,
                CompareAtPrice = 999,
                StockQuantity = 400,
                CategoryId = gym.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1545205597-3d9d02c29597?w=600",
                ShortDescription = "6mm thickness, anti-skid, eco-friendly NBR foam",
                Description = "Strauss 6mm yoga mat provides excellent cushioning and joint support during yoga, pilates and stretching. Anti-skid surface ensures stability during poses. Lightweight and portable with carry strap.",
                AverageRating = 4.5,
                TotalReviews = 5678,
                TotalSold = 18000
            },

            // BEAUTY
            new()
            {
                Name = "Minimalist 10% Niacinamide Serum 30ml",
                Slug = "minimalist-niacinamide-serum-30ml",
                Brand = "Minimalist",
                Sku = "MIN-NIAC-30",
                Price = 599,
                CompareAtPrice = 799,
                StockQuantity = 500,
                IsFeatured = true,
                CategoryId = beauty.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1598440947619-2c35fc9aa908?w=600",
                ShortDescription = "10% Niacinamide + 1% Zinc, reduces blemishes, pore minimizer",
                Description = "Minimalist 10% Niacinamide serum visibly reduces blemishes and congestion. Zinc balances visible sebum production. Suitable for oily and combination skin types.",
                AverageRating = 4.7,
                TotalReviews = 8901,
                TotalSold = 25000,
                Tags = "skincare,serum,niacinamide,minimalist"
            },
            new()
            {
                Name = "L'Oreal Paris Revitalift Vitamin C Serum",
                Slug = "loreal-revitalift-vitamin-c-serum",
                Brand = "L'Oreal Paris",
                Sku = "LOR-VC-SER",
                Price = 899,
                CompareAtPrice = 1199,
                StockQuantity = 250,
                CategoryId = beauty.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=600",
                ShortDescription = "Pure Vitamin C 12%, brightens skin, anti-aging",
                Description = "L'Oreal Paris Revitalift 1.5% Pure Vitamin C serum visibly brightens skin, reduces dark spots and provides anti-aging benefits. Results visible in 1 week.",
                AverageRating = 4.5,
                TotalReviews = 4321,
                TotalSold = 11000
            },
            new()
            {
                Name = "Mamaearth Onion Hair Oil 250ml",
                Slug = "mamaearth-onion-hair-oil-250ml",
                Brand = "Mamaearth",
                Sku = "MME-ONIONOIL",
                Price = 349,
                CompareAtPrice = 499,
                StockQuantity = 800,
                CategoryId = beauty.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1526045612212-70caf35c14df?w=600",
                ShortDescription = "Onion extract, reduces hair fall, toxin-free",
                Description = "Mamaearth Onion Hair Oil enriched with onion oil, redensyl and bhringraj promotes hair growth and reduces hair fall. 100% natural ingredients, toxin-free.",
                AverageRating = 4.3,
                TotalReviews = 12345,
                TotalSold = 45000
            },

            // BOOKS
            new()
            {
                Name = "Atomic Habits by James Clear",
                Slug = "atomic-habits-james-clear",
                Brand = "Penguin",
                Sku = "BK-ATOM-HAB",
                Price = 449,
                CompareAtPrice = 699,
                StockQuantity = 1000,
                IsFeatured = true,
                CategoryId = books.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600",
                ShortDescription = "Tiny Changes, Remarkable Results - #1 NY Times bestseller",
                Description = "Atomic Habits by James Clear is a comprehensive guide to building good habits and breaking bad ones. With over 10 million copies sold, discover the framework behind lasting habit change.",
                AverageRating = 4.9,
                TotalReviews = 45678,
                TotalSold = 120000,
                Tags = "bestseller,habits,self-help,productivity"
            },
            new()
            {
                Name = "The Psychology of Money by Morgan Housel",
                Slug = "psychology-of-money-morgan-housel",
                Brand = "Jaico Publishing",
                Sku = "BK-PSY-MONEY",
                Price = 399,
                CompareAtPrice = 599,
                StockQuantity = 800,
                CategoryId = books.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1544947950-fa07a98d237f?w=600",
                ShortDescription = "Timeless lessons on wealth, greed and happiness",
                Description = "Morgan Housel explores how money moves around in an economy and how our behavior determines success. 19 short stories exploring how humans think about money.",
                AverageRating = 4.8,
                TotalReviews = 23456,
                TotalSold = 85000
            },
            new()
            {
                Name = "Rich Dad Poor Dad by Robert Kiyosaki",
                Slug = "rich-dad-poor-dad-robert-kiyosaki",
                Brand = "Plata Publishing",
                Sku = "BK-RICH-DAD",
                Price = 329,
                CompareAtPrice = 499,
                StockQuantity = 1200,
                CategoryId = books.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=600",
                ShortDescription = "What the rich teach their kids about money that the poor do not",
                Description = "Rich Dad Poor Dad is Robert Kiyosaki's bestselling personal finance book that challenges conventional wisdom about money. Learn the mindset of the wealthy.",
                AverageRating = 4.7,
                TotalReviews = 67890,
                TotalSold = 200000
            },
            new()
            {
                Name = "Zero to One by Peter Thiel",
                Slug = "zero-to-one-peter-thiel",
                Brand = "Crown Business",
                Sku = "BK-ZERO-ONE",
                Price = 399,
                CompareAtPrice = 549,
                StockQuantity = 500,
                CategoryId = books.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1550399105-c4db5fb85c18?w=600",
                ShortDescription = "Notes on Startups, or How to Build the Future",
                Description = "Peter Thiel, legendary entrepreneur and investor, shows how we can find singular ways to create those new things. Packed with contrarian thinking on startups and innovation.",
                AverageRating = 4.6,
                TotalReviews = 12345,
                TotalSold = 40000
            },

            // TOYS
            new()
            {
                Name = "LEGO City Police Station 60316",
                Slug = "lego-city-police-station-60316",
                Brand = "LEGO",
                Sku = "LEGO-60316",
                Price = 7999,
                CompareAtPrice = 9999,
                StockQuantity = 45,
                CategoryId = toys.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1558060370-d644479cb6f7?w=600",
                ShortDescription = "668 pieces, 5 minifigures, LEGO+ app compatible, age 6+",
                Description = "LEGO City Police Station 60316 has everything kids need for fun police play. Includes 5 minifigures, 2 cars, motorcycle and prison van. Compatible with the LEGO Building Instructions app.",
                AverageRating = 4.8,
                TotalReviews = 2341,
                TotalSold = 5600
            },
            new()
            {
                Name = "Funskool Candy Land Board Game",
                Slug = "funskool-candy-land-board-game",
                Brand = "Funskool",
                Sku = "FUN-CANDY-BG",
                Price = 599,
                CompareAtPrice = 799,
                StockQuantity = 200,
                CategoryId = toys.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1606503153255-59d8b8b82176?w=600",
                ShortDescription = "Classic board game, 2-4 players, age 3+",
                Description = "Candy Land classic board game is a sweet adventure for young children. No reading required — just match colors and move through the candy-themed land. Develops color recognition and social skills.",
                AverageRating = 4.5,
                TotalReviews = 1234,
                TotalSold = 6700
            },

            // Additional products for variety
            new()
            {
                Name = "boAt Rockerz 450 Wireless Headphones",
                Slug = "boat-rockerz-450-wireless",
                Brand = "boAt",
                Sku = "BOAT-R450",
                Price = 1499,
                CompareAtPrice = 3990,
                StockQuantity = 250,
                CategoryId = headphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600",
                ShortDescription = "40mm drivers, 15hr playback, 40ms low latency mode",
                Description = "boAt Rockerz 450 delivers powerful audio with 40mm dynamic drivers and immersive Super Bass. With 15 hours playback and a foldable design, music follows you everywhere.",
                AverageRating = 4.1,
                TotalReviews = 45678,
                TotalSold = 120000
            },
            new()
            {
                Name = "Acer Aspire Lite AMD Ryzen 5 15.6\" Laptop",
                Slug = "acer-aspire-lite-ryzen5-15",
                Brand = "Acer",
                Sku = "ACER-ASL-R5",
                Price = 42990,
                CompareAtPrice = 54990,
                StockQuantity = 35,
                CategoryId = laptops.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=600",
                ShortDescription = "Ryzen 5 7520U, 8GB DDR5, 512GB NVMe, Full HD IPS",
                Description = "Acer Aspire Lite with AMD Ryzen 5 7520U delivers reliable performance for everyday computing. Features a Full HD IPS display, DDR5 RAM and fast NVMe SSD.",
                AverageRating = 4.2,
                TotalReviews = 2134,
                TotalSold = 4500
            },
            new()
            {
                Name = "Noise ColorFit Ultra 2 Smartwatch",
                Slug = "noise-colorfit-ultra-2-smartwatch",
                Brand = "Noise",
                Sku = "NOISE-CFU2",
                Price = 1999,
                CompareAtPrice = 7999,
                StockQuantity = 300,
                CategoryId = electronics.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600",
                ShortDescription = "1.96\" AMOLED, Bluetooth calling, 100+ sports modes",
                Description = "Noise ColorFit Ultra 2 with 1.96\" AMOLED display and Bluetooth calling brings premium smartwatch features at an accessible price. 100+ sports modes and 7-day battery life.",
                AverageRating = 4.0,
                TotalReviews = 23456,
                TotalSold = 80000
            },
            new()
            {
                Name = "boAt Airdopes 141 TWS Earbuds",
                Slug = "boat-airdopes-141-tws",
                Brand = "boAt",
                Sku = "BOAT-AD141",
                Price = 999,
                CompareAtPrice = 2990,
                StockQuantity = 600,
                CategoryId = headphones.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1603351154351-5e2d0600bb77?w=600",
                ShortDescription = "8mm drivers, 42hr playtime, IPX4, ASAP Charge",
                Description = "boAt Airdopes 141 with 8mm drivers delivers immersive audio in a lightweight design. 42 total hours of playtime with ASAP Charge — 5 minutes charging gives 75 minutes playback.",
                AverageRating = 4.0,
                TotalReviews = 89012,
                TotalSold = 300000
            },
            new()
            {
                Name = "Fossil Gen 6 Hybrid Smartwatch",
                Slug = "fossil-gen6-hybrid-smartwatch",
                Brand = "Fossil",
                Sku = "FOSSIL-GEN6H",
                Price = 14995,
                CompareAtPrice = 19995,
                StockQuantity = 30,
                CategoryId = electronics.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600",
                ShortDescription = "Analog hands + E-ink display, 2-week battery, wellness tracking",
                Description = "Fossil Gen 6 Hybrid smartwatch combines traditional watch aesthetics with smart features. Features up to 2-week battery life, wellness tracking, and a subtle e-ink display.",
                AverageRating = 4.3,
                TotalReviews = 1234,
                TotalSold = 2800
            },
            new()
            {
                Name = "Himalaya Herbals Purifying Neem Face Wash 150ml",
                Slug = "himalaya-neem-face-wash-150ml",
                Brand = "Himalaya",
                Sku = "HIM-NEEM-FW",
                Price = 175,
                CompareAtPrice = 225,
                StockQuantity = 2000,
                CategoryId = beauty.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1556228720-195a672e8a03?w=600",
                ShortDescription = "Neem + Turmeric, removes excess oil, prevents pimples",
                Description = "Himalaya Purifying Neem Face Wash with neem and turmeric deeply cleanses skin, removes excess oil and prevents pimples. Safe for daily use, dermatologist tested.",
                AverageRating = 4.4,
                TotalReviews = 78901,
                TotalSold = 350000
            },
            new()
            {
                Name = "Fujifilm Instax Mini 12 Instant Camera",
                Slug = "fujifilm-instax-mini-12",
                Brand = "Fujifilm",
                Sku = "FUJI-IM12",
                Price = 6999,
                CompareAtPrice = 8999,
                StockQuantity = 55,
                CategoryId = cameras.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600",
                ShortDescription = "Auto exposure, selfie mode, Close-up lens attachment",
                Description = "Fujifilm Instax Mini 12 instant camera automatically adjusts brightness for perfectly exposed photos. New selfie mode with built-in close-up lens lets you capture beautiful close-up selfies.",
                AverageRating = 4.6,
                TotalReviews = 5678,
                TotalSold = 18000
            },
            new()
            {
                Name = "Milton Thermosteel Flip Lid Flask 500ml",
                Slug = "milton-thermosteel-flip-flask-500ml",
                Brand = "Milton",
                Sku = "MIL-FLIP-500",
                Price = 699,
                CompareAtPrice = 999,
                StockQuantity = 500,
                CategoryId = kitchen.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1602143407151-7111542de6e8?w=600",
                ShortDescription = "Keeps hot 24hr, cold 12hr, stainless steel, leak proof",
                Description = "Milton Thermosteel Flip Lid Flask 500ml maintains temperature for 24 hours hot and 12 hours cold. 100% stainless steel interior, leak-proof flip lid, easy single-hand operation.",
                AverageRating = 4.5,
                TotalReviews = 12345,
                TotalSold = 45000
            },
            new()
            {
                Name = "Puma Men's Cali Sport Sneakers",
                Slug = "puma-men-cali-sport-sneakers",
                Brand = "Puma",
                Sku = "PUMA-CALI-W",
                Price = 6999,
                CompareAtPrice = 8999,
                StockQuantity = 70,
                CategoryId = footwear.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1584735175315-9d5df23be31f?w=600",
                ShortDescription = "California vibes, SoftFoam+ insole, suede and mesh upper",
                Description = "Puma Cali Sport brings California beach vibes with a chunky 90s-inspired silhouette. SoftFoam+ sockliner and rubber outsole provide all-day comfort and grip.",
                AverageRating = 4.4,
                TotalReviews = 2345,
                TotalSold = 6700
            },
            new()
            {
                Name = "Hamleys My Little Pony Deluxe Set",
                Slug = "hamleys-my-little-pony-deluxe-set",
                Brand = "Hamleys",
                Sku = "HAM-MLP-DLX",
                Price = 1999,
                CompareAtPrice = 2999,
                StockQuantity = 80,
                CategoryId = toys.Id,
                ThumbnailUrl = "https://images.unsplash.com/photo-1558060370-d644479cb6f7?w=600",
                ShortDescription = "6 ponies, accessories, age 3+, colorful and durable",
                Description = "Hamleys My Little Pony Deluxe Set includes 6 pony figures with brushable manes and tails plus accessories. Encourage creative play and storytelling for kids aged 3 and above.",
                AverageRating = 4.6,
                TotalReviews = 1234,
                TotalSold = 4500
            },
        };

        var existingSkus = (await context.Products.Select(p => p.Sku).ToListAsync())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var existingSlugs = (await context.Products.Select(p => p.Slug).ToListAsync())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newProducts = products
            .Where(p => !existingSkus.Contains(p.Sku) && !existingSlugs.Contains(p.Slug))
            .ToList();

        if (newProducts.Any())
        {
            context.Products.AddRange(newProducts);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCouponsAsync(ShopVerseDbContext context)
    {
        if (await context.Coupons.AnyAsync()) return;

        var coupons = new List<Coupon>
        {
            new()
            {
                Code = "WELCOME10",
                Description = "10% off on your first order",
                Type = CouponType.Percentage,
                Value = 10,
                MinOrderAmount = 500,
                MaxDiscountAmount = 500,
                UsageLimit = 1000,
                PerUserLimit = 1,
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMonths(12)
            },
            new()
            {
                Code = "SAVE500",
                Description = "Flat ₹500 off on orders above ₹2999",
                Type = CouponType.FixedAmount,
                Value = 500,
                MinOrderAmount = 2999,
                UsageLimit = 500,
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMonths(6)
            },
            new()
            {
                Code = "FREESHIP",
                Description = "Free shipping on all orders",
                Type = CouponType.FreeShipping,
                Value = 0,
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMonths(3)
            },
            new()
            {
                Code = "SUMMER25",
                Description = "25% off summer sale",
                Type = CouponType.Percentage,
                Value = 25,
                MinOrderAmount = 1000,
                MaxDiscountAmount = 2000,
                UsageLimit = 200,
                IsActive = true,
                ExpiresAt = DateTime.UtcNow.AddMonths(2)
            }
        };

        context.Coupons.AddRange(coupons);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoOrdersAsync(ShopVerseDbContext context, UserManager<User> userManager)
    {
        if (await context.Orders.AnyAsync()) return;

        var customer = await userManager.FindByEmailAsync("john@example.com");
        if (customer == null) return;

        var products = await context.Products.Take(3).ToListAsync();
        if (!products.Any()) return;

        // Add address for the customer
        var address = new Address
        {
            UserId = customer.Id,
            FullName = "John Doe",
            Phone = "9876543210",
            Line1 = "42, MG Road",
            Line2 = "Near Central Mall",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            IsDefault = true
        };
        context.Addresses.Add(address);
        await context.SaveChangesAsync();

        var order = new Order
        {
            OrderNumber = $"SV{DateTime.UtcNow:yyyyMMdd}0001",
            UserId = customer.Id,
            ShippingAddressId = address.Id,
            Status = OrderStatus.Delivered,
            DeliveryOption = DeliveryOption.Standard,
            SubTotal = products[0].Price,
            ShippingAmount = 0,
            TaxAmount = products[0].Price * 0.18m,
            DiscountAmount = 0,
            TotalAmount = products[0].Price + products[0].Price * 0.18m,
            DeliveredAt = DateTime.UtcNow.AddDays(-5),
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = products[0].Id,
                    ProductName = products[0].Name,
                    ProductThumbnail = products[0].ThumbnailUrl,
                    Quantity = 1,
                    UnitPrice = products[0].Price,
                    TotalPrice = products[0].Price
                }
            },
            StatusHistory = new List<OrderStatusHistory>
            {
                new() { Status = OrderStatus.Placed, Comment = "Order placed", ChangedAt = DateTime.UtcNow.AddDays(-10) },
                new() { Status = OrderStatus.Confirmed, Comment = "Order confirmed", ChangedAt = DateTime.UtcNow.AddDays(-9) },
                new() { Status = OrderStatus.Packed, Comment = "Order packed", ChangedAt = DateTime.UtcNow.AddDays(-8) },
                new() { Status = OrderStatus.Shipped, Comment = "Shipped via BlueDart", ChangedAt = DateTime.UtcNow.AddDays(-7) },
                new() { Status = OrderStatus.OutForDelivery, Comment = "Out for delivery", ChangedAt = DateTime.UtcNow.AddDays(-6) },
                new() { Status = OrderStatus.Delivered, Comment = "Delivered successfully", ChangedAt = DateTime.UtcNow.AddDays(-5) },
            }
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        // Add a review
        var review = new Review
        {
            UserId = customer.Id,
            ProductId = products[0].Id,
            Rating = 5,
            Title = "Excellent product!",
            Body = "Really happy with this purchase. Quality is top-notch and delivery was fast. Highly recommended!",
            IsVerifiedPurchase = true
        };
        context.Reviews.Add(review);
        await context.SaveChangesAsync();
    }
}
