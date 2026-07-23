<script setup lang="ts">
import SelectInput from '../../../components/inputs/SelectInput.vue'
import TextInput from '../../../components/inputs/TextInput.vue'

const search = defineModel<string>('search', { required: true })
const order = defineModel<'asc' | 'desc'>('order', { required: true })

const orderOptions = [
  { label: 'Name A–Z', value: 'asc' },
  { label: 'Name Z–A', value: 'desc' },
]
</script>

<template>
  <div class="rounded-lg border border-stone-200 bg-white px-4 py-3">
    <div class="grid gap-3 md:grid-cols-[minmax(14rem,1fr)_12rem] md:items-end">
      <TextInput
        v-model="search"
        label="Search"
        type="search"
        placeholder="Name, email, or phone"
      />

      <SelectInput v-model="order" label="Order" :options="orderOptions" />
    </div>
  </div>
</template>
