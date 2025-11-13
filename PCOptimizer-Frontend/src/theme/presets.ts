import { ThemeProfile, ProfileCategory, AnimationPreset } from '../types/theme';

// Default animation presets
export const defaultAnimation: AnimationPreset = {
  duration: 'normal',
  easing: 'easeInOut',
  enableParticles: true,
  enableGlow: true,
  enableTransitions: true,
  reducedMotion: false,
};

// Gaming Theme Presets
export const gamingThemes: Record<string, ThemeProfile> = {
  valorant: {
    id: 'valorant',
    name: 'Valorant',
    category: 'gaming' as ProfileCategory,
    profile: 'valorant',
    palette: {
      primary: '#FF4655',
      secondary: '#0F1923',
      accent: '#53212B',
      background: {
        primary: '#0F1923',
        secondary: '#1C2D3A',
        tertiary: '#2A3F50',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#ECE8E1',
        muted: '#8C8C8C',
      },
      border: '#FF4655',
      success: '#77DD77',
      warning: '#FFB347',
      error: '#FF4655',
      info: '#5BC0EB',
      glow: '#FF4655',
      particle: '#FF465590',
    },
    animation: defaultAnimation,
  },
  gta: {
    id: 'gta',
    name: 'GTA V',
    category: 'gaming' as ProfileCategory,
    profile: 'gta',
    palette: {
      primary: '#76B947',
      secondary: '#1A1A1A',
      accent: '#F7931E',
      background: {
        primary: '#0A0A0A',
        secondary: '#1A1A1A',
        tertiary: '#2A2A2A',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#E0E0E0',
        muted: '#808080',
      },
      border: '#76B947',
      success: '#76B947',
      warning: '#F7931E',
      error: '#E74C3C',
      info: '#3498DB',
      glow: '#76B947',
      particle: '#76B94790',
    },
    animation: defaultAnimation,
  },
  apex: {
    id: 'apex',
    name: 'Apex Legends',
    category: 'gaming' as ProfileCategory,
    profile: 'apex',
    palette: {
      primary: '#DA292E',
      secondary: '#16191C',
      accent: '#FF6B00',
      background: {
        primary: '#0D0F11',
        secondary: '#16191C',
        tertiary: '#1F2326',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#D4D4D4',
        muted: '#7A7A7A',
      },
      border: '#DA292E',
      success: '#6FCF97',
      warning: '#FF6B00',
      error: '#DA292E',
      info: '#2D9CDB',
      glow: '#DA292E',
      particle: '#DA292E90',
    },
    animation: defaultAnimation,
  },
  csgo: {
    id: 'csgo',
    name: 'CS:GO',
    category: 'gaming' as ProfileCategory,
    profile: 'csgo',
    palette: {
      primary: '#F7B73C',
      secondary: '#1B2838',
      accent: '#66C0F4',
      background: {
        primary: '#0F1419',
        secondary: '#1B2838',
        tertiary: '#2A475E',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#C7D5E0',
        muted: '#8F98A0',
      },
      border: '#F7B73C',
      success: '#4CAF50',
      warning: '#F7B73C',
      error: '#F44336',
      info: '#66C0F4',
      glow: '#F7B73C',
      particle: '#F7B73C90',
    },
    animation: defaultAnimation,
  },
};

