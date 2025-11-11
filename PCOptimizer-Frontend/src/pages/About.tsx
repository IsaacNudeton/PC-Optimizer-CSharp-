import React from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  CircularProgress,
  Stack,
  Alert,
} from '@mui/material';
import { systemService } from '../api/systemService';

const About: React.FC = () => {
  const { data, isLoading, error } = useQuery(['systemInfo'], () =>
    systemService.getSystemInfo()
  );

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">Failed to load system information</Alert>;
  }

  return (
    <Grid container spacing={3}>
      {/* Branding Card */}
      <Grid item xs={12}>
        <Card>
          <CardContent sx={{ textAlign: 'center', py: 6 }}>
            <Typography variant="h1" sx={{ fontSize: '64px', mb: 2 }}>
              ◊
            </Typography>
            <Typography variant="h2" gutterBottom>
              PC Optimizer
            </Typography>
            <Typography variant="h3" gutterBottom>
              Event Horizon Edition
            </Typography>
            <Typography variant="body1" sx={{ mt: 2, color: 'text.secondary' }}>
              Advanced System Optimization and Monitoring Suite
            </Typography>
            <Typography variant="body2" sx={{ mt: 1 }}>
              Version {data?.appVersion}
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* System Information */}
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Typography variant="h3" gutterBottom>
              System Information
            </Typography>

            <Stack spacing={2} sx={{ mt: 3 }}>
              <Box sx={{ display: 'grid', gridTemplateColumns: 'auto 1fr', gap: 3 }}>
                <Typography variant="body1" sx={{ fontWeight: 600 }}>
                  Operating System:
                </Typography>
                <Typography variant="body1">
                  {data?.osName} {data?.osVersion}
                </Typography>

                <Typography variant="body1" sx={{ fontWeight: 600 }}>
                  CPU:
                </Typography>
                <Typography variant="body1">
                  {data?.cpuModel} ({data?.cpuCores} cores)
                </Typography>

                <Typography variant="body1" sx={{ fontWeight: 600 }}>
                  Total RAM:
                </Typography>
                <Typography variant="body1">
                  {((data?.totalRam || 0) / 1024 / 1024 / 1024).toFixed(2)} GB
                </Typography>

                <Typography variant="body1" sx={{ fontWeight: 600 }}>
                  Total Disk Space:
                </Typography>
                <Typography variant="body1">
                  {((data?.totalDisk || 0) / 1024 / 1024 / 1024 / 1024).toFixed(2)} TB
                </Typography>
              </Box>
            </Stack>
          </CardContent>
        </Card>
      </Grid>

      {/* About CosmicUI */}
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Typography variant="h3" gutterBottom>
              About CosmicUI Design System
            </Typography>

            <Typography variant="body1" sx={{ mt: 2, lineHeight: 1.8 }}>
              PC Optimizer uses <strong>CosmicUI</strong>, a custom design system inspired by cosmic and
              otherworldly themes. The interface features:
            </Typography>

            <Stack sx={{ mt: 2, gap: 1, ml: 2 }}>
              <Typography variant="body2">
                • <strong>Universal Theme:</strong> Cosmic monochrome with Hawking Radiation blue accents
              </Typography>
              <Typography variant="body2">
                • <strong>Gaming Theme:</strong> Aggressive red and black color scheme
              </Typography>
              <Typography variant="body2">
                • <strong>Work Theme:</strong> Light, ergonomic daylight cosmos design
              </Typography>
              <Typography variant="body2">
                • <strong>Accent Overlays:</strong> Pink, Purple, and Blue customization options
              </Typography>
              <Typography variant="body2">
                • <strong>Real-Time Updates:</strong> Dashboard metrics refresh every 3 seconds
              </Typography>
            </Stack>
          </CardContent>
        </Card>
      </Grid>

      {/* Credits */}
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Typography variant="h3" gutterBottom>
              Technology Stack
            </Typography>

            <Grid container spacing={2} sx={{ mt: 1 }}>
              <Grid item xs={12} sm={6}>
                <Stack spacing={1}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    Frontend:
                  </Typography>
                  <Typography variant="caption">React 18</Typography>
                  <Typography variant="caption">Material-UI (MUI) v5</Typography>
                  <Typography variant="caption">Electron</Typography>
                  <Typography variant="caption">TanStack Query</Typography>
                  <Typography variant="caption">Recharts</Typography>
                </Stack>
              </Grid>

              <Grid item xs={12} sm={6}>
                <Stack spacing={1}>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>
                    Backend:
                  </Typography>
                  <Typography variant="caption">.NET 9 (C#)</Typography>
                  <Typography variant="caption">ASP.NET Core</Typography>
                  <Typography variant="caption">WPF Desktop</Typography>
                </Stack>
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </Grid>

      {/* License */}
      <Grid item xs={12}>
        <Card>
          <CardContent>
            <Typography variant="body2" sx={{ color: 'text.secondary', textAlign: 'center' }}>
              © 2024 PC Optimizer - Event Horizon Edition. All rights reserved.
            </Typography>
          </CardContent>
        </Card>
      </Grid>
    </Grid>
  );
};

export default About;
