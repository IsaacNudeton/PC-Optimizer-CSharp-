# PC Optimizer - Modern UI/UX System

## 🎨 Overview

This is a complete UI/UX overhaul for the PC Optimizer application, featuring:

- **Dynamic Profile-Based Theming**: Themes automatically change based on active applications (games, work apps, media players)
- **Glassmorphism Design**: Modern glass-effect cards with blur and transparency
- **Smooth Animations**: Framer Motion animations throughout the interface
- **Advanced Visualizations**: Circular gauges, radar charts, progress indicators
- **Customizable Themes**: User-configurable themes with per-profile and per-category settings
- **Haptic & Audio Feedback**: Optional tactile and sound feedback for interactions

## 🚀 Quick Start

### Development Server

```powershell
cd PCOptimizer-Frontend
npm install
npm run dev
```

Open http://localhost:5173 to view the app.

### Electron App

```powershell
npm run electron-dev
```

### Production Build

```powershell
npm run build
npm run electron-build
```

## 🎯 Key Features

### 1. Profile-Based Dynamic Theming

The app automatically detects what application you're running and switches themes:

**Gaming Profiles:**
- Valorant (Red/Dark theme)
- GTA V (Green/Orange theme)
- Apex Legends (Red/Orange theme)
- CS:GO (Yellow/Blue theme)

**Work Profiles:**
- VS Code (Blue/Dark theme)
- Adobe Premiere (Purple theme)
- DaVinci Resolve (Red theme)
- Blender (Orange theme)

**Media Profiles:**
- Netflix (Red/Black theme)
- Spotify (Green/Black theme)

**General Themes:**
- Cosmic (Purple/Space theme)
- Cyberpunk (Cyan/Pink theme)
- Minimal (Clean light theme)
- High Performance (Orange/Black theme)

### 2. Theme Customization

Users can:
- Set a global theme for the entire app
- Set category themes (gaming, working, media, general)
- Set individual profile themes
- Enable/disable dynamic theme switching
- Configure animation preferences
- Enable haptic and audio feedback
- Adjust font size and font family
- Import/export custom themes

### 3. Modern Components

#### Cards
- `GlassCard`: Glassmorphic container with variants (default, bordered, elevated, glow)
- `MetricCard`: Display metrics with trends and icons
- `SectionCard`: Sectioned content with headers and actions

#### Visualizations
- `CircularGauge`: Animated circular progress indicator
- `LinearProgress`: Horizontal progress bar with glow effect
- `StatRing`: Compact ring gauge for stats
- `PulseIndicator`: Animated status indicator
- `PerformanceRadar`: Multi-axis radar chart for performance metrics
- `MultiSeriesRadar`: Compare multiple datasets on radar chart

### 4. Animation System

All animations are configurable per theme:
- Duration: fast (150ms), normal (300ms), slow (500ms)
- Easing: linear, ease, easeIn, easeOut, easeInOut, spring
- Toggle particles, glow effects, and transitions
- Reduced motion support for accessibility

## 📁 Project Structure

```
PCOptimizer-Frontend/
├── src/
│   ├── components/
│   │   ├── dashboard/
│   │   │   └── Cards.tsx              # Glass cards and metric cards
│   │   ├── visualizations/
│   │   │   ├── Gauges.tsx             # Circular/linear gauges
│   │   │   └── RadarChart.tsx         # Performance radar charts
│   │   └── ThemeCustomizer.tsx        # Theme customization UI
│   ├── contexts/
│   │   └── EnhancedThemeContext.tsx   # Theme provider with MUI integration
│   ├── hooks/
│   │   └── useProfileDetection.ts     # Profile detection hook
│   ├── pages/
│   │   ├── EnhancedDashboard.tsx      # Modern dashboard page
│   │   └── EnhancedSettings.tsx       # Settings with theme customizer
│   ├── stores/
│   │   └── themeStore.ts              # Zustand store for theme state
│   ├── theme/
│   │   └── presets.ts                 # All theme presets
│   ├── types/
│   │   └── theme.ts                   # TypeScript type definitions
│   ├── utils/
│   │   └── theme.ts                   # Theme utilities and helpers
│   ├── App.tsx                        # Main app with providers
│   ├── index.css                      # Global styles with CSS variables
│   └── main.tsx                       # Entry point
├── tailwind.config.js                 # Tailwind configuration
└── package.json
```

## 🎨 Theme System Architecture

### Theme Store (Zustand)

State management for:
- Global theme (applied to entire app)
- Profile themes (per-application themes)
- Category themes (gaming/working/media/general defaults)
- Active profile detection
- User preferences (haptics, audio, fonts)

