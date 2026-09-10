<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import MultiSelectInput from '../../../components/inputs/MultiSelectInput.vue'
import type { CategoryEntity } from '../../../entities/CategoryEntity'

const props = defineProps<{
  categories: CategoryEntity[]
  categoryIds: string[]
  disabled: boolean
}>()

const categoryOptions = computed(() => props.categories.map((category) => ({
  label: category.title,
  value: category.id,
})))
const selectedIds = ref([...props.categoryIds])

watch(() => props.categoryIds, (categoryIds) => {
  selectedIds.value = [...categoryIds]
})

defineEmits<{
  change: [categoryIds: string[]]
}>()
</script>

<template>
  <MultiSelectInput
    v-model="selectedIds"
    :disabled="disabled"
    confirm
    hide-label
    label="Categories"
    :options="categoryOptions"
    placeholder="Uncategorized"
    @change="$emit('change', $event)"
  />
</template>
