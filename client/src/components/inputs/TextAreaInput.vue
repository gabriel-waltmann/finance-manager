<script setup lang="ts">
withDefaults(
  defineProps<{
    label: string
    maxLength?: number
    placeholder?: string
    required?: boolean
    rows?: number
  }>(),
  {
    maxLength: undefined,
    placeholder: undefined,
    required: false,
    rows: 4,
  },
)

const model = defineModel<string>({ required: true })
</script>

<template>
  <label class="block">
    <span class="text-sm font-medium text-stone-700">{{ label }}</span>
    <textarea
      v-model="model"
      class="mt-1 w-full resize-y rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
      :maxlength="maxLength"
      :placeholder="placeholder"
      :required="required"
      :rows="rows"
    ></textarea>
  </label>
</template>
