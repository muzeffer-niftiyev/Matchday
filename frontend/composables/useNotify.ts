import { useToast } from 'vue-toastification'
import type { ActionResult } from '~/stores/data'

// Small wrapper: translated toasts, and one helper to report an action's result.
export function useNotify() {
  const toast = useToast()
  const { t } = useI18n()

  return {
    success: (key: string, params?: Record<string, unknown>) => toast.success(t(key, params ?? {})),
    error: (key: string, params?: Record<string, unknown>) => toast.error(t(key, params ?? {})),
    info: (key: string, params?: Record<string, unknown>) => toast.info(t(key, params ?? {})),

    // Returns true if the action succeeded; shows the right toast either way.
    result(result: ActionResult, successKey: string, params?: Record<string, unknown>) {
      if (result.ok) {
        toast.success(t(successKey, params ?? {}))
        return true
      }
      toast.error(t(result.error))
      return false
    },
  }
}
