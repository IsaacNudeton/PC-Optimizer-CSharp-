import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { ThemeSettings, ThemeProfile, ProfileDetectionData } from '../types/theme';
import { allThemePresets, generalThemes } from '../theme/presets';

interface ThemeStore extends ThemeSettings {
  // Actions
  setGlobalTheme: (theme: ThemeProfile | null) => void;
  setProfileTheme: (profileId: string, theme: ThemeProfile) => void;
  setCategoryTheme: (category: string, theme: ThemeProfile) => void;
  setActiveProfile: (profile: ProfileDetectionData | null) => void;
  toggleDynamicTheming: () => void;
  toggleHaptics: () => void;
  toggleAudio: () => void;
  setMode: (mode: 'light' | 'dark' | 'auto') => void;
  setFontSize: (size: 'small' | 'medium' | 'large') => void;
  setFontFamily: (family: string) => void;
  getCurrentTheme: () => ThemeProfile;
  resetToDefaults: () => void;
  importTheme: (theme: ThemeProfile) => void;
  exportTheme: (themeId: string) => ThemeProfile | null;
}

const defaultSettings: ThemeSettings = {
  mode: 'dark',
  globalTheme: null,
  profileThemes: {},
  categoryThemes: {
    gaming: allThemePresets.valorant,
    working: allThemePresets.vscode,
    media: allThemePresets.netflix,
    general: generalThemes.cosmic,
  },
  activeProfile: null,
  enableDynamicTheming: true,
  enableHaptics: false,
  enableAudio: false,
  fontFamily: 'Inter, system-ui, sans-serif',
  fontSize: 'medium',
};

export const useThemeStore = create<ThemeStore>()(
  persist(
    (set, get) => ({
      ...defaultSettings,

      setGlobalTheme: (theme) => set({ globalTheme: theme }),

      setProfileTheme: (profileId, theme) =>
        set((state) => ({
          profileThemes: { ...state.profileThemes, [profileId]: theme },
        })),

      setCategoryTheme: (category, theme) =>
        set((state) => ({
          categoryThemes: { ...state.categoryThemes, [category]: theme },
        })),

      setActiveProfile: (profile) =>
        set({ activeProfile: profile?.profile || null }),

      toggleDynamicTheming: () =>
        set((state) => ({ enableDynamicTheming: !state.enableDynamicTheming })),

      toggleHaptics: () =>
        set((state) => ({ enableHaptics: !state.enableHaptics })),

      toggleAudio: () =>
        set((state) => ({ enableAudio: !state.enableAudio })),

      setMode: (mode) => set({ mode }),

      setFontSize: (size) => set({ fontSize: size }),

      setFontFamily: (family) => set({ fontFamily: family }),

      getCurrentTheme: () => {
        const state = get();

        // Priority: Global theme > Active profile theme > Category theme > Default
        if (state.globalTheme) {
          return state.globalTheme;
        }

        if (state.enableDynamicTheming && state.activeProfile) {
          // Check for specific profile theme
          const profileTheme = state.profileThemes[state.activeProfile];
          if (profileTheme) return profileTheme;

          // Check for preset theme by profile name
          const presetTheme = allThemePresets[state.activeProfile];
          if (presetTheme) return presetTheme;

          // Fallback to category theme
          // Determine category from active profile
          const profile = allThemePresets[state.activeProfile];
          if (profile?.category) {
            return state.categoryThemes[profile.category] || generalThemes.cosmic;
          }
        }

        // Default to cosmic theme
        return generalThemes.cosmic;
      },

      resetToDefaults: () => set(defaultSettings),

      importTheme: (theme) =>
        set((state) => ({
          profileThemes: { ...state.profileThemes, [theme.id]: theme },
        })),

      exportTheme: (themeId) => {
        const state = get();
        return state.profileThemes[themeId] || allThemePresets[themeId] || null;
      },
    }),
    {
      name: 'pc-optimizer-theme-store',
      version: 1,
    }
  )
);
