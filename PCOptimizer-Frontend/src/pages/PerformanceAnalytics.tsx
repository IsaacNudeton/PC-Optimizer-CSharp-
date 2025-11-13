import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Stack,
  Typography,
  CircularProgress,
  Grid,
  Alert,
} from '@mui/material';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  BarChart,
  Bar,
} from 'recharts';
import { analyticsService } from '../api/analyticsService';

const PerformanceAnalytics: React.FC = () => {
  const [range, setRange] = useState<'24h' | '7d' | '30d'>('24h');

  const { data, isLoading, error } = useQuery({
    queryKey: ['analytics', range],
    queryFn: () => analyticsService.getMetrics(range),
    refetchInterval: 30000,
    staleTime: 5000,
  });

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">Failed to load analytics data</Alert>;
  }

  const formatData = (dataPoints: any[]) => {
    return dataPoints.map((point) => ({
      ...point,
      time: new Date(point.timestamp).toLocaleString(),
    }));
  };

  const ChartCard: React.FC<{ title: string; children: React.ReactNode }> = ({ title, children }) => (
    <Box sx={{
      background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
      border: '1px solid #444',
      borderRadius: '16px',
      p: 3,
      transition: 'all 0.3s ease',
      '&:hover': {
        border: '1px solid #666',
        boxShadow: '0 8px 24px rgba(0, 0, 0, 0.5)',
      },
    }}>
      <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', mb: 2 }}>
        {title}
      </Typography>
      {children}
    </Box>
  );

  return (
    <Stack spacing={3}>
      {/* Header */}
      <Box sx={{ mb: 2 }}>
        <Typography variant="h3" sx={{ fontWeight: 900, mb: 0.5, fontSize: '42px', letterSpacing: '-1px' }}>
          Performance Analytics
        </Typography>
        <Typography variant="body1" sx={{ color: '#b0b0b0', fontSize: '16px' }}>
          Real-time system performance metrics and historical trends
        </Typography>
      </Box>

      {/* Time Range Selector */}
      <Box sx={{ display: 'flex', gap: 1 }}>
        {(['24h', '7d', '30d'] as const).map((r) => (
          <Button
            key={r}
            onClick={() => setRange(r)}
            sx={{
              px: 3,
              py: 1.5,
              fontWeight: 'bold',
              fontSize: '14px',
              borderRadius: '8px',
              transition: 'all 0.3s ease',
              ...(range === r
                ? {
                    backgroundColor: '#FFB800',
                    color: '#000',
                    '&:hover': {
                      backgroundColor: '#FFC933',
                      boxShadow: '0 4px 12px rgba(255, 184, 0, 0.3)',
                    },
                  }
                : {
                    backgroundColor: 'transparent',
                    color: '#b0b0b0',
                    border: '1px solid #666',
                    '&:hover': {
                      backgroundColor: 'rgba(255, 184, 0, 0.05)',
                      borderColor: '#FFB800',
                      color: '#FFB800',
                    },
                  }),
            }}
          >
            {r === '24h' ? '24 Hours' : r === '7d' ? '7 Days' : '30 Days'}
          </Button>
        ))}
      </Box>

      {/* CPU Usage Chart */}
      <ChartCard title="CPU Usage">
        <ResponsiveContainer width="100%" height={300}>
          <LineChart data={formatData(data?.cpu || [])}>
            <CartesianGrid strokeDasharray="3 3" stroke="#333" />
            <XAxis dataKey="time" stroke="#999" />
            <YAxis domain={[0, 100]} stroke="#999" />
            <Tooltip
              contentStyle={{
                backgroundColor: '#1a1a1a',
                border: '1px solid #444',
                borderRadius: '8px',
              }}
              labelStyle={{ color: '#FFB800' }}
            />
            <Line
              type="monotone"
              dataKey="value"
              stroke="#B8D4E8"
              strokeWidth={3}
              dot={false}
              name="CPU %"
            />
          </LineChart>
        </ResponsiveContainer>
      </ChartCard>

      {/* Memory & Disk - Side by Side */}
      <Grid container spacing={3}>
        <Grid item xs={12} md={6}>
          <ChartCard title="Memory Usage">
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={formatData(data?.ram || [])}>
                <CartesianGrid strokeDasharray="3 3" stroke="#333" />
                <XAxis dataKey="time" stroke="#999" />
                <YAxis domain={[0, 100]} stroke="#999" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1a1a1a',
                    border: '1px solid #444',
                    borderRadius: '8px',
                  }}
                  labelStyle={{ color: '#00D4FF' }}
                />
                <Line
                  type="monotone"
                  dataKey="value"
                  stroke="#00D4FF"
                  strokeWidth={3}
                  dot={false}
                  name="RAM %"
                />
              </LineChart>
            </ResponsiveContainer>
          </ChartCard>
        </Grid>

        <Grid item xs={12} md={6}>
          <ChartCard title="Disk Usage">
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={formatData(data?.disk || [])}>
                <CartesianGrid strokeDasharray="3 3" stroke="#333" />
                <XAxis dataKey="time" stroke="#999" />
                <YAxis domain={[0, 100]} stroke="#999" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: '#1a1a1a',
                    border: '1px solid #444',
                    borderRadius: '8px',
                  }}
                  labelStyle={{ color: '#E8D8B8' }}
                />
                <Line
                  type="monotone"
                  dataKey="value"
                  stroke="#E8D8B8"
                  strokeWidth={3}
                  dot={false}
                  name="Disk %"
                />
              </LineChart>
            </ResponsiveContainer>
          </ChartCard>
        </Grid>
      </Grid>

      {/* System Health Score */}
      <Box sx={{
        background: 'linear-gradient(135deg, #1a5f1a 0%, #0d3d0d 100%)',
        border: '1px solid #4CAF50',
        borderRadius: '16px',
        p: 4,
        transition: 'all 0.3s ease',
        '&:hover': {
          border: '1px solid #66BB6A',
          boxShadow: '0 8px 24px rgba(76, 175, 80, 0.2)',
        },
      }}>
        <Grid container spacing={3} alignItems="center">
          <Grid item xs={12} md={6}>
            <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', color: '#4CAF50', mb: 2 }}>
              SYSTEM HEALTH SCORE
            </Typography>
            <Typography sx={{ fontSize: '64px', fontWeight: 900, color: '#4CAF50', lineHeight: 1 }}>
              {data?.healthScore || 0}%
            </Typography>
            <Typography variant="body2" sx={{ color: '#90EE90', mt: 2 }}>
              {data?.healthScore! >= 80
                ? '✓ Excellent system performance'
                : data?.healthScore! >= 60
                ? '~ Good system performance'
                : '⚠ System needs optimization'}
            </Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <ResponsiveContainer width="100%" height={200}>
              <BarChart data={[{ score: data?.healthScore || 0 }]}>
                <Bar dataKey="score" fill="#4CAF50" radius={[12, 12, 0, 0]} />
                <YAxis domain={[0, 100]} stroke="#66BB6A" />
              </BarChart>
            </ResponsiveContainer>
          </Grid>
        </Grid>
      </Box>
    </Stack>
  );
};

export default PerformanceAnalytics;
