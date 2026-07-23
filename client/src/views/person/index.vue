<script setup lang="ts">
import FilledButton from '../../components/buttons/FilledButton.vue'
import ConfirmDialog from '../../components/dialogs/ConfirmDialog.vue'
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DataTable from '../../components/tables/DataTable.vue'
import PersonFilter from './components/PersonFilter.vue'
import PersonFormDialog from './components/PersonFormDialog.vue'
import { useController } from './useController'

const {
  cancelDelete,
  closeForm,
  deleteTarget,
  deleting,
  editing,
  error,
  executeDelete,
  filters,
  form,
  formOpen,
  hasNextPage,
  loadData,
  loadMoreFailed,
  loadNextPage,
  loadProgress,
  loading,
  loadingMore,
  openCreateForm,
  saving,
  setLoadMoreTarget,
  submitForm,
  tableHeaders,
  tableRows,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Person">
      <template #actions>
        <FilledButton text="New person" @click="openCreateForm" />
      </template>
    </ViewHeader>

    <PersonFilter
      v-model:search="filters.search"
      v-model:order="filters.order"
    />

    <DataTable
      :headers="tableHeaders"
      :rows="tableRows"
      :error="error"
      :loading="loading"
      loading-label="Loading person..."
      empty-label="No person found."
      retry-label="Retry loading person"
      :retry="loadData"
      :has-next-page="hasNextPage"
      :load-more-failed="loadMoreFailed"
      :load-more-target="setLoadMoreTarget"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :retry-more="loadNextPage"
    />
  </section>

  <PersonFormDialog
    v-model:name="form.name"
    v-model:email="form.email"
    v-model:phone-number="form.phoneNumber"
    :editing="editing !== null"
    :open="formOpen"
    :saving="saving"
    @close="closeForm"
    @submit="submitForm"
  />

  <ConfirmDialog
    :open="deleteTarget !== null"
    title="Delete person"
    message="This person will be marked as deleted."
    :busy="deleting"
    @cancel="cancelDelete"
    @confirm="executeDelete"
  />
</template>
