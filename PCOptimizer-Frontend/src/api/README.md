# PC Optimizer Frontend - API Integration Guide

This directory contains all API service files for communicating with the PC Optimizer backend API running on `http://localhost:5211/api`.

## Quick Start

Import services and types directly from the index:

```typescript
import {
  dashboardService,
  systemService,
  optimizerService,
  analyticsService,
  historyService,
  settingsService,
  type DashboardMetrics,
  type SystemInfo,
} from '@/api';

// Use a service
const metrics = await dashboardService.getMetrics();
console.log(`CPU: ${metrics.cpu}%`);
```

## API Services

### Dashboard Service (`dashboardService`)

**Endpoints**: `/dashboard/*`

Handles real-time system metrics, monitoring modes, and metric history.

```typescript
// Get current system metrics (CPU, RAM, Disk, Temperature, etc.)
const metrics = await dashboardService.getMetrics();

// Get historical metrics for charting
const history = await dashboardService.getHistory();

// Get current monitoring mode
const mode = await dashboardService.getMode();

// Change monitoring mode
const newMode = await dashboardService.setMode('Gaming');
```

**Response Types**:
- `DashboardMetrics`: Current system metrics
- `MetricHistory[]`: Historical data points
- `MonitoringMode`: Current/new monitoring mode

---

### System Service (`systemService`)

**Endpoints**: `/system/*`

Handles system information, processes, disk space, and system control.

```typescript
// Get complete system information
const info = await systemService.getSystemInfo();
// Returns: OS name, CPU model, RAM, disk, hostname, username

// Get list of top processes by memory usage
const processes = await systemService.getProcesses();
// Returns: List with count and top 20 processes

// Get disk space information for all drives
const disks = await systemService.getDiskSpace();
// Returns: Array of drive info with usage percentages

// Schedule system restart (60 second delay)
const restartResult = await systemService.restart();

// Schedule system shutdown (60 second delay)
const shutdownResult = await systemService.shutdown();
```

**Response Types**:
- `SystemInfo`: System hardware information
- `ProcessList`: Process count and top processes
- `DiskSpaceResponse`: Drive information
- `SystemRestartResponse`: Operation status

---

### Optimizer Service (`optimizerService`)

**Endpoints**: `/optimizer/*`

Handles all system optimization operations.

```typescript
// GPU Optimization
const nvidiaResult = await optimizerService.optimizeNvidiaGpu({
  enableLowLatency: true,
  maxPerformance: false,
  disableVSync: false,
});

const amdResult = await optimizerService.optimizeAmdGpu();

// Memory optimization
const memoryResult = await optimizerService.cleanupMemory();

// Power plan
const powerResult = await optimizerService.applyGamingPowerPlan();

// Background processes
const bgResult = await optimizerService.killBackgroundProcesses();

// Boot optimization
const bootResult = await optimizerService.optimizeBoot();

// Network optimization
const networkResult = await optimizerService.advancedNetworkOptimization();

// Display optimization
const displayResult = await optimizerService.optimizeDisplay();

// Audio optimization
const audioResult = await optimizerService.optimizeAudio();

// Run ALL optimizations at once
const allResult = await optimizerService.runAllOptimizations();
// Returns: AllOptimizationsResult with total and individual results
```

**Response Types**:
- `OptimizationResult`: Individual operation result
- `AllOptimizationsResult`: Combined results from all optimizations
- `GpuOptimizeRequest`: GPU optimization options

---

### Analytics Service (`analyticsService`)

**Endpoints**: `/analytics/*`

Handles metrics analysis, anomaly detection, and health scoring.

```typescript
// Get analytics metrics for a time range
const metrics = await analyticsService.getMetrics('24h'); // '24h', '7d', '30d'

// Get detected anomalies
const anomalies = await analyticsService.getAnomalies();

// Get system health score
const health = await analyticsService.getHealthScore();
console.log(`Health Score: ${health.score}`);

// Enable anomaly detection
const enableResult = await analyticsService.enableAnomalyDetection();

// Disable anomaly detection
const disableResult = await analyticsService.disableAnomalyDetection();

// Check if anomaly detection is ready
const status = await analyticsService.isAnomalyDetectionReady();
```

