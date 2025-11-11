import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  ButtonGroup,
  Stack,
  Card,
  CardContent,
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
  Legend,
  ResponsiveContainer,
  BarChart,
  Bar,
} from 'recharts';
import { analyticsService } from '../api/analyticsService';

const PerformanceAnalytics: React.FC = () => {
  const [range, setRange] = useState<'24h' | '7d' | '30d'>('24h');

  const { data, isLoading, error } = useQuery(
    ['analytics', range],
    () => analyticsService.getMetrics(range),
    { refetchInterval: 30000, staleTime: 5000 }
  );

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

  return (
    <Stack spacing={3}>
      {/* Time Range Selector */}
      <Box>
        <Typography variant="body1" sx={{ mb: 1 }}>
          Time Range:
        </Typography>
        <ButtonGroup variant="outlined">
          {(['24h', '7d', '30d'] as const).map((r) => (
            <Button
              key={r}
              variant={range === r ? 'contained' : 'outlined'}
              onClick={() => setRange(r)}
            >
              {r === '24h' ? '24 Hours' : r === '7d' ? '7 Days' : '30 Days'}
            </Button>
          ))}
        </ButtonGroup>
      </Box>

      {/* CPU Usage Chart */}
      <Card>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            CPU Usage
          </Typography>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={formatData(data?.cpu || [])}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Line
                type="monotone"
                dataKey="value"
                stroke="#B8D4E8"
                strokeWidth={2}
                dot={false}
                name="CPU %"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Memory Usage Chart */}
      <Card>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            Memory Usage
          </Typography>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={formatData(data?.ram || [])}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Line
                type="monotone"
                dataKey="value"
                stroke="#00D4FF"
                strokeWidth={2}
                dot={false}
                name="RAM %"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Disk Usage Chart */}
      <Card>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            Disk Usage
          </Typography>
          <ResponsiveContainer width="100%" height={300}>
            <LineChart data={formatData(data?.disk || [])}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="time" />
              <YAxis domain={[0, 100]} />
              <Tooltip />
              <Line
                type="monotone"
                dataKey="value"
                stroke="#E8D8B8"
                strokeWidth={2}
                dot={false}
                name="Disk %"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Health Score */}
      <Card>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Typography variant="h3" gutterBottom>
                System Health Score
              </Typography>
              <Typography variant="h1" sx={{ fontSize: '64px', color: 'primary.main' }}>
                {data?.healthScore || 0}%
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <ResponsiveContainer width="100%" height={200}>
                <BarChart data={[{ score: data?.healthScore || 0 }]}>
                  <Bar dataKey="score" fill="#B8D4E8" radius={[8, 8, 0, 0]} />
                  <YAxis domain={[0, 100]} />
                </BarChart>
              </ResponsiveContainer>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Stack>
  );
};

export default PerformanceAnalytics;
