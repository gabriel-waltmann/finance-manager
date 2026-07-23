<script setup lang="ts">
import ErrorAlert from '../../components/alerts/ErrorAlert.vue'
import HorizontalBarChart from '../../components/charts/HorizontalBarChart.vue'
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DashboardFilter from './components/Filter.vue'
import { useController } from './useController'

const {
  chartItems,
  displayAmount,
  error,
  filters,
  hasNextPage,
  loadDashboard,
  loadMoreFailed,
  loadNextPage,
  loadProgress,
  loading,
  loadingMore,
  people,
  refreshing,
  setLoadMoreTarget,
  totalSpend,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Dashboard" :refreshing="refreshing" @refresh="loadDashboard" />

    <DashboardFilter
      v-model:start-date="filters.startDate"
      v-model:end-date="filters.endDate"
      v-model:person-id="filters.personId"
      v-model:order="filters.order"
      :people="people"
    />

    <ErrorAlert v-if="error" :message="error" />

    <HorizontalBarChart
      title="Top spend items"
      :items="chartItems"
      :loading="loading"
      loading-label="Loading dashboard..."
      empty-label="No spend found for this range."
      :value-formatter="displayAmount"
      total-label="Total spend"
      :total-value="totalSpend"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :load-more-failed="loadMoreFailed"
      :has-next-page="hasNextPage"
      :load-more-target="setLoadMoreTarget"
      @retry="loadNextPage"
    />
  </section>
</template>