// Working/Productivity Theme Presets
export const workingThemes: Record<string, ThemeProfile> = {
  vscode: {
    id: 'vscode',
    name: 'VS Code',
    category: 'working' as ProfileCategory,
    profile: 'vscode',
    palette: {
      primary: '#007ACC',
      secondary: '#1E1E1E',
      accent: '#4EC9B0',
      background: {
        primary: '#1E1E1E',
        secondary: '#252526',
        tertiary: '#2D2D30',
      },
      text: {
        primary: '#D4D4D4',
        secondary: '#CCCCCC',
        muted: '#858585',
      },
      border: '#007ACC',
      success: '#4EC9B0',
      warning: '#DDB26F',
      error: '#F48771',
      info: '#569CD6',
      glow: '#007ACC',
      particle: '#007ACC90',
    },
    animation: { ...defaultAnimation, duration: 'fast', enableParticles: false },
  },
  'adobe-premiere': {
    id: 'adobe-premiere',
    name: 'Adobe Premiere Pro',
    category: 'working' as ProfileCategory,
    profile: 'adobe-premiere',
    palette: {
      primary: '#9999FF',
      secondary: '#1E1E1E',
      accent: '#E579FF',
      background: {
        primary: '#18141D',
        secondary: '#1E1E1E',
        tertiary: '#2A2A2A',
      },
      text: {
        primary: '#E4E4E4',
        secondary: '#CCCCCC',
        muted: '#8A8A8A',
      },
      border: '#9999FF',
      success: '#6FCF97',
      warning: '#F2C94C',
      error: '#EB5757',
      info: '#56CCF2',
      glow: '#9999FF',
      particle: '#9999FF90',
    },
    animation: defaultAnimation,
  },
  'davinci-resolve': {
    id: 'davinci-resolve',
    name: 'DaVinci Resolve',
    category: 'working' as ProfileCategory,
    profile: 'davinci-resolve',
    palette: {
      primary: '#E21B1B',
      secondary: '#161616',
      accent: '#FF9A00',
      background: {
        primary: '#0A0A0A',
        secondary: '#161616',
        tertiary: '#222222',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#D4D4D4',
        muted: '#808080',
      },
      border: '#E21B1B',
      success: '#4CAF50',
      warning: '#FF9A00',
      error: '#E21B1B',
      info: '#2196F3',
      glow: '#E21B1B',
      particle: '#E21B1B90',
    },
    animation: defaultAnimation,
  },
  blender: {
    id: 'blender',
    name: 'Blender',
    category: 'working' as ProfileCategory,
    profile: 'blender',
    palette: {
      primary: '#E87D0D',
      secondary: '#222222',
      accent: '#5680C2',
      background: {
        primary: '#1A1A1A',
        secondary: '#222222',
        tertiary: '#2A2A2A',
      },
      text: {
        primary: '#E6E6E6',
        secondary: '#CCCCCC',
        muted: '#8C8C8C',
      },
      border: '#E87D0D',
      success: '#81C784',
      warning: '#FFB74D',
      error: '#E57373',
      info: '#64B5F6',
      glow: '#E87D0D',
      particle: '#E87D0D90',
    },
    animation: defaultAnimation,
  },
};

// Media/Entertainment Theme Presets
export const mediaThemes: Record<string, ThemeProfile> = {
  netflix: {
    id: 'netflix',
    name: 'Netflix',
    category: 'media' as ProfileCategory,
    profile: 'netflix',
    palette: {
      primary: '#E50914',
      secondary: '#141414',
      accent: '#831010',
      background: {
        primary: '#000000',
        secondary: '#141414',
        tertiary: '#2F2F2F',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#E5E5E5',
        muted: '#808080',
      },
      border: '#E50914',
      success: '#46D369',
      warning: '#FFB800',
      error: '#E50914',
      info: '#0080FF',
      glow: '#E50914',
      particle: '#E5091490',
    },
    animation: { ...defaultAnimation, duration: 'slow', enableParticles: false },
  },
  spotify: {
    id: 'spotify',
    name: 'Spotify',
    category: 'media' as ProfileCategory,
    profile: 'spotify',
    palette: {
      primary: '#1DB954',
      secondary: '#191414',
      accent: '#1ED760',
      background: {
        primary: '#000000',
        secondary: '#121212',
        tertiary: '#282828',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#B3B3B3',
        muted: '#535353',
      },
      border: '#1DB954',
      success: '#1DB954',
      warning: '#FFA500',
      error: '#E22134',
      info: '#509BF5',
      glow: '#1DB954',
      particle: '#1DB95490',
    },
    animation: { ...defaultAnimation, enableGlow: true },
  },
};

