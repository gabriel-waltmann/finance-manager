<script setup lang="ts">
interface SelectOption {
  label: string
  value: string
}

defineProps<{
  label: string
  options: SelectOption[]
}>()

const model = defineModel<string>({ required: true })

defineEmits<{
  change: []
}>()
</script>

<template>
  <label class="block">
    <span class="text-sm font-medium text-stone-700">{{ label }}</span>
    <select
      v-model="model"
      class="mt-1 w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
      @change="$emit('change')"
    >
      <option v-for="option in options" :key="option.value" :value="option.value">
        {{ option.label }}
      </option>
    </select>
  </label>
</template>
