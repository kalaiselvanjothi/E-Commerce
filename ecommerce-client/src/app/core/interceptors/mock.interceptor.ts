import { HttpInterceptorFn, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ProductListItem, Category } from '../models/product.models';

export const MOCK_CATEGORIES: Category[] = [
  { id: '1', name: 'Fashion', slug: 'fashion', imageUrl: 'https://images.unsplash.com/photo-1445205170230-053b83016050?auto=format&fit=crop&w=600&q=80', description: 'Trendy clothing, apparel & footwear for men and women', productCount: 142, sortOrder: 1, isActive: true, children: [] },
  { id: '2', name: 'Electronics', slug: 'electronics', imageUrl: 'https://images.unsplash.com/photo-1498049860654-af1a5c566876?auto=format&fit=crop&w=600&q=80', description: 'Smartphones, laptops, audio gadgets & smart wearables', productCount: 215, sortOrder: 2, isActive: true, children: [] },
  { id: '3', name: 'Home & Kitchen', slug: 'home-kitchen', imageUrl: 'https://images.unsplash.com/photo-1556911220-e15b29be8c8f?auto=format&fit=crop&w=600&q=80', description: 'Modern cookware, decor, lighting & kitchen appliances', productCount: 98, sortOrder: 3, isActive: true, children: [] },
  { id: '4', name: 'Beauty', slug: 'beauty', imageUrl: 'https://images.unsplash.com/photo-1596462502278-27bfdc403348?auto=format&fit=crop&w=600&q=80', description: 'Skincare, premium makeup, haircare & organic fragrances', productCount: 110, sortOrder: 4, isActive: true, children: [] },
  { id: '5', name: 'Sports', slug: 'sports', imageUrl: 'https://images.unsplash.com/photo-1517649763962-0c623266010b?auto=format&fit=crop&w=600&q=80', description: 'Fitness gear, athletic footwear, sportswear & gym accessories', productCount: 84, sortOrder: 5, isActive: true, children: [] },
  { id: '6', name: 'Accessories', slug: 'accessories', imageUrl: 'https://images.unsplash.com/photo-1523293182086-7651a899d37f?auto=format&fit=crop&w=600&q=80', description: 'Luxury watches, sunglasses, leather bags & wallets', productCount: 76, sortOrder: 6, isActive: true, children: [] }
];

