# Client view and component pattern

New client pages must use a feature folder under `client/src/views`. Each page separates view composition, page logic, and page-specific components:

```text
client/src/views/
  accounts/
    index.vue
    useController.ts
    components/
      AccountFilter.vue
      AccountTable.vue
```

The dashboard is the reference implementation for this structure:

```text
client/src/views/dashboard/
  index.vue
  useController.ts
  components/
    Filter.vue
```

The dashboard's `Filter.vue` demonstrates where a page-specific component belongs, but it predates the filename convention in this guide. A new equivalent should use a specific name such as `DashboardFilter.vue`.

## Page folder responsibilities

### `index.vue`

The page entry component composes the page. Keep it focused on:

- importing the controller and required components;
- destructuring the state and actions returned by `useController`;
- binding props, models, template refs, and events; and
- arranging page-level markup.

Do not place query setup, derived state, watchers, lifecycle behavior, or workflow logic in `index.vue`. Put that behavior in `useController.ts`.

```vue
<script setup lang="ts">
import ViewHeader from '../../components/headers/ViewHeader.vue'
import AccountFilter from './components/AccountFilter.vue'
import { useController } from './useController'

const { filters, accounts, loading, refresh } = useController()
</script>

<template>
  <section>
    <ViewHeader title="Accounts" @refresh="refresh" />
    <AccountFilter v-model:search="filters.search" />
    <!-- Render page content. -->
  </section>
</template>
```

### `useController.ts`

The page controller owns the page's logic, including:

- query and mutation composable setup;
- reactive form, filter, pagination, and dialog state;
- computed display data;
- watchers and Vue lifecycle hooks;
- DOM and template-ref behavior;
- page actions and event handlers; and
- page-specific error, toast, and navigation behavior.

Return only the state and actions required by `index.vue` or its child components. API calls must still follow the [client data-access pattern](./data-access-pattern.md): `useController.ts` consumes query composables and does not call API controllers or Axios directly.

```ts
import { computed, reactive } from 'vue'
import { useAccountsQuery } from '../../queries/AccountQueries'

export function useController() {
  const filters = reactive({ search: '' })
  const accountsQuery = useAccountsQuery()

  const accounts = computed(() => accountsQuery.data.value ?? [])
  const loading = computed(() => accountsQuery.isPending.value)

  function refresh() {
    void accountsQuery.refetch()
  }

  return {
    accounts,
    filters,
    loading,
    refresh,
  }
}
```

### Page `components/`

Place components used only by one page in that page's `components` folder. These components own their markup and presentation behavior, receive state through props or models, and communicate through typed events.

Do not import a page-local component from another page. When a component becomes reusable across pages, move it to `client/src/components` and update its API so it is not coupled to one view.

## Global components

Reusable components belong under `client/src/components`, grouped by their general component type:

```text
client/src/components/
  alerts/
    ErrorAlert.vue
  buttons/
    RefreshButton.vue
  charts/
    HorizontalBarChart.vue
  inputs/
    DateInput.vue
    SelectInput.vue
```

A global component should express a reusable UI concept through props, models, slots, and typed events. Page queries, mutations, routing decisions, and feature workflows remain in the page's `useController.ts`.

## Component filename convention

Component filenames use PascalCase and follow this order:

```text
<SpecificName><GeneralComponent>.vue
```

The specific name describes the component's purpose or variant. The general component suffix describes what kind of UI element it is.

| Preferred | Specific name | General component |
| --- | --- | --- |
| `DateInput.vue` | `Date` | `Input` |
| `SelectInput.vue` | `Select` | `Input` |
| `RefreshButton.vue` | `Refresh` | `Button` |
| `TransactionTable.vue` | `Transaction` | `Table` |
| `HorizontalBarChart.vue` | `HorizontalBar` | `Chart` |
| `DashboardFilter.vue` | `Dashboard` | `Filter` |

Avoid filenames that contain only a general type, such as `Button.vue`, `Input.vue`, `Table.vue`, or `Filter.vue`. The containing directory does not replace a descriptive filename.

## Table loading standard

Every data table must use automatic infinite loading instead of numbered pages or previous and next navigation buttons.

- Load 20 items in the initial request and 20 more items in each subsequent request by default.
- Request the next batch when a sentinel near the bottom of the table enters the viewport.
- Append new rows to the rows already displayed; do not replace the current rows.
- Prevent concurrent next-page requests and stop requesting when the API reports that no next page exists.
- Keep successfully loaded rows visible if a subsequent request fails, and provide a retry action.
- Show distinct initial-loading, loading-more, empty, error, and all-items-loaded states.
- Include filters and ordering in the query key so changing them starts the corresponding result set from its first batch.

Use TanStack Query's `useInfiniteQuery` for server-backed tables. Keep query configuration in the query module, infinite-loading state and observer behavior in the page's `useController.ts`, and table markup in a component named with the `Table` suffix.

## Adding a new page

1. Create a lowercase feature folder under `client/src/views`.
2. Add `index.vue` as the router entry component.
3. Add `useController.ts` and keep the page's state and behavior there.
4. Add page-specific components under the page's `components` folder using the component filename convention.
5. Move components shared by multiple pages to the appropriate group under `client/src/components`.
6. Register the route using the page folder's `index.vue`.
7. Run `pnpm --dir client run type-check` and `pnpm --dir client run build`.

## Review checklist

- The page lives in its own folder under `client/src/views`.
- `index.vue` is limited to composition and template bindings.
- Page logic lives in `useController.ts`.
- Page-only components live in the page's `components` folder.
- Cross-page components live in `client/src/components`.
- Component filenames follow `<SpecificName><GeneralComponent>.vue`.
- Every data table uses infinite loading with 20 items per load by default.
- Page code consumes query composables instead of API controllers or Axios.
