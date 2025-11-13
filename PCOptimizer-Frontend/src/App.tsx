import React from 'react';
import { ThemeProvider as MuiThemeProvider } from '@mui/material/styles';
import { Box, CssBaseline } from '@mui/material';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import { EnhancedThemeProvider, useEnhancedTheme } from './contexts/EnhancedThemeContext';
import { useProfileDetection } from './hooks/useProfileDetection';
import MainLayout from './layouts/MainLayout';

// Pages
import Dashboard from './pages/Dashboard';
import AdvancedOptimizer from './pages/AdvancedOptimizer';
import PerformanceAnalytics from './pages/PerformanceAnalytics';
import OperationHistory from './pages/OperationHistory';
import Settings from './pages/Settings';
import About from './pages/About';
import Debug from './pages/Debug';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 2,
      staleTime: 5000,
      refetchOnWindowFocus: false,
    },
  },
});

interface AppContentProps {
  children: React.ReactNode;
}

const AppContent: React.FC<AppContentProps> = ({ children }) => {
  const { muiTheme } = useEnhancedTheme();
  
  // Enable profile detection
  useProfileDetection();

  return (
    <MuiThemeProvider theme={muiTheme}>
      <CssBaseline />
      <Box sx={{ bgcolor: 'background.default', minHeight: '100vh' }}>
        {children}
      </Box>
    </MuiThemeProvider>
  );
};

const AppRouter: React.FC = () => {
  return (
    <Routes>
      <Route path="/" element={<MainLayout><Dashboard /></MainLayout>} />
      <Route path="/optimizer" element={<MainLayout pageTitle="Advanced Optimizer" pageSubtitle="Professional system optimization tools and tweaks"><AdvancedOptimizer /></MainLayout>} />
      <Route path="/analytics" element={<MainLayout pageTitle="Performance Analytics" pageSubtitle="Historical data visualization and anomaly detection"><PerformanceAnalytics /></MainLayout>} />
      <Route path="/history" element={<MainLayout pageTitle="Operation History" pageSubtitle="Timeline of all system optimizations"><OperationHistory /></MainLayout>} />
      <Route path="/settings" element={<MainLayout pageTitle="Settings" pageSubtitle="Configure application behavior and preferences"><Settings /></MainLayout>} />
      <Route path="/about" element={<MainLayout pageTitle="About" pageSubtitle="Application information and system details"><About /></MainLayout>} />
      <Route path="/debug" element={<Debug />} />
    </Routes>
  );
};

export default function App() {
  return (
    <EnhancedThemeProvider>
      <QueryClientProvider client={queryClient}>
        <AppContent>
          <Router>
            <AppRouter />
          </Router>
        </AppContent>
      </QueryClientProvider>
    </EnhancedThemeProvider>
  );
}
