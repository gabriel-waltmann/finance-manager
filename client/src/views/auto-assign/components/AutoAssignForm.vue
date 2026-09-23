<script setup lang="ts">
import { computed } from 'vue'
import FilledButton from '../../../components/buttons/FilledButton.vue'
import MultiSelectInput from '../../../components/inputs/MultiSelectInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import type {
  AutoAssignCategoryAction,
  AutoAssignPersonAction,
} from '../../../entities/AutoAssignTransactionEntity'
import type { CategoryEntity } from '../../../entities/CategoryEntity'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  applying: boolean
  canApply: boolean
  categories: CategoryEntity[]
  disabled: boolean
  persons: PersonEntity[]
}>()

const personAction = defineModel<AutoAssignPersonAction>('personAction', { required: true })
const targetPersonId = defineModel<string>('targetPersonId', { required: true })
const categoryAction = defineModel<AutoAssignCategoryAction>('categoryAction', { required: true })
const targetCategoryIds = defineModel<string[]>('targetCategoryIds', { required: true })

const personActionOptions = [
  { label: 'Leave unchanged', value: 'unchanged' },
  { label: 'Assign person', value: 'set' },
  { label: 'Clear person', value: 'clear' },
]

const categoryActionOptions = [
  { label: 'Leave unchanged', value: 'unchanged' },
  { label: 'Add categories', value: 'add' },
  { label: 'Replace categories', value: 'replace' },
  { label: 'Clear categories', value: 'clear' },
]

const personOptions = computed(() => [
  { label: 'Select person', value: '' },
  ...props.persons.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

const categoryOptions = computed(() => props.categories.map((category) => ({
  label: category.title,
  value: category.id,
})))

defineEmits<{
  apply: []
}>()
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-4">
    <div class="mb-3">
      <h2 class="text-base font-semibold text-stone-950">Assignments</h2>
      <p class="mt-1 text-sm text-stone-500">
        Choose at least one change. It will apply to every matching transaction.
      </p>
    </div>

    <div class="grid gap-3 lg:grid-cols-2">
      <div class="grid gap-3 sm:grid-cols-2">
        <SelectInput
          v-model="personAction"
          label="Person action"
          :options="personActionOptions"
          :disabled="disabled || applying"
        />
        <SelectInput
          v-if="personAction === 'set'"
          v-model="targetPersonId"
          label="Person"
          :options="personOptions"
          :disabled="disabled || applying"
          required
        />
      </div>

      <div class="grid gap-3 sm:grid-cols-2">
        <SelectInput
          v-model="categoryAction"
          label="Category action"
          :options="categoryActionOptions"
          :disabled="disabled || applying"
        />
        <MultiSelectInput
          v-if="categoryAction === 'add' || categoryAction === 'replace'"
          v-model="targetCategoryIds"
          label="Categories"
          :options="categoryOptions"
          placeholder="Select categories"
          :disabled="disabled || applying"
        />
      </div>
    </div>

    <div class="mt-4 flex justify-end">
      <FilledButton
        :text="applying ? 'Applying...' : 'Apply assignments'"
        :disabled="!canApply || applying"
        @click="$emit('apply')"
      />
    </div>
  </div>
</template>
