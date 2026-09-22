<script setup lang="ts">
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DataTable from '../../components/tables/DataTable.vue'
import ImportActions from './components/ImportActions.vue'
import ImportFilter from './components/ImportFilter.vue'
import ImportUploadDialog from './components/ImportUploadDialog.vue'
import { useController } from './useController'

const {
  categories,
  closeUpload,
  connectionState,
  error,
  filters,
  hasNextPage,
  loadData,
  loadMoreFailed,
  loadNextPage,
  loadProgress,
  loading,
  loadingMore,
  openUpload,
  persons,
  setLoadMoreTarget,
  tableHeaders,
  tableRows,
  uploadForm,
  uploadFile,
  uploadOpen,
  uploadOptionsLoading,
  uploadPending,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Transaction imports">
      <template #actions>
        <ImportActions
          :connection-state="connectionState"
          :upload-pending="uploadPending"
          @open-upload="openUpload"
        />
      </template>
    </ViewHeader>

    <ImportFilter
      v-model:search="filters.search"
      v-model:status="filters.status"
      v-model:order="filters.order"
    />

    <DataTable
      :headers="tableHeaders"
      :rows="tableRows"
      :error="error"
      :loading="loading"
      loading-label="Loading imports..."
      empty-label="No imports found."
      retry-label="Retry loading imports"
      :retry="loadData"
      :has-next-page="hasNextPage"
      :load-more-failed="loadMoreFailed"
      :load-more-target="setLoadMoreTarget"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :retry-more="loadNextPage"
    />

    <ImportUploadDialog
      v-model:category="uploadForm.category"
      v-model:category-ids="uploadForm.categoryIds"
      v-model:file="uploadForm.file"
      v-model:person-id="uploadForm.personId"
      :categories="categories"
      :open="uploadOpen"
      :options-loading="uploadOptionsLoading"
      :persons="persons"
      :uploading="uploadPending"
      @close="closeUpload"
      @submit="uploadFile"
    />
  </section>
</template>
