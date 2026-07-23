<script setup lang="ts">
import { computed } from 'vue'
import DateInput from '../../../components/inputs/DateInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  people: PersonEntity[]
  pageSize: number
  pageSizeOptions: number[]
}>()

const emit = defineEmits<{
  applyFilters: []
  changeLimit: [value: number]
}>()

const startDate = defineModel<string>('startDate', { required: true })
const endDate = defineModel<string>('endDate', { required: true })
const personId = defineModel<string>('personId', { required: true })
const order = defineModel<'asc' | 'desc'>('order', { required: true })

const personOptions = computed(() => [
  { label: 'All people', value: '' },
  ...props.people.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

const orderOptions = [
  { label: 'Highest', value: 'desc' },
  { label: 'Lowest', value: 'asc' },
]

const pageSizeOptions = computed(() =>
  props.pageSizeOptions.map((option) => ({
    label: String(option),
    value: String(option),
  })),
)

function updatePersonId(value: string) {
  personId.value = value
}

function updateOrder(value: string) {
  if (value === 'asc' || value === 'desc') {
    order.value = value
  }
}

function changePageSize(value: string) {
  emit('changeLimit', Number(value))
}
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
    <div class="grid gap-3 md:grid-cols-[minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_10rem_8rem] md:items-end">
      <DateInput v-model="startDate" label="Start date" @change="$emit('applyFilters')" />
      <DateInput v-model="endDate" label="End date" @change="$emit('applyFilters')" />
      <SelectInput
        :model-value="personId"
        label="Person"
        :options="personOptions"
        @update:model-value="updatePersonId"
        @change="$emit('applyFilters')"
      />
      <SelectInput
        :model-value="order"
        label="Order"
        :options="orderOptions"
        @update:model-value="updateOrder"
        @change="$emit('applyFilters')"
      />
      <SelectInput
        :model-value="String(pageSize)"
        label="Page size"
        :options="pageSizeOptions"
        @update:model-value="changePageSize"
      />
    </div>
  </div>
</template>
