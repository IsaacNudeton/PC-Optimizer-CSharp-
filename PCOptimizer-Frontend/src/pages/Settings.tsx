import React, { useState, useEffect } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Box,
  Card,
  CardContent,
  Typography,
  FormControlLabel,
  Radio,
  RadioGroup,
  Checkbox,
  Button,
  Stack,
  CircularProgress,
  Alert,
  Grid,
} from '@mui/material';
import { settingsService } from '../api/settingsService';
import { useCosmicTheme } from '../context/ThemeContext';

const Settings: React.FC = () => {
  const { profile, accent, setProfile, setAccent } = useCosmicTheme();
  const { data: prefs, isLoading } = useQuery(['userPrefs'], () =>
    settingsService.getPreferences()
  );

  const [localPrefs, setLocalPrefs] = useState(prefs);

  useEffect(() => {
    if (prefs) setLocalPrefs(prefs);
  }, [prefs]);

  const updateMutation = useMutation((newPrefs) => settingsService.updatePreferences(newPrefs), {
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

  return (
    <Stack spacing={3}>
      {/* Monitoring Settings */}
      <Card>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            Monitoring Settings
          </Typography>

          <Typography variant="body1" sx={{ mb: 2, mt: 3 }}>
            Monitoring Mode
          </Typography>
          <RadioGroup
            value={localPrefs?.monitoringMode || 'active'}
            onChange={(e) =>
              setLocalPrefs({
                ...localPrefs!,
                monitoringMode: e.target.value as any,
              })
            }
          >
            <FormControlLabel value="active" control={<Radio />} label="Active Mode (poll every 1-2s)" />
            <FormControlLabel value="background" control={<Radio />} label="Background Mode (poll every 5-10s)" />
            <FormControlLabel value="paused" control={<Radio />} label="Paused (no polling)" />
          </RadioGroup>

          <Stack sx={{ mt: 3, gap: 1 }}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.enableGpuMonitoring || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      enableGpuMonitoring: e.target.checked,
                    })
                  }
                />
              }
              label="Enable GPU monitoring"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.enableTemperatureSensors || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      enableTemperatureSensors: e.target.checked,
                    })
                  }
                />
              }
              label="Enable temperature sensors"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.enableMLDetection || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      enableMLDetection: e.target.checked,
                    })
                  }
                />
              }
              label="Enable ML anomaly detection"
            />
          </Stack>
        </CardContent>
      </Card>

      {/* Theme Settings */}
      <Card>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            CosmicUI Theme Settings
          </Typography>

          <Grid container spacing={4}>
            <Grid item xs={12} md={6}>
              <Typography variant="h4">Profile</Typography>
              <Typography variant="body2" sx={{ mb: 2, color: 'text.secondary' }}>
                Select your preferred theme profile
              </Typography>
              <RadioGroup
                value={profile}
                onChange={(e) => setProfile(e.target.value as any)}
              >
                <FormControlLabel
                  value="Universal"
                  control={<Radio />}
                  label="Universal (Cosmic Monochrome - Default)"
                />
                <FormControlLabel
                  value="Gaming"
                  control={<Radio />}
                  label="Gaming (Red/Black Aggressive)"
                />
                <FormControlLabel
                  value="Work"
                  control={<Radio />}
                  label="Work (Light Daylight)"
                />
              </RadioGroup>
            </Grid>

            <Grid item xs={12} md={6}>
              <Typography variant="h4">Accent Overlay</Typography>
              <Typography variant="body2" sx={{ mb: 2, color: 'text.secondary' }}>
                Customize accent colors
              </Typography>
              <RadioGroup
                value={accent}
                onChange={(e) => setAccent(e.target.value as any)}
              >
                <FormControlLabel
                  value="Default"
                  control={<Radio />}
                  label="Default (Hawking Radiation Blue)"
                />
                <FormControlLabel value="Pink" control={<Radio />} label="Pink (Hello Kitty)" />
                <FormControlLabel value="Purple" control={<Radio />} label="Purple (Nebula)" />
                <FormControlLabel value="Blue" control={<Radio />} label="Blue (Aquatic)" />
              </RadioGroup>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Advanced Settings */}
      <Card>
        <CardContent>
          <Typography variant="h3" gutterBottom>
            Advanced Settings
          </Typography>

          <Stack sx={{ gap: 1 }}>
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.runOnStartup || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      runOnStartup: e.target.checked,
                    })
                  }
                />
              }
              label="Run on Windows startup"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.minimizeToTray || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      minimizeToTray: e.target.checked,
                    })
                  }
                />
              }
              label="Minimize to system tray"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.createRestorePoints || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      createRestorePoints: e.target.checked,
                    })
                  }
                />
              }
              label="Create restore point before optimizations"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={localPrefs?.confirmDangerous || false}
                  onChange={(e) =>
                    setLocalPrefs({
                      ...localPrefs!,
                      confirmDangerous: e.target.checked,
                    })
                  }
                />
              }
              label="Confirm dangerous actions"
            />
          </Stack>

          <Box sx={{ mt: 3, display: 'flex', gap: 2 }}>
            <Button
              variant="contained"
              onClick={() => updateMutation.mutate(localPrefs!)}
              disabled={updateMutation.isPending}
            >
              {updateMutation.isPending ? 'Saving...' : 'Save Settings'}
            </Button>
            <Button variant="outlined" onClick={() => setLocalPrefs(prefs)}>
              Reset
            </Button>
          </Box>

          {updateMutation.isError && (
            <Alert severity="error" sx={{ mt: 2 }}>
              Failed to save settings
            </Alert>
          )}
        </CardContent>
      </Card>
    </Stack>
  );
};

export default Settings;
