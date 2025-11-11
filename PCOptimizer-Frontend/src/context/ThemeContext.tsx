import React, { createContext, useContext, useState, useCallback, useEffect } from 'react';
import { createCosmicTheme } from '../theme/cosmicTheme';

type ThemeProfile = 'Universal' | 'Gaming' | 'Work';
type AccentType = 'Default' | 'Pink' | 'Purple' | 'Blue';

interface ThemeContextType {
  profile: ThemeProfile;
  accent: AccentType;
  muiTheme: ReturnType<typeof createCosmicTheme>;
  setProfile: (profile: ThemeProfile) => void;
  setAccent: (accent: AccentType) => void;
}

const ThemeContext = createContext<ThemeContextType | undefined>(undefined);

export const ThemeProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [profile, setProfileState] = useState<ThemeProfile>('Universal');
  const [accent, setAccentState] = useState<AccentType>('Default');

  // Load from localStorage on mount
  useEffect(() => {
    const savedProfile = (localStorage.getItem('cosmicTheme_profile') as ThemeProfile) || 'Universal';
    const savedAccent = (localStorage.getItem('cosmicTheme_accent') as AccentType) || 'Default';
    setProfileState(savedProfile);
    setAccentState(savedAccent);
  }, []);

  const muiTheme = createCosmicTheme(profile, accent);

  const handleSetProfile = useCallback((newProfile: ThemeProfile) => {
    setProfileState(newProfile);
    localStorage.setItem('cosmicTheme_profile', newProfile);
  }, []);

  const handleSetAccent = useCallback((newAccent: AccentType) => {
    setAccentState(newAccent);
    localStorage.setItem('cosmicTheme_accent', newAccent);
  }, []);

  return (
    <ThemeContext.Provider
      value={{
        profile,
        accent,
        muiTheme,
        setProfile: handleSetProfile,
        setAccent: handleSetAccent,
      }}
    >
      {children}
    </ThemeContext.Provider>
  );
};

export const useCosmicTheme = (): ThemeContextType => {
  const context = useContext(ThemeContext);
  if (!context) {
    throw new Error('useCosmicTheme must be used within ThemeProvider');
  }
  return context;
};
