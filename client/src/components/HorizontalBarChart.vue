<script setup lang="ts">
import { computed } from 'vue'

interface BarChartItem {
  key: string
  label: string
  value: number
  detail?: string
}

const props = withDefaults(
  defineProps<{
    items: BarChartItem[]
    emptyLabel?: string
    valueFormatter?: (value: number) => string
  }>(),
  {
    emptyLabel: 'No data found.',
  },
)

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
</template>
