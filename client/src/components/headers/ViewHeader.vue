<script setup lang="ts">
import ViewTitle from '../titles/ViewTitle.vue'

defineProps<{
  title: string
}>()
</script>

<template>
  <header class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
    <ViewTitle :title="title" />

    <div v-if="$slots.actions" class="flex flex-wrap gap-3">
      <slot name="actions"></slot>
    </div>
  </header>
</template>
