<script setup lang="ts">
import { computed, ref } from 'vue'
import FilledButton from '../../../components/buttons/FilledButton.vue'
import SelectInput from '../../../components/inputs/SelectInput.vue'
import type { FileCategory } from '../../../entities/TransactionImportEntity'

const props = defineProps<{
  connectionState: 'connecting' | 'live' | 'reconnecting'
  uploadPending: boolean
}>()

const uploadCategory = defineModel<FileCategory>('uploadCategory', { required: true })

defineEmits<{
  upload: [event: Event]
}>()

const fileInput = ref<HTMLInputElement | null>(null)

const categoryOptions = [
  { label: 'Credit card', value: 'CreditCard' },
  { label: 'Extrato', value: 'Extrato' },
]

const connectionLabel = computed(() => {
  switch (props.connectionState) {
    case 'live':
      return 'Live updates'
    case 'connecting':
      return 'Connecting'
    case 'reconnecting':
      return 'Reconnecting'
  }
})

function chooseFile() {
  fileInput.value?.click()
}
</script>

<template>
  <div
    class="inline-flex items-center gap-2 rounded-md border border-stone-200 bg-white px-3 py-2 text-sm text-stone-600"
    role="status"
    aria-live="polite"
  >
    <span
      class="size-2 rounded-full"
      :class="connectionState === 'live' ? 'bg-emerald-500' : 'animate-pulse bg-amber-500'"
      aria-hidden="true"
    />
    {{ connectionLabel }}
  </div>

  <SelectInput
    v-model="uploadCategory"
    class="min-w-36"
    label="Import category"
    :options="categoryOptions"
    :disabled="uploadPending"
    hide-label
  />

  <input
    ref="fileInput"
    class="hidden"
    type="file"
    accept=".csv,text/csv"
    @change="$emit('upload', $event)"
  />

  <FilledButton
    :text="uploadPending ? 'Uploading...' : 'Upload CSV'"
    :disabled="uploadPending"
    @click="chooseFile"
  />
</template>