**Response Types**:
- `AnalyticsData`: Time-series metrics (CPU, memory, disk, network, etc.)

---

### History Service (`historyService`)

**Endpoints**: `/history/*`

Handles operation history, logs, and data export.

```typescript
// Get operation history with pagination
const logs = await historyService.getLogs(
  1,  // page number
  20, // items per page
  {
    type: 'optimization', // optional filter
    status: 'success',    // optional filter
    dateFrom: '2024-01-01',
    dateTo: '2024-12-31',
  }
);

// Get specific log entry
const logEntry = await historyService.getLog('log-id-123');

// Export history
const csvBlob = await historyService.export('csv');
const jsonBlob = await historyService.export('json');

// Save exported file
const url = URL.createObjectURL(csvBlob);
const a = document.createElement('a');
a.href = url;
a.download = 'history.csv';
a.click();
```

**Response Types**:
- `PaginatedResponse<OperationHistoryEntry>`: Paginated history
- `OperationHistoryEntry`: Individual operation record
- `Blob`: Exported file content

---

### Settings Service (`settingsService`)

**Endpoints**: `/settings/*`

Handles user preferences and application settings.

```typescript
// Get current preferences
const prefs = await settingsService.getPreferences();

// Update preferences
const updated = await settingsService.updatePreferences({
  theme: 'dark',
  autoOptimize: true,
  notificationsEnabled: true,
  refreshInterval: 5000,
});

// Create system restore point
const restorePoint = await settingsService.createRestorePoint();
console.log(`Restore Point ID: ${restorePoint.pointId}`);

// Get available themes
const themes = await settingsService.getThemes();
```

**Response Types**:
- `SettingsResponse`: Current settings
- `SettingsRequest`: Settings to update
- Restore point and theme response objects

---

## Client Configuration

The API client (`client.ts`) is pre-configured with:

- **Base URL**: `http://localhost:5211/api` (configurable via `REACT_APP_API_URL` env var)
- **Timeout**: 30 seconds
- **Default Headers**: `Content-Type: application/json`
- **Request Interceptor**: Automatically adds auth token from localStorage
- **Response Interceptor**: Handles 401 errors by clearing auth and redirecting

### Using Environment Variables

Create `.env.local`:

```env
REACT_APP_API_URL=http://localhost:5211/api
```

---

## Error Handling

All services throw errors on failure. Use try-catch:

```typescript
try {
  const metrics = await dashboardService.getMetrics();
} catch (error) {
  console.error('Failed to fetch metrics:', error);
  // Handle error appropriately
}
```

Services also log errors to console with prefixes:
- `[Dashboard] Failed to fetch metrics:`
- `[System] Failed to fetch system info:`
- `[Optimizer] Failed to optimize NVIDIA GPU:`

---

## Testing APIs

### Using the Debug Page

1. Navigate to `http://localhost:5173/debug`
2. Click "Run All Tests"
3. View test results and sample response data

The debug page (`Debug.tsx`) tests all API endpoints:
- ✅ Dashboard metrics
- ✅ System information
- ✅ Optimizer services
- ✅ Analytics
- ✅ History
- ✅ Settings

### Running Tests Programmatically

```typescript
import { runAllApiTests } from '@/api/apiTest';

const results = await runAllApiTests();
console.log(results);
```

---

## Type Definitions

All TypeScript types are defined in `types.ts`:

### Dashboard
- `DashboardMetrics`: CPU, RAM, disk, temperature, active processes
- `MetricHistory`: Timestamp-based metric snapshots
- `MonitoringMode`: Monitoring mode string

