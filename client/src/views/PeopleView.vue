<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import ConfirmDialog from '../components/dialogs/ConfirmDialog.vue'
import ModalDialog from '../components/dialogs/ModalDialog.vue'
import {
  useDeletePersonMutation,
  usePeopleQuery,
  useSavePersonMutation,
} from '../queries/PersonQueries'
import { useToast } from '../stores/toast'
import type { PersonEntity, PersonPayload } from '../entities/PersonEntity.ts'

const toast = useToast()

const formOpen = ref(false)
const editing = ref<PersonEntity | null>(null)
const deleteTarget = ref<PersonEntity | null>(null)

const form = reactive({
  name: '',
  email: '',
  phoneNumber: '',
})

const peopleQuery = usePeopleQuery()

const savePersonMutation = useSavePersonMutation({
  onSuccess: (variables) => {
    toast.success(variables.id ? 'Person updated' : 'Person created')
    formOpen.value = false
  },
  onError: (error) => {
    toast.error(readError(error))
  },
})

const deletePersonMutation = useDeletePersonMutation({
  onSuccess: () => {
    toast.success('Person deleted')
    deleteTarget.value = null
  },
  onError: (error) => {
    toast.error(readError(error))
  },
})

const people = computed(() => peopleQuery.data.value ?? [])
const loading = computed(() => peopleQuery.isPending.value)
const saving = computed(() => savePersonMutation.isPending.value)
const deleting = computed(() => deletePersonMutation.isPending.value)
const error = computed(() => readError(peopleQuery.error.value))

watch(
  () => peopleQuery.error.value,
  (queryError) => {
    if (queryError) {
      toast.error(readError(queryError))
    }
  },
)

function loadPeople() {
  void peopleQuery.refetch()
}

function openCreateForm() {
  editing.value = null
  form.name = ''
  form.email = ''
  form.phoneNumber = ''
  formOpen.value = true
}

function openEditForm(person: PersonEntity) {
  editing.value = person
  form.name = person.name
  form.email = person.email
  form.phoneNumber = person.phoneNumber
  formOpen.value = true
}

function closeForm() {
  if (!saving.value) {
    formOpen.value = false
  }
}

function submitForm() {
  const payload: PersonPayload = {
    name: form.name.trim(),
    email: form.email.trim(),
    phoneNumber: form.phoneNumber.trim(),
  }

  savePersonMutation.mutate({
    id: editing.value?.id,
    payload,
  })
}

function confirmDelete(person: PersonEntity) {
  deleteTarget.value = person
}

function executeDelete() {
  if (!deleteTarget.value) {
    return
  }

  deletePersonMutation.mutate(deleteTarget.value.id)
}

function readError(err: unknown): string {
  if (!err) {
    return ''
  }

  return err instanceof Error ? err.message : 'Something went wrong'
}
</script>

<template>
  <section class="space-y-5">
    <div class="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
      <div>
        <p class="text-sm font-medium text-emerald-700">Assignment roster</p>
        <h1 class="mt-1 text-2xl font-semibold text-stone-950">People</h1>
      </div>

      <div class="flex flex-wrap gap-3">
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          @click="loadPeople"
        >
          Refresh
        </button>
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md bg-emerald-600 px-3 py-2 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          @click="openCreateForm"
        >
          New person
        </button>
      </div>
    </div>

    <div v-if="error" class="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
      {{ error }}
    </div>

    <div class="overflow-hidden rounded-lg border border-stone-200 bg-white">
      <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">Loading people...</div>
      <div v-else-if="people.length === 0" class="px-4 py-12 text-center text-sm text-stone-500">
        No people found.
      </div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-stone-200 text-left text-sm">
          <thead class="bg-stone-100 text-xs uppercase text-stone-500">
            <tr>
              <th class="min-w-56 px-4 py-3 font-semibold">Name</th>
              <th class="min-w-64 px-4 py-3 font-semibold">Email</th>
              <th class="min-w-44 px-4 py-3 font-semibold">Phone</th>
              <th class="w-40 px-4 py-3 text-right font-semibold">Actions</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-stone-100">
            <tr v-for="person in people" :key="person.id" class="hover:bg-stone-50">
              <td class="px-4 py-3 font-medium text-stone-950">{{ person.name }}</td>
              <td class="px-4 py-3 text-stone-600">{{ person.email }}</td>
              <td class="px-4 py-3 text-stone-600">{{ person.phoneNumber }}</td>
              <td class="px-4 py-3">
                <div class="flex justify-end gap-2">
                  <button
                    type="button"
                    class="rounded-md px-3 py-2 text-sm font-medium text-stone-600 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    @click="openEditForm(person)"
                  >
                    Edit
                  </button>
                  <button
                    type="button"
                    class="rounded-md px-3 py-2 text-sm font-medium text-stone-600 hover:bg-rose-50 hover:text-rose-700 focus:outline-none focus:ring-2 focus:ring-rose-500"
                    @click="confirmDelete(person)"
                  >
                    Delete
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>

  <ModalDialog :open="formOpen" :title="editing ? 'Edit person' : 'New person'" @close="closeForm">
    <form class="space-y-4" @submit.prevent="submitForm">
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Name</span>
        <input
          v-model="form.name"
          class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
          type="text"
          required
        />
      </label>
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Email</span>
        <input
          v-model="form.email"
          class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
          type="email"
          required
        />
      </label>
      <label class="block">
        <span class="text-sm font-medium text-stone-700">Phone</span>
        <input
          v-model="form.phoneNumber"
          class="mt-1 w-full rounded-md border border-stone-300 px-3 py-2 text-sm focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20"
          type="tel"
          required
        />
      </label>
      <div class="flex justify-end gap-3 pt-2">
        <button
          type="button"
          class="rounded-md border border-stone-300 px-4 py-2 text-sm font-medium text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          :disabled="saving"
          @click="closeForm"
        >
          Cancel
        </button>
        <button
          type="submit"
          class="rounded-md bg-emerald-600 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-60"
          :disabled="saving"
        >
          {{ saving ? 'Saving...' : 'Save' }}
        </button>
      </div>
    </form>
  </ModalDialog>

  <ConfirmDialog
    :open="deleteTarget !== null"
    title="Delete person"
    message="This person will be marked as deleted."
    :busy="deleting"
    @cancel="deleteTarget = null"
    @confirm="executeDelete"
  />
</template>
