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
  Button,
  Pagination,
  Stack,
  Select,
  MenuItem,
  CircularProgress,
  Alert,
  Typography,
  Chip,
} from '@mui/material';
import { historyService } from '../api/historyService';

const OperationHistory: React.FC = () => {
  const [page, setPage] = useState(1);
  const [typeFilter, setTypeFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState('');

  const { data, isLoading, error } = useQuery({
    queryKey: ['historyLogs', page, typeFilter, statusFilter],
    queryFn: () =>
      historyService.getLogs(page, 20, {
        type: typeFilter || undefined,
        status: statusFilter || undefined,
      }),
    refetchInterval: 60000,
    staleTime: 5000,
  });

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
      {/* Header */}
      <Box sx={{ mb: 2 }}>
        <Typography variant="h3" sx={{ fontWeight: 900, mb: 0.5, fontSize: '42px', letterSpacing: '-1px' }}>
          Operation History
        </Typography>
        <Typography variant="body1" sx={{ color: '#b0b0b0', fontSize: '16px' }}>
          View logs of all performed optimizations and system operations
        </Typography>
      </Box>

      {/* Filters */}
      <Box sx={{
        display: 'flex',
        gap: 2,
        flexWrap: 'wrap',
        p: 2,
        background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
        border: '1px solid #444',
        borderRadius: '12px',
      }}>
        <Select
          value={typeFilter}
          onChange={(e) => {
            setTypeFilter(e.target.value);
            setPage(1);
          }}
          displayEmpty
          size="small"
          sx={{
            minWidth: 180,
            backgroundColor: 'rgba(0, 0, 0, 0.3)',
            border: '1px solid #666',
            borderRadius: '8px',
            '& .MuiOutlinedInput-notchedOutline': {
              borderColor: '#666',
            },
          }}
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
          sx={{
            minWidth: 180,
            backgroundColor: 'rgba(0, 0, 0, 0.3)',
            border: '1px solid #666',
            borderRadius: '8px',
            '& .MuiOutlinedInput-notchedOutline': {
              borderColor: '#666',
            },
          }}
        >
          <MenuItem value="">All Status</MenuItem>
          <MenuItem value="success">Success</MenuItem>
          <MenuItem value="failed">Failed</MenuItem>
        </Select>

        <Box sx={{ flexGrow: 1 }} />

        <Button
          variant="contained"
          size="small"
          sx={{
            backgroundColor: '#FFB800',
            color: '#000',
            fontWeight: 'bold',
            transition: 'all 0.3s ease',
            '&:hover': {
              backgroundColor: '#FFC933',
              boxShadow: '0 4px 12px rgba(255, 184, 0, 0.3)',
            },
          }}
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
      <TableContainer
        sx={{
          background: 'linear-gradient(135deg, #2d2d2d 0%, #1a1a1a 100%)',
          border: '1px solid #444',
          borderRadius: '12px',
          overflow: 'hidden',
        }}
      >
        <Table>
          <TableHead>
            <TableRow sx={{ backgroundColor: 'rgba(0, 0, 0, 0.3)', borderBottom: '1px solid #444' }}>
              <TableCell sx={{ fontWeight: 700, color: '#FFB800', fontSize: '13px' }}>Date & Time</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#FFB800', fontSize: '13px' }}>Type</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#FFB800', fontSize: '13px' }}>Duration</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#FFB800', fontSize: '13px' }}>Status</TableCell>
              <TableCell sx={{ fontWeight: 700, color: '#FFB800', fontSize: '13px' }}>Issues Fixed</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.map((log) => (
              <TableRow
                key={log.id}
                sx={{
                  borderBottom: '1px solid #333',
                  transition: 'all 0.3s ease',
                  '&:hover': {
                    backgroundColor: 'rgba(255, 184, 0, 0.05)',
                  },
                  '&:last-child': {
                    borderBottom: 'none',
                  },
                }}
              >
                <TableCell sx={{ py: 2, color: '#e0e0e0', fontSize: '14px' }}>
                  {new Date(log.timestamp).toLocaleString()}
                </TableCell>
                <TableCell sx={{ py: 2, color: '#e0e0e0', fontSize: '14px', fontWeight: 500 }}>
                  {log.operationType}
                </TableCell>
                <TableCell sx={{ py: 2, color: '#b0b0b0', fontSize: '14px' }}>
                  {Math.floor(log.duration)}s
                </TableCell>
                <TableCell sx={{ py: 2 }}>
                  <Chip
                    label={log.status === 'success' ? '✓ Success' : '✕ Failed'}
                    sx={{
                      ...(log.status === 'success'
                        ? {
                            backgroundColor: 'rgba(76, 175, 80, 0.15)',
                            color: '#4CAF50',
                            border: '1px solid #4CAF50',
                          }
                        : {
                            backgroundColor: 'rgba(244, 67, 54, 0.15)',
                            color: '#F44336',
                            border: '1px solid #F44336',
                          }),
                      fontWeight: 600,
                    }}
                    variant="outlined"
                    size="small"
                  />
                </TableCell>
                <TableCell sx={{ py: 2, color: '#FFB800', fontWeight: 600, fontSize: '14px' }}>
                  {log.itemsProcessed}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Summary & Pagination */}
      <Box sx={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: 2,
        p: 2,
        background: 'rgba(0, 0, 0, 0.2)',
        borderRadius: '8px',
      }}>
        <Typography variant="body2" sx={{ color: '#b0b0b0' }}>
          Showing <span style={{ color: '#FFB800', fontWeight: 'bold' }}>
            {(page - 1) * 20 + 1}
          </span>
          {' '}to <span style={{ color: '#FFB800', fontWeight: 'bold' }}>
            {Math.min(page * 20, data?.totalCount || 0)}
          </span>
          {' '}of{' '}
          <span style={{ color: '#FFB800', fontWeight: 'bold' }}>{data?.totalCount || 0}</span> records
        </Typography>
        <Pagination
          count={Math.ceil((data?.totalCount || 0) / 20)}
          page={page}
          onChange={(_, newPage) => setPage(newPage)}
          sx={{
            '& .MuiPaginationItem-root': {
              color: '#b0b0b0',
              border: '1px solid #666',
              '&:hover': {
                backgroundColor: 'rgba(255, 184, 0, 0.1)',
                borderColor: '#FFB800',
                color: '#FFB800',
              },
            },
            '& .Mui-selected': {
              backgroundColor: '#FFB800 !important',
              color: '#000 !important',
              borderColor: '#FFB800 !important',
            },
          }}
        />
      </Box>
    </Stack>
  );
};

export default OperationHistory;
