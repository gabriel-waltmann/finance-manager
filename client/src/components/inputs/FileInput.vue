<script setup lang="ts">
import { ref, watch } from 'vue'

withDefaults(
  defineProps<{
    accept?: string
    disabled?: boolean
    label: string
    required?: boolean
  }>(),
  {
    accept: undefined,
    disabled: false,
    required: false,
  },
)

const model = defineModel<File | null>({ required: true })
const input = ref<HTMLInputElement | null>(null)

watch(model, (file) => {
  if (!file && input.value) {
    input.value.value = ''
  }
})

function selectFile(event: Event) {
  model.value = (event.target as HTMLInputElement).files?.[0] ?? null
}
</script>

<template>
  <label class="block">
    <span class="text-sm font-medium text-stone-700">{{ label }}</span>
    <input
      ref="input"
      class="mt-1 block w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 file:mr-3 file:rounded file:border-0 file:bg-stone-100 file:px-3 file:py-1 file:text-sm file:font-medium file:text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
      type="file"
      :accept="accept"
      :disabled="disabled"
      :required="required"
      @change="selectFile"
    />
  </label>
</template>
