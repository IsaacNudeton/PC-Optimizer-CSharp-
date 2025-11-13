// Theme type definitions for the PC Optimizer

export type ProfileCategory = 'gaming' | 'working' | 'media' | 'general';

export type GameProfile = 
  | 'valorant' 
  | 'gta' 
  | 'apex' 
  | 'fortnite' 
  | 'csgo' 
  | 'overwatch'
  | 'leagueoflegends'
  | 'minecraft'
  | 'custom';

export type WorkProfile = 
  | 'vscode' 
  | 'visual-studio' 
  | 'adobe-premiere' 
  | 'adobe-photoshop'
  | 'davinci-resolve'
  | 'blender'
  | 'unity'
  | 'unreal-engine'
  | 'custom';

export type MediaProfile =
  | 'netflix'
  | 'youtube'
  | 'spotify'
  | 'vlc'
  | 'custom';

export interface ThemePalette {
  primary: string;
  secondary: string;
  accent: string;
  background: {
    primary: string;
    secondary: string;
    tertiary: string;
  };
  text: {
    primary: string;
    secondary: string;
    muted: string;
  };
  border: string;
  success: string;
  warning: string;
  error: string;
  info: string;
  glow: string; // For neon effects
  particle: string; // For particle effects
}

export interface AnimationPreset {
  duration: 'fast' | 'normal' | 'slow';
  easing: 'linear' | 'ease' | 'easeIn' | 'easeOut' | 'easeInOut' | 'spring';
  enableParticles: boolean;
  enableGlow: boolean;
  enableTransitions: boolean;
  reducedMotion: boolean;
}

export interface ThemeProfile {
  id: string;
  name: string;
  category: ProfileCategory;
  profile: GameProfile | WorkProfile | MediaProfile | 'general';
  palette: ThemePalette;
  icon?: string; // URL or data URI for custom icon
  animation: AnimationPreset;
  customCSS?: string;
}

export interface ThemeSettings {
  mode: 'light' | 'dark' | 'auto';
  globalTheme: ThemeProfile | null; // Applied to entire app if set
  profileThemes: Record<string, ThemeProfile>; // Per-profile overrides
  categoryThemes: Record<ProfileCategory, ThemeProfile>; // Per-category defaults
  activeProfile: string | null; // Currently active profile
  enableDynamicTheming: boolean; // Auto-switch themes based on active app
  enableHaptics: boolean;
  enableAudio: boolean;
  fontFamily: string;
  fontSize: 'small' | 'medium' | 'large';
}

export interface ProfileDetectionData {
  category: ProfileCategory;
  profile: string;
  appName: string;
  icon?: string;
  pid?: number;
}