// General/Default Theme Presets
export const generalThemes: Record<string, ThemeProfile> = {
  cosmic: {
    id: 'cosmic',
    name: 'Cosmic',
    category: 'general' as ProfileCategory,
    profile: 'general',
    palette: {
      primary: '#6366F1',
      secondary: '#1E1B4B',
      accent: '#A78BFA',
      background: {
        primary: '#0F172A',
        secondary: '#1E293B',
        tertiary: '#334155',
      },
      text: {
        primary: '#F1F5F9',
        secondary: '#CBD5E1',
        muted: '#64748B',
      },
      border: '#6366F1',
      success: '#10B981',
      warning: '#F59E0B',
      error: '#EF4444',
      info: '#3B82F6',
      glow: '#6366F1',
      particle: '#6366F190',
    },
    animation: defaultAnimation,
  },
  cyberpunk: {
    id: 'cyberpunk',
    name: 'Cyberpunk',
    category: 'general' as ProfileCategory,
    profile: 'general',
    palette: {
      primary: '#00F0FF',
      secondary: '#0A0E27',
      accent: '#FF2A6D',
      background: {
        primary: '#050A1E',
        secondary: '#0A0E27',
        tertiary: '#1A1F3A',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#D4E5FF',
        muted: '#6B7A99',
      },
      border: '#00F0FF',
      success: '#00FF9F',
      warning: '#FFD700',
      error: '#FF2A6D',
      info: '#00F0FF',
      glow: '#00F0FF',
      particle: '#00F0FF90',
    },
    animation: { ...defaultAnimation, enableGlow: true, enableParticles: true },
  },
  minimal: {
    id: 'minimal',
    name: 'Minimal',
    category: 'general' as ProfileCategory,
    profile: 'general',
    palette: {
      primary: '#2563EB',
      secondary: '#F8FAFC',
      accent: '#7C3AED',
      background: {
        primary: '#FFFFFF',
        secondary: '#F8FAFC',
        tertiary: '#F1F5F9',
      },
      text: {
        primary: '#0F172A',
        secondary: '#334155',
        muted: '#94A3B8',
      },
      border: '#E2E8F0',
      success: '#059669',
      warning: '#D97706',
      error: '#DC2626',
      info: '#0284C7',
      glow: '#2563EB',
      particle: '#2563EB20',
    },
    animation: { ...defaultAnimation, enableGlow: false, enableParticles: false, duration: 'fast' },
  },
  'high-performance': {
    id: 'high-performance',
    name: 'High Performance',
    category: 'general' as ProfileCategory,
    profile: 'general',
    palette: {
      primary: '#FF6B00',
      secondary: '#1A1A1A',
      accent: '#FFD700',
      background: {
        primary: '#0A0A0A',
        secondary: '#1A1A1A',
        tertiary: '#2A2A2A',
      },
      text: {
        primary: '#FFFFFF',
        secondary: '#E0E0E0',
        muted: '#909090',
      },
      border: '#FF6B00',
      success: '#00FF00',
      warning: '#FFD700',
      error: '#FF0000',
      info: '#00BFFF',
      glow: '#FF6B00',
      particle: '#FF6B0090',
    },
    animation: defaultAnimation,
  },
};

// Export all themes
export const allThemePresets: Record<string, ThemeProfile> = {
  ...gamingThemes,
  ...workingThemes,
  ...mediaThemes,
  ...generalThemes,
};

// Helper to get theme by profile
export function getThemeByProfile(profile: string): ThemeProfile | undefined {
  return allThemePresets[profile];
}

// Helper to get themes by category
export function getThemesByCategory(category: ProfileCategory): ThemeProfile[] {
  return Object.values(allThemePresets).filter(theme => theme.category === category);
}
