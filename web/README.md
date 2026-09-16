# OnlineShop — Web Frontend

React + Vite + TypeScript SPA for the Buyer storefront and the Seller/Admin dashboard. See the repo root [README.md](../README.md) for the full project overview (architecture, backend, Telegram bot, AI features).

## Stack

- React 19 + `react-router-dom` for routing
- `axios` for API calls (`src/api/`)
- Plain CSS (`src/index.css`) — no component library
- React Context for cross-cutting client state: `AuthContext`, `CartContext`, `WishlistContext`

## Structure

```
src/
  api/            One file per backend module (catalog, orders, cart, wishlist, vouchers, admin...)
  auth/           AuthContext, CartContext, WishlistContext, route guards (RequireBuyer/RequireStaff/RequireAdmin)
  components/     Shared UI (ProductCard, Breadcrumbs, OrderStatusTimeline, NotificationBell, AiChatWidget...)
  pages/          Buyer-facing pages (Home, ProductDetail, Cart, Checkout, Orders, Wishlist...)
  pages/admin/    Seller/Admin dashboard pages, under /admin/*
  utils/          Formatting helpers, category styling, localStorage-based recently-viewed
```

## Running locally

```bash
npm install
npm run dev
```

Vite's dev server proxies `/api` and `/uploads` to the backend Api at `http://localhost:5032` (see `vite.config.ts`) — start the backend first (`dotnet run --project ../backend/src/Api`).

## Build

```bash
npm run build   # runs `tsc -b` then `vite build`
```
