import { useState } from 'react';
import { motion } from 'framer-motion';
import { useThemeStore } from '../stores/themeStore';
import { SectionCard } from './dashboard/Cards';
import { cn } from '../utils/theme';
import { allThemePresets } from '../theme/presets';
import toast from 'react-hot-toast';

export function ThemeCustomizer() {
  const {
    globalTheme,
    categoryThemes,
    enableDynamicTheming,
    enableHaptics,
    enableAudio,
    mode,
    fontSize,
    setGlobalTheme,
    setCategoryTheme,
    toggleDynamicTheming,
    toggleHaptics,
    toggleAudio,
    setMode,
    setFontSize,
    resetToDefaults,
  } = useThemeStore();

  const [activeTab, setActiveTab] = useState<'global' | 'category' | 'preferences'>('global');

  const themePresetOptions = Object.values(allThemePresets);

  const handleGlobalThemeChange = (themeId: string | null) => {
    if (themeId === null) {
      setGlobalTheme(null);
      toast.success('Global theme cleared');
    } else {
      const theme = allThemePresets[themeId];
      if (theme) {
        setGlobalTheme(theme);
        toast.success(`Applied ${theme.name} theme globally`);
      }
    }
  };

  const handleCategoryThemeChange = (category: string, themeId: string) => {
    const theme = allThemePresets[themeId];
    if (theme) {
      setCategoryTheme(category, theme);
      toast.success(`Applied ${theme.name} to ${category}`);
    }
  };

  const handleReset = () => {
    if (confirm('Reset all theme settings to defaults?')) {
      resetToDefaults();
      toast.success('Theme settings reset to defaults');
    }
  };

  return (
    <div className="space-y-6">
      {/* Tabs */}
      <div className="flex gap-2 border-b border-white/10 pb-2">
        {(['global', 'category', 'preferences'] as const).map((tab) => (
          <button
            key={tab}
            onClick={() => setActiveTab(tab)}
            className={cn(
              'px-4 py-2 rounded-lg font-medium transition-all duration-300',
              activeTab === tab
                ? 'bg-primary text-white shadow-glow'
                : 'text-text-secondary hover:text-text-primary hover:bg-white/5'
            )}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {/* Global Theme */}
      {activeTab === 'global' && (
        <SectionCard
          title="Global Theme"
          subtitle="Apply a single theme to the entire application"
          icon="🎨"
        >
          <div className="space-y-4">
            <div className="flex items-center justify-between p-4 glass rounded-lg">
              <div>
                <div className="font-medium">Dynamic Theming</div>
                <div className="text-sm text-text-muted">
                  Auto-switch themes based on active application
                </div>
              </div>
              <button
                onClick={toggleDynamicTheming}
                className={cn(
                  'relative w-14 h-8 rounded-full transition-colors duration-300',
                  enableDynamicTheming ? 'bg-primary' : 'bg-gray-600'
                )}
              >
                <motion.div
                  className="absolute top-1 w-6 h-6 bg-white rounded-full shadow-md"
                  animate={{ left: enableDynamicTheming ? '28px' : '4px' }}
                  transition={{ duration: 0.2 }}
                />
              </button>
            </div>

            <div className="space-y-2">
              <label className="block text-sm font-medium">
                Select Global Theme
              </label>
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                <ThemePreviewCard
                  name="None (Use Dynamic)"
                  isActive={globalTheme === null}
                  onClick={() => handleGlobalThemeChange(null)}
                />
                {themePresetOptions.map((theme) => (
                  <ThemePreviewCard
                    key={theme.id}
                    name={theme.name}
                    palette={theme.palette}
                    isActive={globalTheme?.id === theme.id}
                    onClick={() => handleGlobalThemeChange(theme.id)}
                  />
                ))}
              </div>
            </div>
          </div>
        </SectionCard>
      )}

      {/* Category Themes */}
      {activeTab === 'category' && (
        <SectionCard
          title="Category Themes"
          subtitle="Set default themes for each activity category"
          icon="📁"
        >
          <div className="space-y-6">
            {Object.entries(categoryThemes).map(([category, theme]) => (
              <div key={category} className="space-y-2">
                <h4 className="font-medium capitalize">{category}</h4>
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                  {themePresetOptions
                    .filter((t) => t.category === category)
                    .map((t) => (
                      <ThemePreviewCard
                        key={t.id}
                        name={t.name}
                        palette={t.palette}
                        isActive={theme.id === t.id}
                        onClick={() => handleCategoryThemeChange(category, t.id)}
                      />
                    ))}
                </div>
              </div>
            ))}
          </div>
        </SectionCard>
      )}

      {/* Preferences */}
      {activeTab === 'preferences' && (
        <SectionCard
          title="Preferences"
          subtitle="Customize appearance and feedback settings"
          icon="⚙️"
        >
          <div className="space-y-6">
            {/* Mode */}
            <div className="space-y-2">
              <label className="block text-sm font-medium">Theme Mode</label>
              <div className="flex gap-2">
                {(['light', 'dark', 'auto'] as const).map((m) => (
                  <button
                    key={m}
                    onClick={() => setMode(m)}
                    className={cn(
                      'px-4 py-2 rounded-lg font-medium transition-all',
                      mode === m
                        ? 'bg-primary text-white'
                        : 'glass text-text-secondary hover:text-text-primary'
                    )}
                  >
                    {m.charAt(0).toUpperCase() + m.slice(1)}
                  </button>
                ))}
              </div>
            </div>

            {/* Font Size */}
            <div className="space-y-2">
              <label className="block text-sm font-medium">Font Size</label>
              <div className="flex gap-2">
                {(['small', 'medium', 'large'] as const).map((size) => (
                  <button
                    key={size}
                    onClick={() => setFontSize(size)}
                    className={cn(
                      'px-4 py-2 rounded-lg font-medium transition-all',
                      fontSize === size
                        ? 'bg-primary text-white'
                        : 'glass text-text-secondary hover:text-text-primary'
                    )}
                  >
                    {size.charAt(0).toUpperCase() + size.slice(1)}
                  </button>
                ))}
              </div>
            </div>

            {/* Feedback Options */}
            <div className="space-y-4">
              <div className="flex items-center justify-between p-4 glass rounded-lg">
                <div>
                  <div className="font-medium">Haptic Feedback</div>
                  <div className="text-sm text-text-muted">
                    Vibration on interactions
                  </div>
                </div>
                <button
                  onClick={toggleHaptics}
                  className={cn(
                    'relative w-14 h-8 rounded-full transition-colors duration-300',
                    enableHaptics ? 'bg-primary' : 'bg-gray-600'
                  )}
                >
                  <motion.div
                    className="absolute top-1 w-6 h-6 bg-white rounded-full shadow-md"
                    animate={{ left: enableHaptics ? '28px' : '4px' }}
                    transition={{ duration: 0.2 }}
                  />
                </button>
              </div>

              <div className="flex items-center justify-between p-4 glass rounded-lg">
                <div>
                  <div className="font-medium">Audio Feedback</div>
                  <div className="text-sm text-text-muted">
                    Sounds on interactions
                  </div>
                </div>
                <button
                  onClick={toggleAudio}
                  className={cn(
                    'relative w-14 h-8 rounded-full transition-colors duration-300',
                    enableAudio ? 'bg-primary' : 'bg-gray-600'
                  )}
                >
                  <motion.div
                    className="absolute top-1 w-6 h-6 bg-white rounded-full shadow-md"
                    animate={{ left: enableAudio ? '28px' : '4px' }}
                    transition={{ duration: 0.2 }}
                  />
                </button>
              </div>
            </div>

            {/* Reset Button */}
            <button
              onClick={handleReset}
              className="w-full px-4 py-3 bg-error/20 hover:bg-error/30 text-error rounded-lg font-medium transition-all"
            >
              Reset to Defaults
            </button>
          </div>
        </SectionCard>
      )}
    </div>
  );
}

interface ThemePreviewCardProps {
  name: string;
  palette?: any;
  isActive: boolean;
  onClick: () => void;
}

function ThemePreviewCard({ name, palette, isActive, onClick }: ThemePreviewCardProps) {
  return (
    <motion.button
      onClick={onClick}
      className={cn(
        'p-3 rounded-lg border-2 transition-all text-left',
        isActive
          ? 'border-primary bg-primary/10'
          : 'border-white/10 bg-white/5 hover:border-white/20'
      )}
      whileHover={{ scale: 1.02 }}
      whileTap={{ scale: 0.98 }}
    >
      <div className="flex items-center gap-2 mb-2">
        {palette && (
          <div className="flex gap-1">
            <div
              className="w-4 h-4 rounded-full"
              style={{ backgroundColor: palette.primary }}
            />
            <div
              className="w-4 h-4 rounded-full"
              style={{ backgroundColor: palette.secondary }}
            />
            <div
              className="w-4 h-4 rounded-full"
              style={{ backgroundColor: palette.accent }}
            />
          </div>
        )}
      </div>
      <div className="text-sm font-medium">{name}</div>
    </motion.button>
  );
}
