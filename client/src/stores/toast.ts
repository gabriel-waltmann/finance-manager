import { readonly, ref } from 'vue'

export interface ToastMessage {
  id: number
  type: 'success' | 'error'
  text: string
}

const messages = ref<ToastMessage[]>([])
let nextId = 1

function push(type: ToastMessage['type'], text: string) {
  const id = nextId++
  messages.value = [...messages.value, { id, type, text }]

  window.setTimeout(() => remove(id), 4200)
}

function remove(id: number) {
  messages.value = messages.value.filter((message) => message.id !== id)
}

export function useToast() {
  return {
    messages: readonly(messages),
    success: (text: string) => push('success', text),
    error: (text: string) => push('error', text),
    remove,
  }
}
