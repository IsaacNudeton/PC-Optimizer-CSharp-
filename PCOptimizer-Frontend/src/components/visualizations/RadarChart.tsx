import { motion } from 'framer-motion';
import { cn } from '../../utils/theme';

export interface RadarDataPoint {
  label: string;
  value: number; // 0-100
  color?: string;
}

interface PerformanceRadarProps {
  data: RadarDataPoint[];
  size?: number;
  levels?: number;
  color?: string;
  fillOpacity?: number;
  showLabels?: boolean;
  showValues?: boolean;
  animated?: boolean;
  className?: string;
}

export function PerformanceRadar({
  data,
  size = 300,
  levels = 5,
  color = 'var(--color-primary)',
  fillOpacity = 0.3,
  showLabels = true,
  showValues = false,
  animated = true,
  className,
}: PerformanceRadarProps) {
  const center = size / 2;
  const maxRadius = (size * 0.35);
  const angleStep = (2 * Math.PI) / data.length;

  // Generate level circles
  const levelCircles = Array.from({ length: levels }, (_, i) => {
    const radius = ((i + 1) / levels) * maxRadius;
    return (
      <circle
        key={i}
        cx={center}
        cy={center}
        r={radius}
        fill="none"
        stroke="currentColor"
        strokeWidth="1"
        className="text-white/10"
      />
    );
  });

  // Generate axis lines
  const axisLines = data.map((_, i) => {
    const angle = i * angleStep - Math.PI / 2;
    const x = center + maxRadius * Math.cos(angle);
    const y = center + maxRadius * Math.sin(angle);
    return (
      <line
        key={i}
        x1={center}
        y1={center}
        x2={x}
        y2={y}
        stroke="currentColor"
        strokeWidth="1"
        className="text-white/10"
      />
    );
  });

  // Generate data points and polygon
  const points = data.map((point, i) => {
    const angle = i * angleStep - Math.PI / 2;
    const radius = (point.value / 100) * maxRadius;
    const x = center + radius * Math.cos(angle);
    const y = center + radius * Math.sin(angle);
    return { x, y, ...point };
  });

  const polygonPoints = points.map((p) => `${p.x},${p.y}`).join(' ');

  // Label positions (outside the radar)
  const labels = data.map((point, i) => {
    const angle = i * angleStep - Math.PI / 2;
    const labelRadius = maxRadius + 40;
    const x = center + labelRadius * Math.cos(angle);
    const y = center + labelRadius * Math.sin(angle);
    
    return {
      x,
      y,
      label: point.label,
      value: point.value,
    };
  });

  return (
    <div className={cn('relative inline-block', className)}>
      <svg width={size} height={size}>
        {/* Level circles */}
        {levelCircles}

        {/* Axis lines */}
        {axisLines}

        {/* Data polygon */}
        <motion.polygon
          points={animated ? '' : polygonPoints}
          fill={color}
          fillOpacity={fillOpacity}
          stroke={color}
          strokeWidth="2"
          initial={{ points: data.map(() => `${center},${center}`).join(' ') }}
          animate={{ points: polygonPoints }}
          transition={{ duration: 1, ease: 'easeOut' }}
          style={{
            filter: `drop-shadow(0 0 8px ${color}80)`,
          }}
        />

        {/* Data points */}
        {points.map((point, i) => (
          <motion.circle
            key={i}
            cx={animated ? center : point.x}
            cy={animated ? center : point.y}
            r="4"
            fill={point.color || color}
            initial={{ cx: center, cy: center, opacity: 0 }}
            animate={{ cx: point.x, cy: point.y, opacity: 1 }}
            transition={{ duration: 0.8, delay: i * 0.1 }}
            style={{
              filter: `drop-shadow(0 0 4px ${point.color || color})`,
            }}
          />
        ))}

        {/* Labels */}
        {showLabels &&
          labels.map((label, i) => (
            <motion.g
              key={i}
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ delay: 0.5 + i * 0.1 }}
            >
              <text
                x={label.x}
                y={label.y}
                textAnchor="middle"
                dominantBaseline="middle"
                className="text-xs font-medium fill-[var(--color-text-secondary)]"
              >
                {label.label}
                {showValues && ` (${label.value}%)`}
              </text>
            </motion.g>
          ))}
      </svg>
    </div>
  );
}

interface MultiSeriesRadarProps {
  series: {
    name: string;
    data: RadarDataPoint[];
    color: string;
  }[];
  size?: number;
  levels?: number;
  showLegend?: boolean;
  className?: string;
}

export function MultiSeriesRadar({
  series,
  size = 300,
  levels = 5,
  showLegend = true,
  className,
}: MultiSeriesRadarProps) {
  const center = size / 2;
  const maxRadius = size * 0.35;
  const angleStep = (2 * Math.PI) / (series[0]?.data.length || 1);

  // Use first series for labels
  const labels = series[0]?.data || [];

  // Level circles
  const levelCircles = Array.from({ length: levels }, (_, i) => {
    const radius = ((i + 1) / levels) * maxRadius;
    return (
      <circle
        key={i}
        cx={center}
        cy={center}
        r={radius}
        fill="none"
        stroke="currentColor"
        strokeWidth="1"
        className="text-white/10"
      />
    );
  });

  // Axis lines
  const axisLines = labels.map((_, i) => {
    const angle = i * angleStep - Math.PI / 2;
    const x = center + maxRadius * Math.cos(angle);
    const y = center + maxRadius * Math.sin(angle);
    return (
      <line
        key={i}
        x1={center}
        y1={center}
        x2={x}
        y2={y}
        stroke="currentColor"
        strokeWidth="1"
        className="text-white/10"
      />
    );
  });

  return (
    <div className={cn('inline-block', className)}>
      <svg width={size} height={size}>
        {levelCircles}
        {axisLines}

        {/* Render each series */}
        {series.map((s, seriesIndex) => {
          const points = s.data.map((point, i) => {
            const angle = i * angleStep - Math.PI / 2;
            const radius = (point.value / 100) * maxRadius;
            const x = center + radius * Math.cos(angle);
            const y = center + radius * Math.sin(angle);
            return { x, y };
          });

          const polygonPoints = points.map((p) => `${p.x},${p.y}`).join(' ');

          return (
            <motion.polygon
              key={seriesIndex}
              points={polygonPoints}
              fill={s.color}
              fillOpacity={0.2}
              stroke={s.color}
              strokeWidth="2"
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              transition={{ duration: 0.8, delay: seriesIndex * 0.2 }}
              style={{
                filter: `drop-shadow(0 0 4px ${s.color}60)`,
              }}
            />
          );
        })}

        {/* Labels */}
        {labels.map((label, i) => {
          const angle = i * angleStep - Math.PI / 2;
          const labelRadius = maxRadius + 40;
          const x = center + labelRadius * Math.cos(angle);
          const y = center + labelRadius * Math.sin(angle);

          return (
            <text
              key={i}
              x={x}
              y={y}
              textAnchor="middle"
              dominantBaseline="middle"
              className="text-xs font-medium fill-[var(--color-text-secondary)]"
            >
              {label.label}
            </text>
          );
        })}
      </svg>

      {/* Legend */}
      {showLegend && (
        <div className="flex flex-wrap gap-4 mt-4 justify-center">
          {series.map((s, i) => (
            <div key={i} className="flex items-center gap-2">
              <div
                className="w-3 h-3 rounded-full"
                style={{ backgroundColor: s.color }}
              />
              <span className="text-sm text-[var(--color-text-secondary)]">
                {s.name}
              </span>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
