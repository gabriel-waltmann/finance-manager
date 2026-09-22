<script setup lang="ts">
import { computed } from 'vue'
import FilledButton from '../../../components/buttons/FilledButton.vue'
import ModalDialog from '../../../components/dialogs/ModalDialog.vue'
import FileInput from '../../../components/inputs/FileInput.vue'
import MultiSelectInput from '../../../components/inputs/MultiSelectInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import type { CategoryEntity } from '../../../entities/CategoryEntity'
import type { PersonEntity } from '../../../entities/PersonEntity'
import type { FileCategory } from '../../../entities/TransactionImportEntity'

const props = defineProps<{
  categories: CategoryEntity[]
  open: boolean
  optionsLoading: boolean
  persons: PersonEntity[]
  uploading: boolean
}>()

const category = defineModel<'' | FileCategory>('category', { required: true })
const categoryIds = defineModel<string[]>('categoryIds', { required: true })
const file = defineModel<File | null>('file', { required: true })
const personId = defineModel<string>('personId', { required: true })

const importTypeOptions = [
  { label: 'Select import type', value: '' },
  { label: 'Credit card', value: 'CreditCard' },
  { label: 'Extrato', value: 'Extrato' },
]

const personOptions = computed(() => [
  { label: 'Unassigned', value: '' },
  ...props.persons.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

const categoryOptions = computed(() => props.categories.map((item) => ({
  label: item.title,
  value: item.id,
})))

defineEmits<{
  close: []
  submit: []
}>()
</script>

<template>
  <ModalDialog :open="open" title="Upload CSV" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="$emit('submit')">
      <SelectInput
        v-model="category"
        label="Import type"
        :options="importTypeOptions"
        :disabled="uploading"
        required
      />

      <FileInput
        v-model="file"
        label="CSV file"
        accept=".csv,text/csv"
        :disabled="uploading"
        required
      />

      <SelectInput
        v-model="personId"
        label="Person"
        :options="personOptions"
        :disabled="uploading || optionsLoading"
      />

      <MultiSelectInput
        v-model="categoryIds"
        label="Categories"
        :options="categoryOptions"
        placeholder="Select categories"
        :disabled="uploading || optionsLoading"
      />

      <div class="flex justify-end gap-3 pt-2">
        <FilledButton
          text="Cancel"
          color="stone"
          type="button"
          :disabled="uploading"
          @click="$emit('close')"
        />
        <FilledButton
          :text="uploading ? 'Uploading...' : 'Upload'"
          type="submit"
          :disabled="uploading"
        />
      </div>
    </form>
  </ModalDialog>
</template>
