import React from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  Grid,
  CircularProgress,
  Stack,
  Alert,
} from '@mui/material';
import { systemService } from '../api/systemService';

const About: React.FC = () => {
  const { data, isLoading, error } = useQuery({
    queryKey: ['systemInfo'],
    queryFn: () => systemService.getSystemInfo(),
  });

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

  const InfoCard: React.FC<{ title: string; icon?: string; children: React.ReactNode }> = ({ title, icon, children }) => (
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
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
        {icon && <Typography sx={{ fontSize: '24px' }}>{icon}</Typography>}
        <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', m: 0 }}>
          {title}
        </Typography>
      </Box>
      {children}
    </Box>
  );

  return (
    <Stack spacing={4}>
      {/* Branding Hero Section */}
      <Box sx={{
        background: 'linear-gradient(135deg, #FFB800 0%, #FFA500 50%, #FF9800 100%)',
        borderRadius: '20px',
        p: 6,
        textAlign: 'center',
        position: 'relative',
        overflow: 'hidden',
        transition: 'all 0.3s ease',
        '&::before': {
          content: '""',
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: 'radial-gradient(circle at 20% 50%, rgba(255, 184, 0, 0.2), transparent 50%), radial-gradient(circle at 80% 80%, rgba(255, 152, 0, 0.2), transparent 50%)',
          pointerEvents: 'none',
        },
        '&:hover': {
          boxShadow: '0 12px 48px rgba(255, 184, 0, 0.3)',
        },
      }}>
        <Typography sx={{
          fontSize: '80px',
          fontWeight: 900,
          mb: 2,
          color: '#000',
          textShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
          zIndex: 1,
          position: 'relative',
        }}>
          ◇
        </Typography>
        <Typography sx={{
          fontSize: '56px',
          fontWeight: 900,
          mb: 1,
          color: '#000',
          letterSpacing: '-2px',
          zIndex: 1,
          position: 'relative',
        }}>
          PC Optimizer
        </Typography>
        <Typography sx={{
          fontSize: '28px',
          fontWeight: 700,
          mb: 3,
          color: 'rgba(0, 0, 0, 0.8)',
          zIndex: 1,
          position: 'relative',
        }}>
          Event Horizon Edition
        </Typography>
        <Typography sx={{
          fontSize: '16px',
          fontWeight: 500,
          color: 'rgba(0, 0, 0, 0.7)',
          mb: 2,
          zIndex: 1,
          position: 'relative',
        }}>
          Advanced System Optimization & Monitoring Suite
        </Typography>
        <Typography sx={{
          fontSize: '14px',
          fontWeight: 600,
          color: 'rgba(0, 0, 0, 0.6)',
          zIndex: 1,
          position: 'relative',
        }}>
          v{data?.appVersion} • Designed for Performance
        </Typography>
      </Box>

      {/* System Information Grid */}
      <Box>
        <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', mb: 2, color: '#FFB800' }}>
          SYSTEM INFORMATION
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} sm={6}>
            <InfoCard icon="💻" title="Operating System">
              <Typography sx={{ color: '#e0e0e0', fontWeight: 500 }}>
                {data?.osName} {data?.osVersion}
              </Typography>
            </InfoCard>
          </Grid>
          <Grid item xs={12} sm={6}>
            <InfoCard icon="⚙️" title="Processor">
              <Typography sx={{ color: '#e0e0e0', fontWeight: 500 }}>
                {data?.cpuModel}
              </Typography>
              <Typography variant="caption" sx={{ color: '#b0b0b0', mt: 1, display: 'block' }}>
                {data?.cpuCores} cores
              </Typography>
            </InfoCard>
          </Grid>
          <Grid item xs={12} sm={6}>
            <InfoCard icon="🧠" title="Total Memory">
              <Typography sx={{ color: '#00D4FF', fontWeight: 600, fontSize: '20px' }}>
                {((data?.totalRam || 0) / 1024 / 1024 / 1024).toFixed(2)} GB
              </Typography>
            </InfoCard>
          </Grid>
          <Grid item xs={12} sm={6}>
            <InfoCard icon="💾" title="Storage Capacity">
              <Typography sx={{ color: '#E8D8B8', fontWeight: 600, fontSize: '20px' }}>
                {((data?.totalDisk || 0) / 1024 / 1024 / 1024 / 1024).toFixed(2)} TB
              </Typography>
            </InfoCard>
          </Grid>
        </Grid>
      </Box>

      {/* Design System */}
      <Box>
        <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', mb: 2, color: '#FFB800' }}>
          DESIGN & EXPERIENCE
        </Typography>
        <InfoCard title="CosmicUI Design System">
          <Typography variant="body2" sx={{ color: '#b0b0b0', lineHeight: 1.8, mb: 2 }}>
            Built with a modern, premium design system optimized for performance professionals.
          </Typography>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6}>
              <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5 }}>
                <Typography sx={{ color: '#FFB800', fontWeight: 700 }}>▪</Typography>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>
                    Dark Theme
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
                    Eye-friendly cosmic design
                  </Typography>
                </Box>
              </Box>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5 }}>
                <Typography sx={{ color: '#FFB800', fontWeight: 700 }}>▪</Typography>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>
                    Gold Accents
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
                    Premium color palette
                  </Typography>
                </Box>
              </Box>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5 }}>
                <Typography sx={{ color: '#FFB800', fontWeight: 700 }}>▪</Typography>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>
                    Real-Time Metrics
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
                    Live system monitoring
                  </Typography>
                </Box>
              </Box>
            </Grid>
            <Grid item xs={12} sm={6}>
              <Box sx={{ display: 'flex', gap: 1.5, mb: 1.5 }}>
                <Typography sx={{ color: '#FFB800', fontWeight: 700 }}>▪</Typography>
                <Box>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>
                    Advanced Analytics
                  </Typography>
                  <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
                    Performance insights
                  </Typography>
                </Box>
              </Box>
            </Grid>
          </Grid>
        </InfoCard>
      </Box>

      {/* Technology Stack */}
      <Box>
        <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', mb: 2, color: '#FFB800' }}>
          TECHNOLOGY STACK
        </Typography>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <InfoCard title="Frontend Technologies">
              <Stack spacing={1.5}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>React</Typography>
                  <Typography variant="caption" sx={{ color: '#4CAF50' }}>18</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>Material-UI</Typography>
                  <Typography variant="caption" sx={{ color: '#00D4FF' }}>v5</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>Electron</Typography>
                  <Typography variant="caption" sx={{ color: '#FFB800' }}>40.0</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>TanStack Query</Typography>
                  <Typography variant="caption" sx={{ color: '#FF9800' }}>v5</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>Recharts</Typography>
                  <Typography variant="caption" sx={{ color: '#E8D8B8' }}>2.x</Typography>
                </Box>
              </Stack>
            </InfoCard>
          </Grid>

          <Grid item xs={12} md={6}>
            <InfoCard title="Backend Technologies">
              <Stack spacing={1.5}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>.NET Runtime</Typography>
                  <Typography variant="caption" sx={{ color: '#512BD4' }}>9.0</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>ASP.NET Core</Typography>
                  <Typography variant="caption" sx={{ color: '#512BD4' }}>9.0</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>Language</Typography>
                  <Typography variant="caption" sx={{ color: '#239120' }}>C#</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', pb: 1.5, borderBottom: '1px solid #333' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>Database</Typography>
                  <Typography variant="caption" sx={{ color: '#FF9800' }}>SQL</Typography>
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                  <Typography variant="body2" sx={{ fontWeight: 600, color: '#e0e0e0' }}>Desktop UI</Typography>
                  <Typography variant="caption" sx={{ color: '#512BD4' }}>WPF</Typography>
                </Box>
              </Stack>
            </InfoCard>
          </Grid>
        </Grid>
      </Box>

      {/* Footer */}
      <Box sx={{
        textAlign: 'center',
        py: 4,
        borderTop: '1px solid #444',
      }}>
        <Typography variant="body2" sx={{ color: '#b0b0b0', mb: 1 }}>
          © 2024 PC Optimizer - Event Horizon Edition
        </Typography>
        <Typography variant="caption" sx={{ color: '#666' }}>
          Advanced System Optimization & Monitoring Suite
        </Typography>
      </Box>
    </Stack>
  );
};

export default About;
