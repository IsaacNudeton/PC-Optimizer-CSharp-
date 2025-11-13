/**
 * Debug Page - API Testing
 * Utility page for testing backend API connections
 * Access at http://localhost:5173/debug
 */

import { useState } from 'react';
import { Container, Paper, Button, Box, Typography, Alert } from '@mui/material';

export default function Debug() {
  const [message, setMessage] = useState('Click the button to test API connections');
  const [loading, setLoading] = useState(false);

  const handleRunTests = async () => {
    setLoading(true);
    try {
      // Dynamically import to avoid circular dependencies
      const { runAllApiTests } = await import('../api/apiTest');
      const results = await runAllApiTests();
      console.log('Test Results:', results);
      setMessage(`Tests complete! Passed: ${results.filter((r) => r.status === 'success').length}/${results.length}`);
    } catch (error) {
      console.error('Test error:', error);
      setMessage(`Error: ${error instanceof Error ? error.message : 'Unknown error'}`);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      <Paper sx={{ p: 4 }}>
        <Typography variant="h4" gutterBottom>
          🔧 API Debug Page
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          Test backend API connectivity
        </Typography>

        <Box sx={{ my: 3 }}>
          <Button
            variant="contained"
            onClick={handleRunTests}
            disabled={loading}
            size="large"
          >
            {loading ? 'Running Tests...' : '▶️ Run API Tests'}
          </Button>
        </Box>

        <Alert severity="info">
          {message}
        </Alert>

        <Typography variant="body2" color="text.secondary" sx={{ mt: 3 }}>
          Check browser console (F12) for detailed test results
        </Typography>
      </Paper>
    </Container>
  );
}
