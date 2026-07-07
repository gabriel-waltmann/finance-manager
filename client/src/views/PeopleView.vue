<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { Edit3, Plus, RefreshCw, Trash2 } from 'lucide-vue-next'
import ConfirmDialog from '../components/ConfirmDialog.vue'
import ModalDialog from '../components/ModalDialog.vue'
import { createPerson, deletePerson, listPeople, updatePerson } from '../api/finance'
import { useToast } from '../stores/toast'
import type { Person, PersonPayload } from '../types'

const toast = useToast()

const people = ref<Person[]>([])
const loading = ref(true)
const saving = ref(false)
const deleting = ref(false)
const error = ref('')
const formOpen = ref(false)
const editing = ref<Person | null>(null)
const deleteTarget = ref<Person | null>(null)

const form = reactive({
  name: '',
  email: '',
  phoneNumber: '',
})

onMounted(() => {
  void loadPeople()
})

async function loadPeople() {
  loading.value = true
  error.value = ''

  try {
    people.value = await listPeople()
  } catch (err) {
    error.value = readError(err)
    toast.error(error.value)
  } finally {
    loading.value = false
  }
}

function openCreateForm() {
  editing.value = null
  form.name = ''
  form.email = ''
  form.phoneNumber = ''
  formOpen.value = true
}

function openEditForm(person: Person) {
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

async function submitForm() {
  const payload: PersonPayload = {
    name: form.name.trim(),
    email: form.email.trim(),
    phoneNumber: form.phoneNumber.trim(),
  }

  saving.value = true

  try {
    if (editing.value) {
      await updatePerson(editing.value.id, payload)
      toast.success('Person updated')
    } else {
      await createPerson(payload)
      toast.success('Person created')
    }

    formOpen.value = false
    await loadPeople()
  } catch (err) {
    toast.error(readError(err))
  } finally {
    saving.value = false
  }
}

function confirmDelete(person: Person) {
  deleteTarget.value = person
}

async function executeDelete() {
  if (!deleteTarget.value) {
    return
  }

  deleting.value = true

  try {
    await deletePerson(deleteTarget.value.id)
    toast.success('Person deleted')
    deleteTarget.value = null
    await loadPeople()
  } catch (err) {
    toast.error(readError(err))
  } finally {
    deleting.value = false
  }
}

function readError(err: unknown): string {
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
          <RefreshCw class="size-4" aria-hidden="true" />
          Refresh
        </button>
        <button
          type="button"
          class="inline-flex items-center gap-2 rounded-md bg-emerald-600 px-3 py-2 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
          @click="openCreateForm"
        >
          <Plus class="size-4" aria-hidden="true" />
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
              <th class="w-28 px-4 py-3 text-right font-semibold">Actions</th>
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
                    class="inline-flex size-8 items-center justify-center rounded-md text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
                    title="Edit person"
                    @click="openEditForm(person)"
                  >
                    <Edit3 class="size-4" aria-hidden="true" />
                    <span class="sr-only">Edit person</span>
                  </button>
                  <button
                    type="button"
                    class="inline-flex size-8 items-center justify-center rounded-md text-stone-500 hover:bg-rose-50 hover:text-rose-700 focus:outline-none focus:ring-2 focus:ring-rose-500"
                    title="Delete person"
                    @click="confirmDelete(person)"
                  >
                    <Trash2 class="size-4" aria-hidden="true" />
                    <span class="sr-only">Delete person</span>
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
