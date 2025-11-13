import React, { createContext, useContext, useEffect, ReactNode } from 'react';
import { createTheme, Theme as MuiTheme } from '@mui/material/styles';
import { Toaster } from 'react-hot-toast';
import { useThemeStore } from '../stores/themeStore';
import { applyThemeToCSSVariables, triggerHaptic, playSound } from '../utils/theme';
import { ThemeProfile } from '../types/theme';

interface EnhancedThemeContextValue {
  theme: ThemeProfile;
  muiTheme: MuiTheme;
  triggerFeedback: (type?: 'light' | 'medium' | 'heavy') => void;
  playTone: (frequency?: number) => void;
}

const EnhancedThemeContext = createContext<EnhancedThemeContextValue | undefined>(undefined);

export function useEnhancedTheme() {
  const context = useContext(EnhancedThemeContext);
  if (!context) {
    throw new Error('useEnhancedTheme must be used within EnhancedThemeProvider');
  }
  return context;
}

interface EnhancedThemeProviderProps {
  children: ReactNode;
}

export function EnhancedThemeProvider({ children }: EnhancedThemeProviderProps) {
  const {
    getCurrentTheme,
    mode,
    enableHaptics,
    enableAudio,
    fontFamily,
    fontSize,
  } = useThemeStore();

  const theme = getCurrentTheme();

  // Apply theme to CSS variables whenever theme changes
  useEffect(() => {
    applyThemeToCSSVariables(theme);
  }, [theme]);

  // Create Material-UI theme from current theme
  const muiTheme = React.useMemo(
    () =>
      createTheme({
        palette: {
          mode: mode === 'auto' ? 'dark' : mode,
          primary: {
            main: theme.palette.primary,
          },
          secondary: {
            main: theme.palette.secondary,
          },
          background: {
            default: theme.palette.background.primary,
            paper: theme.palette.background.secondary,
          },
          text: {
            primary: theme.palette.text.primary,
            secondary: theme.palette.text.secondary,
          },
          error: {
            main: theme.palette.error,
          },
          warning: {
            main: theme.palette.warning,
          },
          info: {
            main: theme.palette.info,
          },
          success: {
            main: theme.palette.success,
          },
        },
        typography: {
          fontFamily: fontFamily,
          fontSize: fontSize === 'small' ? 12 : fontSize === 'large' ? 16 : 14,
        },
        shape: {
          borderRadius: 12,
        },
        components: {
          MuiButton: {
            styleOverrides: {
              root: {
                textTransform: 'none',
                fontWeight: 600,
                transition: 'all 0.3s ease',
                '&:hover': {
                  transform: 'translateY(-2px)',
                  boxShadow: `0 4px 12px ${theme.palette.primary}40`,
                },
              },
            },
          },
          MuiCard: {
            styleOverrides: {
              root: {
                backgroundImage: 'none',
                backdropFilter: 'blur(20px)',
                backgroundColor: `${theme.palette.background.secondary}cc`,
                border: `1px solid ${theme.palette.border}20`,
                transition: 'all 0.3s ease',
                '&:hover': {
                  borderColor: `${theme.palette.border}60`,
                  boxShadow: `0 8px 24px ${theme.palette.glow}20`,
                },
              },
            },
          },
          MuiPaper: {
            styleOverrides: {
              root: {
                backgroundImage: 'none',
                backgroundColor: theme.palette.background.secondary,
              },
            },
          },
        },
      }),
    [theme, mode, fontFamily, fontSize]
  );

  const triggerFeedback = (type: 'light' | 'medium' | 'heavy' = 'light') => {
    if (enableHaptics) {
      triggerHaptic(type);
    }
  };

  const playTone = (frequency: number = 440) => {
    if (enableAudio) {
      playSound(frequency, 100, 'sine');
    }
  };

  const value: EnhancedThemeContextValue = {
    theme,
    muiTheme,
    triggerFeedback,
    playTone,
  };

  return (
    <EnhancedThemeContext.Provider value={value}>
      {children}
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 3000,
          style: {
            background: theme.palette.background.secondary,
            color: theme.palette.text.primary,
            border: `1px solid ${theme.palette.border}40`,
            borderRadius: '12px',
            backdropFilter: 'blur(20px)',
          },
          success: {
            iconTheme: {
              primary: theme.palette.success,
              secondary: theme.palette.background.secondary,
            },
          },
          error: {
            iconTheme: {
              primary: theme.palette.error,
              secondary: theme.palette.background.secondary,
            },
          },
        }}
      />
    </EnhancedThemeContext.Provider>
  );
}

// Export for backward compatibility
export { useEnhancedTheme as useCosmicTheme };
