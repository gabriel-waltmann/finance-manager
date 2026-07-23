import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/dashboard/index.vue'
import TransactionsView from '../views/transactions/index.vue'
import PersonView from '../views/person/index.vue'
import ImportsView from '../views/imports/index.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: DashboardView,
    },
    {
      path: '/transactions',
      name: 'transactions',
      component: TransactionsView,
    },
    {
      path: '/person',
      name: 'person',
      component: PersonView,
    },
    {
      path: '/imports',
      name: 'imports',
      component: ImportsView,
    },
  ],
})

export default router
