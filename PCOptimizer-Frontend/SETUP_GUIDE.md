# PC Optimizer Frontend - Setup Guide

## Prerequisites
- Node.js v18+ (Already installed at C:\Program Files\nodejs\)
- REST API running on http://localhost:5000

## Quick Start

### Step 1: Install Dependencies
```bash
cd PCOptimizer-Frontend
npm install
```
Or run the batch script:
```
install.bat
```

### Step 2: Start Development Server
```bash
npm run dev
```
Or run the batch script:
```
start.bat
```

This will:
- Start Vite dev server on http://localhost:5173
- Launch Electron window with dev tools
- Hot reload on file changes

### Step 3: Build for Production
```bash
npm run electron-build
```

This creates:
- Executable installer (NSIS)
- Portable .exe file
- Both in the `release` directory

## Project Structure

```
PCOptimizer-Frontend/
├── electron/              # Electron main/preload processes
│   ├── main.ts
│   └── preload.ts
├── src/
│   ├── api/              # REST API service clients
│   │   ├── client.ts     # Axios instance
│   │   ├── dashboardService.ts
│   │   ├── optimizerService.ts
│   │   ├── analyticsService.ts
│   │   ├── historyService.ts
│   │   ├── settingsService.ts
│   │   └── systemService.ts
│   ├── pages/            # Page components
│   │   ├── Dashboard.tsx
│   │   ├── AdvancedOptimizer.tsx
│   │   ├── PerformanceAnalytics.tsx
│   │   ├── OperationHistory.tsx
│   │   ├── Settings.tsx
│   │   └── About.tsx
│   ├── components/       # Reusable components
│   ├── layouts/          # Layout components
│   ├── context/          # React context (theme, etc)
│   ├── theme/            # Theme definitions
│   ├── App.tsx           # Main app component
│   └── main.tsx          # React entry point
├── public/               # Static assets
├── package.json          # Dependencies
├── vite.config.ts        # Vite configuration
├── tsconfig.json         # TypeScript configuration
└── index.html            # HTML entry point
```

## Available npm Scripts

| Command | Purpose |
|---------|---------|
| `npm run dev` | Start dev server (port 5173) |
| `npm run build` | Build React app for production |
| `npm run preview` | Preview production build |
| `npm start` | Run electron + dev server (electron-dev) |
| `npm run electron` | Launch Electron with built React app |
| `npm run electron-build` | Build Electron app + create installer |

## REST API Endpoints

The frontend connects to: `http://localhost:5000/api/`

### Dashboard API
- `GET /dashboard/metrics` - Real-time system metrics
- `GET /dashboard/history` - Historical metrics

### Optimizer API
- `POST /optimizer/gpu/nvidia` - NVIDIA GPU optimization
- `POST /optimizer/gpu/amd` - AMD GPU optimization
- `POST /optimizer/memory/cleanup` - Clean memory
- `POST /optimizer/power-plan/gaming` - Gaming power plan
- `POST /optimizer/boot/optimize` - Optimize boot
- `POST /optimizer/all` - Run all optimizations

### Analytics API
- `GET /analytics/anomalies` - Recent anomalies
- `GET /analytics/performance-trends` - Performance trends
- `GET /analytics/system-health` - System health score

### Settings API
- `GET /settings/theme` - Get current theme
- `POST /settings/theme` - Apply theme
- `GET /settings/monitoring-mode` - Get monitoring mode
- `POST /settings/monitoring-mode` - Set monitoring mode

### System API
- `GET /system/info` - System information
- `GET /system/processes` - Running processes
- `GET /system/disk-space` - Disk space info

## Troubleshooting

### npm install fails
If npm install fails, ensure:
1. Node.js is properly installed: `"C:\Program Files\nodejs\npm.cmd" --version`
2. Internet connection is active
3. Delete `node_modules` folder and `package-lock.json`, then retry

### Electron won't start
1. Ensure dev server is running (port 5173)
2. Check console output for errors
3. Try rebuilding: `npm run build && npm run electron`

### REST API connection errors
1. Ensure C# REST API is running on http://localhost:5000
2. Check firewall isn't blocking localhost connections
3. Verify API endpoints at http://localhost:5000/api/

## Next Steps

1. Run `install.bat` to install all dependencies
2. Run `start.bat` to start development
3. Open browser to http://localhost:5173 (or check Electron window)
4. Start developing React components connected to REST API
5. When ready, run `npm run electron-build` to create installer

## Building the UI

The project includes comprehensive TypeScript service files for all API endpoints. Use these to build React components that call the REST API:

```typescript
import { dashboardService } from '@/api/dashboardService'

// Fetch metrics
const metrics = await dashboardService.getMetrics()
const history = await dashboardService.getHistory()
```

Refer to `src/pages/Dashboard.tsx` for example component implementation.
