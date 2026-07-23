<script setup lang="ts">
import { ref, watch } from 'vue'

withDefaults(
  defineProps<{
    label: string
    required?: boolean
  }>(),
  {
    required: false,
  },
)

const model = defineModel<string>({ required: true })

const currencyFormatter = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

const displayValue = ref(formatModelValue(model.value))

watch(model, (value) => {
  displayValue.value = formatModelValue(value)
})

function formatModelValue(value: string): string {
  if (!value) {
    return ''
  }

  const amount = Number(value)

  return Number.isFinite(amount) ? currencyFormatter.format(amount) : ''
}

function handleInput(event: Event) {
  const input = event.target as HTMLInputElement
  const digits = input.value.replace(/\D/g, '')

  if (!digits) {
    model.value = ''
    displayValue.value = input.value.includes('-') ? '-' : ''
    input.value = displayValue.value
    input.setCustomValidity(displayValue.value ? 'Enter a valid BRL amount.' : '')
    return
  }

  const isNegative = input.value.includes('-')
  const amount = Number(digits) / 100
  const normalizedValue = `${isNegative ? '-' : ''}${amount.toFixed(2)}`

  model.value = normalizedValue
  displayValue.value = currencyFormatter.format(Number(normalizedValue))
  input.value = displayValue.value
  input.setCustomValidity('')
  input.setSelectionRange(input.value.length, input.value.length)
}
</script>

<template>
  <label class="block">
    <span class="text-sm font-medium text-stone-700">{{ label }}</span>
    <input
      :value="displayValue"
      class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
      type="text"
      inputmode="decimal"
      :required="required"
      @input="handleInput"
    />
  </label>
</template>
