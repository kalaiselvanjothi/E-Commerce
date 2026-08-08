# 📜 Changelog

All notable changes to the **ShopVerse** e-commerce platform will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-08-08

### ✨ Added
- **Catalog Seeding**: Populated 180 realistic catalog products across 6 core categories (`Fashion`, `Electronics`, `Home & Kitchen`, `Beauty`, `Sports`, `Accessories`) with 1,080 variants and 360 gallery images.
- **Product Details Enhancements**:
  - StockQuantity-aware quantity increment/decrement controls with dynamic stock badge (`In Stock (X available)`).
  - Live unit price, total price, total savings, and **18% GST tax** calculations.
  - Dynamic size pills (`XS` to `XXL`) for apparel/sports items and dynamic color swatches.
  - Vertical thumbnail gallery with active image selection and automatic broken link fallbacks.
- **Real-Time PostgreSQL Review System**:
  - Verified Purchase check (`OrderStatus.Delivered`) assigning Verified Buyer badges.
  - Review form with star rating, title, and body submitting directly to ASP.NET Core `ReviewsController`.
  - Immediate aggregate rating & review list update without page refresh.
- **Logistics Order Tracking**:
  - Query parameter preloading (`/track-order?orderNumber=ORD-XXXXX`).
  - 6-step visual delivery progress timeline.
- **Cart & Coupon Recalculation**:
  - Live coupon validation (`WELCOME10`, `SAVE500`, `SUMMER25`) with minimum order checks and subtotal recalculation.

### 🎨 Fixed
- **Visual Grid Alignment**: Restructured 4-column quick support cards and 2-column contact grid with clear 32px-48px box separation.
- **Track Package Button Alignment**: Aligned button baseline with input fields using label-spacers on `/track-order`.
- **Dark Mode Contrast**: Enhanced input field background, border, text, and placeholder visibility.
