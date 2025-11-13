import { createTheme, ThemeOptions } from '@mui/material/styles';

// CosmicUI Color Palette - Updated for Stitch Design
const cosmicColorPalettes = {
  universal: {
    deepBlack: '#1a1a1a',
    darkGray: '#2d2d2d',
    lightCharcoal: '#3a3a3a',
    darkMatter: '#262626',
    primaryText: '#FFFFFF',
    secondaryText: '#b0b0b0',
    tertiaryText: '#808080',
    accentBright: '#FFB800',
    accentMedium: '#FFA500',
    accentDark: '#FF9500',
    electricBlue: '#00BCD4',
    success: '#4CAF50',
    warning: '#FF9800',
    danger: '#F44336',
  },
  gaming: {
    deepBlack: '#0A0A0A',
    darkGray: '#1A1A1A',
    lightCharcoal: '#2A2A2A',
    darkMatter: '#262626',
    primaryText: '#FFFFFF',
    secondaryText: '#B3B3B3',
    tertiaryText: '#808080',
    accentBright: '#FF0000',
    accentMedium: '#DC143C',
    accentDark: '#FF4500',
    electricBlue: '#FF6347',
    success: '#00D4FF',
    warning: '#FFD700',
    danger: '#FF0000',
  },
  work: {
    deepBlack: '#FFFFFF',
    darkGray: '#F0F4F8',
    lightCharcoal: '#E8EEF5',
    darkMatter: '#DFE7F0',
    primaryText: '#000000',
    secondaryText: '#666666',
    tertiaryText: '#999999',
    accentBright: '#1976D2',
    accentMedium: '#1565C0',
    accentDark: '#0D47A1',
    electricBlue: '#00B0FF',
    success: '#4CAF50',
    warning: '#FF9800',
    danger: '#F44336',
  },
};

const accentOverlays = {
  default: {},
  pink: {
    accentBright: '#FF69B4',
    accentMedium: '#FFB6D9',
    accentDark: '#FF1493',
  },
  purple: {
    accentBright: '#9370DB',
    accentMedium: '#BA55D3',
    accentDark: '#8A2BE2',
  },
  blue: {
    accentBright: '#00BFFF',
    accentMedium: '#1E90FF',
    accentDark: '#0047AB',
  },
};

type ThemeProfile = 'Universal' | 'Gaming' | 'Work';
type AccentType = 'Default' | 'Pink' | 'Purple' | 'Blue';

export const createCosmicTheme = (profile: ThemeProfile = 'Universal', accent: AccentType = 'Default') => {
  const baseKey = profile.toLowerCase() as keyof typeof cosmicColorPalettes;
  const accentKey = accent.toLowerCase() as keyof typeof accentOverlays;

  const basePalette = { ...cosmicColorPalettes[baseKey] };
  const accentOverride = accentOverlays[accentKey] || {};

  const colors = { ...basePalette, ...accentOverride };

  const themeOptions: ThemeOptions = {
    palette: {
      mode: 'dark',
      background: {
        default: colors.deepBlack,
        paper: colors.darkGray,
      },
      primary: {
        main: colors.accentBright,
        light: colors.accentMedium,
        dark: colors.accentDark,
      },
      secondary: {
        main: colors.accentMedium,
        light: colors.accentBright,
        dark: colors.accentDark,
      },
      success: {
        main: colors.success,
      },
      warning: {
        main: colors.warning,
      },
      error: {
        main: colors.danger,
      },
      text: {
        primary: colors.primaryText,
        secondary: colors.secondaryText,
        disabled: colors.tertiaryText,
      },
    },
    typography: {
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      h1: {
        fontSize: '48px',
        fontWeight: 900,
        color: colors.primaryText,
      },
      h2: {
        fontSize: '32px',
        fontWeight: 700,
        color: colors.accentBright,
      },
      h3: {
        fontSize: '24px',
        fontWeight: 600,
        color: colors.accentMedium,
      },
      h4: {
        fontSize: '18px',
        fontWeight: 600,
        color: colors.accentMedium,
      },
      body1: {
        fontSize: '16px',
        fontWeight: 400,
        color: colors.primaryText,
      },
      body2: {
        fontSize: '14px',
        fontWeight: 400,
        color: colors.secondaryText,
      },
      caption: {
        fontSize: '12px',
        fontWeight: 400,
        color: colors.tertiaryText,
      },
    },
    components: {
      MuiButton: {
        styleOverrides: {
          root: {
            borderRadius: '8px',
            textTransform: 'none',
            fontWeight: 600,
            padding: '10px 16px',
          },
          contained: {
            backgroundColor: colors.accentBright,
            color: colors.deepBlack,
            '&:hover': {
              backgroundColor: colors.accentDark,
            },
            '&:disabled': {
              backgroundColor: colors.tertiaryText,
              color: colors.deepBlack,
            },
          },
          outlined: {
            borderColor: colors.accentBright,
            color: colors.accentBright,
            '&:hover': {
              backgroundColor: colors.lightCharcoal,
              borderColor: colors.accentMedium,
            },
          },
        },
      },
      MuiCard: {
        styleOverrides: {
          root: {
            backgroundColor: colors.darkGray,
            borderRadius: '12px',
            border: `1px solid #444`,
            padding: '16px',
          },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            backgroundColor: colors.darkGray,
            borderRadius: '12px',
          },
        },
      },
      MuiAppBar: {
        styleOverrides: {
          root: {
            backgroundColor: colors.darkGray,
            borderBottom: `1px solid #444`,
          },
        },
      },
      MuiDrawer: {
        styleOverrides: {
          paper: {
            backgroundColor: colors.darkGray,
            borderRight: `1px solid #444`,
          },
        },
      },
      MuiRadio: {
        styleOverrides: {
          root: {
            color: colors.accentBright,
          },
        },
      },
      MuiCheckbox: {
        styleOverrides: {
          root: {
            color: colors.accentBright,
          },
        },
      },
      MuiSelect: {
        styleOverrides: {
          root: {
            '&:hover': {
              backgroundColor: colors.lightCharcoal,
            },
          },
        },
      },
      MuiTableCell: {
        styleOverrides: {
          root: {
            borderColor: colors.lightCharcoal,
          },
        },
      },
      MuiChip: {
        styleOverrides: {
          root: {
            backgroundColor: colors.lightCharcoal,
            color: colors.primaryText,
          },
          outlined: {
            borderColor: colors.accentBright,
            color: colors.accentBright,
          },
        },
      },
    },
  };

  return createTheme(themeOptions);
};
