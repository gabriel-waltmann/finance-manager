<script setup lang="ts">
import { PhCaretDown, PhX } from '@phosphor-icons/vue'
import { computed, nextTick, onBeforeUnmount, onMounted, ref, useId, watch } from 'vue'

interface MultiSelectOption {
  label: string
  value: string
}

const props = withDefaults(
  defineProps<{
    disabled?: boolean
    confirm?: boolean
    hideLabel?: boolean
    label: string
    options: MultiSelectOption[]
    placeholder?: string
  }>(),
  {
    disabled: false,
    confirm: false,
    hideLabel: false,
    placeholder: 'Select options',
  },
)

const model = defineModel<string[]>({ required: true })
const emit = defineEmits<{
  change: [values: string[]]
}>()
const open = ref(false)
const root = ref<HTMLElement | null>(null)
const trigger = ref<HTMLButtonElement | null>(null)
const panel = ref<HTMLElement | null>(null)
const panelStyle = ref<Record<string, string>>({})
const draftValues = ref<string[]>([])
const activeOptionIndex = ref(-1)
const panelId = `multi-select-${useId()}`

const activeValues = computed(() => props.confirm && open.value ? draftValues.value : model.value)
const selectedOptions = computed(() => props.options.filter((option) => (
  activeValues.value.includes(option.value)
)))

function toggleOpen() {
  if (props.disabled) return
  if (open.value) {
    closePanel()
    return
  }

  draftValues.value = [...model.value]
  open.value = true
  void prepareOpenPanel()
}

async function prepareOpenPanel() {
  await positionPanel()
  const firstSelectedIndex = props.options.findIndex((option) => (
    activeValues.value.includes(option.value)
  ))
  focusOption(firstSelectedIndex >= 0 ? firstSelectedIndex : 0)
}

async function positionPanel() {
  await nextTick()
  const bounds = trigger.value?.getBoundingClientRect()
  if (!bounds) return

  const width = Math.max(bounds.width, 256)
  const left = Math.min(bounds.left, window.innerWidth - width - 8)
  const horizontalPosition = {
    left: `${Math.max(left, 8)}px`,
    width: `${width}px`,
  }
  panelStyle.value = {
    ...horizontalPosition,
    top: `${bounds.bottom + 4}px`,
  }

  await nextTick()
  const panelHeight = panel.value?.offsetHeight ?? 0
  const top = bounds.bottom + panelHeight + 8 <= window.innerHeight
    ? bounds.bottom + 4
    : Math.max(bounds.top - panelHeight - 4, 8)
  panelStyle.value = {
    ...horizontalPosition,
    top: `${top}px`,
  }
}

function focusOption(index: number) {
  const optionElements = panel.value?.querySelectorAll<HTMLElement>('[role="option"]')
  if (!optionElements?.length) {
    activeOptionIndex.value = -1
    return
  }

  const nextIndex = (index + optionElements.length) % optionElements.length
  activeOptionIndex.value = nextIndex
  optionElements[nextIndex]?.focus()
}

function handleOptionKeydown(event: KeyboardEvent, index: number, value: string) {
  if (event.key === 'ArrowDown') {
    event.preventDefault()
    event.stopPropagation()
    focusOption(index + 1)
    return
  }

  if (event.key === 'ArrowUp') {
    event.preventDefault()
    event.stopPropagation()
    focusOption(index - 1)
    return
  }

  if (event.key === ' ' || event.key === 'Spacebar') {
    event.preventDefault()
    event.stopPropagation()
    toggleValue(value)
  }
}

function toggleValue(value: string) {
  const values = activeValues.value
  const nextValues = values.includes(value)
    ? values.filter((item) => item !== value)
    : [...values, value]

  if (props.confirm) {
    draftValues.value = nextValues
    return
  }

  model.value = nextValues
  emit('change', nextValues)
}

function applySelection() {
  model.value = [...draftValues.value]
  emit('change', [...draftValues.value])
  closePanel()
}

function clearSelection() {
  if (props.disabled || model.value.length === 0) return
  model.value = []
  emit('change', [])
}

function closeOnOutsideClick(event: MouseEvent) {
  const target = event.target as Node
  if (!root.value?.contains(target) && !panel.value?.contains(target)) closePanel()
}

function closeOnEscape(event: KeyboardEvent) {
  if (event.key === 'Escape' && open.value) {
    event.preventDefault()
    if (props.confirm) applySelection()
    else closePanel()
    void nextTick(() => trigger.value?.focus())
  }
}

function closePanel() {
  open.value = false
  activeOptionIndex.value = -1
}

