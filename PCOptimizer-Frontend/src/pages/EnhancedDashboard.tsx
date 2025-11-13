import { motion } from 'framer-motion';
import { Activity, Zap, TrendingUp, PlayCircle, Settings, Cpu, HardDrive, Gauge } from 'lucide-react';
import { MetricCard, SectionCard } from '../components/dashboard/Cards';
import { CircularGauge, LinearProgress, StatRing, PulseIndicator } from '../components/visualizations/Gauges';
import { PerformanceRadar } from '../components/visualizations/RadarChart';
import { useEnhancedTheme } from '../contexts/EnhancedThemeContext';
import { useThemeStore } from '../stores/themeStore';
import toast from 'react-hot-toast';

export default function Dashboard() {
  const { theme, triggerFeedback } = useEnhancedTheme();
  const { getCurrentTheme } = useThemeStore();
  const currentTheme = getCurrentTheme();

  // Mock data - replace with real API calls
  const systemMetrics = {
    cpu: 45,
    gpu: 62,
    ram: 78,
    disk: 55,
    temp: 65,
    fps: 144,
  };

  const radarData = [
    { label: 'CPU', value: 85 },
    { label: 'GPU', value: 78 },
    { label: 'Memory', value: 65 },
    { label: 'Disk', value: 72 },
    { label: 'Network', value: 90 },
    { label: 'Battery', value: 60 },
  ];

  const activeOptimizations = [
    { name: 'Game Mode', status: 'active', impact: 'high' },
    { name: 'Memory Cleaner', status: 'running', impact: 'medium' },
    { name: 'Startup Optimization', status: 'scheduled', impact: 'low' },
  ];

  const handleQuickOptimize = () => {
    triggerFeedback('medium');
    toast.success('Quick optimization started!');
  };

  const handleDeepClean = () => {
    triggerFeedback('heavy');
    toast.promise(
      new Promise((resolve) => setTimeout(resolve, 2000)),
      {
        loading: 'Running deep clean...',
        success: 'Deep clean completed!',
        error: 'Deep clean failed',
      }
    );
  };

  return (
    <div className="min-h-screen p-6 gradient-bg particles">
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.5 }}
        className="max-w-[1600px] mx-auto space-y-6"
      >
        {/* Header */}
        <div className="flex items-center justify-between mb-8">
          <div>
            <h1 className="text-4xl font-bold text-text-primary mb-2">
              PC Optimizer Dashboard
            </h1>
            <p className="text-text-muted">
              Active Profile: <span className="text-primary font-semibold">{currentTheme.name}</span>
            </p>
          </div>
          <div className="flex gap-3">
            <motion.button
              onClick={handleQuickOptimize}
              className="px-6 py-3 bg-primary hover:bg-primary/80 rounded-xl font-semibold flex items-center gap-2 shadow-glow"
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
            >
              <Zap size={20} />
              Quick Optimize
            </motion.button>
            <motion.button
              onClick={handleDeepClean}
              className="px-6 py-3 glass glass-hover rounded-xl font-semibold flex items-center gap-2"
              whileHover={{ scale: 1.05 }}
              whileTap={{ scale: 0.95 }}
            >
              <Settings size={20} />
              Deep Clean
            </motion.button>
          </div>
        </div>

        {/* System Metrics Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
          <MetricCard
            title="CPU Usage"
            value={systemMetrics.cpu}
            unit="%"
            icon={<Cpu />}
            trend="down"
            trendValue="↓ 5%"
            color={theme.palette.primary}
          />
          <MetricCard
            title="GPU Usage"
            value={systemMetrics.gpu}
            unit="%"
            icon={<Activity />}
            trend="up"
            trendValue="↑ 12%"
            color={theme.palette.accent}
          />
          <MetricCard
            title="RAM Usage"
            value={systemMetrics.ram}
            unit="%"
            icon={<HardDrive />}
            trend="neutral"
            trendValue="→ 0%"
            color={theme.palette.warning}
          />
          <MetricCard
            title="Disk Usage"
            value={systemMetrics.disk}
            unit="%"
            icon={<HardDrive />}
            color={theme.palette.info}
          />
          <MetricCard
            title="Temperature"
            value={systemMetrics.temp}
            unit="°C"
            icon={<Gauge />}
            trend="down"
            trendValue="↓ 3°C"
            color={theme.palette.error}
          />
          <MetricCard
            title="FPS"
            value={systemMetrics.fps}
            icon={<TrendingUp />}
            trend="up"
            trendValue="↑ 20"
            color={theme.palette.success}
          />
        </div>

        {/* Main Dashboard Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* System Intelligence - 2 columns */}
          <div className="lg:col-span-2 space-y-6">
            {/* Performance Radar */}
            <SectionCard
              title="System Performance"
              subtitle="Real-time performance metrics across all components"
              icon={<Activity className="text-primary" />}
            >
              <div className="flex justify-center py-4">
                <PerformanceRadar
                  data={radarData}
                  size={400}
                  color={theme.palette.primary}
                  showLabels
                  showValues
                />
              </div>
            </SectionCard>

            {/* Active Workflows */}
            <SectionCard
              title="Active Workflows"
              subtitle="Currently running optimization processes"
              icon={<PlayCircle className="text-success" />}
            >
              <div className="space-y-3">
                {activeOptimizations.map((opt, i) => (
                  <motion.div
                    key={i}
                    className="p-4 glass rounded-lg"
                    initial={{ opacity: 0, x: -20 }}
                    animate={{ opacity: 1, x: 0 }}
                    transition={{ delay: i * 0.1 }}
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-3">
                        <PulseIndicator
                          active={opt.status === 'active' || opt.status === 'running'}
                          color={
                            opt.status === 'active'
                              ? theme.palette.success
                              : opt.status === 'running'
                              ? theme.palette.info
                              : theme.palette.warning
                          }
                        />
                        <div>
                          <div className="font-medium">{opt.name}</div>
                          <div className="text-sm text-text-muted capitalize">
                            {opt.status}
                          </div>
                        </div>
                      </div>
                      <div className={`px-3 py-1 rounded-full text-xs font-semibold ${
                        opt.impact === 'high'
                          ? 'bg-success/20 text-success'
                          : opt.impact === 'medium'
                          ? 'bg-warning/20 text-warning'
                          : 'bg-info/20 text-info'
                      }`}>
                        {opt.impact.toUpperCase()}
                      </div>
                    </div>
                  </motion.div>
                ))}
              </div>
            </SectionCard>
          </div>

          {/* System Gauges - 1 column */}
          <div className="space-y-6">
            {/* Circular Gauges */}
            <SectionCard
              title="Resource Monitor"
              subtitle="Live system resource usage"
              icon={<Gauge className="text-primary" />}
            >
              <div className="space-y-6">
                <div className="flex justify-center">
                  <CircularGauge
                    value={systemMetrics.cpu}
                    label="CPU"
                    color={theme.palette.primary}
                    size={140}
                    strokeWidth={12}
                  />
                </div>
                
                <div className="grid grid-cols-2 gap-4">
                  <StatRing
                    value={systemMetrics.ram}
                    label="RAM"
                    size={90}
                    color={theme.palette.warning}
                    subLabel="Used"
                  />
                  <StatRing
                    value={systemMetrics.gpu}
                    label="GPU"
                    size={90}
                    color={theme.palette.accent}
                    subLabel="Load"
                  />
                </div>

                <div className="space-y-3">
                  <LinearProgress
                    value={systemMetrics.disk}
                    label="Disk Usage"
                    color={theme.palette.info}
                    showValue
                    animated
                  />
                  <LinearProgress
                    value={systemMetrics.temp}
                    label="Temperature"
                    color={theme.palette.error}
                    showValue
                    animated
                  />
                </div>
              </div>
            </SectionCard>

            {/* Quick Actions */}
            <SectionCard
              title="Quick Actions"
              icon={<Zap className="text-warning" />}
            >
              <div className="space-y-2">
                {[
                  { name: 'Clear Cache', icon: '🧹' },
                  { name: 'Boost Performance', icon: '🚀' },
                  { name: 'Update Drivers', icon: '📦' },
                  { name: 'System Scan', icon: '🔍' },
                ].map((action, i) => (
                  <motion.button
                    key={i}
                    className="w-full p-3 glass glass-hover rounded-lg font-medium text-left flex items-center gap-3"
                    whileHover={{ scale: 1.02, x: 5 }}
                    whileTap={{ scale: 0.98 }}
                    onClick={() => {
                      triggerFeedback('light');
                      toast.success(`${action.name} started`);
                    }}
                  >
                    <span className="text-2xl">{action.icon}</span>
                    <span>{action.name}</span>
                  </motion.button>
                ))}
              </div>
            </SectionCard>
          </div>
        </div>
      </motion.div>
    </div>
  );
}
