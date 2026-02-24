import { create } from "zustand";

export type WizardStep = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7;
export type TemplateType = "barneskole" | "ungdomsskole" | "combined" | "videregaende" | "custom";

interface WizardState {
  currentStep: WizardStep;
  completedSteps: Set<WizardStep>;
  template: TemplateType | null;

  setCurrentStep: (step: WizardStep) => void;
  markStepCompleted: (step: WizardStep) => void;
  setTemplate: (template: TemplateType) => void;
  reset: () => void;
  canGoToStep: (step: WizardStep) => boolean;
}

export const useWizardStore = create<WizardState>((set, get) => ({
  currentStep: 0,
  completedSteps: new Set<WizardStep>(),
  template: null,

  setCurrentStep: (step) => set({ currentStep: step }),

  markStepCompleted: (step) =>
    set((state) => ({
      completedSteps: new Set([...state.completedSteps, step]),
    })),

  setTemplate: (template) => set({ template }),

  reset: () =>
    set({
      currentStep: 0,
      completedSteps: new Set<WizardStep>(),
      template: null,
    }),

  canGoToStep: (step) => {
    const { completedSteps } = get();
    if (step === 0) return true;
    return completedSteps.has((step - 1) as WizardStep) || completedSteps.has(step);
  },
}));
