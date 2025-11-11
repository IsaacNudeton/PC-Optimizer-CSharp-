import { apiClient } from './client';
import { OptimizationResult, AllOptimizationsResult, GpuOptimizeRequest } from './types';

/**
 * Optimizer API Service
 * Handles all optimization operations (GPU, memory, power, etc.)
 */
export const optimizerService = {
  /**
   * Optimize NVIDIA GPU settings
   */
  optimizeNvidiaGpu: async (request?: GpuOptimizeRequest): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/gpu/nvidia', request || {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to optimize NVIDIA GPU:', error);
      throw error;
    }
  },

  /**
   * Optimize AMD GPU settings
   */
  optimizeAmdGpu: async (request?: GpuOptimizeRequest): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/gpu/amd', request || {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to optimize AMD GPU:', error);
      throw error;
    }
  },

  /**
   * Clean up memory (clear standby memory)
   */
  cleanupMemory: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/memory/cleanup', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to cleanup memory:', error);
      throw error;
    }
  },

  /**
   * Create and activate gaming power plan
   */
  applyGamingPowerPlan: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/power-plan/gaming', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to apply gaming power plan:', error);
      throw error;
    }
  },

  /**
   * Kill unnecessary background processes
   */
  killBackgroundProcesses: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/background/kill', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to kill background processes:', error);
      throw error;
    }
  },

  /**
   * Optimize boot settings
   */
  optimizeBoot: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/boot/optimize', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to optimize boot:', error);
      throw error;
    }
  },

  /**
   * Apply advanced network optimizations
   */
  advancedNetworkOptimization: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/network/advanced', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to apply network optimization:', error);
      throw error;
    }
  },

  /**
   * Optimize display settings
   */
  optimizeDisplay: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/display/optimize', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to optimize display:', error);
      throw error;
    }
  },

  /**
   * Optimize audio settings for low latency
   */
  optimizeAudio: async (): Promise<OptimizationResult> => {
    try {
      const { data } = await apiClient.post<OptimizationResult>('/optimizer/audio/optimize', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to optimize audio:', error);
      throw error;
    }
  },

  /**
   * Run all optimizations at once
   */
  runAllOptimizations: async (): Promise<AllOptimizationsResult> => {
    try {
      const { data } = await apiClient.post<AllOptimizationsResult>('/optimizer/all', {});
      return data;
    } catch (error) {
      console.error('[Optimizer] Failed to run all optimizations:', error);
      throw error;
    }
  },
};
