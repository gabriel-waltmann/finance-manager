<script setup lang="ts">
import { displayBRLCurrency } from '@/lib/format'

defineProps<{
  amount: number
  loading: boolean
  unavailable: boolean
}>()
</script>

<template>
  <div
    class="rounded-lg border border-stone-200 bg-white px-4 py-4"
    aria-live="polite"
  >
    <p class="text-sm font-medium text-stone-600">Amount total</p>

    <p v-if="loading" class="mt-1 text-sm text-stone-500" role="status">
      Loading total...
    </p>
    <p v-else-if="unavailable" class="mt-1 text-sm text-stone-500">
      Unavailable
    </p>
    <p v-else class="mt-1 text-2xl font-semibold text-stone-950">
      {{ displayBRLCurrency(amount) }}
    </p>
  </div>
</template>
