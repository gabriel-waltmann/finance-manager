<script setup lang="ts">
import { RefreshCw } from 'lucide-vue-next'

withDefaults(
  defineProps<{
    refreshing?: boolean
  }>(),
  {
    refreshing: false,
  },
)

defineEmits<{
  refresh: []
}>()
</script>

<template>
  <button
    type="button"
    class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
    :disabled="refreshing"
    :aria-busy="refreshing"
    @click="$emit('refresh')"
  >
    <RefreshCw class="size-4" aria-hidden="true" />
    Refresh
  </button>
</template>
