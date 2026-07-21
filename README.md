# B RAJ TRADERS (BRT) — Complete Website
### Garlic & Grocery Commission Mandi — B2B Dealer-to-Dealer Wholesale Platform

Full ASP.NET Core 8 MVC solution — public site + Admin panel, wired end-to-end to a real SQL Server database. No cart, no checkout, no online payment — pure dealer-to-dealer request → review → confirm workflow, as specified.

---

## Product catalog — now fully seeded from your documents

I read both Word docs (`VARITIES OF GARLIC.docx` and `GROCERY & SPICES BRT.docx`) and seeded every product into the database on first run:

- **Garlic** — MP Garlic (Bomb, Laddu, Poona Laddu, Medium), Himachal Garlic (AAA, AA, A, C), Kashmir Garlic (Bold, Medium) — 10 products, each with 50 KG / 30 KG Bag packing options
- **Grocery & Spices** — all ~30 items from your list (Turmeric, Black Pepper VPR Bold/Medium, Jeera NN Gold/GST, Sombu NN Real Gold/Tara, Mustard, Methi Horse/Mango, Split Cassia, Cloves, Star Anise, Ajwain, Dry Ginger, Rock Sugar, Poppy Seeds, Raisins, Black Raisins, Cardamom 8.0mm/7.5mm/Mini Bold, Bay Leaves, Kalpasi, Marathi Moggu, Mace, Nutmeg, Black Jeera) — each with the exact packing sizes from your doc (e.g. Cloves = 10 KG Box, Cardamom = 5 KG Pack, etc.)
- **Loose retail packs** (50g/100g/250g/500g/1Kg) auto-added to Jeera, Sombu, Methi, Mustard, and Black Pepper — matching the original spec's Loose Grocery section
- Every product also got Tamil names filled in automatically

**⚠️ Important — Today's Prices are placeholders.** I don't have your real current market rates, so I filled in reasonable ballpark figures (₹/kg) just so the site isn't empty and every product shows a live price on day one. **Please go to Admin → Market Prices and correct every price to today's actual rate before going live** — that screen is built exactly for this daily task.

No new migration is needed for this update (only seed data changed, not the database schema) — as long as you've already run `Add-Migration InitialCreate` (or `AddTamilNames` if you did it in two steps), just run the app and the full catalog will populate automatically on first launch.

Product photos aren't in your Word docs, so every product shows a neutral icon (🧄 / 🌶️) until you upload a real photo per product via Admin → Products → Edit → Product Image.

---

## What's included

### Public Website (mobile-first, animated) — redesigned
- New image-based hero (your reference spice photo as background, `wwwroot/images/hero-spices.png`) with right-aligned brand heading, tagline, a styled quote line, and **Request Order** + **WhatsApp** CTA buttons — OM SHANTI text header removed, replaced by this hero
- Branded nav bar (logo mark + BRT wordmark) with an **EN | தமிழ்** toggle button that switches every product/category name site-wide between English and Tamil, sticky + shrinks on scroll
- Home — hero, today's market highlights, wholesale categories (with icons), approved testimonials, FAQ accordion, disclaimer strip (scroll-reveal animated)
- Garlic Wholesale / Grocery Wholesale — grouped by sub-category, circular product photo/icon, live price + price movement (▲▼) + packing chips
- Loose Grocery Products — retail-pack listing (50g–1Kg)
- Product Detail page — full price block, packing options, Request/Call/WhatsApp CTAs, bilingual name
- Request Bulk Order — multi-item order form, no login required
- Contact page — phone/WhatsApp, embedded Google Map, contact form
- Richer footer — brand, contact, quick links

### Admin Panel (`/Admin/Account/Login`) — single Admin role, cookie auth
- Dashboard — order counts, product count, today's-price status alert, recent orders
- Categories — full CRUD for Categories + nested Sub-categories, **English + Tamil name fields**, **image upload** (file picker, saved to `wwwroot/uploads/categories`) or paste a URL instead
- Products — full CRUD, **English + Tamil name fields**, **image upload** (file picker, saved to `wwwroot/uploads/products`, 5MB limit, JPG/PNG/WEBP/GIF) or paste a URL, inline Packing Type management
- Market Prices — daily price entry per product, GST toggle
- Market Highlights — manage the homepage "Today's Market Highlights" strip
- Orders — filter by status, full order detail, status pipeline with audit history
- Website Content — tabs for Banners, Testimonials, FAQ, Gallery, Contact messages
- Settings — edit site-wide values

### Database (SQL Server via EF Core Migrations)
Normalized schema — Categories → SubCategories → Products → PackingTypes (all with `NameTamil` fields now), MarketPrices + MarketHighlights, OrderRequests + Items + StatusHistory, full CMS tables, SiteSettings, single AdminUsers table.

---

**If you already ran `Add-Migration InitialCreate` on an earlier version of this project:** this update added `NameTamil` fields to Category/SubCategory/Product. Run one more migration before starting the app:
```
Add-Migration AddTamilNames
```
If this is your first time setting up, just do the normal Step 3 below — `InitialCreate` will already include everything.

## How to run

This project uses **EF Core Migrations** (not EnsureCreated), so the DB schema is properly versioned — you can evolve it later without dropping data.

1. Open `BRT.sln` in Visual Studio 2022 (.NET 8 SDK), or use `dotnet` CLI
2. Update `appsettings.json` → `ConnectionStrings:DefaultConnection` with your SQL Server instance
3. **Create the migration (one-time, only needed the first time or after you change a model):**
   - Visual Studio → Tools → NuGet Package Manager → **Package Manager Console** → run:
     ```
     Add-Migration InitialCreate
     ```
   - Or via terminal (needs `dotnet-ef` tool — `dotnet tool install --global dotnet-ef` if you don't have it):
     ```
     dotnet ef migrations add InitialCreate
     ```
   This creates a `Migrations/` folder with the schema script — commit this folder to source control.
4. Run the project (**Ctrl+F5** in Visual Studio, or `dotnet run`). On startup, `Database.Migrate()` runs automatically — it creates the database + all tables + applies the migration, then seeds the admin account and starter data. You do **not** need to separately run `Update-Database` — startup does it for you.
5. Visit `/` for the public site, `/Admin/Account/Login` for the admin panel

**If you change any entity later** (add a field, new table, etc.), repeat step 3 with a new name (e.g. `Add-Migration AddDealerNotes`) and just re-run the app — it auto-applies on startup.

**Default admin login (change immediately in `appsettings.json` → `AdminSeed` before first run, or update the DB row after):**
- Email: `admin@brajtraders.com`
- Password: `Brt@2000#Admin`

## First things to do after first run
1. Log in to Admin → change the seeded admin password/email via SQL or extend the Settings screen
2. Categories → Products → add your real Garlic/Grocery catalog with images (replace `MP Garlic`/`Himachal Garlic`/`Kashmir Garlic` starter sub-categories as needed)
3. Market Prices → enter today's prices for every active product (there's a dashboard warning until you do)
4. Website Content → add banners, a few testimonials (approve them), FAQ, gallery photos
5. Settings → confirm WhatsApp number / address / map URL

## Notes / known limits (be upfront about these)
- Product images are URL-based (paste a hosted image link) — no file upload yet; add one later if needed
- Order request form supports up to 5 product rows per submission (edit `OrderRequestViewModel.Items` count to increase)
- No SMS/email notifications wired — WhatsApp is via `api.whatsapp.com` deep links (opens chat, doesn't auto-send)
- This was hand-written to spec and has **not** been compiled in a real .NET environment (sandbox has no NuGet access) — please run a build after download and ping me with any error output so I can fix it fast
