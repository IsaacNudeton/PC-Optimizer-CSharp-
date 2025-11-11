import React, { useState, useEffect } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Checkbox,
  Button,
  Stack,
  LinearProgress,
  Dialog,
  DialogTitle,
  DialogContent,
  CircularProgress,
  Grid,
  Alert,
  FormControlLabel,
} from '@mui/material';
import { optimizerService } from '../api/optimizerService';

const AdvancedOptimizer: React.FC = () => {
  const { data: modules, isLoading } = useQuery(['optimizerModules'], () =>
    optimizerService.getModules()
  );

  const [selectedModules, setSelectedModules] = useState<string[]>([]);
  const [jobId, setJobId] = useState<string | null>(null);
  const [progress, setProgress] = useState(0);
  const [currentModule, setCurrentModule] = useState('');
  const [showResults, setShowResults] = useState(false);
  const [results, setResults] = useState<any>(null);

  const runMutation = useMutation(
    () => optimizerService.runOptimization(selectedModules),
    {
      onSuccess: (res) => {
        setJobId(res.jobId);
        setProgress(0);
        pollProgress(res.jobId);
      },
    }
  );

  const pollProgress = async (id: string) => {
    const interval = setInterval(async () => {
      try {
        const p = await optimizerService.getProgress(id);
        setProgress(p.progress);
        setCurrentModule(p.currentModule);

        if (p.progress >= 100) {
          clearInterval(interval);
          // Fetch results
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
      <Stack spacing={2} sx={{ mb: 3 }}>
        {modules?.map((module) => (
          <Card key={module.id}>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'flex-start', gap: 2 }}>
                <Checkbox
                  checked={selectedModules.includes(module.id)}
                  onChange={() => handleModuleToggle(module.id)}
                  sx={{ mt: 1 }}
                />
                <Box sx={{ flexGrow: 1 }}>
                  <Typography variant="h4">{module.name}</Typography>
                  <Typography variant="body2" sx={{ mt: 1, color: 'text.secondary' }}>
                    {module.description}
                  </Typography>
                  {module.estimatedCleanup && (
                    <Typography variant="caption" sx={{ mt: 1, display: 'block' }}>
                      Est. cleanup: {(module.estimatedCleanup / 1024 / 1024 / 1024).toFixed(2)} GB
                    </Typography>
                  )}
                </Box>
              </Box>
            </CardContent>
          </Card>
        ))}
      </Stack>

      <Button
        variant="contained"
        size="large"
        disabled={selectedModules.length === 0 || runMutation.isPending || jobId !== null}
        onClick={() => runMutation.mutate()}
        sx={{ mt: 3 }}
      >
        {runMutation.isPending ? 'Starting...' : `Run Optimization (${selectedModules.length})`}
      </Button>

      {/* Progress Dialog */}
      <Dialog open={jobId !== null && progress < 100} maxWidth="sm" fullWidth disableEscapeKeyDown>
        <DialogTitle>Optimization in Progress</DialogTitle>
        <DialogContent sx={{ pt: 2 }}>
          <Box>
            <Typography variant="body2" sx={{ mb: 2 }}>
              Current Module: <strong>{currentModule}</strong>
            </Typography>
            <LinearProgress variant="determinate" value={progress} sx={{ mb: 2, height: 8, borderRadius: '4px' }} />
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
