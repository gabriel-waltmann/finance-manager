<script setup lang="ts">
import ModalDialog from './ModalDialog.vue'

defineProps<{
  open: boolean
  title: string
  message: string
  confirmLabel?: string
  busy?: boolean
}>()

defineEmits<{
  cancel: []
  confirm: []
}>()
</script>

<template>
  <ModalDialog :open="open" :title="title" @close="$emit('cancel')">
    <p class="text-sm leading-6 text-stone-600">{{ message }}</p>
    <div class="mt-6 flex justify-end gap-3">
      <button
        type="button"
        class="rounded-md border border-stone-300 px-4 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
        :disabled="busy"
        @click="$emit('cancel')"
      >
        Cancel
      </button>
      <button
        type="button"
        class="rounded-md bg-rose-600 px-4 py-2 text-sm font-medium text-white hover:bg-rose-700 focus:outline-none focus:ring-2 focus:ring-rose-500 disabled:cursor-not-allowed disabled:opacity-60"
        :disabled="busy"
        @click="$emit('confirm')"
      >
        {{ busy ? 'Deleting...' : (confirmLabel ?? 'Delete') }}
      </button>
    </div>
  </ModalDialog>
</template>
