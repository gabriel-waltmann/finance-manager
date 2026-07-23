<script setup lang="ts">
import SelectInput from '../../../components/inputs/SelectInput.vue'
import TextInput from '../../../components/inputs/TextInput.vue'
import type { FileProcessingStatus } from '../../../entities/TransactionImportEntity'

const search = defineModel<string>('search', { required: true })
const status = defineModel<'' | FileProcessingStatus>('status', { required: true })
const order = defineModel<'asc' | 'desc'>('order', { required: true })

const statusOptions = [
  { label: 'All statuses', value: '' },
  { label: 'Submitted', value: 'Submitted' },
  { label: 'Processing', value: 'Processing' },
  { label: 'Finished', value: 'Finished' },
  { label: 'Failed', value: 'Failed' },
]

const orderOptions = [
  { label: 'Newest', value: 'desc' },
  { label: 'Oldest', value: 'asc' },
]
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
    <div class="grid gap-3 md:grid-cols-2 lg:grid-cols-[minmax(14rem,1fr)_minmax(0,1fr)_10rem] lg:items-end">
      <TextInput v-model="search" label="Search" type="search" placeholder="File name" />
      <SelectInput v-model="status" label="Status" :options="statusOptions" />
      <SelectInput v-model="order" label="Order" :options="orderOptions" />
    </div>
  </div>
</template>
