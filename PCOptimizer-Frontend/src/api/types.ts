/**
 * API Response Types for PC Optimizer Backend
 */

// Dashboard Types
export interface DashboardMetrics {
  cpu: number;
  ram: number;
  disk: number;
  temperature: number;
  activeProcesses: number;
  systemStatus: string;
  lastOptimization?: string;
  lastOptimizationFixes?: number;
}

export interface MetricHistory {
  timestamp: string;
  cpu: number;
  ram: number;
  temperature: number;
}

export interface MonitoringMode {
  mode: string;
}

// System Types
export interface SystemInfo {
  osName: string;
  osVersion: string;
  processorCount: number;
  cpuModel: string;
  totalRam: number;
  totalDisk: number;
  machineName: string;
  userName: string;
}

export interface ProcessInfo {
  pid: number;
  name: string;
  memory: number; // In MB
}

export interface ProcessList {
  count: number;
  topProcesses: ProcessInfo[];
}

export interface DriveInfo {
  drive: string;
  total: number; // In GB
  free: number; // In GB
  used: number; // In GB
  percent: number; // Usage percentage
}

export interface DiskSpaceResponse {
  drives: DriveInfo[];
}

export interface SystemRestartResponse {
  success: boolean;
  message: string;
}

// Optimizer Types
export interface OptimizationResult {
  success: boolean;
  message: string;
  category?: string;
  changes?: number | string;
}

export interface AllOptimizationsResult {
  success: boolean;
  totalOptimizations: number;
  optimizations: OptimizationResult[];
}

export interface GpuOptimizeRequest {
  enableLowLatency?: boolean;
  maxPerformance?: boolean;
  disableVSync?: boolean;
}

// Analytics Types
export interface AnalyticsData {
  timestamp: string;
  cpuUsage: number;
  memoryUsage: number;
  diskIo: number;
  temperature: number;
  networkUpload: number;
  networkDownload: number;
}

// History Types
export interface OperationHistoryEntry {
  id: string;
  timestamp: string;
  operationType: string;
  status: 'success' | 'warning' | 'failed';
  itemsProcessed: number;
  spaceFreed?: number;
  duration: number;
  details?: string;
}

// Settings Types
export interface SettingsRequest {
  theme?: string;
  autoOptimize?: boolean;
  notificationsEnabled?: boolean;
  refreshInterval?: number;
  [key: string]: any;
}

export interface SettingsResponse {
  theme: string;
  autoOptimize: boolean;
  notificationsEnabled: boolean;
  refreshInterval: number;
  [key: string]: any;
}

// Error Response
export interface ApiError {
  error: string;
  statusCode?: number;
}
