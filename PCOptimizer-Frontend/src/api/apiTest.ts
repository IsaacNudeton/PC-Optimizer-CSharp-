/**
 * API Connection Test Utility
 * Tests all API endpoints to ensure backend connectivity
 */

import {
  dashboardService,
  systemService,
  optimizerService,
  analyticsService,
  historyService,
  settingsService,
} from './index';

interface TestResult {
  endpoint: string;
  status: 'success' | 'error' | 'skipped';
  message?: string;
  duration?: number;
  data?: unknown;
}

/**
 * Run all API connection tests
 */
export async function runAllApiTests(): Promise<TestResult[]> {
  const results: TestResult[] = [];
  console.log('🔍 Starting API Connection Tests...\n');

  // Dashboard Tests
  console.log('📊 Testing Dashboard Endpoints...');
  results.push(await testDashboardMetrics());
  results.push(await testDashboardHistory());
  results.push(await testDashboardMode());

  // System Tests
  console.log('\n💻 Testing System Endpoints...');
  results.push(await testSystemInfo());
  results.push(await testSystemProcesses());
  results.push(await testSystemDiskSpace());

  // Analytics Tests
  console.log('\n📈 Testing Analytics Endpoints...');
  results.push(await testAnalyticsMetrics());
  results.push(await testAnalyticsHealthScore());

  // History Tests
  console.log('\n📜 Testing History Endpoints...');
  results.push(await testHistoryLogs());

  // Settings Tests
  console.log('\n⚙️ Testing Settings Endpoints...');
  results.push(await testSettingsPreferences());

  // Print Summary
  console.log('\n' + '='.repeat(60));
  console.log('TEST SUMMARY');
  console.log('='.repeat(60));
  const passed = results.filter((r) => r.status === 'success').length;
  const failed = results.filter((r) => r.status === 'error').length;
  const skipped = results.filter((r) => r.status === 'skipped').length;

  console.log(`✅ Passed: ${passed}/${results.length}`);
  console.log(`❌ Failed: ${failed}/${results.length}`);
  console.log(`⏭️  Skipped: ${skipped}/${results.length}`);
  console.log('='.repeat(60) + '\n');

  return results;
}

// Dashboard Tests
async function testDashboardMetrics(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await dashboardService.getMetrics();
    return {
      endpoint: 'GET /dashboard/metrics',
      status: 'success',
      message: `CPU: ${data.cpu}%, RAM: ${data.ram}%, Disk: ${data.disk}%`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /dashboard/metrics',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

async function testDashboardHistory(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await dashboardService.getHistory();
    return {
      endpoint: 'GET /dashboard/history',
      status: 'success',
      message: `Retrieved ${data.length} history entries`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /dashboard/history',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

async function testDashboardMode(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await dashboardService.getMode();
    return {
      endpoint: 'GET /dashboard/mode',
      status: 'success',
      message: `Current mode: ${data.mode}`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /dashboard/mode',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

// System Tests
async function testSystemInfo(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await systemService.getSystemInfo();
    return {
      endpoint: 'GET /system/info',
      status: 'success',
      message: `${data.osName} - ${data.cpuModel}`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /system/info',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

async function testSystemProcesses(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await systemService.getProcesses();
    return {
      endpoint: 'GET /system/processes',
      status: 'success',
      message: `Retrieved ${data.count} processes (top ${data.topProcesses.length})`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /system/processes',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

async function testSystemDiskSpace(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await systemService.getDiskSpace();
    return {
      endpoint: 'GET /system/disk-space',
      status: 'success',
      message: `Retrieved info for ${data.drives.length} drives`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /system/disk-space',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

// Analytics Tests
async function testAnalyticsMetrics(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await analyticsService.getMetrics('24h');
    return {
      endpoint: 'GET /analytics/metrics',
      status: 'success',
      message: `Retrieved analytics for 24h`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /analytics/metrics',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

async function testAnalyticsHealthScore(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await analyticsService.getHealthScore();
    return {
      endpoint: 'GET /analytics/health-score',
      status: 'success',
      message: `Health score: ${data.score}`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /analytics/health-score',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

// History Tests
async function testHistoryLogs(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await historyService.getLogs(1, 10);
    return {
      endpoint: 'GET /history/logs',
      status: 'success',
      message: `Retrieved ${data.data.length} logs (Total: ${data.totalCount})`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /history/logs',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}

// Settings Tests
async function testSettingsPreferences(): Promise<TestResult> {
  const startTime = performance.now();
  try {
    const data = await settingsService.getPreferences();
    return {
      endpoint: 'GET /settings/preferences',
      status: 'success',
      message: `Theme: ${data.theme}, Auto-optimize: ${data.autoOptimize}`,
      duration: performance.now() - startTime,
      data,
    };
  } catch (error) {
    return {
      endpoint: 'GET /settings/preferences',
      status: 'error',
      message: error instanceof Error ? error.message : 'Unknown error',
      duration: performance.now() - startTime,
    };
  }
}
