import { motion } from 'framer-motion';
import { ThemeCustomizer } from '../components/ThemeCustomizer';
import { SectionCard } from '../components/dashboard/Cards';
import { Settings as SettingsIcon, Palette } from 'lucide-react';

export default function EnhancedSettings() {
  return (
    <div className="min-h-screen p-6 gradient-bg">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="max-w-5xl mx-auto space-y-6"
      >
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-4xl font-bold text-text-primary mb-2">Settings</h1>
          <p className="text-text-muted">
            Customize your PC Optimizer experience
          </p>
        </div>

        {/* Theme Customization */}
        <SectionCard
          title="Theme & Appearance"
          subtitle="Customize colors, animations, and visual preferences"
          icon={<Palette className="text-primary" />}
        >
          <ThemeCustomizer />
        </SectionCard>

        {/* Additional Settings Sections */}
        <SectionCard
          title="Application Settings"
          subtitle="Configure application behavior"
          icon={<SettingsIcon className="text-accent" />}
        >
          <div className="space-y-4">
            <div className="p-4 glass rounded-lg">
              <h3 className="font-medium mb-2">Startup Behavior</h3>
              <p className="text-sm text-text-muted mb-3">
                Configure what happens when the application starts
              </p>
              <div className="space-y-2">
                <label className="flex items-center gap-3 cursor-pointer">
                  <input type="checkbox" className="w-5 h-5" />
                  <span>Launch on system startup</span>
                </label>
                <label className="flex items-center gap-3 cursor-pointer">
                  <input type="checkbox" className="w-5 h-5" />
                  <span>Start minimized</span>
                </label>
                <label className="flex items-center gap-3 cursor-pointer">
                  <input type="checkbox" className="w-5 h-5" />
                  <span>Auto-run optimization on startup</span>
                </label>
              </div>
            </div>

            <div className="p-4 glass rounded-lg">
              <h3 className="font-medium mb-2">Notifications</h3>
              <p className="text-sm text-text-muted mb-3">
                Control when and how you receive notifications
              </p>
              <div className="space-y-2">
                <label className="flex items-center gap-3 cursor-pointer">
                  <input type="checkbox" defaultChecked className="w-5 h-5" />
                  <span>Show optimization completion notifications</span>
                </label>
                <label className="flex items-center gap-3 cursor-pointer">
                  <input type="checkbox" defaultChecked className="w-5 h-5" />
                  <span>Alert when system resources are high</span>
                </label>
                <label className="flex items-center gap-3 cursor-pointer">
                  <input type="checkbox" className="w-5 h-5" />
                  <span>Weekly performance reports</span>
                </label>
              </div>
            </div>
          </div>
        </SectionCard>
      </motion.div>
    </div>
  );
}
