<script setup lang="ts">
import FilledButton from '@/components/buttons/FilledButton.vue'
import ConfirmDialog from '@/components/dialogs/ConfirmDialog.vue'
import ViewHeader from '@/components/headers/ViewHeader.vue'
import DataTable from '@/components/tables/DataTable.vue'
import CategoryFilter from './components/CategoryFilter.vue'
import CategoryFormDialog from './components/CategoryFormDialog.vue'
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
    <ViewHeader title="Categories">
      <template #actions>
        <FilledButton text="New category" @click="openCreateForm" />
      </template>
    </ViewHeader>

    <CategoryFilter
      v-model:search="filters.search"
      v-model:order="filters.order"
    />

    <DataTable
      :headers="tableHeaders"
      :rows="tableRows"
      :error="error"
      :loading="loading"
      loading-label="Loading categories..."
      empty-label="No categories found."
      retry-label="Retry loading categories"
      :retry="loadData"
      :has-next-page="hasNextPage"
      :load-more-failed="loadMoreFailed"
      :load-more-target="setLoadMoreTarget"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :retry-more="loadNextPage"
    />
  </section>

  <CategoryFormDialog
    v-model:title="form.title"
    v-model:description="form.description"
    :editing="editing !== null"
    :open="formOpen"
    :saving="saving"
    @close="closeForm"
    @submit="submitForm"
  />

  <ConfirmDialog
    :open="deleteTarget !== null"
    title="Delete category"
    message="This category will be hidden, but its transaction history will be preserved."
    :busy="deleting"
    @cancel="cancelDelete"
    @confirm="executeDelete"
  />
</template>
