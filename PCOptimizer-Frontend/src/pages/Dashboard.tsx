import React from 'react';
import {
  Box,
  Typography,
  Card,
  CardContent,
  CardHeader,
  Grid,
  Button,
  Alert,
  CircularProgress,
  Stack,
  LinearProgress,
} from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, AreaChart, Area } from 'recharts';
import { dashboardService, type DashboardMetrics } from '@/api';
import { Zap, TrendingUp, AlertCircle } from 'lucide-react';

// Premium Gauge component for CPU/GPU/RAM
interface GaugeCircleProps {
  value: number;
  label: string;
  color?: string;
  isLoading?: boolean;
  icon?: React.ReactNode;
}

const GaugeCircle: React.FC<GaugeCircleProps> = ({ value, label, color = '#FFB800', isLoading = false, icon }) => (
  <Box sx={{
    textAlign: 'center',
    p: 2.5,
    background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
    borderRadius: '16px',
    border: '1px solid #444',
    transition: 'all 0.3s ease',
    '&:hover': {
      border: '1px solid #666',
      transform: 'translateY(-2px)',
      boxShadow: '0 8px 24px rgba(0, 0, 0, 0.5)',
    },
  }}>
    {icon && <Box sx={{ mb: 1, display: 'flex', justifyContent: 'center' }}>{icon}</Box>}
    {isLoading ? (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
        <CircularProgress size={100} thickness={3} />
      </Box>
    ) : (
      <Box sx={{ position: 'relative', display: 'inline-flex', my: 1 }}>
        <CircularProgress
          variant="determinate"
          value={100}
          size={110}
          thickness={3}
          sx={{ color: '#444' }}
        />
        <CircularProgress
          variant="determinate"
          value={Math.min(value, 100)}
          size={110}
          thickness={3}
          sx={{
            position: 'absolute',
            left: 0,
            color: color,
            transition: 'all 0.5s ease',
          }}
        />
        <Box
          sx={{
            top: 0,
            left: 0,
            bottom: 0,
            right: 0,
            position: 'absolute',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexDirection: 'column',
          }}
        >
          <Typography variant="h4" sx={{ fontWeight: 'bold', color: color }}>
            {Math.round(value)}%
          </Typography>
        </Box>
      </Box>
    )}
    <Typography sx={{ mt: 2.5, fontSize: '14px', fontWeight: '600', letterSpacing: '0.5px' }}>
      {label}
    </Typography>
  </Box>
);

// Mock data for charts - TODO: Replace with real API data from PCOptimizer-API
const mockPerformanceData = [
  { time: '00:00', cpu: 45, gpu: 30 },
  { time: '04:00', cpu: 52, gpu: 35 },
  { time: '08:00', cpu: 48, gpu: 32 },
  { time: '12:00', cpu: 65, gpu: 48 },
  { time: '16:00', cpu: 72, gpu: 55 },
  { time: '20:00', cpu: 58, gpu: 42 },
  { time: '24:00', cpu: 45, gpu: 30 },
];

