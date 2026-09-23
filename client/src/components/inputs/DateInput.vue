<script setup lang="ts">
withDefaults(
  defineProps<{
    disabled?: boolean
    label: string
    required?: boolean
  }>(),
  {
    disabled: false,
    required: false,
  },
)

const model = defineModel<string>({ required: true })

defineEmits<{
  change: []
}>()
</script>

<template>
  <label class="block">
    <span class="text-sm font-medium text-stone-700">{{ label }}</span>
    <input
      v-model="model"
      class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
      type="date"
      :disabled="disabled"
      :required="required"
      @change="$emit('change')"
    />
  </label>
</template>
