<script setup lang="ts">
import { computed } from 'vue'
import FilledButton from '../../../components/buttons/FilledButton.vue'
import ModalDialog from '../../../components/dialogs/ModalDialog.vue'
import CurrencyInput from '../../../components/inputs/CurrencyInput.vue'
import DateInput from '../../../components/inputs/DateInput.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import TextInput from '../../../components/inputs/TextInput.vue'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  editing: boolean
  open: boolean
  persons: PersonEntity[]
  saving: boolean
}>()

const date = defineModel<string>('date', { required: true })
const title = defineModel<string>('title', { required: true })
const amount = defineModel<string>('amount', { required: true })
const personId = defineModel<string>('personId', { required: true })

const personOptions = computed(() => [
  { label: 'Unassigned', value: '' },
  ...props.persons.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

defineEmits<{
  close: []
  submit: []
}>()
</script>

<template>
  <ModalDialog :open="open" :title="editing ? 'Edit transaction' : 'New transaction'" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="$emit('submit')">
      <DateInput v-model="date" label="Date" required />
      
      <TextInput v-model="title" label="Title" required />
      
      <CurrencyInput v-model="amount" label="Amount" required />
      
      <SelectInput v-model="personId" label="Person" :options="personOptions" />
      
      <div class="flex justify-end gap-3 pt-2">
        <FilledButton
          text="Cancel"
          color="stone"
          type="button"
          :disabled="saving"
          @click="$emit('close')"
        />
      
        <FilledButton
          :text="saving ? 'Saving...' : 'Save'"
          type="submit"
          :disabled="saving"
        />
      </div>
    </form>
  </ModalDialog>
</template>
