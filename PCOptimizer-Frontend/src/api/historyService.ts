import { apiClient } from './client';
import { OperationHistoryEntry } from './types';

/**
 * Paginated response wrapper
 */
export interface PaginatedResponse<T> {
  data: T[];
  page: number;
  pageSize: number;
  totalCount: number;
}

/**
 * History API Service
 * Handles operation history, logs, and exports
 */
export const historyService = {
  /**
   * Get operation history logs with pagination
   */
  getLogs: async (
    page: number = 1,
    limit: number = 20,
    filters?: { type?: string; status?: string; dateFrom?: string; dateTo?: string }
  ): Promise<PaginatedResponse<OperationHistoryEntry>> => {
    try {
      const { data } = await apiClient.get<PaginatedResponse<OperationHistoryEntry>>('/history/logs', {
        params: { page, limit, ...filters },
      });
      return data;
    } catch (error) {
      console.error('[History] Failed to fetch logs:', error);
      throw error;
    }
  },

  /**
   * Get a specific history log entry
   */
  getLog: async (id: string): Promise<OperationHistoryEntry> => {
    try {
      const { data } = await apiClient.get<OperationHistoryEntry>(`/history/logs/${id}`);
      return data;
    } catch (error) {
      console.error('[History] Failed to fetch log:', error);
      throw error;
    }
  },

  /**
   * Export history logs in the specified format
   */
  export: async (format: 'csv' | 'json'): Promise<Blob> => {
    try {
      const { data } = await apiClient.post<Blob>(
        `/history/export?format=${format}`,
        {},
        {
          responseType: 'blob',
        }
      );
      return data;
    } catch (error) {
      console.error('[History] Failed to export logs:', error);
      throw error;
    }
  },
};
