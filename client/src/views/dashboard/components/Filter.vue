<script setup lang="ts">
import { computed } from 'vue'
import DateInput from '../../../components/inputs/DateInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  persons: PersonEntity[]
}>()

const startDate = defineModel<string>('startDate', { required: true })
const endDate = defineModel<string>('endDate', { required: true })
const personId = defineModel<string>('personId', { required: true })
const order = defineModel<'asc' | 'desc'>('order', { required: true })

const personOptions = computed(() => [
  { label: 'Any person', value: '' },
  ...props.persons.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

const orderOptions = [
  { label: 'Highest', value: 'desc' },
  { label: 'Lowest', value: 'asc' },
]

function updatePersonId(value: string) {
  personId.value = value
}

function updateOrder(value: string) {
  if (value === 'asc' || value === 'desc') {
    order.value = value
  }
}
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
    <div class="grid gap-3 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_10rem] md:items-end">
      <DateInput v-model="startDate" label="Start date" />
      <DateInput v-model="endDate" label="End date" />
      <SelectInput
        :model-value="personId"
        label="Person"
        :options="personOptions"
        @update:model-value="updatePersonId"
      />
      <SelectInput
        :model-value="order"
        label="Order"
        :options="orderOptions"
        @update:model-value="updateOrder"
      />
    </div>
  </div>
</template>
