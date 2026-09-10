<script setup lang="ts">
import { PhPencilSimple, PhTrash } from '@phosphor-icons/vue'

defineEmits<{
  deleteCategory: []
  edit: []
}>()
</script>

<template>
  <div class="flex justify-end gap-2">
    <button
      type="button"
      class="rounded-md p-2 text-stone-600 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
      aria-label="Edit category"
      title="Edit category"
      @click="$emit('edit')"
    >
      <PhPencilSimple :size="18" weight="bold" aria-hidden="true" />
    </button>
    <button
      type="button"
      class="rounded-md p-2 text-stone-600 hover:bg-rose-50 hover:text-rose-700 focus:outline-none focus:ring-2 focus:ring-rose-500"
      aria-label="Delete category"
      title="Delete category"
      @click="$emit('deleteCategory')"
    >
      <PhTrash :size="18" weight="bold" aria-hidden="true" />
    </button>
  </div>
</template>
