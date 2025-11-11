import { apiClient } from './client';
import { DashboardMetrics, MetricHistory, MonitoringMode } from './types';

/**
 * Dashboard API Service
 * Handles all dashboard-related API calls
 */
export const dashboardService = {
  /**
   * Get current system metrics
   */
  getMetrics: async (): Promise<DashboardMetrics> => {
    try {
      const { data } = await apiClient.get<DashboardMetrics>('/dashboard/metrics');
      return data;
    } catch (error) {
      console.error('[Dashboard] Failed to fetch metrics:', error);
      throw error;
    }
  },

  /**
   * Get metric history for charts
   */
  getHistory: async (): Promise<MetricHistory[]> => {
    try {
      const { data } = await apiClient.get<{ data: MetricHistory[] }>('/dashboard/history');
      return data.data;
    } catch (error) {
      console.error('[Dashboard] Failed to fetch history:', error);
      throw error;
    }
  },

  /**
   * Get current monitoring mode
   */
  getMode: async (): Promise<MonitoringMode> => {
    try {
      const { data } = await apiClient.get<MonitoringMode>('/dashboard/mode');
      return data;
    } catch (error) {
      console.error('[Dashboard] Failed to fetch mode:', error);
      throw error;
    }
  },

  /**
   * Set monitoring mode (Universal, Gaming, Work)
   */
  setMode: async (mode: string): Promise<MonitoringMode> => {
    try {
      const { data } = await apiClient.post<MonitoringMode>('/dashboard/mode', { mode });
      return data;
    } catch (error) {
      console.error('[Dashboard] Failed to set mode:', error);
      throw error;
    }
  },
};
