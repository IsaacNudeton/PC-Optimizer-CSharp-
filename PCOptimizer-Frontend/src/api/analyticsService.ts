import { apiClient } from './client';
import { AnalyticsData } from './types';

/**
 * Analytics API Service
 * Handles metrics, anomaly detection, and health scoring
 */
export const analyticsService = {
  /**
   * Get analytics metrics for a specified time range
   */
  getMetrics: async (range: '24h' | '7d' | '30d'): Promise<AnalyticsData> => {
    try {
      const { data } = await apiClient.get<AnalyticsData>(`/analytics/metrics?range=${range}`);
      return data;
    } catch (error) {
      console.error('[Analytics] Failed to fetch metrics:', error);
      throw error;
    }
  },

  /**
   * Get detected anomalies
   */
  getAnomalies: async (): Promise<AnalyticsData[]> => {
    try {
      const { data } = await apiClient.get<{ data: AnalyticsData[] }>('/analytics/anomalies');
      return data.data || [];
    } catch (error) {
      console.error('[Analytics] Failed to fetch anomalies:', error);
      throw error;
    }
  },

  /**
   * Get system health score
   */
  getHealthScore: async (): Promise<{ score: number }> => {
    try {
      const { data } = await apiClient.get<{ score: number }>('/analytics/health-score');
      return data;
    } catch (error) {
      console.error('[Analytics] Failed to fetch health score:', error);
      throw error;
    }
  },

  /**
   * Enable anomaly detection
   */
  enableAnomalyDetection: async (): Promise<{ success: boolean; message: string }> => {
    try {
      const { data } = await apiClient.post<{ success: boolean; message: string }>('/analytics/anomaly-detection/enable', {});
      return data;
    } catch (error) {
      console.error('[Analytics] Failed to enable anomaly detection:', error);
      throw error;
    }
  },

  /**
   * Disable anomaly detection
   */
  disableAnomalyDetection: async (): Promise<{ success: boolean; message: string }> => {
    try {
      const { data } = await apiClient.post<{ success: boolean; message: string }>('/analytics/anomaly-detection/disable', {});
      return data;
    } catch (error) {
      console.error('[Analytics] Failed to disable anomaly detection:', error);
      throw error;
    }
  },

  /**
   * Check if anomaly detection is ready
   */
  isAnomalyDetectionReady: async (): Promise<{ ready: boolean; message?: string }> => {
    try {
      const { data } = await apiClient.get<{ ready: boolean; message?: string }>('/analytics/anomaly-detection/ready');
      return data;
    } catch (error) {
      console.error('[Analytics] Failed to check anomaly detection status:', error);
      throw error;
    }
  },
};
