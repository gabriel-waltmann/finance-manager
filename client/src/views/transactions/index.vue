<script setup lang="ts">
import ErrorAlert from '../../components/alerts/ErrorAlert.vue'
import FilledButton from '../../components/buttons/FilledButton.vue'
import ConfirmDialog from '../../components/dialogs/ConfirmDialog.vue'
import ViewHeader from '../../components/headers/ViewHeader.vue'
import DataTable from '../../components/tables/DataTable.vue'
import TransactionFilter from './components/TransactionFilter.vue'
import TransactionFormDialog from './components/TransactionFormDialog.vue'
import TransactionTotalCard from './components/TransactionTotalCard.vue'
import { useController } from './useController'

const {
  cancelDelete,
  closeForm,
  deleteTarget,
  deleting,
  editing,
  executeDelete,
  filters,
  form,
  formOpen,
  hasNextPage,
  loadData,
  loadMoreFailed,
  loadNextPage,
  loadProgress,
  loadedAmountTotal,
  loading,
  loadingMore,
  openCreateForm,
  personError,
  persons,
  saving,
  setLoadMoreTarget,
  submitForm,
  tableHeaders,
  tableRows,
  transactionError,
} = useController()
</script>

<template>
  <section class="space-y-5">
    <ViewHeader title="Transactions">
      <template #actions>
        <FilledButton text="New transaction" @click="openCreateForm" />
      </template>
    </ViewHeader>

    <TransactionFilter
      v-model:search="filters.search"
      v-model:start-date="filters.startDate"
      v-model:end-date="filters.endDate"
      v-model:person-filter="filters.personFilter"
      v-model:order="filters.order"
      :persons="persons"
    />

    <TransactionTotalCard
      :amount="loadedAmountTotal"
      :loading="loading"
      :unavailable="Boolean(transactionError) && tableRows.length === 0"
    />

    <ErrorAlert v-if="personError" :message="personError" />

    <DataTable
      :headers="tableHeaders"
      :rows="tableRows"
      :error="transactionError"
      :loading="loading"
      loading-label="Loading transactions..."
      empty-label="No transactions found."
      retry-label="Retry loading transactions"
      :retry="loadData"
      :has-next-page="hasNextPage"
      :load-more-failed="loadMoreFailed"
      :load-more-target="setLoadMoreTarget"
      :load-progress="loadProgress"
      :loading-more="loadingMore"
      :retry-more="loadNextPage"
    />
  </section>

  <TransactionFormDialog
    v-model:date="form.date"
    v-model:title="form.title"
    v-model:amount="form.amount"
    v-model:person-id="form.personId"
    :editing="editing !== null"
    :open="formOpen"
    :persons="persons"
    :saving="saving"
    @close="closeForm"
    @submit="submitForm"
  />

  <ConfirmDialog
    :open="deleteTarget !== null"
    title="Delete transaction"
    message="This transaction will be marked as deleted."
    :busy="deleting"
    @cancel="cancelDelete"
    @confirm="executeDelete"
  />
</template>
