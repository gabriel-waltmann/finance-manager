<script setup lang="ts">
import type { FileProcessingStatus } from '../../../entities/TransactionImportEntity'

defineProps<{
  status: FileProcessingStatus
}>()

function statusClasses(status: FileProcessingStatus): string {
  switch (status) {
    case 'Submitted':
      return 'bg-sky-50 text-sky-700 ring-sky-600/20'
    case 'Processing':
      return 'bg-amber-50 text-amber-700 ring-amber-600/20'
    case 'Finished':
      return 'bg-emerald-50 text-emerald-700 ring-emerald-600/20'
    case 'Failed':
      return 'bg-rose-50 text-rose-700 ring-rose-600/20'
  }
}
</script>

<template>
  <span
    class="inline-flex rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset"
    :class="statusClasses(status)"
  >
    {{ status }}
  </span>
</template>
