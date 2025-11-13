import React, { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Box,
  Card,
  CardContent,
  CardHeader,
  Typography,
  Switch,
  Button,
  Stack,
  LinearProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  CircularProgress,
  Alert,
} from '@mui/material';
import { optimizerService } from '../api/optimizerService';

const AdvancedOptimizer: React.FC = () => {
  // TODO: Connect to PCOptimizer-API endpoints:
  // GET /api/optimizer/modules - Get available optimization modules
  // POST /api/optimizer/run - Run selected optimization modules
  // GET /api/optimizer/progress/{jobId} - Get optimization progress
  // GET /api/optimizer/results/{jobId} - Get optimization results

  const { data: modules, isLoading } = useQuery({
    queryKey: ['optimizerModules'],
    queryFn: () => optimizerService.getModules(),
  });

  const [selectedModules, setSelectedModules] = useState<string[]>([]);
  const [jobId, setJobId] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [currentModule, setCurrentModule] = useState('');
  const [showResults, setShowResults] = useState(false);
  const [results, setResults] = useState<any>(null);

  const runMutation = useMutation({
    mutationFn: () => optimizerService.runOptimization(selectedModules),
    onSuccess: (res) => {
      setJobId(res.jobId);
      setProgress(0);
      pollProgress(res.jobId);
    },
  });

  const pollProgress = async (id: string) => {
    const interval = setInterval(async () => {
      try {
        const p = await optimizerService.getProgress(id);
        setProgress(p.progress);
        setCurrentModule(p.currentModule);

        if (p.progress >= 100) {
          clearInterval(interval);
          const res = await optimizerService.getResults(id);
          setResults(res);
          setShowResults(true);
        }
      } catch (error) {
        clearInterval(interval);
      }
    }, 500);
  };

  const handleModuleToggle = (moduleId: string) => {
    setSelectedModules((prev) =>
      prev.includes(moduleId) ? prev.filter((id) => id !== moduleId) : [...prev, moduleId]
    );
  };

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      {/* Header */}
      <Box sx={{ mb: 4 }}>
        <Typography variant="h3" sx={{ fontWeight: 900, mb: 0.5, fontSize: '42px', letterSpacing: '-1px' }}>
          Performance Tuning
        </Typography>
        <Typography variant="body1" sx={{ color: '#b0b0b0', fontSize: '16px' }}>
          Configure advanced optimization modules to maximize system performance and responsiveness
        </Typography>
      </Box>

      {/* Selected Modules Count */}
      {selectedModules.length > 0 && (
        <Box sx={{
          mb: 3,
          p: 2,
          background: 'linear-gradient(135deg, rgba(255, 184, 0, 0.1) 0%, rgba(255, 152, 0, 0.05) 100%)',
          border: '1px solid rgba(255, 184, 0, 0.3)',
          borderRadius: '12px',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}>
          <Typography variant="body2" sx={{ fontWeight: '600', color: '#FFB800' }}>
            {selectedModules.length} module{selectedModules.length !== 1 ? 's' : ''} selected for optimization
          </Typography>
          <Typography variant="caption" sx={{ color: '#b0b0b0' }}>
            Est. improvement will be calculated after running
          </Typography>
        </Box>
      )}

      {/* Optimization Modules - Premium Cards */}
      <Stack spacing={2}>
        {modules?.map((module) => (
          <Box
            key={module.id}
            sx={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              p: 3,
              background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
              border: '1px solid #444',
              borderRadius: '16px',
              transition: 'all 0.3s ease',
              cursor: 'pointer',
              '&:hover': {
                border: '1px solid #666',
                boxShadow: '0 8px 24px rgba(0, 0, 0, 0.5)',
              },
              ...(selectedModules.includes(module.id) && {
                border: '1px solid #FFB800',
                backgroundColor: 'rgba(255, 184, 0, 0.05)',
              }),
            }}
            onClick={() => handleModuleToggle(module.id)}
          >
            <Box sx={{ flexGrow: 1, pr: 2 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                <Typography variant="h6" sx={{ fontWeight: '700', fontSize: '16px', m: 0 }}>
                  {module.name}
                </Typography>
                {module.estimatedCleanup && (
                  <Box sx={{
                    px: 1.5,
                    py: 0.5,
                    backgroundColor: 'rgba(255, 184, 0, 0.15)',
                    borderRadius: '6px',
                    border: '1px solid rgba(255, 184, 0, 0.3)',
                  }}>
                    <Typography variant="caption" sx={{ fontWeight: '600', color: '#FFB800' }}>
                      {(module.estimatedCleanup / 1024 / 1024 / 1024).toFixed(2)} GB
                    </Typography>
                  </Box>
                )}
              </Box>
              <Typography variant="body2" sx={{ color: '#b0b0b0', lineHeight: 1.5 }}>
                {module.description}
              </Typography>
            </Box>
            <Switch
              checked={selectedModules.includes(module.id)}
              onChange={(e) => {
                e.stopPropagation();
                handleModuleToggle(module.id);
              }}
              sx={{
                ml: 2,
                '& .MuiSwitch-thumb': {
                  backgroundColor: selectedModules.includes(module.id) ? '#FFB800' : '#666',
                },
              }}
            />
          </Box>
        ))}
      </Stack>

      <Button
        variant="contained"
        size="large"
        disabled={selectedModules.length === 0 || runMutation.isPending || jobId !== null}
        onClick={() => runMutation.mutate()}
        sx={{
          mt: 3,
          backgroundColor: '#FFB800',
          color: '#000',
          fontWeight: 'bold',
          '&:disabled': { backgroundColor: '#666' },
        }}
      >
        {runMutation.isPending ? 'Starting...' : `Apply Changes (${selectedModules.length})`}
      </Button>

      {/* Progress Dialog */}
      <Dialog open={jobId !== null && progress < 100} maxWidth="sm" fullWidth disableEscapeKeyDown>
        <DialogTitle>Optimization in Progress</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Box>
            <Typography variant="body2" sx={{ mb: 2 }}>
              Current Module: <strong>{currentModule}</strong>
            </Typography>
            <LinearProgress
              variant="determinate"
              value={progress}
              sx={{ mb: 2, height: 8, borderRadius: '4px' }}
            />
            <Typography variant="caption">
              {progress}% Complete | Estimated time remaining: {Math.ceil((100 - progress) * 3)} seconds
            </Typography>
          </Box>
        </DialogContent>
      </Dialog>

      {/* Results Dialog */}
      <Dialog open={showResults} maxWidth="sm" fullWidth>
        <DialogTitle>Optimization Complete ✓</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Stack spacing={2}>
            <Alert severity="success">Optimization completed successfully!</Alert>
            <Box>
              <Typography variant="body2">
                <strong>Modules Completed:</strong> {results?.completedModules.length}
              </Typography>
              <Typography variant="body2">
                <strong>Issues Fixed:</strong> {results?.issuesFixed}
              </Typography>
              <Typography variant="body2">
                <strong>Disk Freed:</strong> {(results?.diskFreed / 1024 / 1024 / 1024).toFixed(2)} GB
              </Typography>
              <Typography variant="body2">
                <strong>Duration:</strong> {Math.floor(results?.duration / 60)}m {results?.duration % 60}s
              </Typography>
            </Box>
            <Button
              variant="contained"
              fullWidth
              onClick={() => {
                setShowResults(false);
                setJobId(null);
                setProgress(0);
                setSelectedModules([]);
              }}
            >
              Close
            </Button>
          </Stack>
        </DialogContent>
      </Dialog>
    </Box>
  );
};

export default AdvancedOptimizer;
