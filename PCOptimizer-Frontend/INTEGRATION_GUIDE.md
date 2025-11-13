# Integration Guide for Enhanced UI

## Quick Integration Steps

### Step 1: Route Updates

Update `src/App.tsx` to use the new enhanced pages:

```tsx
// Replace Dashboard import
import Dashboard from './pages/Dashboard';
// With
import Dashboard from './pages/EnhancedDashboard';

// Replace Settings import  
import Settings from './pages/Settings';
// With
import Settings from './pages/EnhancedSettings';
```

Or add new routes:

```tsx
<Route path="/enhanced" element={<MainLayout><EnhancedDashboard /></MainLayout>} />
<Route path="/enhanced-settings" element={<MainLayout><EnhancedSettings /></MainLayout>} />
```

### Step 2: Test the Theme System

1. Start the dev server: `npm run dev`
2. Navigate to Settings
3. Try different theme presets:
   - Global themes (apply to entire app)
   - Category themes (gaming, working, media)
   - Toggle dynamic theming
4. Enable haptics/audio feedback
5. Test animations and transitions

### Step 3: Backend Connection

Update `src/hooks/useProfileDetection.ts` to connect to your C# backend:

```typescript
const detectProfile = async () => {
  try {
    const response = await fetch('http://localhost:5000/api/system/active-profile');
    const data = await response.json();
    
    const profile: ProfileDetectionData = {
      category: data.category as ProfileCategory,
      profile: data.profileId,
      appName: data.appName,
      icon: data.icon,
      pid: data.pid,
    };
    
    setCurrentProfile(profile);
    setActiveProfile(profile);
  } catch (error) {
    console.error('Failed to detect profile:', error);
  }
};
```

### Step 4: Add Real Data

Replace mock data in `EnhancedDashboard.tsx`:

```typescript
// Instead of mock data
const systemMetrics = {
  cpu: 45,
  gpu: 62,
  // ...
};

// Use React Query
const { data: systemMetrics } = useQuery({
  queryKey: ['systemMetrics'],
  queryFn: async () => {
    const response = await fetch('http://localhost:5000/api/system/metrics');
    return response.json();
  },
  refetchInterval: 1000, // Update every second
});
```

## Component Usage Examples

### Using GlassCard

```tsx
import { GlassCard } from '@/components/dashboard/Cards';

<GlassCard variant="glow" hover>
  <div className="p-6">
    Content with glassmorphism effect
  </div>
</GlassCard>
```

### Using CircularGauge

```tsx
import { CircularGauge } from '@/components/visualizations/Gauges';
import { useEnhancedTheme } from '@/contexts/EnhancedThemeContext';

function MyComponent() {
  const { theme } = useEnhancedTheme();
  
  return (
    <CircularGauge
      value={75}
      label="CPU"
      color={theme.palette.primary}
      size={140}
      animated
    />
  );
}
```

### Using PerformanceRadar

```tsx
import { PerformanceRadar } from '@/components/visualizations/RadarChart';

const data = [
  { label: 'CPU', value: 85 },
  { label: 'GPU', value: 78 },
  { label: 'Memory', value: 65 },
  { label: 'Disk', value: 72 },
];

<PerformanceRadar
  data={data}
  size={400}
  showLabels
  showValues
/>
```

### Using Theme Store

```tsx
import { useThemeStore } from '@/stores/themeStore';

function MyComponent() {
  const {
    getCurrentTheme,
    setGlobalTheme,
    toggleDynamicTheming,
  } = useThemeStore();
  
  const currentTheme = getCurrentTheme();
  
  return (
    <div>
      <h3>Current Theme: {currentTheme.name}</h3>
      <button onClick={toggleDynamicTheming}>
        Toggle Dynamic Theming
      </button>
    </div>
  );
}
```

### Adding Haptic Feedback

```tsx
import { useEnhancedTheme } from '@/contexts/EnhancedThemeContext';

function MyButton() {
  const { triggerFeedback, playTone } = useEnhancedTheme();
  
  const handleClick = () => {
    triggerFeedback('medium'); // light, medium, heavy
    playTone(440); // Frequency in Hz
    // Your action here
  };
  
  return (
    <button onClick={handleClick}>
      Click Me
    </button>
  );
}
```

## Testing Checklist

- [ ] All theme presets load correctly
- [ ] Dynamic theming switches based on mock profile
- [ ] Animations run smoothly (check with different durations)
- [ ] Haptic feedback works (on supported devices)
- [ ] Audio feedback plays (when enabled)
- [ ] Toast notifications appear correctly
- [ ] Responsive layout works on different screen sizes
- [ ] Dark/light mode toggle works
- [ ] Font size changes apply correctly
- [ ] Theme settings persist after refresh
- [ ] All gauges and charts animate properly
- [ ] Hover effects work on interactive elements
- [ ] Keyboard navigation works for accessibility

## Performance Tips

1. **Reduce Motion**: For low-end systems, set `reducedMotion: true` in theme animation settings
2. **Disable Particles**: Set `enableParticles: false` to reduce GPU load
3. **Disable Glow Effects**: Set `enableGlow: false` for better performance
4. **Fast Animations**: Use `duration: 'fast'` for snappier UI

## Troubleshooting

### Theme not applying
- Check if `EnhancedThemeProvider` wraps your app
- Verify CSS variables are being set (inspect element, check computed styles)
- Ensure Tailwind config includes the content paths

### Animations not working
- Check Framer Motion is installed: `npm list framer-motion`
- Verify `enableTransitions` is true in theme settings
- Check browser DevTools for performance throttling

### Profile detection not working
- Check network tab for API calls
- Verify backend endpoint is running
- Check CORS settings on backend
- Review console for errors

### Build errors
- Run `npm install` to ensure all dependencies are installed
- Check for TypeScript errors: `npm run build`
- Clear cache: `rm -rf node_modules .vite dist && npm install`

## Next Development Steps

1. **Add More Components**
   - Create reusable button variants
   - Build modal/dialog components
   - Add form components with validation
   - Create data tables with sorting/filtering

2. **Expand Visualizations**
   - Line/area charts for historical data
   - Heatmaps for temperature monitoring
   - Network graphs for process dependencies
   - Sparklines for compact metrics

3. **Enhanced Interactions**
   - Drag-and-drop for customization
   - Context menus with right-click
   - Keyboard shortcuts (Ctrl+K command palette)
   - Multi-select for batch operations

4. **Accessibility**
   - Add ARIA labels to all interactive elements
   - Ensure keyboard navigation works everywhere
   - Add screen reader announcements
   - Test with accessibility tools

5. **Polish**
   - Add loading skeletons
   - Implement error boundaries
   - Add empty states for no data
   - Create onboarding tour

## Support

For questions or issues with the UI system:
1. Check the `UI_SYSTEM_README.md` for documentation
2. Review component examples in `src/pages/EnhancedDashboard.tsx`
3. Check type definitions in `src/types/theme.ts`
4. Test with the theme customizer in Settings

---

**Happy coding! 🚀**
