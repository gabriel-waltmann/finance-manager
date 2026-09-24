<script setup lang="ts">
import { computed } from 'vue'
import FilledButton from '@/components/buttons/FilledButton.vue'

const props = defineProps<{
  connectionState: 'connecting' | 'live' | 'reconnecting'
  uploadPending: boolean
}>()

defineEmits<{
  openUpload: []
}>()

const connectionLabel = computed(() => {
  switch (props.connectionState) {
    case 'live':
      return 'Live updates'
    case 'connecting':
      return 'Connecting'
    case 'reconnecting':
      return 'Reconnecting'
  }
})
</script>

<template>
  <div
    class="inline-flex items-center gap-2 rounded-md border border-stone-200 bg-white px-3 py-2 text-sm text-stone-600"
    role="status"
    aria-live="polite"
  >
    <span
      class="size-2 rounded-full"
      :class="connectionState === 'live' ? 'bg-emerald-500' : 'animate-pulse bg-amber-500'"
      aria-hidden="true"
    />
    {{ connectionLabel }}
  </div>

  <FilledButton
    :text="uploadPending ? 'Uploading...' : 'Upload CSV'"
    :disabled="uploadPending"
    @click="$emit('openUpload')"
  />
</template>