const Dashboard: React.FC = () => {
  // TODO: Connect to PCOptimizer-API endpoints:
  // GET /api/dashboard/stats - Get current CPU, RAM, GPU, Temperature
  // GET /api/dashboard/performance-history - Get historical performance data
  // GET /api/dashboard/system-health - Get system health status
  // GET /api/dashboard/anomalies - Get ML anomaly detections

  const { data: metrics, isLoading, error } = useQuery<DashboardMetrics>({
    queryKey: ['dashboard-metrics'],
    queryFn: dashboardService.getMetrics,
    refetchInterval: 5000,
    retry: true,
  });

  return (
    <Box>
      {/* Header with Status */}
      <Box sx={{ mb: 4, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Box>
          <Typography variant="h3" sx={{ fontWeight: 900, mb: 0.5, fontSize: '42px', letterSpacing: '-1px' }}>
            System Status
          </Typography>
          <Typography variant="body1" sx={{ color: '#b0b0b0', fontSize: '16px' }}>
            Real-time performance monitoring and optimization insights
          </Typography>
        </Box>
      </Box>

      {/* Status Alert */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Failed to connect to backend. Make sure API is running on localhost:5211
        </Alert>
      )}

      {/* Primary Metrics - Large, Bold, Impactful */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={6} md={3}>
          <GaugeCircle
            value={metrics?.cpu || 0}
            label="CPU Usage"
            color="#FFB800"
            isLoading={isLoading}
            icon={<Zap size={28} color="#FFB800" />}
          />
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <GaugeCircle
            value={metrics?.gpu || 0}
            label="GPU Usage"
            color="#00BCD4"
            isLoading={isLoading}
            icon={<TrendingUp size={28} color="#00BCD4" />}
          />
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <GaugeCircle
            value={metrics?.ram || 0}
            label="RAM Usage"
            color="#4CAF50"
            isLoading={isLoading}
          />
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Box sx={{
            textAlign: 'center',
            p: 2.5,
            background: 'linear-gradient(135deg, #1a5f1a 0%, #0d3d0d 100%)',
            borderRadius: '16px',
            border: '1px solid #2d7a2d',
            transition: 'all 0.3s ease',
            '&:hover': {
              border: '1px solid #4CAF50',
              transform: 'translateY(-2px)',
              boxShadow: '0 8px 24px rgba(76, 175, 80, 0.2)',
            },
          }}>
            <Typography variant="h6" sx={{ fontWeight: '600', mb: 1.5, fontSize: '14px', letterSpacing: '0.5px' }}>
              SYSTEM HEALTH
            </Typography>
            {isLoading ? (
              <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                <CircularProgress size={60} thickness={3} sx={{ color: '#4CAF50' }} />
              </Box>
            ) : (
              <Box>
                <Typography variant="h3" sx={{ color: '#4CAF50', fontWeight: 'bold', mb: 1, fontSize: '48px' }}>
                  {metrics?.systemHealth || 98}%
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
                  <Box sx={{ width: '8px', height: '8px', borderRadius: '50%', bgcolor: '#4CAF50' }} />
                  <Typography variant="body2" sx={{ color: '#b0b0b0', fontWeight: '500' }}>
                    {metrics?.systemStatus || 'Healthy'}
                  </Typography>
                </Box>
              </Box>
            )}
          </Box>
        </Grid>
      </Grid>

      {/* Real-time Performance Chart */}
      <Card sx={{ mb: 3 }}>
        <CardHeader title="Real-time Performance" />
        <CardContent>
          {isLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 3 }}>
              <CircularProgress />
            </Box>
          ) : (
            <ResponsiveContainer width="100%" height={300}>
              <AreaChart data={mockPerformanceData}>
                <defs>
                  <linearGradient id="colorCpu" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#FFB800" stopOpacity={0.8} />
                    <stop offset="95%" stopColor="#FFB800" stopOpacity={0} />
                  </linearGradient>
                  <linearGradient id="colorGpu" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="5%" stopColor="#00BCD4" stopOpacity={0.8} />
                    <stop offset="95%" stopColor="#00BCD4" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#444" />
                <XAxis dataKey="time" stroke="#b0b0b0" />
                <YAxis stroke="#b0b0b0" />
                <Tooltip contentStyle={{ backgroundColor: '#2d2d2d', border: '1px solid #444' }} />
                <Area type="monotone" dataKey="cpu" stroke="#FFB800" fillOpacity={1} fill="url(#colorCpu)" />
                <Area type="monotone" dataKey="gpu" stroke="#00BCD4" fillOpacity={1} fill="url(#colorGpu)" />
              </AreaChart>
            </ResponsiveContainer>
          )}
        </CardContent>
      </Card>

      {/* Action & Insights Section */}
      <Grid container spacing={3} sx={{ mt: 2 }}>
        {/* Quick Actions - BIG AND BOLD */}
        <Grid item xs={12} md={6}>
          <Box sx={{
            background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
            border: '1px solid #444',
            borderRadius: '16px',
            p: 3,
          }}>
            <Typography variant="h6" sx={{ fontWeight: '700', mb: 2.5, fontSize: '16px', letterSpacing: '0.5px' }}>
              QUICK ACTIONS
            </Typography>
            <Stack spacing={2}>
              <Button
                variant="contained"
                fullWidth
                sx={{
                  backgroundColor: '#FFB800',
                  color: '#000',
                  fontWeight: 'bold',
                  padding: '14px 24px',
                  fontSize: '16px',
                  borderRadius: '12px',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    backgroundColor: '#FFA500',
                    transform: 'translateY(-2px)',
                    boxShadow: '0 12px 32px rgba(255, 184, 0, 0.3)',
                  },
                }}
              >
                🚀 Optimize Now
              </Button>
              <Button
                variant="outlined"
                fullWidth
                sx={{
                  borderColor: '#666',
                  color: '#fff',
                  fontWeight: '600',
                  padding: '12px 24px',
                  fontSize: '14px',
                  borderRadius: '12px',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    borderColor: '#FFB800',
                    backgroundColor: 'rgba(255, 184, 0, 0.05)',
                  },
                }}
              >
                🗑️ Clear Cache
              </Button>
            </Stack>
          </Box>
        </Grid>

        {/* ML Anomaly Detection - Premium Alert */}
        <Grid item xs={12} md={6}>
          <Box sx={{
            background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
            border: '1px solid #444',
            borderRadius: '16px',
            p: 3,
          }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2.5 }}>
              <AlertCircle size={20} color="#FFA726" />
              <Typography variant="h6" sx={{ fontWeight: '700', fontSize: '16px', letterSpacing: '0.5px' }}>
                SYSTEM ALERTS
              </Typography>
            </Box>
            <Stack spacing={1.5}>
              <Box sx={{
                p: 2,
                backgroundColor: 'rgba(255, 152, 0, 0.1)',
                borderLeft: '3px solid #FFA726',
                borderRadius: '8px',
                transition: 'all 0.3s ease',
                '&:hover': { backgroundColor: 'rgba(255, 152, 0, 0.15)' },
              }}>
                <Typography variant="body2" sx={{ fontWeight: '700', color: '#FFA726', mb: 0.3 }}>
                  ⚠ High Memory Usage
                </Typography>
                <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
                  Chrome consuming 2.4 GB - Consider optimization
                </Typography>
              </Box>
              <Box sx={{
                p: 2,
                backgroundColor: 'rgba(76, 175, 80, 0.1)',
                borderLeft: '3px solid #4CAF50',
                borderRadius: '8px',
                transition: 'all 0.3s ease',
                '&:hover': { backgroundColor: 'rgba(76, 175, 80, 0.15)' },
              }}>
                <Typography variant="body2" sx={{ fontWeight: '700', color: '#4CAF50', mb: 0.3 }}>
                  ✓ CPU Healthy
                </Typography>
                <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
                  No anomalies detected in processor usage
                </Typography>
              </Box>
            </Stack>
          </Box>
        </Grid>
      </Grid>

      {/* Temperature & System Info Footer */}
      <Grid container spacing={3} sx={{ mt: 2 }}>
        <Grid item xs={12} md={6}>
          <Box sx={{
            background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
            border: '1px solid #444',
            borderRadius: '16px',
            p: 3,
          }}>
            <Typography variant="h6" sx={{ fontWeight: '700', mb: 2.5, fontSize: '16px', letterSpacing: '0.5px' }}>
              THERMAL STATUS
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={6}>
                <Box sx={{
                  textAlign: 'center',
                  p: 2,
                  backgroundColor: 'rgba(255, 184, 0, 0.05)',
                  borderRadius: '12px',
                  border: '1px solid #444',
                }}>
                  <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#FFB800', mb: 0.5 }}>
                    {metrics?.temperature || 45}°C
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#b0b0b0', fontSize: '12px', fontWeight: '600' }}>
                    CPU Temperature
                  </Typography>
                </Box>
              </Grid>
              <Grid item xs={6}>
                <Box sx={{
                  textAlign: 'center',
                  p: 2,
                  backgroundColor: 'rgba(0, 188, 212, 0.05)',
                  borderRadius: '12px',
                  border: '1px solid #444',
                }}>
                  <Typography variant="h4" sx={{ fontWeight: 'bold', color: '#00BCD4', mb: 0.5 }}>
                    {metrics?.gpuTemp || 38}°C
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#b0b0b0', fontSize: '12px', fontWeight: '600' }}>
                    GPU Temperature
                  </Typography>
                </Box>
              </Grid>
            </Grid>
          </Box>
        </Grid>

        <Grid item xs={12} md={6}>
          <Box sx={{
            background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
            border: '1px solid #444',
            borderRadius: '16px',
            p: 3,
          }}>
            <Typography variant="h6" sx={{ fontWeight: '700', mb: 2.5, fontSize: '16px', letterSpacing: '0.5px' }}>
              SYSTEM INFORMATION
            </Typography>
            <Stack spacing={1.5}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="body2" sx={{ color: '#b0b0b0' }}>Active Processes:</Typography>
                <Typography variant="body2" sx={{ fontWeight: 'bold', color: '#FFB800' }}>
                  {metrics?.activeProcesses || 142}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="body2" sx={{ color: '#b0b0b0' }}>System Status:</Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <Box sx={{ width: '8px', height: '8px', borderRadius: '50%', bgcolor: '#4CAF50' }} />
                  <Typography variant="body2" sx={{ fontWeight: 'bold', color: '#4CAF50' }}>
                    {metrics?.systemStatus || 'Optimal'}
                  </Typography>
                </Box>
              </Box>
              <LinearProgress
                variant="determinate"
                value={metrics?.systemHealth || 98}
                sx={{
                  mt: 1,
                  backgroundColor: '#444',
                  '& .MuiLinearProgress-bar': {
                    backgroundColor: '#4CAF50',
                  },
                }}
              />
              <Typography variant="caption" sx={{ color: '#b0b0b0', textAlign: 'right', mt: 0.5 }}>
                Health Score: {metrics?.systemHealth || 98}%
              </Typography>
            </Stack>
          </Box>
        </Grid>
      </Grid>
    </Box>
  );
};

export default Dashboard;