export const MOCK_PRODUCTS: ProductListItem[] = [
  {
    id: 'prod-1',
    name: 'Apple iPhone 15 Pro Max - 256GB Titanium',
    slug: 'apple-iphone-15-pro-max',
    brand: 'Apple',
    categoryName: 'Electronics',
    categorySlug: 'electronics',
    price: 134900,
    compareAtPrice: 159900,
    discountPercent: 15,
    primaryImageUrl: 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=600&q=80',
    stock: 12,
    averageRating: 4.9,
    totalReviews: 428,
    totalSold: 1250,
    isFeatured: true,
    isActive: true,
    tags: ['mobile', 'apple', 'flagship']
  },
  {
    id: 'prod-2',
    name: 'Sony WH-1000XM5 Wireless Headphones',
    slug: 'sony-wh1000xm5-headphones',
    brand: 'Sony',
    categoryName: 'Electronics',
    categorySlug: 'electronics',
    price: 24990,
    compareAtPrice: 34990,
    discountPercent: 28,
    primaryImageUrl: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&q=80',
    stock: 25,
    averageRating: 4.8,
    totalReviews: 892,
    totalSold: 3400,
    isFeatured: true,
    isActive: true,
    tags: ['audio', 'sony', 'wireless']
  },
  {
    id: 'prod-3',
    name: 'Nike Air Max 270 React Running Shoes',
    slug: 'nike-air-max-270-react',
    brand: 'Nike',
    categoryName: 'Fashion',
    categorySlug: 'fashion',
    price: 12495,
    compareAtPrice: 14995,
    discountPercent: 16,
    primaryImageUrl: 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=600&q=80',
    stock: 18,
    averageRating: 4.7,
    totalReviews: 310,
    totalSold: 980,
    isFeatured: true,
    isActive: true,
    tags: ['shoes', 'nike', 'fashion']
  },
  {
    id: 'prod-4',
    name: 'Minimalist Ceramic Pour-Over Coffee Set',
    slug: 'ceramic-pour-over-coffee-maker',
    brand: 'Artisan Home',
    categoryName: 'Home & Kitchen',
    categorySlug: 'home-kitchen',
    price: 3490,
    compareAtPrice: 4990,
    discountPercent: 30,
    primaryImageUrl: 'https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=600&q=80',
    stock: 45,
    averageRating: 4.6,
    totalReviews: 124,
    totalSold: 450,
    isFeatured: true,
    isActive: true,
    tags: ['home', 'coffee']
  },
  {
    id: 'prod-5',
    name: 'Glow Recipe Watermelon Niacinamide Glow Drops',
    slug: 'glow-recipe-watermelon-glow-drops',
    brand: 'Glow Recipe',
    categoryName: 'Beauty',
    categorySlug: 'beauty',
    price: 2990,
    compareAtPrice: 3500,
    discountPercent: 14,
    primaryImageUrl: 'https://images.unsplash.com/photo-1608248597261-833258657b45?auto=format&fit=crop&w=600&q=80',
    stock: 60,
    averageRating: 4.9,
    totalReviews: 540,
    totalSold: 2100,
    isFeatured: true,
    isActive: true,
    tags: ['beauty', 'skincare']
  },
  {
    id: 'prod-6',
    name: 'Smart Fitness Tracker Watch with GPS & AMOLED Display',
    slug: 'smart-fitness-tracker-watch',
    brand: 'FitTech',
    categoryName: 'Sports',
    categorySlug: 'sports',
    price: 8990,
    compareAtPrice: 12990,
    discountPercent: 30,
    primaryImageUrl: 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?auto=format&fit=crop&w=600&q=80',
    stock: 30,
    averageRating: 4.5,
    totalReviews: 215,
    totalSold: 800,
    isFeatured: true,
    isActive: true,
    tags: ['sports', 'fitness', 'smartwatch']
  },
  {
    id: 'prod-7',
    name: 'Italian Grain Leather Executive Briefcase',
    slug: 'italian-grain-leather-briefcase',
    brand: 'Bellroy',
    categoryName: 'Accessories',
    categorySlug: 'accessories',
    price: 18500,
    compareAtPrice: 22000,
    discountPercent: 15,
    primaryImageUrl: 'https://images.unsplash.com/photo-1548036328-c9fa89d128fa?auto=format&fit=crop&w=600&q=80',
    stock: 8,
    averageRating: 4.9,
    totalReviews: 78,
    totalSold: 230,
    isFeatured: false,
    isActive: true,
    tags: ['leather', 'bag', 'accessories']
  },
  {
    id: 'prod-8',
    name: 'Samsung Galaxy Watch 6 Classic 47mm LTE',
    slug: 'samsung-galaxy-watch-6-classic',
    brand: 'Samsung',
    categoryName: 'Electronics',
    categorySlug: 'electronics',
    price: 36999,
    compareAtPrice: 43999,
    discountPercent: 16,
    primaryImageUrl: 'https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?auto=format&fit=crop&w=600&q=80',
    stock: 14,
    averageRating: 4.7,
    totalReviews: 195,
    totalSold: 640,
    isFeatured: true,
    isActive: true,
    tags: ['samsung', 'smartwatch', 'electronics']
  }
];

