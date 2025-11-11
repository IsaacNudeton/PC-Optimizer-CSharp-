import { apiClient } from './client';
import { SettingsRequest, SettingsResponse } from './types';

/**
 * Settings API Service
 * Handles user preferences, settings, and application configuration
 */
export const settingsService = {
  /**
   * Get current user preferences
   */
  getPreferences: async (): Promise<SettingsResponse> => {
    try {
      const { data } = await apiClient.get<SettingsResponse>('/settings/preferences');
      return data;
    } catch (error) {
      console.error('[Settings] Failed to fetch preferences:', error);
      throw error;
    }
  },

  /**
   * Update user preferences
   */
  updatePreferences: async (prefs: SettingsRequest): Promise<SettingsResponse> => {
    try {
      const { data } = await apiClient.post<SettingsResponse>('/settings/preferences', prefs);
      return data;
    } catch (error) {
      console.error('[Settings] Failed to update preferences:', error);
      throw error;
    }
  },

  /**
   * Create a system restore point
   */
  createRestorePoint: async (): Promise<{ success: boolean; pointId: string; message?: string }> => {
    try {
      const { data } = await apiClient.post<{ success: boolean; pointId: string; message?: string }>('/settings/restore-point', {});
      return data;
    } catch (error) {
      console.error('[Settings] Failed to create restore point:', error);
      throw error;
    }
  },

  /**
   * Get available themes
   */
  getThemes: async (): Promise<{ themes: string[]; currentTheme: string }> => {
    try {
      const { data } = await apiClient.get<{ themes: string[]; currentTheme: string }>('/settings/themes');
      return data;
    } catch (error) {
      console.error('[Settings] Failed to fetch themes:', error);
      throw error;
    }
  },
};
