using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BRT.Models;

namespace BRT.Data
{
    public static class DbInitializer
    {
        private record PackSeed(string PackSize, string Unit, bool Wholesale, bool Loose, int Stock);
        private record ProductSeed(string SubCategorySlug, string Name, string NameTamil, string? Grade, decimal Price, PackSeed[] Packs, bool LooseAvailable = false);

        public static void Seed(ApplicationDbContext context, IConfiguration config)
        {
            // Applies any pending EF Core migrations (creates the DB + tables if it doesn't exist yet)
            context.Database.Migrate();

            // --- Seed single Admin user ---
            if (!context.AdminUsers.Any())
            {
                var hasher = new PasswordHasher<AdminUser>();
                var admin = new AdminUser
                {
                    FullName = config["AdminSeed:FullName"] ?? "BRT Admin",
                    Email = config["AdminSeed:Email"] ?? "admin@brajtraders.com",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                admin.PasswordHash = hasher.HashPassword(admin, config["AdminSeed:Password"] ?? "Brt@2000#Admin");
                context.AdminUsers.Add(admin);
                context.SaveChanges();
            }

            // --- Seed categories/subcategories ---
            Dictionary<string, int> subCategoryIds = new();
            if (!context.Categories.Any())
            {
                var garlic = new Category { Name = "Garlic", NameTamil = "பூண்டு", Slug = "garlic", Type = CategoryType.Garlic, DisplayOrder = 1 };
                var grocery = new Category { Name = "Grocery", NameTamil = "மளிகை", Slug = "grocery", Type = CategoryType.Grocery, DisplayOrder = 2 };
                context.Categories.AddRange(garlic, grocery);
                context.SaveChanges();

                var subs = new List<SubCategory>
                {
                    new() { CategoryId = garlic.Id, Name = "MP Garlic", NameTamil = "எம்பி பூண்டு", Slug = "mp-garlic", DisplayOrder = 1 },
                    new() { CategoryId = garlic.Id, Name = "Himachal Garlic", NameTamil = "இமாச்சல் பூண்டு", Slug = "himachal-garlic", DisplayOrder = 2 },
                    new() { CategoryId = garlic.Id, Name = "Kashmir Garlic", NameTamil = "காஷ்மீர் பூண்டு", Slug = "kashmir-garlic", DisplayOrder = 3 },
                    new() { CategoryId = grocery.Id, Name = "Spices", NameTamil = "மசாலா பொருட்கள்", Slug = "spices", DisplayOrder = 1 }
                };
                context.SubCategories.AddRange(subs);
                context.SaveChanges();
            }

            foreach (var s in context.SubCategories.ToList())
                subCategoryIds[s.Slug] = s.Id;

            // --- Seed full product catalog (from VARIETIES OF GARLIC + GROCERY & SPICES BRT docs) ---
            if (!context.Products.Any() && subCategoryIds.Count > 0)
            {
                var bag50 = new PackSeed("50 KG Bag", "KG", true, false, 20);
                var bag30 = new PackSeed("30 KG Bag", "KG", true, false, 20);
                PackSeed[] looseGrocery = {
                    new("50g", "G", false, true, 100),
                    new("100g", "G", false, true, 100),
                    new("250g", "G", false, true, 100),
                    new("500g", "G", false, true, 100),
                    new("1 Kg", "KG", false, true, 100)
                };

                var seeds = new List<ProductSeed>
                {
                    // ===== MP GARLIC =====
                    new("mp-garlic", "MP Garlic Bomb", "எம்பி பூண்டு பாம்ப்", "Bomb", 210, new[] { bag50, bag30 }),
                    new("mp-garlic", "MP Garlic Laddu", "எம்பி பூண்டு லட்டு", "Laddu", 180, new[] { bag50, bag30 }),
                    new("mp-garlic", "MP Garlic Poona Laddu", "எம்பி பூண்டு பூனா லட்டு", "Poona Laddu", 155, new[] { bag50, bag30 }),
                    new("mp-garlic", "MP Garlic Medium", "எம்பி பூண்டு மீடியம்", "Medium", 120, new[] { bag50, bag30 }),

                    // ===== HIMACHAL GARLIC =====
                    new("himachal-garlic", "Himachal Garlic AAA", "இமாச்சல் பூண்டு AAA", "AAA", 230, new[] { bag50, bag30 }),
                    new("himachal-garlic", "Himachal Garlic AA", "இமாச்சல் பூண்டு AA", "AA", 195, new[] { bag50, bag30 }),
                    new("himachal-garlic", "Himachal Garlic A", "இமாச்சல் பூண்டு A", "A", 165, new[] { bag50, bag30 }),
                    new("himachal-garlic", "Himachal Garlic C", "இமாச்சல் பூண்டு C", "C", 130, new[] { bag50, bag30 }),

                    // ===== KASHMIR GARLIC =====
                    new("kashmir-garlic", "Kashmir Garlic Bold", "காஷ்மீர் பூண்டு போல்ட்", "Bold", 205, new[] { bag50, bag30 }),
                    new("kashmir-garlic", "Kashmir Garlic Medium", "காஷ்மீர் பூண்டு மீடியம்", "Medium", 160, new[] { bag50, bag30 }),

                    // ===== GROCERY / SPICES =====
                    new("spices", "Turmeric", "மஞ்சள்", null, 140, new[] { bag50 }),

                    new("spices", "Black Pepper VPR Bold", "கருமிளகு VPR போல்ட்", "VPR Bold", 550, new[] { bag50 }, true),
                    new("spices", "Black Pepper VPR Medium", "கருமிளகு VPR மீடியம்", "VPR Medium", 500, new[] { bag50 }, true),

                    new("spices", "Jeera NN Gold", "சீரகம் NN கோல்டு", "NN Gold", 320, new[] { bag30 }, true),
                    new("spices", "Jeera GST", "சீரகம் GST", "GST", 300, new[] { bag30 }, true),

                    new("spices", "Sombu (Souff) NN Real Gold", "சோம்பு NN ரியல் கோல்டு", "NN Real Gold", 180, new[] { bag30 }, true),
                    new("spices", "Sombu (Souff) Tara", "சோம்பு டாரா", "Tara", 160, new[] { bag30 }, true),

                    new("spices", "Mustard Camel", "கடுகு கேமல்", "Camel", 90, new[] { bag50 }, true),
                    new("spices", "Small Mustard", "சிறிய கடுகு", null, 85, new[] { bag50 }, true),

                    new("spices", "Methi Horse", "வெந்தயம் ஹார்ஸ்", "Horse", 70, new[] { bag50 }),
                    new("spices", "Methi Mango", "வெந்தயம் மேங்கோ", "Mango", 65, new[] { bag50 }),

                    new("spices", "Split Cassia", "பட்டை", null, 250, new[] { new PackSeed("10 KG Box", "KG", true, false, 20) }),
                    new("spices", "Cloves", "கிராம்பு", null, 750, new[] { new PackSeed("10 KG Box", "KG", true, false, 20) }),
                    new("spices", "Star Anise", "அன்னாசி பூ", null, 900, new[] { new PackSeed("5 KG Box", "KG", true, false, 20) }),
                    new("spices", "Ajwain Seeds", "ஓமம்", null, 180, new[] { new PackSeed("1 KG Pack", "KG", true, false, 30) }),
                    new("spices", "Dry Ginger", "சுக்கு", null, 350, new[] { bag50 }),
                    new("spices", "Rock Sugar Crystal", "கல் சர்க்கரை", null, 90, new[] { bag50 }),
                    new("spices", "Poppy Seeds", "கசகசா", null, 950, new[] { bag50 }),
                    new("spices", "Raisins", "திராட்சை", null, 180, new[] { new PackSeed("10 KG Box", "KG", true, false, 20) }),
                    new("spices", "Black Raisins", "கருப்பு திராட்சை", null, 220, new[] { new PackSeed("15 KG Box", "KG", true, false, 20), new PackSeed("250g Packet", "G", false, true, 60) }),

                    new("spices", "Cardamom 8.0mm", "ஏலக்காய் 8.0mm", "8.0mm", 1800, new[] { new PackSeed("5 KG Pack", "KG", true, false, 15) }),
                    new("spices", "Cardamom 7.5mm", "ஏலக்காய் 7.5mm", "7.5mm", 1600, new[] { new PackSeed("5 KG Pack", "KG", true, false, 15) }),
                    new("spices", "Cardamom Mini Bold", "ஏலக்காய் மினி போல்ட்", "Mini Bold", 1400, new[] { new PackSeed("5 KG Pack", "KG", true, false, 15) }),

                    new("spices", "Bay Leaves", "பிரியாணி இலை", null, 200, new[] { new PackSeed("1 KG Pack", "KG", true, false, 30) }),
                    new("spices", "Black Stone Flower (Kalpasi)", "கல்பாசி", null, 1400, new[] { new PackSeed("20 KG Bag", "KG", true, false, 10) }),
                    new("spices", "Kapok Buds (Marathi Moggu)", "மராத்தி மொக்கு", null, 1600, new[] { bag50 }),
                    new("spices", "Mace (Japatri)", "ஜாபத்திரி", null, 2200, new[] { new PackSeed("5 KG Box", "KG", true, false, 10) }),
                    new("spices", "Nutmeg (Jathikai)", "ஜாதிக்காய்", null, 700, new[] { bag50 }),
                    new("spices", "Black Jeera", "கருஞ்சீரகம்", null, 450, new[] { bag30 }),
                };

                var today = DateTime.UtcNow.Date;
                foreach (var s in seeds)
                {
                    if (!subCategoryIds.TryGetValue(s.SubCategorySlug, out var subId)) continue;

                    var product = new Product
                    {
                        SubCategoryId = subId,
                        Name = s.Name,
                        NameTamil = s.NameTamil,
                        Slug = Slugify(s.Name),
                        Grade = s.Grade,
                        IsLooseAvailable = s.LooseAvailable,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Products.Add(product);
                    context.SaveChanges(); // need product.Id for packing types + price

                    foreach (var p in s.Packs)
                    {
                        context.PackingTypes.Add(new PackingType
                        {
                            ProductId = product.Id,
                            PackSize = p.PackSize,
                            PackUnit = p.Unit,
                            IsWholesale = p.Wholesale,
                            IsLoose = p.Loose,
                            StockQuantity = p.Stock,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });
                    }

                    if (s.LooseAvailable)
                    {
                        foreach (var p in looseGrocery)
                        {
                            context.PackingTypes.Add(new PackingType
                            {
                                ProductId = product.Id,
                                PackSize = p.PackSize,
                                PackUnit = p.Unit,
                                IsWholesale = p.Wholesale,
                                IsLoose = p.Loose,
                                StockQuantity = p.Stock,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }

                    // Today's placeholder price — Admin should confirm/update via Market Prices screen
                    context.MarketPrices.Add(new MarketPrice
                    {
                        ProductId = product.Id,
                        TodayPrice = s.Price,
                        PreviousPrice = s.Price,
                        GSTIncluded = true,
                        PriceDate = today,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = "Seed"
                    });
                }
                context.SaveChanges();
            }

            // --- Seed a few homepage highlights so the section isn't empty on first run ---
            if (!context.MarketHighlights.Any())
            {
                var picks = new (string ProductName, string Text, TrendDirection Trend)[]
                {
                    ("MP Garlic Bomb", "MP Garlic Bomb ↑", TrendDirection.Up),
                    ("Jeera NN Gold", "Jeera NN Gold ↓", TrendDirection.Down),
                    ("Cardamom 8.0mm", "Cardamom Fresh Stock", TrendDirection.NewStock),
                    ("Black Pepper VPR Bold", "Black Pepper Stable", TrendDirection.Stable),
                };
                var today = DateTime.UtcNow.Date;
                foreach (var (productName, text, trend) in picks)
                {
                    var slug = Slugify(productName);
                    var product = context.Products.IgnoreQueryFilters().FirstOrDefault(p => p.Slug == slug);
                    if (product == null) continue;
                    context.MarketHighlights.Add(new MarketHighlight
                    {
                        ProductId = product.Id,
                        HighlightText = text,
                        TrendDirection = trend,
                        DisplayDate = today,
                        IsActive = true
                    });
                }
                context.SaveChanges();
            }

            // --- Seed FAQ ---
            if (!context.FAQs.Any())
            {
                context.FAQs.AddRange(
                    new FAQ { Question = "Do you supply in bulk?", Answer = "Yes — B Raj Traders deals exclusively in bulk, dealer-to-dealer wholesale supply of garlic and grocery products. We don't offer retail/single-unit sales.", DisplayOrder = 1, IsActive = true },
                    new FAQ { Question = "Which areas do you deliver?", Answer = "We supply across India, with direct dedicated supply to Pondicherry and 15+ districts across Tamil Nadu.", DisplayOrder = 2, IsActive = true },
                    new FAQ { Question = "How can I place an order?", Answer = "Browse our Wholesale Products, pick what you need, and submit a Request Bulk Order. Our admin team will review, confirm pricing, and coordinate dispatch with you directly over WhatsApp or phone.", DisplayOrder = 3, IsActive = true },
                    new FAQ { Question = "What products do you offer?", Answer = "MP, Himachal, and Kashmir garlic across all grades, plus a full range of grocery spices — turmeric, pepper, jeera, cardamom, cloves, and more — in both wholesale and retail pack sizes.", DisplayOrder = 4, IsActive = true }
                );
                context.SaveChanges();
            }

            if (!context.SiteSettings.Any())
            {
                context.SiteSettings.AddRange(
                    new SiteSetting { Key = "WhatsAppNumber", Value = config["SiteInfo:WhatsAppNumber"] ?? "919865680694" },
                    new SiteSetting { Key = "Address", Value = config["SiteInfo:Address"] ?? "" },
                    new SiteSetting { Key = "GoogleMapUrl", Value = config["SiteInfo:GoogleMapUrl"] ?? "" }
                );
                context.SaveChanges();
            }
        }

        private static string Slugify(string name) =>
            name.Trim().ToLowerInvariant()
                .Replace(" ", "-").Replace("(", "").Replace(")", "")
                .Replace("&", "and").Replace(".", "");
    }
}
