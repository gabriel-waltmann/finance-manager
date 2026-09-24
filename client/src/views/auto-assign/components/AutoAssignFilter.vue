<script setup lang="ts">
import { computed } from 'vue'
import DateInput from '../../../components/inputs/DateInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import TextInput from '../../../components/inputs/TextInput.vue'
import type { CategoryEntity } from '../../../entities/CategoryEntity'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  categories: CategoryEntity[]
  disabled?: boolean
  persons: PersonEntity[]
}>()

const startDate = defineModel<string>('startDate', { required: true })
const endDate = defineModel<string>('endDate', { required: true })
const title = defineModel<string>('title', { required: true })
const personFilter = defineModel<string>('personFilter', { required: true })
const categoryFilter = defineModel<string>('categoryFilter', { required: true })

const personOptions = computed(() => [
  { label: 'Any person', value: '' },
  { label: 'Unassigned', value: 'unassigned' },
  ...props.persons.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

const categoryOptions = computed(() => [
  { label: 'Any category', value: '' },
  { label: 'Uncategorized', value: 'uncategorized' },
  ...props.categories.map((category) => ({
    label: category.title,
    value: category.id,
  })),
])
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-4">
    <div class="mb-3">
      <h2 class="text-base font-semibold text-stone-950">Choose transactions</h2>
      <p class="mt-1 text-sm text-stone-500">Use at least one filter to preview transactions.</p>
    </div>

    <div class="grid gap-3 md:grid-cols-2 xl:grid-cols-5 xl:items-end">
      <TextInput
        v-model="title"
        label="Title"
        type="search"
        placeholder="Transaction title"
        :max-length="200"
        :disabled="disabled"
      />
      <DateInput v-model="startDate" label="Start date" :disabled="disabled" />
      <DateInput v-model="endDate" label="End date" :disabled="disabled" />
      <SelectInput
        v-model="personFilter"
        label="Person"
        :options="personOptions"
        :disabled="disabled"
      />
      <SelectInput
        v-model="categoryFilter"
        label="Category"
        :options="categoryOptions"
        :disabled="disabled"
      />
    </div>
  </div>
</template>
