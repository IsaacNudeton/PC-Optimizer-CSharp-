import { ThemeProfile, AnimationPreset } from '../types/theme';
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

// Tailwind merge utility
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Apply theme to CSS variables
export function applyThemeToCSSVariables(theme: ThemeProfile): void {
  const root = document.documentElement;

  // Colors
  root.style.setProperty('--color-primary', theme.palette.primary);
  root.style.setProperty('--color-secondary', theme.palette.secondary);
  root.style.setProperty('--color-accent', theme.palette.accent);

  root.style.setProperty('--color-bg-primary', theme.palette.background.primary);
  root.style.setProperty('--color-bg-secondary', theme.palette.background.secondary);
  root.style.setProperty('--color-bg-tertiary', theme.palette.background.tertiary);

  root.style.setProperty('--color-text-primary', theme.palette.text.primary);
  root.style.setProperty('--color-text-secondary', theme.palette.text.secondary);
  root.style.setProperty('--color-text-muted', theme.palette.text.muted);

  root.style.setProperty('--color-border', theme.palette.border);
  root.style.setProperty('--color-success', theme.palette.success);
  root.style.setProperty('--color-warning', theme.palette.warning);
  root.style.setProperty('--color-error', theme.palette.error);
  root.style.setProperty('--color-info', theme.palette.info);

  root.style.setProperty('--color-glow', theme.palette.glow);
  root.style.setProperty('--color-particle', theme.palette.particle);

  // Animation settings
  const durations = { fast: '150ms', normal: '300ms', slow: '500ms' };
  root.style.setProperty('--animation-duration', durations[theme.animation.duration]);
  root.style.setProperty('--animation-easing', theme.animation.easing);

  // Apply custom CSS if provided
  if (theme.customCSS) {
    let styleElement = document.getElementById('custom-theme-css');
    if (!styleElement) {
      styleElement = document.createElement('style');
      styleElement.id = 'custom-theme-css';
      document.head.appendChild(styleElement);
    }
    styleElement.textContent = theme.customCSS;
  }
}

// Animation duration helpers
export function getAnimationDuration(preset: AnimationPreset): number {
  const durations = { fast: 150, normal: 300, slow: 500 };
  return durations[preset.duration];
}

// Framer Motion variants for common animations
export const fadeInVariants = {
  hidden: { opacity: 0 },
  visible: { opacity: 1 },
};

export const slideUpVariants = {
  hidden: { opacity: 0, y: 20 },
  visible: { opacity: 1, y: 0 },
};

export const slideInVariants = {
  hidden: { opacity: 0, x: -20 },
  visible: { opacity: 1, x: 0 },
};

export const scaleVariants = {
  hidden: { opacity: 0, scale: 0.8 },
  visible: { opacity: 1, scale: 1 },
};

export const glowVariants = {
  initial: { boxShadow: '0 0 0px rgba(var(--color-glow), 0)' },
  hover: { boxShadow: '0 0 20px rgba(var(--color-glow), 0.6)' },
};

// Generate CSS for glassmorphism effect
export function getGlassEffect(opacity: number = 0.1): string {
  return cn(
    'backdrop-blur-xl',
    'bg-white/[' + opacity + ']',
    'border border-white/10',
    'shadow-lg'
  );
}

// Vibration/Haptic feedback
export function triggerHaptic(pattern: 'light' | 'medium' | 'heavy' = 'light'): void {
  if ('vibrate' in navigator) {
    const patterns = {
      light: 10,
      medium: 20,
      heavy: 50,
    };
    navigator.vibrate(patterns[pattern]);
  }
}

// Audio feedback
let audioContext: AudioContext | null = null;

export function playSound(
  frequency: number = 440,
  duration: number = 100,
  type: OscillatorType = 'sine'
): void {
  if (!audioContext) {
    audioContext = new (window.AudioContext || (window as any).webkitAudioContext)();
  }

  const oscillator = audioContext.createOscillator();
  const gainNode = audioContext.createGain();

  oscillator.connect(gainNode);
  gainNode.connect(audioContext.destination);

  oscillator.frequency.value = frequency;
  oscillator.type = type;

  gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
  gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + duration / 1000);

  oscillator.start(audioContext.currentTime);
  oscillator.stop(audioContext.currentTime + duration / 1000);
}

// Icon/Image utilities
export function getGameIcon(gameName: string): string {
  const icons: Record<string, string> = {
    valorant: '🎯',
    gta: '🚗',
    apex: '⚔️',
    csgo: '🔫',
    fortnite: '🏰',
    overwatch: '🎮',
    leagueoflegends: '⚡',
    minecraft: '🧱',
  };
  return icons[gameName.toLowerCase()] || '🎮';
}

export function getWorkIcon(appName: string): string {
  const icons: Record<string, string> = {
    vscode: '💻',
    'visual-studio': '🔧',
    'adobe-premiere': '🎬',
    'adobe-photoshop': '🎨',
    'davinci-resolve': '🎞️',
    blender: '🎲',
    unity: '🎯',
    'unreal-engine': '🎮',
  };
  return icons[appName.toLowerCase()] || '💼';
}

export function getMediaIcon(appName: string): string {
  const icons: Record<string, string> = {
    netflix: '📺',
    youtube: '▶️',
    spotify: '🎵',
    vlc: '🎬',
  };
  return icons[appName.toLowerCase()] || '🎵';
}

// Hex to RGB converter for CSS variables
export function hexToRgb(hex: string): string {
  const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
  return result
    ? `${parseInt(result[1], 16)}, ${parseInt(result[2], 16)}, ${parseInt(result[3], 16)}`
    : '0, 0, 0';
}

// Format theme for export
export function exportThemeToJSON(theme: ThemeProfile): string {
  return JSON.stringify(theme, null, 2);
}

// Import theme from JSON
export function importThemeFromJSON(json: string): ThemeProfile | null {
  try {
    const theme = JSON.parse(json);
    // Basic validation
    if (theme.id && theme.name && theme.palette) {
      return theme as ThemeProfile;
    }
    return null;
  } catch {
    return null;
  }
}
