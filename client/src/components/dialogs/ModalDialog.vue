<script setup lang="ts">
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
        </header>
        <div class="px-5 py-5">
          <slot />
        </div>
      </section>
    </div>
  </Teleport>
</template>