### System
- `SystemInfo`: OS, CPU, RAM, disk info
- `ProcessInfo`: Process ID, name, memory
- `ProcessList`: Count and top processes array
- `DriveInfo`: Drive usage information
- `DiskSpaceResponse`: Array of drive info

### Optimizer
- `OptimizationResult`: Success, message, category, changes
- `AllOptimizationsResult`: Combined optimization results
- `GpuOptimizeRequest`: GPU optimization options

### Analytics
- `AnalyticsData`: Time-series metrics with CPU, memory, etc.

### History
- `OperationHistoryEntry`: Log entry with timestamp, status, etc.

### Settings
- `SettingsRequest`: Partial settings update
- `SettingsResponse`: Complete settings response

---

## Common Patterns

### Loading State with React Query

```typescript
import { useQuery } from '@tanstack/react-query';

function DashboardComponent() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['dashboard-metrics'],
    queryFn: dashboardService.getMetrics,
    refetchInterval: 5000, // Refetch every 5 seconds
  });

  if (isLoading) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;

  return <div>CPU: {data?.cpu}%</div>;
}
```

### Error Handling Component

```typescript
async function performOptimization() {
  try {
    const result = await optimizerService.runAllOptimizations();
    showSuccessToast(`Optimization complete: ${result.totalOptimizations} tasks`);
  } catch (error) {
    showErrorToast('Optimization failed. Please try again.');
  }
}
```

### Exporting Data

```typescript
async function exportHistory() {
  const csvBlob = await historyService.export('csv');
  const url = URL.createObjectURL(csvBlob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `history-${new Date().toISOString()}.csv`;
  link.click();
  URL.revokeObjectURL(url);
}
```

---

## Troubleshooting

### "Cannot GET /debug"
- Make sure you've added the Debug import and route to `App.tsx`
- Route should be at `/debug`

### "Failed to fetch metrics: Network Error"
- Verify backend is running on port 5211
- Check `client.ts` base URL configuration
- Ensure `REACT_APP_API_URL` env var is correct (if using)

### "401 Unauthorized"
- Check that auth token is stored in localStorage
- Token should be added automatically by request interceptor
- If needed, clear localStorage and re-authenticate

### Type Errors
- Ensure all types are imported from `/api` or `/api/types`
- Use `type` keyword for type-only imports: `import type { DashboardMetrics }`

---

## Architecture

```
src/api/
├── client.ts              # Axios HTTP client configuration
├── types.ts               # Centralized TypeScript interfaces
├── index.ts               # Barrel export for convenience
├── apiTest.ts             # API test utility functions
├── dashboardService.ts    # Dashboard API endpoints
├── systemService.ts       # System API endpoints
├── optimizerService.ts    # Optimizer API endpoints
├── analyticsService.ts    # Analytics API endpoints
├── historyService.ts      # History API endpoints
├── settingsService.ts     # Settings API endpoints
└── README.md             # This file

src/pages/
└── Debug.tsx             # Debug page for testing all APIs
```

---

## Adding New Services

When adding a new API service:

1. **Define types in `types.ts`**
2. **Create service file** (e.g., `newService.ts`)
3. **Add error handling** with try-catch and logging
4. **Use centralized types** from `types.ts`
5. **Export from `index.ts`**
6. **Test in Debug page** by adding test function to `apiTest.ts`

Example:

```typescript
// newService.ts
import { apiClient } from './client';
import { YourResponseType } from './types';

export const newService = {
  getExample: async (): Promise<YourResponseType> => {
    try {
      const { data } = await apiClient.get<YourResponseType>('/endpoint');
      return data;
    } catch (error) {
      console.error('[NewService] Failed to fetch example:', error);
      throw error;
    }
  },
};
```

---

## Backend Compatibility

**Current Backend API**: Running on `http://localhost:5211`

**Supported Controllers**:
- Dashboard Controller
- System Controller
- Optimizer Controller
- Analytics Controller
- History Controller
- Settings Controller

All endpoints are fully integrated and tested.

---

**Last Updated**: 2025-01-01
**Version**: 1.0.0
