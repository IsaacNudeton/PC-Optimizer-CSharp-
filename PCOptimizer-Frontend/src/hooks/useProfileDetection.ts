import { useEffect, useState } from 'react';
import { useThemeStore } from '../stores/themeStore';
import { ProfileDetectionData, ProfileCategory } from '../types/theme';

// Mock profile detection - In production, this would connect to your backend
export function useProfileDetection() {
  const [currentProfile, setCurrentProfile] = useState<ProfileDetectionData | null>(null);
  const { setActiveProfile, enableDynamicTheming } = useThemeStore();

  useEffect(() => {
    if (!enableDynamicTheming) return;

    // Simulate detecting active application
    // In production, this would call your C# backend API
    const detectProfile = async () => {
      try {
        // Example: Call backend endpoint
        // const response = await fetch('/api/system/active-profile');
        // const data = await response.json();
        
        // Mock data for demonstration
        const mockProfile: ProfileDetectionData = {
          category: 'gaming' as ProfileCategory,
          profile: 'valorant',
          appName: 'VALORANT',
          icon: '🎯',
        };

        setCurrentProfile(mockProfile);
        setActiveProfile(mockProfile);
      } catch (error) {
        console.error('Failed to detect profile:', error);
      }
    };

    // Detect on mount
    detectProfile();

    // Poll for changes every 5 seconds
    const interval = setInterval(detectProfile, 5000);

    return () => clearInterval(interval);
  }, [enableDynamicTheming, setActiveProfile]);

  return currentProfile;
}

// Hook to manually set profile
export function useSetProfile() {
  const { setActiveProfile } = useThemeStore();

  const setProfile = (profile: ProfileDetectionData | null) => {
    setActiveProfile(profile);
  };

  return setProfile;
}

// Hook to get available profile presets
export function useProfilePresets() {
  return {
    gaming: [
      { id: 'valorant', name: 'Valorant', icon: '🎯' },
      { id: 'gta', name: 'GTA V', icon: '🚗' },
      { id: 'apex', name: 'Apex Legends', icon: '⚔️' },
      { id: 'csgo', name: 'CS:GO', icon: '🔫' },
    ],
    working: [
      { id: 'vscode', name: 'VS Code', icon: '💻' },
      { id: 'adobe-premiere', name: 'Premiere Pro', icon: '🎬' },
      { id: 'davinci-resolve', name: 'DaVinci Resolve', icon: '🎞️' },
      { id: 'blender', name: 'Blender', icon: '🎲' },
    ],
    media: [
      { id: 'netflix', name: 'Netflix', icon: '📺' },
      { id: 'spotify', name: 'Spotify', icon: '🎵' },
    ],
    general: [
      { id: 'cosmic', name: 'Cosmic', icon: '🌌' },
      { id: 'cyberpunk', name: 'Cyberpunk', icon: '🔮' },
      { id: 'minimal', name: 'Minimal', icon: '⚪' },
      { id: 'high-performance', name: 'High Performance', icon: '⚡' },
    ],
  };
}
