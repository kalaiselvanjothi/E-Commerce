# 🛒 ShopVerse — Premium Full-Stack E-Commerce Platform

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-18-DD0031?logo=angular&logoColor=white)](https://angular.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?logo=dotnet&logoColor=white)](https://learn.microsoft.com/en-us/ef/core/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-22C55E.svg)]()

> **ShopVerse** is an enterprise-grade, high-performance full-stack e-commerce web application built with **ASP.NET Core 8 Web API** and **Angular 18 (Standalone Components & Signals)** backed by a **PostgreSQL** database and integrated with **Razorpay Payment Gateway**.

---

## 📸 Screenshots & Overview

| Desktop Dark Theme | Mobile Responsive |
| :---: | :---: |
| ![ShopVerse Dark Mode](https://raw.githubusercontent.com/kalaiselvanjothi/E-Commerce/main/docs/images/dark-hero.png) | ![ShopVerse Mobile View](https://raw.githubusercontent.com/kalaiselvanjothi/E-Commerce/main/docs/images/mobile-view.png) |

| Product Details & Review System | Interactive Order Tracking |
| :---: | :---: |
| ![Product Details Page](https://raw.githubusercontent.com/kalaiselvanjothi/E-Commerce/main/docs/images/product-detail.png) | ![Order Tracking Timeline](https://raw.githubusercontent.com/kalaiselvanjothi/E-Commerce/main/docs/images/order-track.png) |

---

## ✨ Features List

### 🛍️ Customer Experience
- **Catalog Navigation & Filtering**: 180+ high-quality catalog products across 6 core categories (`Fashion`, `Electronics`, `Home & Kitchen`, `Beauty`, `Sports`, `Accessories`) with live dynamic PostgreSQL category counts.
- **Advanced Multi-Facet Search & Filters**: Brand filter, price range slider, size picker, color swatches, tag search, instant search dropdown suggestions with `⌘K` keyboard shortcut support.
- **Product Details & Variant Picker**:
  - Size variants (`XS`, `S`, `M`, `L`, `XL`, `XXL`) for apparel & sports items.
  - Dynamic color swatches with active state selection.
  - Dynamic quantity selector enforcing stock limits (`stockQuantity`), updating unit price, total price, total savings, and **18% GST tax** calculations live.
  - High-resolution vertical thumbnail gallery with image zoom and automatic broken image fallback.
- **Real-Time PostgreSQL Review System**:
  - Rating breakdown & average score calculations.
  - Logged-in user review submission with Star Rating, Title, and Detailed Body.
  - Verified Purchase check (`OrderStatus.Delivered`) assigning Verified Buyer badges.
  - Immediate page-refreshless UI updates.
- **Dynamic Shopping Cart**:
  - Immediate stock validation and background sync with Product Details page.
  - Real-time coupon code validation (`WELCOME10`, `SAVE500`, `SUMMER25`) against database rules (min order, active status, expiration).
  - Free shipping progress bar (`₹499` threshold).
- **Checkout & Razorpay Payment Integration**:
  - Address selection & order review.
  - Razorpay payment modal integration with SSL protection verification.
- **Live Logistics Tracking**:
  - Order tracking (`/track-order`) with query parameter preloading (`?orderNumber=ORD-XXXXX`).
  - 6-step visual delivery progress timeline (`Placed` ➔ `Confirmed` ➔ `Packed` ➔ `Shipped` ➔ `Out for Delivery` ➔ `Delivered`).
- **User Account Management**: Order history, order cancellation, return request modal, PDF invoice downloading, and address book management.
- **Wishlist & Offers**: Direct toggle wishlist items and exclusive promo deals grid with 1-click coupon code copy.

### 🛡️ Enterprise Security & Performance
- **Authentication**: JWT Access Token + HttpOnly Refresh Token flow with bcrypt password hashing.
- **Role-Based Authorization**: `Admin` and `Customer` role guards in Angular and ASP.NET Core API.
- **Design System**: Modern 8px-grid design system with dark/light mode toggle, custom SCSS mixins, fluid typography, and zero magic numbers.

---

## 🛠️ Technology Stack

### Backend Architecture
- **Framework**: ASP.NET Core 8.0 Web API (Clean Architecture principles)
- **Database**: PostgreSQL 16 via Npgsql.EntityFrameworkCore.PostgreSQL
- **ORM**: Entity Framework Core 8.0 (Code-First with Seed Data)
- **Authentication**: JWT (JSON Web Tokens) + ASP.NET Core Authentication/Authorization
- **Payments**: Razorpay Payment Gateway API
- **API Documentation**: Swagger / OpenAPI

### Frontend Architecture
- **Framework**: Angular 18 (Standalone Components, Signals, Reactive Forms)
- **State Management**: Angular Signals & RxJS Observables
- **Styling**: Vanilla SCSS with custom design system tokens, CSS variables & mixins
- **Routing**: Angular Router with lazy loading, route guards, and query param preloading
- **HTTP**: HttpClient with AuthInterceptor, ErrorInterceptor, and LoadingInterceptor

---

## 📁 Repository Folder Structure

```
E-Commerce/
├── ecommerce-api/                   # ASP.NET Core 8.0 Web API Solution
│   ├── src/
│   │   ├── ShopVerse.API/          # API Controllers, Middleware, Launch Settings
│   │   ├── ShopVerse.Application/  # DTOs, Business Services, Interfaces
│   │   ├── ShopVerse.Domain/       # EF Core Entities, Domain Enums
│   │   └── ShopVerse.Infrastructure/ # DbContext, Repositories, Razorpay & SeedData
│   └── ShopVerse.sln
│
├── ecommerce-client/                # Angular 18 Single Page Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/               # Guards, Interceptors, Models, Services
│   │   │   ├── features/           # Account, Admin, Auth, Cart, Contact, Home, Products, Track
│   │   │   ├── layout/             # Header, Footer, Shell Layout
│   │   │   └── shared/             # Components (ProductCard, StarRating, Pagination)
│   │   ├── styles/                 # Mixins, Design Variables, Global SCSS
│   │   └── main.ts
│   ├── package.json
│   └── angular.json
│
├── .gitignore                       # Multi-platform production gitignore
├── README.md                        # Documentation
├── LICENSE                          # MIT License
├── CONTRIBUTING.md                  # Contribution guidelines
├── SECURITY.md                      # Security policy
└── CODE_OF_CONDUCT.md               # Code of conduct
```

---

## ⚡ Quick Start & Installation

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+ & npm](https://nodejs.org/)
- [PostgreSQL 16](https://www.postgresql.org/)

### 1. Backend Setup (`ShopVerse.API`)

```bash
# Navigate to API directory
cd ecommerce-api/src/ShopVerse.API

# Restore dependencies
dotnet restore

# Run database migrations / seed data
dotnet ef database update --project ../ShopVerse.Infrastructure

# Start the Web API server
dotnet run
```
*API will run at `http://localhost:5034` with Swagger documentation at `http://localhost:5034/swagger`.*

### 2. Frontend Setup (`ecommerce-client`)

```bash
# Navigate to Angular client directory
cd ecommerce-client

# Install dependencies
npm install

# Start development server
npm start
```
*App will launch at `http://localhost:4200`.*

---

## 🔌 API Endpoints Summary

| Method | Endpoint | Description | Auth Required |
|---|---|---|---|
| `POST` | `/api/auth/register` | Register new customer account | No |
| `POST` | `/api/auth/login` | Authenticate user & receive JWT | No |
| `GET` | `/api/products` | Get paged product list with filters | No |
| `GET` | `/api/products/{slug}` | Get full product detail with variants & reviews | No |
| `GET` | `/api/categories` | Get category hierarchy with live product counts | No |
| `GET` | `/api/cart` | Get current user shopping cart | Yes |
| `POST` | `/api/cart/items` | Add item & variant to cart | Yes |
| `POST` | `/api/cart/coupon` | Apply coupon code with validation | Yes |
| `POST` | `/api/products/{productId}/reviews` | Create review for product | Yes |
| `GET` | `/api/orders/track` | Track order live logistics status | No |
| `POST` | `/api/payments/razorpay/create-order` | Initialize Razorpay payment session | Yes |

---

## 📄 License

Distributed under the **MIT License**. See [`LICENSE`](LICENSE) for more information.

---

## 👨‍💻 Author

**Kalaiselvan Jothi**  
GitHub: [@kalaiselvanjothi](https://github.com/kalaiselvanjothi)  
Repository: [kalaiselvanjothi/E-Commerce](https://github.com/kalaiselvanjothi/E-Commerce)
