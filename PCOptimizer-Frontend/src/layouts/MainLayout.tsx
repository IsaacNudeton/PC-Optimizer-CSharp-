import React, { useState, useEffect } from 'react';
import {
  Box,
  Drawer,
  AppBar,
  Toolbar,
  Button,
  Stack,
  Typography,
  Chip,
  useTheme,
} from '@mui/material';
import { useLocation, useNavigate } from 'react-router-dom';
import DashboardIcon from '@mui/icons-material/Dashboard';
import BoltIcon from '@mui/icons-material/Bolt';
import TimelineIcon from '@mui/icons-material/Timeline';
import HistoryIcon from '@mui/icons-material/History';
import SettingsIcon from '@mui/icons-material/Settings';
import InfoIcon from '@mui/icons-material/Info';

const DRAWER_WIDTH = 220;

const PAGES = [
  { id: 'dashboard', label: 'Dashboard', icon: DashboardIcon, path: '/' },
  { id: 'optimizer', label: 'Advanced Optimizer', icon: BoltIcon, path: '/optimizer' },
  { id: 'analytics', label: 'Analytics', icon: TimelineIcon, path: '/analytics' },
  { id: 'history', label: 'History Log', icon: HistoryIcon, path: '/history' },
  { id: 'settings', label: 'Settings', icon: SettingsIcon, path: '/settings' },
  { id: 'about', label: 'About', icon: InfoIcon, path: '/about' },
];

interface MainLayoutProps {
  children: React.ReactNode;
  pageTitle?: string;
  pageSubtitle?: string;
}

const MainLayout: React.FC<MainLayoutProps> = ({ children, pageTitle = 'Dashboard', pageSubtitle = 'Real-time system monitoring' }) => {
  const theme = useTheme();
  const navigate = useNavigate();
  const location = useLocation();
  const [currentTime, setCurrentTime] = useState(new Date().toLocaleTimeString());

  useEffect(() => {
    const interval = setInterval(() => {
      setCurrentTime(new Date().toLocaleTimeString());
    }, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>
      {/* Left Sidebar */}
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          flexShrink: 0,
          '& .MuiDrawer-paper': {
            width: DRAWER_WIDTH,
            boxSizing: 'border-box',
            bgcolor: 'background.paper',
            borderRight: '1px solid #444',
          },
        }}
      >
        {/* Logo Section */}
        <Box
          sx={{
            p: 2,
            borderBottom: '1px solid #444',
            textAlign: 'center',
          }}
        >
          <Typography variant="h1" sx={{ fontSize: '28px', mb: 1 }}>
            ◊
          </Typography>
          <Typography variant="h4" sx={{ fontSize: '16px', mb: 0.5, fontWeight: 600 }}>
            PC Optimizer
          </Typography>
          <Typography variant="caption" sx={{ display: 'block', color: 'primary.main' }}>
            Event Horizon
          </Typography>
        </Box>

        {/* Navigation Items */}
        <Stack sx={{ p: 2, flexGrow: 1, gap: 1 }}>
          {PAGES.map((page) => {
            const Icon = page.icon;
            const isActive = location.pathname === page.path;

            return (
              <Button
                key={page.id}
                variant={isActive ? 'contained' : 'outlined'}
                startIcon={<Icon sx={{ fontSize: '20px' }} />}
                onClick={() => navigate(page.path)}
                fullWidth
                sx={{
                  justifyContent: 'flex-start',
                  pl: 2,
                  borderRadius: '8px',
                  textTransform: 'none',
                  fontWeight: 600,
                  fontSize: '13px',
                  borderLeft: isActive ? '3px solid' : 'none',
                  borderLeftColor: 'primary.main',
                  ...(isActive && {
                    borderLeft: '3px solid',
                    borderLeftColor: 'primary.main',
                  }),
                }}
              >
                {page.label}
              </Button>
            );
          })}
        </Stack>

        {/* Footer Status */}
        <Box
          sx={{
            p: 2,
            borderTop: `2px solid ${theme.palette.primary.main}`,
          }}
        >
          <Typography variant="caption" sx={{ display: 'block', mb: 1 }}>
            Status
          </Typography>
          <Box sx={{ width: '100%' }}>
            <Chip label="Active Mode" color="success" variant="outlined" size="small" />
          </Box>
        </Box>
      </Drawer>

      {/* Main Content */}
      <Box sx={{ flexGrow: 1, display: 'flex', flexDirection: 'column' }}>
        {/* Top Header Bar */}
        <AppBar position="static">
          <Toolbar
            sx={{
              justifyContent: 'space-between',
              py: 2,
              bgcolor: 'background.paper',
              borderBottom: `2px solid ${theme.palette.primary.main}`,
            }}
          >
            <Box>
              <Typography variant="h2" sx={{ fontSize: '28px', mb: 0.5 }}>
                {pageTitle}
              </Typography>
              <Typography variant="body2" sx={{ color: 'text.secondary' }}>
                {pageSubtitle}
              </Typography>
            </Box>

            <Box sx={{ textAlign: 'right' }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, justifyContent: 'flex-end', mb: 1 }}>
                <Box
                  sx={{
                    width: 12,
                    height: 12,
                    borderRadius: '50%',
                    bgcolor: 'success.main',
                  }}
                />
                <Typography variant="body1">Active Mode</Typography>
              </Box>
              <Typography variant="caption">{currentTime}</Typography>
            </Box>
          </Toolbar>
        </AppBar>

        {/* Content Area */}
        <Box
          sx={{
            flexGrow: 1,
            p: 3,
            overflow: 'auto',
            bgcolor: 'background.default',
          }}
        >
          {children}
        </Box>
      </Box>
    </Box>
  );
};

export default MainLayout;
