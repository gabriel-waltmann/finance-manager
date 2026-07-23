<script setup lang="ts">
import { X } from 'lucide-vue-next'

defineProps<{
  open: boolean
  title: string
}>()

defineEmits<{
  close: []
}>()
</script>

<template>
  <Teleport to="body">
    <div
      v-if="open"
      class="fixed inset-0 z-40 flex min-h-dvh items-center justify-center bg-stone-950/40 px-4 py-6"
      role="presentation"
      @mousedown.self="$emit('close')"
    >
      <section
        class="w-full max-w-xl rounded-lg border border-stone-200 bg-white shadow-xl"
        role="dialog"
        aria-modal="true"
        :aria-label="title"
      >
        <header class="flex items-center justify-between border-b border-stone-200 px-5 py-4">
          <h2 class="text-base font-semibold text-stone-950">{{ title }}</h2>
          <button
            type="button"
            class="inline-flex size-9 items-center justify-center rounded-md text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            title="Close"
            @click="$emit('close')"
          >
            <X class="size-4" aria-hidden="true" />
            <span class="sr-only">Close</span>
          </button>
        </header>
        <div class="px-5 py-5">
          <slot />
        </div>
      </section>
    </div>
  </Teleport>
</template>
