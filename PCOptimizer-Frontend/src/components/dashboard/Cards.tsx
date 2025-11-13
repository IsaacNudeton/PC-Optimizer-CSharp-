import { ReactNode } from 'react';
import { motion, HTMLMotionProps } from 'framer-motion';
import { cn } from '../../utils/theme';

interface GlassCardProps extends Omit<HTMLMotionProps<'div'>, 'children'> {
  children: ReactNode;
  variant?: 'default' | 'bordered' | 'elevated' | 'glow';
  hover?: boolean;
  glowColor?: string;
  className?: string;
}

export function GlassCard({
  children,
  variant = 'default',
  hover = true,
  glowColor,
  className,
  ...props
}: GlassCardProps) {
  const baseStyles = 'rounded-xl backdrop-blur-xl transition-all duration-300';

  const variantStyles = {
    default: 'bg-white/5 border border-white/10',
    bordered: 'bg-white/10 border-2 border-[var(--color-primary)]/30',
    elevated: 'bg-white/10 border border-white/10 shadow-2xl',
    glow: 'bg-white/5 border border-[var(--color-primary)]/50 shadow-[0_0_20px_var(--color-glow)]',
  };

  const hoverStyles = hover
    ? 'hover:bg-white/10 hover:border-[var(--color-primary)]/50 hover:shadow-[0_8px_32px_rgba(0,0,0,0.2)] hover:-translate-y-1'
    : '';

  return (
    <motion.div
      className={cn(baseStyles, variantStyles[variant], hoverStyles, className)}
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      exit={{ opacity: 0, y: -20 }}
      transition={{ duration: 0.3 }}
      {...props}
    >
      {children}
    </motion.div>
  );
}

interface MetricCardProps {
  title: string;
  value: string | number;
  unit?: string;
  icon?: ReactNode;
  trend?: 'up' | 'down' | 'neutral';
  trendValue?: string;
  color?: string;
  onClick?: () => void;
}

export function MetricCard({
  title,
  value,
  unit,
  icon,
  trend,
  trendValue,
  color = 'var(--color-primary)',
  onClick,
}: MetricCardProps) {
  const trendIcons = {
    up: '↗',
    down: '↘',
    neutral: '→',
  };

  const trendColors = {
    up: 'text-green-400',
    down: 'text-red-400',
    neutral: 'text-gray-400',
  };

  return (
    <GlassCard
      variant="bordered"
      hover={!!onClick}
      onClick={onClick}
      className={cn('p-6 cursor-pointer', onClick && 'active:scale-95')}
      whileHover={{ scale: onClick ? 1.02 : 1 }}
      whileTap={{ scale: onClick ? 0.98 : 1 }}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="text-sm font-medium text-[var(--color-text-muted)] mb-2">
            {title}
          </div>
          <div className="flex items-baseline gap-2">
            <span
              className="text-4xl font-bold"
              style={{ color }}
            >
              {value}
            </span>
            {unit && (
              <span className="text-lg font-medium text-[var(--color-text-secondary)]">
                {unit}
              </span>
            )}
          </div>
          {trend && trendValue && (
            <div className={cn('flex items-center gap-1 mt-2 text-sm', trendColors[trend])}>
              <span>{trendIcons[trend]}</span>
              <span>{trendValue}</span>
            </div>
          )}
        </div>
        {icon && (
          <div
            className="text-3xl opacity-50"
            style={{ color }}
          >
            {icon}
          </div>
        )}
      </div>
    </GlassCard>
  );
}

interface SectionCardProps {
  title: string;
  subtitle?: string;
  children: ReactNode;
  action?: ReactNode;
  icon?: ReactNode;
  className?: string;
}

export function SectionCard({
  title,
  subtitle,
  children,
  action,
  icon,
  className,
}: SectionCardProps) {
  return (
    <GlassCard variant="elevated" hover={false} className={cn('p-6', className)}>
      <div className="flex items-start justify-between mb-6">
        <div className="flex items-center gap-3">
          {icon && (
            <div className="text-2xl text-[var(--color-primary)]">{icon}</div>
          )}
          <div>
            <h3 className="text-xl font-bold text-[var(--color-text-primary)]">
              {title}
            </h3>
            {subtitle && (
              <p className="text-sm text-[var(--color-text-muted)] mt-1">
                {subtitle}
              </p>
            )}
          </div>
        </div>
        {action && <div>{action}</div>}
      </div>
      <div>{children}</div>
    </GlassCard>
  );
}
