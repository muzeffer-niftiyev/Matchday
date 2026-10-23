import { useToast } from 'vue-toastification'

// Small wrapper: translated toasts.
export function useNotify() {
  const toast = useToast()
  const { t } = useI18n()

  return {
    success: (key: string, params?: Record<string, unknown>) => toast.success(t(key, params ?? {})),
    error: (key: string, params?: Record<string, unknown>) => toast.error(t(key, params ?? {})),
    info: (key: string, params?: Record<string, unknown>) => toast.info(t(key, params ?? {})),

    // A ready-made message (for example an error text that came from the API).
    errorText: (message: string) => toast.error(message),
  }
}
