<script setup lang="ts">
defineProps<{
  title: string
}>()
</script>

<template>
  <h1 class="text-2xl font-semibold text-stone-950">{{ title }}</h1>
</template>