export const MOCK_TRACKING_DATA: Record<string, any> = {
  'ORD-89412': {
    orderId: 'ORD-89412',
    orderDate: '2026-08-01T10:30:00Z',
    customerName: 'Alex Morgan',
    email: 'alex.morgan@example.com',
    status: 'Out for Delivery',
    currentStep: 5,
    estimatedDeliveryDate: '2026-08-06',
    courierName: 'ShopVerse Express',
    trackingNumber: 'SVX-99481023',
    shippingAddress: '42 Wallaby Way, Suite 100, San Francisco, CA 94103',
    items: [
      { name: 'Sony WH-1000XM5 Wireless Headphones', quantity: 1, price: 24990, image: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=300&q=80' },
      { name: 'Glow Recipe Watermelon Glow Drops', quantity: 2, price: 2990, image: 'https://images.unsplash.com/photo-1608248597261-833258657b45?auto=format&fit=crop&w=300&q=80' }
    ],
    timeline: [
      { step: 'Order Placed', timestamp: 'Aug 01, 10:30 AM', location: 'Online Store', completed: true },
      { step: 'Payment Confirmed', timestamp: 'Aug 01, 10:32 AM', location: 'Payment Gateway', completed: true },
      { step: 'Packed', timestamp: 'Aug 02, 04:15 PM', location: 'Fulfillment Hub - SF', completed: true },
      { step: 'Shipped', timestamp: 'Aug 03, 08:00 AM', location: 'Courier Sorting Facility', completed: true },
      { step: 'Out for Delivery', timestamp: 'Aug 05, 08:45 AM', location: 'Local Logistics Hub', completed: true, current: true },
      { step: 'Delivered', timestamp: 'Pending', location: 'Destination Address', completed: false }
    ]
  },
  'ORD-10294': {
    orderId: 'ORD-10294',
    orderDate: '2026-08-04T14:15:00Z',
    customerName: 'Sarah Jenkins',
    email: 'sarah.j@example.com',
    status: 'Packed',
    currentStep: 3,
    estimatedDeliveryDate: '2026-08-08',
    courierName: 'FedEx Priority',
    trackingNumber: 'FX-884920192',
    shippingAddress: '742 Evergreen Terrace, Springfield, OR 97477',
    items: [
      { name: 'Apple iPhone 15 Pro Max', quantity: 1, price: 134900, image: 'https://images.unsplash.com/photo-1695048133142-1a20484d2569?auto=format&fit=crop&w=300&q=80' }
    ],
    timeline: [
      { step: 'Order Placed', timestamp: 'Aug 04, 02:15 PM', location: 'Online Store', completed: true },
      { step: 'Payment Confirmed', timestamp: 'Aug 04, 02:16 PM', location: 'Payment Gateway', completed: true },
      { step: 'Packed', timestamp: 'Aug 05, 11:30 AM', location: 'Fulfillment Center 1', completed: true, current: true },
      { step: 'Shipped', timestamp: 'Pending', location: 'Transit Hub', completed: false },
      { step: 'Out for Delivery', timestamp: 'Pending', location: 'Local Logistics Center', completed: false },
      { step: 'Delivered', timestamp: 'Pending', location: 'Destination Address', completed: false }
    ]
  }
};

export const mockInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 0 || error.status === 404 || error.status === 500) {
        const url = req.url.toLowerCase();

        if (url.includes('/categories')) {
          return of(new HttpResponse({ status: 200, body: { data: MOCK_CATEGORIES } }));
        }

        if (url.includes('/products/featured')) {
          return of(new HttpResponse({ status: 200, body: { data: MOCK_PRODUCTS.filter(p => p.isFeatured) } }));
        }

        if (url.includes('/products/suggestions')) {
          const queryParam = req.params.get('q') || '';
          const filtered = MOCK_PRODUCTS
            .filter(p => p.name.toLowerCase().includes(queryParam.toLowerCase()))
            .map(p => ({ id: p.id, name: p.name, slug: p.slug, categoryName: p.categoryName, price: p.price, primaryImageUrl: p.primaryImageUrl }));
          return of(new HttpResponse({ status: 200, body: { data: filtered } }));
        }

        if (url.includes('/products/filter-meta')) {
          return of(new HttpResponse({
            status: 200,
            body: {
              data: {
                minPrice: 1000,
                maxPrice: 200000,
                brands: ['Apple', 'Sony', 'Nike', 'Samsung', 'Bellroy', 'Artisan Home', 'Glow Recipe', 'FitTech'],
                categories: MOCK_CATEGORIES
              }
            }
          }));
        }

        if (url.includes('/wishlist')) {
          const mockWishlist = {
            id: 'wishlist-1',
            userId: 'user-1',
            itemCount: 4,
            items: [
              {
                id: 'wi-1',
                productId: MOCK_PRODUCTS[1].id,
                productName: MOCK_PRODUCTS[1].name,
                productSlug: MOCK_PRODUCTS[1].slug,
                brand: MOCK_PRODUCTS[1].brand,
                productImage: MOCK_PRODUCTS[1].primaryImageUrl,
                price: MOCK_PRODUCTS[1].price,
                compareAtPrice: MOCK_PRODUCTS[1].compareAtPrice,
                discountPercent: MOCK_PRODUCTS[1].discountPercent,
                stock: MOCK_PRODUCTS[1].stock,
                averageRating: MOCK_PRODUCTS[1].averageRating,
                addedAt: '2026-08-05T00:00:00Z'
              },
              {
                id: 'wi-2',
                productId: MOCK_PRODUCTS[2].id,
                productName: MOCK_PRODUCTS[2].name,
                productSlug: MOCK_PRODUCTS[2].slug,
                brand: MOCK_PRODUCTS[2].brand,
                productImage: MOCK_PRODUCTS[2].primaryImageUrl,
                price: MOCK_PRODUCTS[2].price,
                compareAtPrice: MOCK_PRODUCTS[2].compareAtPrice,
                discountPercent: MOCK_PRODUCTS[2].discountPercent,
                stock: MOCK_PRODUCTS[2].stock,
                averageRating: MOCK_PRODUCTS[2].averageRating,
                addedAt: '2026-08-05T00:00:00Z'
              },
              {
                id: 'wi-3',
                productId: MOCK_PRODUCTS[6].id,
                productName: MOCK_PRODUCTS[6].name,
                productSlug: MOCK_PRODUCTS[6].slug,
                brand: MOCK_PRODUCTS[6].brand,
                productImage: MOCK_PRODUCTS[6].primaryImageUrl,
                price: MOCK_PRODUCTS[6].price,
                compareAtPrice: MOCK_PRODUCTS[6].compareAtPrice,
                discountPercent: MOCK_PRODUCTS[6].discountPercent,
                stock: MOCK_PRODUCTS[6].stock,
                averageRating: MOCK_PRODUCTS[6].averageRating,
                addedAt: '2026-08-05T00:00:00Z'
              },
              {
                id: 'wi-4',
                productId: MOCK_PRODUCTS[7].id,
                productName: MOCK_PRODUCTS[7].name,
                productSlug: MOCK_PRODUCTS[7].slug,
                brand: MOCK_PRODUCTS[7].brand,
                productImage: MOCK_PRODUCTS[7].primaryImageUrl,
                price: MOCK_PRODUCTS[7].price,
                compareAtPrice: MOCK_PRODUCTS[7].compareAtPrice,
                discountPercent: MOCK_PRODUCTS[7].discountPercent,
                stock: MOCK_PRODUCTS[7].stock,
                averageRating: MOCK_PRODUCTS[7].averageRating,
                addedAt: '2026-08-05T00:00:00Z'
              }
            ]
          };
          return of(new HttpResponse({ status: 200, body: { data: mockWishlist } }));
        }

        if (url.includes('/cart')) {
          const mockCart = {
            id: 'cart-1',
            itemCount: 2,
            subTotal: 37485,
            discountTotal: 2500,
            shippingFee: 0,
            taxAmount: 6297,
            grandTotal: 41282,
            items: [
              {
                id: 'ci-1',
                productId: MOCK_PRODUCTS[1].id,
                productName: MOCK_PRODUCTS[1].name,
                productSlug: MOCK_PRODUCTS[1].slug,
                brand: MOCK_PRODUCTS[1].brand,
                productImage: MOCK_PRODUCTS[1].primaryImageUrl,
                unitPrice: MOCK_PRODUCTS[1].price,
                quantity: 1,
                lineTotal: 24990,
                stock: MOCK_PRODUCTS[1].stock
              },
              {
                id: 'ci-2',
                productId: MOCK_PRODUCTS[2].id,
                productName: MOCK_PRODUCTS[2].name,
                productSlug: MOCK_PRODUCTS[2].slug,
                brand: MOCK_PRODUCTS[2].brand,
                productImage: MOCK_PRODUCTS[2].primaryImageUrl,
                unitPrice: MOCK_PRODUCTS[2].price,
                quantity: 1,
                lineTotal: 12495,
                stock: MOCK_PRODUCTS[2].stock
              }
            ]
          };
          return of(new HttpResponse({ status: 200, body: { data: mockCart } }));
        }

        if (url.includes('/products/')) {
          const slug = url.split('/products/')[1]?.split('?')[0];
          const found = MOCK_PRODUCTS.find(p => p.slug === slug || p.id === slug) || MOCK_PRODUCTS[0];
          const detail = {
            ...found,
            sku: 'SV-PROD-100',
            specifications: {
              'Brand': found.brand,
              'Category': found.categoryName,
              'Model Year': '2026',
              'Warranty': '1 Year Manufacturer Warranty',
              'Shipping': 'Express 2-Day Shipping Available'
            },
            description: 'Experience exceptional craft and cutting-edge design with ShopVerse verified products. Built to deliver outstanding quality, long-lasting durability, and ultimate performance for your everyday lifestyle.',
            images: [
              { id: 'img-1', url: found.primaryImageUrl || 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80', isPrimary: true, sortOrder: 1 },
              { id: 'img-2', url: 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?auto=format&fit=crop&w=800&q=80', isPrimary: false, sortOrder: 2 },
              { id: 'img-3', url: 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=800&q=80', isPrimary: false, sortOrder: 3 }
            ],
            variants: [],
            relatedProducts: MOCK_PRODUCTS.slice(0, 4),
            createdAt: '2026-01-01T00:00:00Z'
          };
          return of(new HttpResponse({ status: 200, body: { data: detail } }));
        }

        if (url.includes('/products')) {
          return of(new HttpResponse({
            status: 200,
            body: {
              data: {
                items: MOCK_PRODUCTS,
                totalCount: MOCK_PRODUCTS.length,
                page: 1,
                pageSize: 12,
                totalPages: 1,
                hasPreviousPage: false,
                hasNextPage: false
              }
            }
          }));
        }

        if (url.includes('/orders/track')) {
          const orderId = req.params.get('orderId') || 'ORD-89412';
          const data = MOCK_TRACKING_DATA[orderId] || MOCK_TRACKING_DATA['ORD-89412'];
          return of(new HttpResponse({ status: 200, body: { data } }));
        }

        if (url.includes('/contact')) {
          return of(new HttpResponse({
            status: 200,
            body: { data: { success: true, message: 'Thank you for reaching out! We have received your inquiry and will respond within 24 hours.' } }
          }));
        }

        if (url.includes('/payments/razorpay/create')) {
          return of(new HttpResponse({
            status: 200,
            body: {
              data: {
                razorpayOrderId: 'rzp_order_mock_' + Date.now(),
                amount: 2499,
                currency: 'INR',
                keyId: 'rzp_test_TM2EZCjOe9XYM4',
                customerName: 'Customer',
                customerEmail: 'customer@example.com',
                customerPhone: '9876543210',
                orderNumber: 'ORD-89412'
              }
            }
          }));
        }

        if (url.includes('/payments/razorpay/verify')) {
          return of(new HttpResponse({
            status: 200,
            body: {
              data: {
                id: 'ord_12345',
                orderNumber: 'ORD-89412',
                status: 'Confirmed',
                paymentStatus: 'Paid',
                paymentMethod: 'Razorpay'
              }
            }
          }));
        }

        if (url.includes('/auth/register')) {
          const body = req.body as any;
          const mockUser = {
            id: 'user-' + Date.now(),
            firstName: body?.firstName || 'Customer',
            lastName: body?.lastName || 'User',
            email: body?.email || 'user@example.com',
            roles: ['Customer']
          };
          const mockAuthResponse = {
            accessToken: 'mock_jwt_access_token_' + Date.now(),
            refreshToken: 'mock_jwt_refresh_token_' + Date.now(),
            expiresAt: new Date(Date.now() + 3600000).toISOString(),
            user: mockUser
          };
          return of(new HttpResponse({ status: 200, body: { data: mockAuthResponse, message: 'Account created successfully' } }));
        }

        if (url.includes('/auth/login')) {
          const body = req.body as any;
          const mockUser = {
            id: 'user-demo',
            firstName: 'Demo',
            lastName: 'Customer',
            email: body?.email || 'john@example.com',
            roles: ['Customer']
          };
          const mockAuthResponse = {
            accessToken: 'mock_jwt_access_token_' + Date.now(),
            refreshToken: 'mock_jwt_refresh_token_' + Date.now(),
            expiresAt: new Date(Date.now() + 3600000).toISOString(),
            user: mockUser
          };
          return of(new HttpResponse({ status: 200, body: { data: mockAuthResponse, message: 'Login successful' } }));
        }
      }
      return throwError(() => error);
    })
  );
};
