<script setup lang="ts">
import FilledButton from '@/components/buttons/FilledButton.vue'
import ModalDialog from '@/components/dialogs/ModalDialog.vue'
import TextAreaInput from '@/components/inputs/TextAreaInput.vue'
import TextInput from '@/components/inputs/TextInput.vue'

defineProps<{
  editing: boolean
  open: boolean
  saving: boolean
}>()

const title = defineModel<string>('title', { required: true })
const description = defineModel<string>('description', { required: true })

defineEmits<{
  close: []
  submit: []
}>()
</script>

<template>
  <ModalDialog :open="open" :title="editing ? 'Edit category' : 'New category'" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="$emit('submit')">
      <TextInput v-model="title" label="Title" :max-length="120" required />

      <TextAreaInput
        v-model="description"
        label="Description"
        :max-length="500"
        placeholder="Optional details about this category"
      />

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
