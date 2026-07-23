<script setup lang="ts">
import { computed, type ComponentPublicInstance } from 'vue'
import type { DataTableHeader, DataTableRow } from './types'

const props = withDefaults(
  defineProps<{
    emptyLabel?: string
    error?: string
    hasNextPage?: boolean
    headers: DataTableHeader[]
    loading?: boolean
    loadingLabel?: string
    loadingMore?: boolean
    loadMoreFailed?: boolean
    loadMoreTarget?: (target: Element | ComponentPublicInstance | null) => void
    loadProgress?: string
    retry?: () => void
    retryLabel?: string
    retryMore?: () => void
    retryMoreLabel?: string
    rows: DataTableRow[]
    summaryLabel?: string
    summaryValue?: string
    title?: string
  }>(),
  {
    emptyLabel: 'No data found.',
    error: '',
    hasNextPage: false,
    loading: false,
    loadingLabel: 'Loading data...',
    loadingMore: false,
    loadMoreFailed: false,
    retryLabel: 'Retry loading data',
    retryMoreLabel: 'Retry loading more',
  },
)

const hasSummary = computed(() =>
  props.summaryLabel !== undefined && props.summaryValue !== undefined,
)

const showPagination = computed(() =>
  props.loadProgress !== undefined && (props.rows.length > 0 || hasSummary.value),
)
</script>

<template>
  <div class="overflow-hidden rounded-lg border border-stone-200 bg-white">
    <div v-if="title" class="border-b border-stone-200 px-4 py-3">
      <h2 class="text-base font-semibold text-stone-950">{{ title }}</h2>
    </div>

    <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500" role="status">
      {{ loadingLabel }}
    </div>

    <div v-else-if="error && rows.length === 0" class="px-4 py-12 text-center text-sm">
      <p class="text-rose-700" role="alert">{{ error }}</p>
      <button
        v-if="retry"
        type="button"
        class="mt-4 rounded-md border border-stone-300 bg-white px-3 py-2 font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
        @click="retry"
      >
        {{ retryLabel }}
      </button>
    </div>

    <template v-else>
      <div
        v-if="error && !loadMoreFailed"
        class="border-b border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700"
        role="alert"
      >
        {{ error }}
      </div>

      <div v-if="rows.length === 0" class="px-4 py-12 text-center text-sm text-stone-500">
        {{ emptyLabel }}
      </div>

      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-stone-200 text-left text-sm">
          <thead class="bg-stone-100 text-xs uppercase text-stone-500">
            <tr>
              <th
                v-for="header in headers"
                :key="header.key"
                scope="col"
                :class="[
                  'px-4 py-3 font-semibold',
                  header.align === 'right' ? 'text-right' : 'text-left',
                  header.class,
                ]"
              >
                {{ header.label }}
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr v-for="row in rows" :key="row.key" class="hover:bg-stone-50">
              <td
                v-for="(cell, cellIndex) in row.cells"
                :key="`${row.key}-${headers[cellIndex]?.key ?? cellIndex}`"
                :class="[
                  'px-4 py-3 text-stone-600',
                  headers[cellIndex]?.align === 'right' ? 'text-right' : 'text-left',
                  headers[cellIndex]?.class,
                ]"
              >
                <template v-if="typeof cell === 'string'">{{ cell }}</template>
                <component v-else :is="cell.component" v-bind="cell.props" />
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div
        v-if="hasSummary"
        class="flex items-center justify-between border-t border-stone-200 px-4 py-4"
      >
        <span class="text-sm font-medium text-stone-600">{{ summaryLabel }}</span>
        <span class="text-base font-semibold text-stone-950">{{ summaryValue }}</span>
      </div>

      <template v-if="showPagination">
        <div class="flex items-center justify-between gap-3 border-t border-stone-200 px-4 py-3 text-sm text-stone-600">
          <p>{{ loadProgress }}</p>
          <div class="min-h-9 text-right">
            <p v-if="loadingMore" class="py-2" role="status">Loading more...</p>
            <button
              v-else-if="loadMoreFailed && retryMore"
              type="button"
              class="rounded-md border border-stone-300 bg-white px-3 py-2 font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
              @click="retryMore"
            >
              {{ retryMoreLabel }}
            </button>
            <p v-else-if="!hasNextPage && rows.length > 0" class="py-2">All items loaded</p>
          </div>
        </div>
        <div v-if="loadMoreTarget" :ref="loadMoreTarget" class="h-px" aria-hidden="true"></div>
      </template>
    </template>
  </div>
</template>
