<script setup lang="ts">
import { computed } from 'vue'
import DateInput from '../../../components/inputs/DateInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import TextInput from '../../../components/inputs/TextInput.vue'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  persons: PersonEntity[]
}>()

const search = defineModel<string>('search', { required: true })
const startDate = defineModel<string>('startDate', { required: true })
const endDate = defineModel<string>('endDate', { required: true })
const personFilter = defineModel<string>('personFilter', { required: true })
const order = defineModel<'asc' | 'desc'>('order', { required: true })

const personOptions = computed(() => [
  { label: 'Any person', value: '' },
  { label: 'Unassigned', value: 'unassigned' },
  ...props.persons.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

const orderOptions = [
  { label: 'Newest', value: 'desc' },
  { label: 'Oldest', value: 'asc' },
]
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
    <div class="grid gap-3 md:grid-cols-2 lg:grid-cols-[minmax(14rem,1.4fr)_minmax(0,1fr)_minmax(0,1fr)_minmax(0,1fr)_10rem] lg:items-end">
      <TextInput v-model="search" label="Search" type="search" placeholder="Title or person" />

      <DateInput v-model="startDate" label="Start date" />
      
      <DateInput v-model="endDate" label="End date" />
      
      <SelectInput v-model="personFilter" label="Person" :options="personOptions" />
      
      <SelectInput v-model="order" label="Order" :options="orderOptions" />
    </div>
  </div>
</template>
