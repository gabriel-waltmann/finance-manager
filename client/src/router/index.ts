import { createRouter, createWebHistory } from 'vue-router'
import DashboardView from '../views/DashboardView.vue'
import TransactionsView from '../views/TransactionsView.vue'
import PeopleView from '../views/PeopleView.vue'
import ImportsView from '../views/ImportsView.vue'

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
      path: '/people',
      name: 'people',
      component: PeopleView,
    },
    {
      path: '/imports',
      name: 'imports',
      component: ImportsView,
    },
  ],
})

export default router
