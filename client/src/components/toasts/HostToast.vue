<script setup lang="ts">
import { computed } from 'vue'
import { useToast } from '../../stores/toast'

const toast = useToast()
const messages = computed(() => toast.messages.value)
</script>

<template>
  <div class="fixed right-4 top-4 z-50 flex w-[min(28rem,calc(100vw-2rem))] flex-col gap-3">
    <div
      v-for="message in messages"
      :key="message.id"
      class="flex items-start gap-3 rounded-lg border bg-white px-4 py-3 shadow-lg"
      :class="message.type === 'success' ? 'border-emerald-200' : 'border-rose-200'"
      role="status"
    >
      <p class="min-w-0 flex-1 text-sm leading-5 text-stone-700">{{ message.text }}</p>
      <button
        type="button"
        class="shrink-0 rounded-md px-2 py-1 text-xs font-medium text-stone-500 hover:bg-stone-100 hover:text-stone-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
        @click="toast.remove(message.id)"
      >
        Dismiss
      </button>
    </div>
  </div>
</template>
