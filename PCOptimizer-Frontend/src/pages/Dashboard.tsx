import React from 'react';
import { Box, Typography, Card, Grid, CardContent, Alert, CircularProgress } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { dashboardService, type DashboardMetrics } from '@/api';

interface MetricCardProps {
  label: string;
  value: string | number;
  unit?: string;
  isLoading?: boolean;
}

const MetricCard: React.FC<MetricCardProps> = ({ label, value, unit = '', isLoading = false }) => (
  <Card sx={{ height: '100%' }}>
    <CardContent>
      <Typography color="textSecondary" gutterBottom>
        {label}
      </Typography>
      {isLoading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 1 }}>
          <CircularProgress size={32} />
        </Box>
      ) : (
        <Typography variant="h5">
          {value}{unit && ` ${unit}`}
        </Typography>
      )}
    </CardContent>
  </Card>
);

const Dashboard: React.FC = () => {
  const { data: metrics, isLoading, error, refetch } = useQuery<DashboardMetrics>({
    queryKey: ['dashboard-metrics'],
    queryFn: dashboardService.getMetrics,
    refetchInterval: 5000, // Refetch every 5 seconds
    retry: true,
  });

  const formatNumber = (value: number | undefined): string => {
    if (value === undefined) return 'N/A';
    return Math.round(value).toString();
  };

  return (
    <Grid container spacing={3}>
      {/* Status Alert */}
      <Grid item xs={12}>
        {error ? (
          <Alert severity="error">
            Failed to load dashboard metrics. Make sure the backend API is running on localhost:5211
            <Box sx={{ mt: 1 }}>
              <Typography variant="caption">Error: {error instanceof Error ? error.message : 'Unknown error'}</Typography>
            </Box>
          </Alert>
        ) : (
          <Alert severity="success">
            Dashboard connected to backend API at localhost:5211
          </Alert>
        )}
      </Grid>

      {/* Metric Cards */}
      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="CPU Usage"
          value={formatNumber(metrics?.cpu)}
          unit="%"
          isLoading={isLoading}
        />
      </Grid>

      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="RAM Usage"
          value={formatNumber(metrics?.ram)}
          unit="%"
          isLoading={isLoading}
        />
      </Grid>

      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="Disk Usage"
          value={formatNumber(metrics?.disk)}
          unit="%"
          isLoading={isLoading}
        />
      </Grid>

      <Grid item xs={12} sm={6} md={3}>
        <MetricCard
          label="Temperature"
          value={formatNumber(metrics?.temperature)}
          unit="°C"
          isLoading={isLoading}
        />
      </Grid>

      {/* Additional Info Cards */}
      <Grid item xs={12} sm={6}>
        <Card sx={{ height: '100%' }}>
          <CardContent>
            <Typography color="textSecondary" gutterBottom>
              Active Processes
            </Typography>
            <Typography variant="h5">
              {isLoading ? <CircularProgress size={32} /> : metrics?.activeProcesses || 'N/A'}
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      <Grid item xs={12} sm={6}>
        <Card sx={{ height: '100%' }}>
          <CardContent>
            <Typography color="textSecondary" gutterBottom>
              System Status
            </Typography>
            <Typography
              variant="h5"
              sx={{
                color: metrics?.systemStatus === 'Healthy' ? 'success.main' : 'warning.main'
              }}
            >
              {isLoading ? <CircularProgress size={32} /> : metrics?.systemStatus || 'N/A'}
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* Last Optimization Info */}
      {metrics?.lastOptimization && (
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography color="textSecondary" gutterBottom>
                Last Optimization
              </Typography>
              <Typography variant="body1" sx={{ mb: 1 }}>
                {metrics.lastOptimization}
              </Typography>
              {metrics.lastOptimizationFixes && (
                <Typography variant="body2" color="success.main">
                  Fixed {metrics.lastOptimizationFixes} issues
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      )}

      {/* Debug Info */}
      <Grid item xs={12}>
        <Alert severity="info">
          Real-time metrics auto-refresh every 5 seconds. Visit <strong>http://localhost:5173/debug</strong> to test all API endpoints
        </Alert>
      </Grid>
    </Grid>
  );
};

export default Dashboard;
