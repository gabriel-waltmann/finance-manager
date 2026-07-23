<script setup lang="ts">
import type { Component } from 'vue'

withDefaults(
  defineProps<{
    text: string
    icon?: Component
    color?: 'green' | 'stone'
    disabled?: boolean
    type?: 'button' | 'reset' | 'submit'
  }>(),
  {
    color: 'green',
    disabled: false,
    type: 'button',
  },
)
</script>

<template>
  <button
    :type="type"
    :disabled="disabled"
    class="inline-flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium text-white focus:outline-none focus:ring-2 disabled:cursor-not-allowed disabled:opacity-60"
    :class="color === 'stone'
      ? 'bg-stone-600 hover:bg-stone-700 focus:ring-stone-500'
      : 'bg-emerald-600 hover:bg-emerald-700 focus:ring-emerald-500'"
  >
    <component :is="icon" v-if="icon" class="size-4" aria-hidden="true" />
    {{ text }}
  </button>
</template>
