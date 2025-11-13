import React, { useState, useEffect } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Box,
  Typography,
  FormControlLabel,
  Radio,
  RadioGroup,
  Switch,
  Button,
  Stack,
  CircularProgress,
  Alert,
} from '@mui/material';
import { settingsService } from '../api/settingsService';

const Settings: React.FC = () => {
  // Theme-related variables removed - use EnhancedSettings page for theme customization
  const { data: prefs, isLoading } = useQuery({
    queryKey: ['userPrefs'],
    queryFn: () => settingsService.getPreferences(),
  });

  const [localPrefs, setLocalPrefs] = useState(prefs);

  useEffect(() => {
    if (prefs) setLocalPrefs(prefs);
  }, [prefs]);

  const updateMutation = useMutation({
    mutationFn: (newPrefs: any) => settingsService.updatePreferences(newPrefs),
    onSuccess: () => {
      alert('Settings saved successfully!');
    },
  });

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <CircularProgress />
      </Box>
    );
  }

  // TODO: Connect to PCOptimizer-API endpoints:
  // GET /api/settings/preferences - Get user preferences
  // POST /api/settings/preferences - Update user preferences
  // GET /api/settings/monitoring-modes - Get available monitoring modes

  const SettingRow: React.FC<{ label: string; children: React.ReactNode }> = ({ label, children }) => (
    <Box sx={{
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      p: 2,
      borderBottom: '1px solid #333',
      '&:last-child': { borderBottom: 'none' },
    }}>
      <Typography sx={{ fontWeight: 500, fontSize: '14px' }}>{label}</Typography>
      {children}
    </Box>
  );

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h3" sx={{ fontWeight: 900, mb: 0.5, fontSize: '42px', letterSpacing: '-1px' }}>
          Settings
        </Typography>
        <Typography variant="body1" sx={{ color: '#b0b0b0', fontSize: '16px' }}>
          Configure your optimization preferences and monitoring behavior
        </Typography>
      </Box>

      {/* Application Settings */}
      <Box sx={{
        mb: 3,
        background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
        border: '1px solid #444',
        borderRadius: '16px',
        overflow: 'hidden',
        transition: 'all 0.3s ease',
        '&:hover': {
          border: '1px solid #666',
          boxShadow: '0 8px 24px rgba(0, 0, 0, 0.5)',
        },
      }}>
        <Box sx={{ p: 3, borderBottom: '1px solid #444', backgroundColor: 'rgba(0, 0, 0, 0.3)' }}>
          <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', m: 0 }}>
            Application
          </Typography>
          <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
            General app behavior and startup options
          </Typography>
        </Box>
        <Stack spacing={0}>
          <SettingRow label="Enable automatic optimization">
            <Switch defaultChecked />
          </SettingRow>
          <SettingRow label="Show notifications">
            <Switch defaultChecked />
          </SettingRow>
          <SettingRow label="Dark mode">
            <Typography sx={{ color: '#4CAF50', fontWeight: 600, fontSize: '12px' }}>ENABLED</Typography>
          </SettingRow>
          <SettingRow label="Check for updates on startup">
            <Switch defaultChecked />
          </SettingRow>
        </Stack>
      </Box>

      {/* Monitoring Settings */}
      <Box sx={{
        mb: 3,
        background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
        border: '1px solid #444',
        borderRadius: '16px',
        overflow: 'hidden',
        transition: 'all 0.3s ease',
        '&:hover': {
          border: '1px solid #666',
          boxShadow: '0 8px 24px rgba(0, 0, 0, 0.5)',
        },
      }}>
        <Box sx={{ p: 3, borderBottom: '1px solid #444', backgroundColor: 'rgba(0, 0, 0, 0.3)' }}>
          <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', m: 0 }}>
            Monitoring
          </Typography>
          <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
            System metrics collection and detection settings
          </Typography>
        </Box>
        <Box sx={{ p: 3 }}>
          <Typography variant="body2" sx={{ mb: 2, color: '#FFB800', fontWeight: 600, fontSize: '12px' }}>
            MONITORING MODE
          </Typography>
          <RadioGroup
            value={localPrefs?.monitoringMode || 'active'}
            onChange={(e) =>
              setLocalPrefs({
                ...localPrefs!,
                monitoringMode: e.target.value as any,
              })
            }
            sx={{ mb: 3 }}
          >
            <FormControlLabel
              value="active"
              control={<Radio />}
              label={<Box><Typography variant="body2" sx={{ fontWeight: 500 }}>Active Mode</Typography><Typography variant="caption" sx={{ color: '#b0b0b0' }}>Poll every 1-2 seconds</Typography></Box>}
            />
            <FormControlLabel
              value="background"
              control={<Radio />}
              label={<Box><Typography variant="body2" sx={{ fontWeight: 500 }}>Background Mode</Typography><Typography variant="caption" sx={{ color: '#b0b0b0' }}>Poll every 5-10 seconds</Typography></Box>}
            />
            <FormControlLabel
              value="paused"
              control={<Radio />}
              label={<Box><Typography variant="body2" sx={{ fontWeight: 500 }}>Paused</Typography><Typography variant="caption" sx={{ color: '#b0b0b0' }}>No polling</Typography></Box>}
            />
          </RadioGroup>

          <Stack spacing={0}>
            <SettingRow label="Enable GPU monitoring">
              <Switch
                checked={localPrefs?.enableGpuMonitoring || false}
                onChange={(e) =>
                  setLocalPrefs({
                    ...localPrefs!,
                    enableGpuMonitoring: e.target.checked,
                  })
                }
              />
            </SettingRow>
            <SettingRow label="Enable temperature sensors">
              <Switch
                checked={localPrefs?.enableTemperatureSensors || false}
                onChange={(e) =>
                  setLocalPrefs({
                    ...localPrefs!,
                    enableTemperatureSensors: e.target.checked,
                  })
                }
              />
            </SettingRow>
            <SettingRow label="Enable ML anomaly detection">
              <Switch
                checked={localPrefs?.enableMLDetection || false}
                onChange={(e) =>
                  setLocalPrefs({
                    ...localPrefs!,
                    enableMLDetection: e.target.checked,
                  })
                }
              />
            </SettingRow>
          </Stack>
        </Box>
      </Box>

      {/* Advanced Settings */}
      <Box sx={{
        mb: 3,
        background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
        border: '1px solid #444',
        borderRadius: '16px',
        overflow: 'hidden',
        transition: 'all 0.3s ease',
        '&:hover': {
          border: '1px solid #666',
          boxShadow: '0 8px 24px rgba(0, 0, 0, 0.5)',
        },
      }}>
        <Box sx={{ p: 3, borderBottom: '1px solid #444', backgroundColor: 'rgba(0, 0, 0, 0.3)' }}>
          <Typography variant="h6" sx={{ fontWeight: 700, fontSize: '16px', m: 0 }}>
            Advanced
          </Typography>
          <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
            System integration and safety options
          </Typography>
        </Box>
        <Stack spacing={0}>
          <SettingRow label="Run on Windows startup">
            <Switch
              checked={localPrefs?.runOnStartup || false}
              onChange={(e) =>
                setLocalPrefs({
                  ...localPrefs!,
                  runOnStartup: e.target.checked,
                })
              }
            />
          </SettingRow>
          <SettingRow label="Minimize to system tray">
            <Switch
              checked={localPrefs?.minimizeToTray || false}
              onChange={(e) =>
                setLocalPrefs({
                  ...localPrefs!,
                  minimizeToTray: e.target.checked,
                })
              }
            />
          </SettingRow>
          <SettingRow label="Create restore point before optimizations">
            <Switch
              checked={localPrefs?.createRestorePoints || false}
              onChange={(e) =>
                setLocalPrefs({
                  ...localPrefs!,
                  createRestorePoints: e.target.checked,
                })
              }
            />
          </SettingRow>
          <SettingRow label="Confirm dangerous actions">
            <Switch
              checked={localPrefs?.confirmDangerous || false}
              onChange={(e) =>
                setLocalPrefs({
                  ...localPrefs!,
                  confirmDangerous: e.target.checked,
                })
              }
            />
          </SettingRow>
        </Stack>
      </Box>

      {/* Save Actions */}
      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <Button
          variant="contained"
          size="large"
          sx={{
            backgroundColor: '#FFB800',
            color: '#000',
            fontWeight: 'bold',
            transition: 'all 0.3s ease',
            '&:hover': {
              backgroundColor: '#FFC933',
              transform: 'translateY(-2px)',
              boxShadow: '0 8px 24px rgba(255, 184, 0, 0.3)',
            },
            '&:disabled': {
              backgroundColor: '#666',
            },
          }}
          onClick={() => updateMutation.mutate(localPrefs!)}
          disabled={updateMutation.isPending}
        >
          {updateMutation.isPending ? 'Saving...' : 'Save Settings'}
        </Button>
        <Button
          variant="outlined"
          size="large"
          sx={{
            borderColor: '#666',
            color: '#FFF',
            fontWeight: 'bold',
            transition: 'all 0.3s ease',
            '&:hover': {
              borderColor: '#999',
              backgroundColor: 'rgba(255, 184, 0, 0.05)',
            },
          }}
          onClick={() => setLocalPrefs(prefs)}
        >
          Reset
        </Button>
      </Box>

      {updateMutation.isError && (
        <Alert severity="error" sx={{ mb: 3 }}>
          Failed to save settings
        </Alert>
      )}
    </Box>
  );
};

export default Settings;
