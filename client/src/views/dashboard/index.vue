<script setup lang="ts">
import ErrorAlert from '../../components/alerts/ErrorAlert.vue'
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DataTable from '../../components/tables/DataTable.vue'
import DashboardFilter from './components/Filter.vue'
import { useController } from './useController'

const {
  displayAmount,
  error,
  filters,
  hasNextPage,
  loadMoreFailed,
  loadNextPage,
  loadProgress,
  loading,
  loadingMore,
  people,
  setLoadMoreTarget,
  tableHeaders,
  tableRows,
  totalSpend,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Dashboard" />

    <DashboardFilter
      v-model:start-date="filters.startDate"
      v-model:end-date="filters.endDate"
      v-model:person-id="filters.personId"
      v-model:order="filters.order"
      :people="people"
    />

    <ErrorAlert v-if="error" :message="error" />

    <DataTable
      title="Top spend items"
      :headers="tableHeaders"
      :rows="tableRows"
      :loading="loading"
      loading-label="Loading dashboard..."
      empty-label="No spend found for this range."
      summary-label="Total spend"
      :summary-value="displayAmount(totalSpend)"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :load-more-failed="loadMoreFailed"
      :has-next-page="hasNextPage"
      :retry-more="loadNextPage"
      :load-more-target="setLoadMoreTarget"
    />
  </section>
</template>
