<script setup lang="ts">
import { computed, type ComponentPublicInstance } from 'vue'

interface BarChartItem {
  key: string
  label: string
  value: number
  detail?: string
}

const props = withDefaults(
  defineProps<{
    title: string
    items: BarChartItem[]
    loading?: boolean
    loadingLabel?: string
    emptyLabel?: string
    valueFormatter?: (value: number) => string
    totalLabel: string
    totalValue: number
    loadProgress: string
    loadingMore?: boolean
    loadMoreFailed?: boolean
    hasNextPage?: boolean
    loadMoreTarget: (target: Element | ComponentPublicInstance | null) => void
  }>(),
  {
    loading: false,
    loadingLabel: 'Loading chart...',
    emptyLabel: 'No data found.',
    loadingMore: false,
    loadMoreFailed: false,
    hasNextPage: false,
  },
)

defineEmits<{
  retry: []
}>()

const maxValue = computed(() =>
  props.items.reduce((max, item) => Math.max(max, item.value), 0),
)

const normalizedItems = computed(() =>
  props.items.map((item) => ({
    ...item,
    percent: getPercent(item.value),
  })),
)

function getPercent(value: number): number {
  if (maxValue.value <= 0) {
    return 0
  }

  return Math.max(4, Math.round((value / maxValue.value) * 1000) / 10)
}

function formatValue(value: number): string {
  return props.valueFormatter ? props.valueFormatter(value) : String(value)
}
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white">
    <div class="border-b border-stone-200 px-4 py-3">
      <h2 class="text-base font-semibold text-stone-950">{{ title }}</h2>
    </div>

    <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">
      {{ loadingLabel }}
    </div>

    <div v-else class="px-4 py-4">
      <div v-if="items.length === 0" class="px-4 py-12 text-center text-sm text-stone-500">
        {{ emptyLabel }}
      </div>
      <div v-else class="space-y-4">
        <div
          v-for="item in normalizedItems"
          :key="item.key"
          class="grid gap-2 sm:grid-cols-[minmax(8rem,16rem)_minmax(0,1fr)_8rem] sm:items-center"
        >
          <div class="min-w-0">
            <p class="break-words text-sm font-medium text-stone-950">{{ item.label }}</p>
            <p v-if="item.detail" class="mt-0.5 text-xs text-stone-500">{{ item.detail }}</p>
          </div>
          <div class="h-3 overflow-hidden rounded-full bg-stone-100" aria-hidden="true">
            <div class="h-full rounded-full bg-emerald-600" :style="{ width: `${item.percent}%` }"></div>
          </div>
          <p class="text-sm font-semibold text-stone-800 sm:text-right">
            {{ formatValue(item.value) }}
          </p>
        </div>
      </div>

      <div class="mt-4 flex items-center justify-between border-t border-stone-200 pt-4">
        <span class="text-sm font-medium text-stone-600">{{ totalLabel }}</span>
        <span class="text-base font-semibold text-stone-950">{{ formatValue(totalValue) }}</span>
      </div>
      <div class="mt-4 flex items-center justify-between gap-3 border-t border-stone-200 pt-4 text-sm text-stone-600">
        <p>{{ loadProgress }}</p>
        <div class="min-h-9 text-right">
          <p v-if="loadingMore" class="py-2" role="status">Loading more...</p>
          <button
            v-else-if="loadMoreFailed"
            type="button"
            class="rounded-md border border-stone-300 bg-white px-3 py-2 font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            @click="$emit('retry')"
          >
            Retry loading more
          </button>
          <p v-else-if="!hasNextPage && items.length > 0" class="py-2">All items loaded</p>
        </div>
      </div>
      <div :ref="loadMoreTarget" class="h-px" aria-hidden="true"></div>
    </div>
  </div>
</template>
