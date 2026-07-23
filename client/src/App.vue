<script setup lang="ts">
import { Landmark } from 'lucide-vue-next'
import { RouterLink, RouterView } from 'vue-router'
import ToastHost from './components/toasts/HostToast.vue'

const navItems = [
  { to: '/', label: 'Dashboard' },
  { to: '/transactions', label: 'Transactions' },
  { to: '/imports', label: 'Imports' },
  { to: '/people', label: 'People' },
] as const
</script>

<template>
  <div class="min-h-dvh bg-stone-50 text-stone-900">
    <header class="border-b border-stone-200 bg-white">
      <div class="mx-auto flex max-w-7xl flex-col gap-4 px-4 py-4 sm:px-6 lg:flex-row lg:items-center lg:justify-between lg:px-8">
        <div class="flex items-center gap-3">
          <span class="inline-flex size-10 items-center justify-center rounded-lg bg-emerald-600 text-white">
            <Landmark class="size-5" aria-hidden="true" />
          </span>
          <div>
            <p class="text-lg font-semibold text-stone-950">Finance Manager</p>
            <p class="text-sm text-stone-500">Transactions, people, and assignments</p>
          </div>
        </div>

        <nav class="flex gap-2" aria-label="Main navigation">
          <RouterLink
            v-for="item in navItems"
            :key="item.to"
            :to="item.to"
            class="rounded-md px-3 py-2 text-sm font-medium text-stone-600 hover:bg-stone-100 hover:text-stone-950 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            active-class="bg-emerald-50 text-emerald-700"
          >
            {{ item.label }}
          </RouterLink>
        </nav>
      </div>
    </header>

    <main class="mx-auto max-w-7xl px-4 py-6 sm:px-6 lg:px-8">
      <RouterView />
    </main>

    <ToastHost />
  </div>
</template>
