# Finance Manager Client

Vue 3, TypeScript, Vite, and Tailwind CSS client for managing transactions, people, CSV imports, and person assignments.

## Development

```bash
pnpm install
pnpm run dev
```

The Vite dev server proxies `/api/*` to `http://localhost:5266` by default. Override it with `VITE_API_TARGET` if the ASP.NET API is running elsewhere.

## Architecture

API-backed client features follow the `entities -> controllers -> queries -> views` data-access pattern. See the [client data-access guide](../docs/client/data-access-pattern.md) before adding a new entity, endpoint, query, mutation, or Axios middleware.

## Checks

```bash
pnpm run type-check
pnpm run build
```
