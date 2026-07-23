<script setup lang="ts">
import { ChevronLeft, ChevronRight } from 'lucide-vue-next'
import ErrorAlert from '../../components/alerts/ErrorAlert.vue'
import HorizontalBarChart from '../../components/charts/HorizontalBarChart.vue'
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DashboardFilter from './components/Filter.vue'
import { useController } from './useController'

const {
  applyFilters,
  changeLimit,
  chartItems,
  displayAmount,
  error,
  filters,
  goToNextPage,
  goToPreviousPage,
  hasNextPage,
  hasPreviousPage,
  loadDashboard,
  loading,
  pageRange,
  pageSizeOptions,
  pagination,
  people,
  refreshing,
  totalSpend,
  visibleTotalPages,
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
      :page-size="pagination.limit"
      :page-size-options="pageSizeOptions"
      @apply-filters="applyFilters"
      @change-limit="changeLimit"
    />

    <ErrorAlert v-if="error" :message="error" />

    <div class="rounded-lg border border-stone-200 bg-white">
      <div class="border-b border-stone-200 px-4 py-3">
        <h2 class="text-base font-semibold text-stone-950">Top spend items</h2>
      </div>
      
      <div v-if="loading" class="px-4 py-12 text-center text-sm text-stone-500">Loading dashboard...</div>
      
      <div v-else class="px-4 py-4">
        <HorizontalBarChart
          :items="chartItems"
          empty-label="No spend found for this range."
          :value-formatter="displayAmount"
        />
        <div class="mt-4 flex items-center justify-between border-t border-stone-200 pt-4">
          <span class="text-sm font-medium text-stone-600">Total spend</span>
          <span class="text-base font-semibold text-stone-950">{{ displayAmount(totalSpend) }}</span>
        </div>
        <div class="mt-4 flex flex-col gap-3 border-t border-stone-200 pt-4 text-sm text-stone-600 md:flex-row md:items-center md:justify-between">
          <p>{{ pageRange }}</p>
          <div class="flex items-center gap-3">
            <button
              type="button"
              class="inline-flex size-8 items-center justify-center rounded-md border border-stone-300 text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-50"
              title="Previous page"
              :disabled="!hasPreviousPage"
              @click="goToPreviousPage"
            >
              <ChevronLeft class="size-4" aria-hidden="true" />
              <span class="sr-only">Previous page</span>
            </button>
            <span class="min-w-20 text-center font-medium text-stone-700">
              {{ pagination.page }} / {{ visibleTotalPages }}
            </span>
            <button
              type="button"
              class="inline-flex size-8 items-center justify-center rounded-md border border-stone-300 text-stone-500 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:opacity-50"
              title="Next page"
              :disabled="!hasNextPage"
              @click="goToNextPage"
            >
              <ChevronRight class="size-4" aria-hidden="true" />
              <span class="sr-only">Next page</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>
