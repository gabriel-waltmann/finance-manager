<script setup lang="ts">
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DataTable from '../../components/tables/DataTable.vue'
import ImportActions from './components/ImportActions.vue'
import ImportFilter from './components/ImportFilter.vue'
import { useController } from './useController'

const {
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
  setLoadMoreTarget,
  tableHeaders,
  tableRows,
  uploadCategory,
  uploadFile,
  uploadPending,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Transaction imports">
      <template #actions>
        <ImportActions
          v-model:upload-category="uploadCategory"
          :connection-state="connectionState"
          :upload-pending="uploadPending"
          @upload="uploadFile"
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
  </section>
</template>
