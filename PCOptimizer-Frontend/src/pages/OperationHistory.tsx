import React, { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Button,
  Pagination,
  Stack,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
} from '@mui/material';
import { historyService } from '../api/historyService';

const OperationHistory: React.FC = () => {
  const [page, setPage] = useState(1);
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const { data, isLoading, error } = useQuery(
    ['historyLogs', page, typeFilter, statusFilter],
    () =>
      historyService.getLogs(page, 20, {
        type: typeFilter || undefined,
        status: statusFilter || undefined,
      }),
    { refetchInterval: 60000, staleTime: 5000 }
  );

  if (isLoading) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '400px' }}>
        <CircularProgress />
      </Box>
    );
  }

  if (error) {
    return <Alert severity="error">Failed to load history</Alert>;
  }

  return (
    <Stack spacing={3}>
      {/* Filters */}
      <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
        <Select
          value={typeFilter}
          onChange={(e) => {
            setTypeFilter(e.target.value);
            setPage(1);
          }}
          displayEmpty
          size="small"
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">All Types</MenuItem>
          <MenuItem value="Cleanup">Cleanup</MenuItem>
          <MenuItem value="Defragmentation">Defragmentation</MenuItem>
          <MenuItem value="Startup Optimization">Startup Optimization</MenuItem>
        </Select>

        <Select
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setPage(1);
          }}
          displayEmpty
          size="small"
          sx={{ minWidth: 150 }}
        >
          <MenuItem value="">All Status</MenuItem>
          <MenuItem value="success">Success</MenuItem>
          <MenuItem value="failed">Failed</MenuItem>
        </Select>

        <Button
          variant="outlined"
          onClick={async () => {
            const blob = await historyService.export('csv');
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'history.csv';
            a.click();
          }}
        >
          Export CSV
        </Button>
      </Box>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Date & Time</TableCell>
              <TableCell>Type</TableCell>
              <TableCell>Duration</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Issues Fixed</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.map((log) => (
              <TableRow key={log.id} hover>
                <TableCell>{new Date(log.timestamp).toLocaleString()}</TableCell>
                <TableCell>{log.type}</TableCell>
                <TableCell>{Math.floor(log.duration)}s</TableCell>
                <TableCell>
                  <Chip
                    label={log.status === 'success' ? 'Success' : 'Failed'}
                    color={log.status === 'success' ? 'success' : 'error'}
                    variant="outlined"
                    size="small"
                  />
                </TableCell>
                <TableCell>{log.issuesFixed}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Pagination */}
      <Box sx={{ display: 'flex', justifyContent: 'center' }}>
        <Pagination
          count={Math.ceil((data?.totalCount || 0) / 20)}
          page={page}
          onChange={(_, newPage) => setPage(newPage)}
          color="primary"
        />
      </Box>

      {/* Summary */}
      <Box>
        <select value={statusFilter} hidden />
        Showing {(page - 1) * 20 + 1} to {Math.min(page * 20, data?.totalCount || 0)} of{' '}
        {data?.totalCount || 0} records
      </Box>
    </Stack>
  );
};

export default OperationHistory;
