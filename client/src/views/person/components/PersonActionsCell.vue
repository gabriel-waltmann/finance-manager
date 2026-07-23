<script setup lang="ts">
import { PhPencilSimple, PhTrash } from '@phosphor-icons/vue'

defineEmits<{
  deletePerson: []
  edit: []
}>()
</script>

<template>
  <div class="flex justify-end gap-2">
    <button
      type="button"
      class="rounded-md p-2 text-stone-600 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
      aria-label="Edit person"
      title="Edit person"
      @click="$emit('edit')"
    >
      <PhPencilSimple :size="18" weight="bold" aria-hidden="true" />
    </button>
    <button
      type="button"
      class="rounded-md p-2 text-stone-600 hover:bg-rose-50 hover:text-rose-700 focus:outline-none focus:ring-2 focus:ring-rose-500"
      aria-label="Delete person"
      title="Delete person"
      @click="$emit('deletePerson')"
    >
      <PhTrash :size="18" weight="bold" aria-hidden="true" />
    </button>
  </div>
</template>
