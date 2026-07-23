<script setup lang="ts">
import RefreshButton from '../buttons/RefreshButton.vue'
import ViewTitle from '../titles/ViewTitle.vue'

withDefaults(
  defineProps<{
    title: string
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
  <header class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
    <ViewTitle :title="title" />
    <RefreshButton :refreshing="refreshing" @refresh="$emit('refresh')" />
  </header>
</template>
