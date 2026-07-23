<script setup lang="ts">
import { computed } from 'vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import type { PersonEntity } from '../../../entities/PersonEntity'

const props = defineProps<{
  disabled: boolean
  people: PersonEntity[]
  personId: string
}>()

const personOptions = computed(() => [
  { label: 'Unassigned', value: '' },
  ...props.people.map((person) => ({
    label: person.name,
    value: person.id,
  })),
])

defineEmits<{
  change: [event: Event]
}>()
</script>

<template>
  <SelectInput
    :model-value="personId"
    :disabled="disabled"
    hide-label
    label="Person"
    :options="personOptions"
    @change="$emit('change', $event)"
  />
</template>
