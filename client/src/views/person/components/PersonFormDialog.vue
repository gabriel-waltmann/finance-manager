<script setup lang="ts">
import FilledButton from '../../../components/buttons/FilledButton.vue'
import ModalDialog from '../../../components/dialogs/ModalDialog.vue'
import TextInput from '../../../components/inputs/TextInput.vue'

defineProps<{
  editing: boolean
  open: boolean
  saving: boolean
}>()

const name = defineModel<string>('name', { required: true })
const email = defineModel<string>('email', { required: true })
const phoneNumber = defineModel<string>('phoneNumber', { required: true })

defineEmits<{
  close: []
  submit: []
}>()
</script>

<template>
  <ModalDialog :open="open" :title="editing ? 'Edit person' : 'New person'" @close="$emit('close')">
    <form class="space-y-4" @submit.prevent="$emit('submit')">
      <TextInput v-model="name" label="Name" required />

      <TextInput v-model="email" label="Email" type="email" required />

      <TextInput v-model="phoneNumber" label="Phone" type="tel" required />

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