### Theme Context (React Context + MUI)

Provides:
- Current theme object
- MUI theme (auto-generated from current theme)
- Feedback functions (triggerFeedback, playTone)
- CSS variable application

### Theme Presets

Pre-configured themes in `src/theme/presets.ts`:
- Color palettes
- Animation settings
- Category associations
- Custom CSS (optional)

## 🔧 Customization Guide

### Creating a Custom Theme

```typescript
import { ThemeProfile } from './types/theme';

const myTheme: ThemeProfile = {
  id: 'my-theme',
  name: 'My Custom Theme',
  category: 'general',
  profile: 'general',
  palette: {
    primary: '#FF6B6B',
    secondary: '#1A1A2E',
    accent: '#FFD93D',
    background: {
      primary: '#0F0F0F',
      secondary: '#1A1A2E',
      tertiary: '#2A2A3E',
    },
    text: {
      primary: '#FFFFFF',
      secondary: '#E0E0E0',
      muted: '#888888',
    },
    border: '#FF6B6B',
    success: '#4ECDC4',
    warning: '#FFD93D',
    error: '#FF6B6B',
    info: '#6BCF7D',
    glow: '#FF6B6B',
    particle: 'rgba(255, 107, 107, 0.5)',
  },
  animation: {
    duration: 'normal',
    easing: 'easeInOut',
    enableParticles: true,
    enableGlow: true,
    enableTransitions: true,
    reducedMotion: false,
  },
};

// Import in settings
useThemeStore().importTheme(myTheme);
```

### Adding New Profile Detection

Edit `src/hooks/useProfileDetection.ts`:

```typescript
const detectProfile = async () => {
  // Call your C# backend API
  const response = await fetch('http://localhost:5000/api/system/active-profile');
  const data = await response.json();
  
  const profile: ProfileDetectionData = {
    category: data.category,
    profile: data.profileId,
    appName: data.appName,
    icon: data.icon,
  };
  
  setCurrentProfile(profile);
  setActiveProfile(profile);
};
```

### Styling with Tailwind + CSS Variables

The theme system uses CSS variables that update automatically:

```tsx
// Use CSS variables in Tailwind classes
<div className="bg-[var(--color-bg-primary)] text-[var(--color-text-primary)]">
  Content
</div>

// Or use the Tailwind aliases
<div className="bg-bg-primary text-text-primary border-primary">
  Content
</div>
```

## 🔗 Backend Integration

### Required API Endpoints

1. **Profile Detection**
   - `GET /api/system/active-profile`
   - Returns: `{ category, profileId, appName, icon?, pid? }`

2. **System Metrics**
   - `GET /api/system/metrics`
   - Returns: `{ cpu, gpu, ram, disk, temp, fps }`

3. **Optimization Status**
   - `GET /api/optimization/status`
   - Returns: `{ activeOptimizations: [...] }`

4. **Quick Actions**
   - `POST /api/optimization/quick-optimize`
   - `POST /api/optimization/deep-clean`
   - `POST /api/optimization/clear-cache`

## 🎯 Next Steps

1. **Connect Real Data**: Replace mock data with actual API calls
2. **Add More Profiles**: Expand gaming/work/media profile presets
3. **Custom Icon Upload**: Implement icon upload for custom themes
4. **Theme Marketplace**: Share and download community themes
5. **Performance Profiles**: Auto-apply optimization profiles per game
6. **Keyboard Shortcuts**: Add hotkeys for quick actions
7. **System Tray**: Minimize to tray with quick menu
8. **Analytics**: Track optimization results over time

## 📚 Dependencies

### Core UI
- `react` & `react-dom`: UI framework
- `framer-motion`: Animations
- `@mui/material`: Material-UI components
- `lucide-react`: Icon library
- `react-hot-toast`: Toast notifications

### State & Data
- `zustand`: State management
- `@tanstack/react-query`: Data fetching
- `axios`: HTTP client

### Styling
- `tailwindcss`: Utility-first CSS
- `clsx` & `tailwind-merge`: Class merging
- `autoprefixer`: CSS vendor prefixes

### Dialog & UI Primitives
- `@radix-ui/react-*`: Accessible UI primitives

### Charts
- `recharts`: Data visualization

## 🤝 Contributing

When adding new features:

1. Follow the existing component structure
2. Use TypeScript for type safety
3. Add animations with Framer Motion
4. Use Tailwind classes + CSS variables
5. Ensure accessibility (ARIA labels, keyboard navigation)
6. Test with different themes

## 📝 License

This project is part of the PC Optimizer suite.

---

**Built with ❤️ for gamers, developers, and power users**