watch(() => props.disabled, (disabled) => {
  if (disabled) closePanel()
})

onMounted(() => {
  document.addEventListener('mousedown', closeOnOutsideClick)
  document.addEventListener('keydown', closeOnEscape)
  window.addEventListener('resize', closePanel)
  window.addEventListener('scroll', closePanel, true)
})

onBeforeUnmount(() => {
  document.removeEventListener('mousedown', closeOnOutsideClick)
  document.removeEventListener('keydown', closeOnEscape)
  window.removeEventListener('resize', closePanel)
  window.removeEventListener('scroll', closePanel, true)
})
</script>

<template>
  <div ref="root" class="relative">
    <span :class="hideLabel ? 'sr-only' : 'text-sm font-medium text-stone-700'">{{ label }}</span>
    <div class="flex gap-2" :class="{ 'mt-1': !hideLabel }">
      <button
        ref="trigger"
        type="button"
        class="flex min-h-10 min-w-0 flex-1 items-center justify-between gap-2 rounded-md border border-stone-300 bg-white px-3 py-2 text-left text-sm text-stone-700 focus:border-emerald-500 focus:outline-none focus:ring-2 focus:ring-emerald-500/20 disabled:cursor-not-allowed disabled:bg-stone-100"
        :aria-controls="panelId"
        :aria-expanded="open"
        aria-haspopup="listbox"
        :disabled="disabled"
        @click="toggleOpen"
      >
        <span v-if="selectedOptions.length === 0" class="truncate text-stone-500">
          {{ placeholder }}
        </span>
        <span v-else class="flex min-w-0 flex-wrap gap-1">
          <span
            v-for="option in selectedOptions.slice(0, 2)"
            :key="option.value"
            class="max-w-32 truncate rounded bg-emerald-50 px-2 py-0.5 text-xs font-medium text-emerald-700"
          >
            {{ option.label }}
          </span>
          <span
            v-if="selectedOptions.length > 2"
            class="rounded bg-stone-100 px-2 py-0.5 text-xs font-medium text-stone-600"
          >
            +{{ selectedOptions.length - 2 }}
          </span>
        </span>
        <PhCaretDown class="shrink-0" :size="16" aria-hidden="true" />
      </button>

      <button
        v-if="model.length > 0"
        type="button"
        class="rounded-md border border-stone-300 bg-white px-2 text-stone-500 hover:bg-stone-100 hover:text-stone-900 focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:cursor-not-allowed disabled:bg-stone-100"
        :disabled="disabled"
        :aria-label="`Clear ${label.toLowerCase()}`"
        :title="`Clear ${label.toLowerCase()}`"
        @click="clearSelection"
      >
        <PhX :size="16" weight="bold" aria-hidden="true" />
      </button>
    </div>

    <Teleport to="body">
      <div
        v-if="open"
        :id="panelId"
        ref="panel"
        class="fixed z-50 rounded-md border border-stone-200 bg-white p-1 shadow-lg"
        :style="panelStyle"
        role="listbox"
        aria-multiselectable="true"
        @wheel.prevent.stop
      >
        <p v-if="options.length === 0" class="px-3 py-2 text-sm text-stone-500">
          No options available
        </p>
        <label
          v-for="(option, index) in options"
          v-else
          :key="option.value"
          class="flex cursor-pointer items-center gap-2 rounded px-3 py-2 text-sm text-stone-700 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-inset focus:ring-emerald-500 focus-within:bg-emerald-50"
          role="option"
          :aria-selected="activeValues.includes(option.value)"
          :tabindex="activeOptionIndex === index ? 0 : -1"
          @focusin="activeOptionIndex = index"
          @keydown="handleOptionKeydown($event, index, option.value)"
        >
          <input
            type="checkbox"
            class="size-4 rounded border-stone-300 text-emerald-600 focus:ring-emerald-500"
            :checked="activeValues.includes(option.value)"
            :disabled="disabled"
            tabindex="-1"
            @change="toggleValue(option.value)"
          />
          <span>{{ option.label }}</span>
        </label>
        <div v-if="confirm && options.length > 0" class="flex justify-end gap-2 border-t border-stone-200 bg-white px-2 py-2">
          <button
            type="button"
            class="rounded-md px-3 py-1.5 text-sm font-medium text-stone-600 hover:bg-stone-100 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            @click="closePanel"
          >
            Cancel
          </button>
          <button
            type="button"
            class="rounded-md bg-emerald-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-500"
            @click="applySelection"
          >
            Apply
          </button>
        </div>
      </div>
    </Teleport>
  </div>
</template>
