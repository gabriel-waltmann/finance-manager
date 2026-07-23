import type { Component } from 'vue'

export interface DataTableHeader {
  key: string
  label: string
  align?: 'left' | 'right'
  class?: string
}

export interface DataTableComponentCell {
  component: Component
  props?: Record<string, unknown>
}

export type DataTableCell = string | DataTableComponentCell

export interface DataTableRow {
  key: string
  cells: DataTableCell[]
}
