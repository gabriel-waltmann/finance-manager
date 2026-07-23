<script setup lang="ts">
interface SelectOption {
  label: string
  value: string
}

defineProps<{
  disabled?: boolean
  hideLabel?: boolean
  label: string
  options: SelectOption[]
}>()

const model = defineModel<string>({ required: true })

defineEmits<{
  change: [event: Event]
}>()
</script>

<template>
  <label class="block">
    <span :class="hideLabel ? 'sr-only' : 'text-sm font-medium text-stone-700'">{{ label }}</span>
    <select
      v-model="model"
      class="w-full rounded-md border border-stone-300 bg-white px-3 py-2 text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
      :class="{ 'mt-1': !hideLabel }"
      :disabled="disabled"
      @change="$emit('change', $event)"
    >
      <option v-for="option in options" :key="option.value" :value="option.value">
        {{ option.label }}
      </option>
    </select>
  </label>
</template>
