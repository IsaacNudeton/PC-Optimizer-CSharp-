import { motion } from 'framer-motion';
import { cn } from '../../utils/theme';

interface CircularGaugeProps {
  value: number; // 0-100
  size?: number;
  strokeWidth?: number;
  label?: string;
  color?: string;
  showValue?: boolean;
  animated?: boolean;
}

export function CircularGauge({
  value,
  size = 120,
  strokeWidth = 8,
  label,
  color = 'var(--color-primary)',
  showValue = true,
  animated = true,
}: CircularGaugeProps) {
  const radius = (size - strokeWidth) / 2;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (value / 100) * circumference;

  return (
    <div className="relative inline-flex items-center justify-center">
      <svg width={size} height={size} className="transform -rotate-90">
        {/* Background circle */}
        <circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke="currentColor"
          strokeWidth={strokeWidth}
          className="text-white/10"
        />
        {/* Progress circle */}
        <motion.circle
          cx={size / 2}
          cy={size / 2}
          r={radius}
          fill="none"
          stroke={color}
          strokeWidth={strokeWidth}
          strokeDasharray={circumference}
          strokeDashoffset={animated ? circumference : offset}
          strokeLinecap="round"
          className="transition-all duration-500"
          animate={animated ? { strokeDashoffset: offset } : {}}
          transition={{ duration: 1, ease: 'easeInOut' }}
          style={{
            filter: `drop-shadow(0 0 8px ${color}80)`,
          }}
        />
      </svg>
      <div className="absolute inset-0 flex flex-col items-center justify-center">
        {showValue && (
          <motion.span
            className="text-2xl font-bold"
            style={{ color }}
            initial={{ opacity: 0, scale: 0.5 }}
            animate={{ opacity: 1, scale: 1 }}
            transition={{ delay: 0.2, duration: 0.5 }}
          >
            {Math.round(value)}%
          </motion.span>
        )}
        {label && (
          <span className="text-xs text-[var(--color-text-muted)] mt-1">
            {label}
          </span>
        )}
      </div>
    </div>
  );
}

interface LinearProgressProps {
  value: number; // 0-100
  height?: number;
  color?: string;
  backgroundColor?: string;
  animated?: boolean;
  showValue?: boolean;
  label?: string;
  className?: string;
}

export function LinearProgress({
  value,
  height = 8,
  color = 'var(--color-primary)',
  backgroundColor = 'var(--color-bg-tertiary)',
  animated = true,
  showValue = false,
  label,
  className,
}: LinearProgressProps) {
  return (
    <div className={cn('w-full', className)}>
      {(label || showValue) && (
        <div className="flex justify-between items-center mb-2">
          {label && (
            <span className="text-sm font-medium text-[var(--color-text-secondary)]">
              {label}
            </span>
          )}
          {showValue && (
            <span className="text-sm font-bold" style={{ color }}>
              {Math.round(value)}%
            </span>
          )}
        </div>
      )}
      <div
        className="relative w-full rounded-full overflow-hidden"
        style={{ height, backgroundColor }}
      >
        <motion.div
          className="absolute left-0 top-0 h-full rounded-full"
          style={{
            background: `linear-gradient(90deg, ${color}, ${color}dd)`,
            boxShadow: `0 0 10px ${color}80`,
          }}
          initial={{ width: 0 }}
          animate={{ width: `${value}%` }}
          transition={animated ? { duration: 1, ease: 'easeOut' } : { duration: 0 }}
        />
      </div>
    </div>
  );
}

interface StatRingProps {
  value: number; // 0-100
  max?: number;
  size?: number;
  thickness?: number;
  color?: string;
  label?: string;
  subLabel?: string;
}

export function StatRing({
  value,
  max = 100,
  size = 100,
  thickness = 10,
  color = 'var(--color-primary)',
  label,
  subLabel,
}: StatRingProps) {
  const percentage = (value / max) * 100;
  const radius = (size - thickness) / 2;
  const circumference = 2 * Math.PI * radius;
  const offset = circumference - (percentage / 100) * circumference;

  return (
    <div className="flex flex-col items-center gap-2">
      <div className="relative" style={{ width: size, height: size }}>
        <svg width={size} height={size} className="transform -rotate-90">
          {/* Background ring */}
          <circle
            cx={size / 2}
            cy={size / 2}
            r={radius}
            fill="none"
            stroke="currentColor"
            strokeWidth={thickness}
            className="text-white/5"
          />
          {/* Value ring */}
          <motion.circle
            cx={size / 2}
            cy={size / 2}
            r={radius}
            fill="none"
            stroke={color}
            strokeWidth={thickness}
            strokeDasharray={circumference}
            strokeDashoffset={offset}
            strokeLinecap="round"
            initial={{ strokeDashoffset: circumference }}
            animate={{ strokeDashoffset: offset }}
            transition={{ duration: 1.5, ease: 'easeOut' }}
            style={{
              filter: `drop-shadow(0 0 6px ${color}60)`,
            }}
          />
        </svg>
        <div className="absolute inset-0 flex items-center justify-center">
          <div className="text-center">
            <motion.div
              className="text-xl font-bold"
              style={{ color }}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.3 }}
            >
              {value}
            </motion.div>
            {subLabel && (
              <div className="text-xs text-[var(--color-text-muted)]">
                {subLabel}
              </div>
            )}
          </div>
        </div>
      </div>
      {label && (
        <span className="text-sm font-medium text-[var(--color-text-secondary)]">
          {label}
        </span>
      )}
    </div>
  );
}

interface PulseIndicatorProps {
  active?: boolean;
  color?: string;
  size?: 'sm' | 'md' | 'lg';
  label?: string;
}

export function PulseIndicator({
  active = true,
  color = 'var(--color-success)',
  size = 'md',
  label,
}: PulseIndicatorProps) {
  const sizes = {
    sm: 'w-2 h-2',
    md: 'w-3 h-3',
    lg: 'w-4 h-4',
  };

  return (
    <div className="flex items-center gap-2">
      <div className="relative">
        <div
          className={cn('rounded-full', sizes[size])}
          style={{ backgroundColor: active ? color : 'var(--color-text-muted)' }}
        />
        {active && (
          <motion.div
            className={cn('absolute inset-0 rounded-full', sizes[size])}
            style={{ backgroundColor: color }}
            animate={{
              scale: [1, 1.5, 1],
              opacity: [0.7, 0, 0.7],
            }}
            transition={{
              duration: 2,
              repeat: Infinity,
              ease: 'easeInOut',
            }}
          />
        )}
      </div>
      {label && (
        <span className="text-sm text-[var(--color-text-secondary)]">{label}</span>
      )}
    </div>
  );
}
