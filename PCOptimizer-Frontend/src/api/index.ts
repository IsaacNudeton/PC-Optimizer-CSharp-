/**
 * PC Optimizer API Services
 * Central export point for all API services and types
 */

// Services
export { dashboardService } from './dashboardService';
export { systemService } from './systemService';
export { optimizerService } from './optimizerService';
export { analyticsService } from './analyticsService';
export { historyService, type PaginatedResponse } from './historyService';
export { settingsService } from './settingsService';

// API Client
export { apiClient } from './client';

// Types
export type {
  // Dashboard
  DashboardMetrics,
  MetricHistory,
  MonitoringMode,
  // System
  SystemInfo,
  ProcessInfo,
  ProcessList,
  DriveInfo,
  DiskSpaceResponse,
  SystemRestartResponse,
  // Optimizer
  OptimizationResult,
  AllOptimizationsResult,
  GpuOptimizeRequest,
  // Analytics
  AnalyticsData,
  // History
  OperationHistoryEntry,
  // Settings
  SettingsRequest,
  SettingsResponse,
  // Error
  ApiError,
} from './types';
