<script setup lang="ts">
import { computed } from 'vue'
import { CheckCircle2, X, XCircle } from 'lucide-vue-next'
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
      <CheckCircle2
        v-if="message.type === 'success'"
        class="mt-0.5 size-5 shrink-0 text-emerald-600"
        aria-hidden="true"
      />
      <XCircle v-else class="mt-0.5 size-5 shrink-0 text-rose-600" aria-hidden="true" />
      <p class="min-w-0 flex-1 text-sm leading-5 text-stone-700">{{ message.text }}</p>
      <button
        type="button"
        class="inline-flex size-7 shrink-0 items-center justify-center rounded-md text-stone-400 hover:bg-stone-100 hover:text-stone-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
        title="Dismiss"
        @click="toast.remove(message.id)"
      >
        <X class="size-4" aria-hidden="true" />
        <span class="sr-only">Dismiss</span>
      </button>
    </div>
  </div>
</template>
