import { apiClient } from './client';
import { SystemInfo, ProcessList, DiskSpaceResponse, SystemRestartResponse } from './types';

/**
 * System API Service
 * Handles system information, processes, disk space, and system control
 */
export const systemService = {
  /**
   * Get complete system information
   */
  getSystemInfo: async (): Promise<SystemInfo> => {
    try {
      const { data } = await apiClient.get<SystemInfo>('/system/info');
      return data;
    } catch (error) {
      console.error('[System] Failed to fetch system info:', error);
      throw error;
    }
  },

  /**
   * Get list of top processes by memory usage
   */
  getProcesses: async (): Promise<ProcessList> => {
    try {
      const { data } = await apiClient.get<ProcessList>('/system/processes');
      return data;
    } catch (error) {
      console.error('[System] Failed to fetch processes:', error);
      throw error;
    }
  },

  /**
   * Get disk space information for all drives
   */
  getDiskSpace: async (): Promise<DiskSpaceResponse> => {
    try {
      const { data } = await apiClient.get<DiskSpaceResponse>('/system/disk-space');
      return data;
    } catch (error) {
      console.error('[System] Failed to fetch disk space:', error);
      throw error;
    }
  },

  /**
   * Schedule system restart (60 second delay)
   */
  restart: async (): Promise<SystemRestartResponse> => {
    try {
      const { data } = await apiClient.post<SystemRestartResponse>('/system/restart', {});
      return data;
    } catch (error) {
      console.error('[System] Failed to restart system:', error);
      throw error;
    }
  },

  /**
   * Schedule system shutdown (60 second delay)
   */
  shutdown: async (): Promise<SystemRestartResponse> => {
    try {
      const { data } = await apiClient.post<SystemRestartResponse>('/system/shutdown', {});
      return data;
    } catch (error) {
      console.error('[System] Failed to shutdown system:', error);
      throw error;
    }
  },
};
