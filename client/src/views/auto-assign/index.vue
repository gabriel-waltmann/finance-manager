<script setup lang="ts">
import ErrorAlert from '../../components/alerts/ErrorAlert.vue'
import ConfirmDialog from '../../components/dialogs/ConfirmDialog.vue'
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DataTable from '../../components/tables/DataTable.vue'
import AutoAssignFilter from './components/AutoAssignFilter.vue'
import AutoAssignForm from './components/AutoAssignForm.vue'
import { useController } from './useController'

const {
  applying,
  applyAssignments,
  assignment,
  assignmentDisabled,
  canApply,
  categories,
  categoryError,
  closeConfirmation,
  confirmationMessage,
  confirmOpen,
  filterError,
  filters,
  hasNextPage,
  loadData,
  loadMoreFailed,
  loadNextPage,
  loadProgress,
  loading,
  loadingMore,
  openConfirmation,
  personError,
  persons,
  previewEmptyLabel,
  setLoadMoreTarget,
  summaryLabel,
  summaryValue,
  tableHeaders,
  tableRows,
  transactionError,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Auto assign" />

    <AutoAssignFilter
      v-model:title="filters.title"
      v-model:start-date="filters.startDate"
      v-model:end-date="filters.endDate"
      v-model:person-filter="filters.personFilter"
      v-model:category-filter="filters.categoryFilter"
      :persons="persons"
      :categories="categories"
      :disabled="applying"
    />

    <ErrorAlert v-if="filterError" :message="filterError" />
    <ErrorAlert v-if="personError" :message="personError" />
    <ErrorAlert v-if="categoryError" :message="categoryError" />

    <DataTable
      title="Matching transactions"
      :headers="tableHeaders"
      :rows="tableRows"
      :error="transactionError"
      :loading="loading"
      loading-label="Loading matching transactions..."
      :empty-label="previewEmptyLabel"
      retry-label="Retry loading matching transactions"
      :retry="loadData"
      :has-next-page="hasNextPage"
      :load-more-failed="loadMoreFailed"
      :load-more-target="setLoadMoreTarget"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :retry-more="loadNextPage"
      :summary-label="summaryLabel"
      :summary-value="summaryValue"
    />

    <AutoAssignForm
      v-model:person-action="assignment.personAction"
      v-model:target-person-id="assignment.targetPersonId"
      v-model:category-action="assignment.categoryAction"
      v-model:target-category-ids="assignment.targetCategoryIds"
      :applying="applying"
      :can-apply="canApply"
      :categories="categories"
      :disabled="assignmentDisabled"
      :persons="persons"
      @apply="openConfirmation"
    />
  </section>

  <ConfirmDialog
    :open="confirmOpen"
    title="Apply assignments"
    :message="confirmationMessage"
    confirm-label="Apply assignments"
    busy-label="Applying..."
    :busy="applying"
    @cancel="closeConfirmation"
    @confirm="applyAssignments"
  />
</template>
