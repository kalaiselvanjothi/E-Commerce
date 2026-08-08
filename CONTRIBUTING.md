# 🤝 Contributing to ShopVerse

Thank you for taking the time to contribute to **ShopVerse**! We welcome bug reports, feature requests, documentation improvements, and pull requests.

---

## 🛠️ Getting Started

1. **Fork the Repository**: Click the **Fork** button at the top right of the [ShopVerse Repository](https://github.com/kalaiselvanjothi/E-Commerce).
2. **Clone your Fork**:
   ```bash
   git clone https://github.com/YOUR-USERNAME/E-Commerce.git
   cd E-Commerce
   ```
3. **Create a Feature Branch**:
   ```bash
   git checkout -b feature/amazing-new-feature
   ```

---

## 📐 Coding & Quality Guidelines

### ASP.NET Core Web API
- Adhere to Clean Architecture boundaries (`Domain` ➔ `Application` ➔ `Infrastructure` ➔ `API`).
- Run `dotnet build` and ensure **0 compiler warnings and 0 errors**.
- Keep controllers thin; place business logic inside Application services.

### Angular 18 Client
- Use Angular Standalone Components and Signals (`signal()`, `computed()`).
- Maintain the 8px design token system in SCSS; do not hardcode magic numbers.
- Verify `npm run build` succeeds cleanly before pushing.

---

## 🚀 Submitting a Pull Request

1. Commit your changes with clear, descriptive commit messages:
   ```bash
   git commit -m "feat(product-detail): add stockQuantity fallback and GST recalculation"
   ```
2. Push to your branch:
   ```bash
   git push origin feature/amazing-new-feature
   ```
3. Open a **Pull Request** against `main` on GitHub with a clear explanation of your changes and verification steps.
