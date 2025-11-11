const { app, BrowserWindow } = require('electron');
const path = require('path');
const fs = require('fs');

let mainWindow;

// Check if running in development mode
// If dist folder doesn't exist, we're in dev mode
const distPath = path.join(__dirname, 'dist');
const isDev = !fs.existsSync(distPath);

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    webPreferences: {
      nodeIntegration: false,
      contextIsolation: true,
    },
  });

  let startUrl;
  if (isDev) {
    // Try to detect the actual port Vite is using
    const vitePort = process.env.VITE_PORT || 5173;
    startUrl = `http://localhost:${vitePort}`;
  } else {
    startUrl = `file://${path.join(__dirname, 'dist/index.html')}`;
  }

  mainWindow.loadURL(startUrl);

  // Open DevTools in development
  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });
}

app.on('ready', createWindow);

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  if (mainWindow === null) {
    createWindow();
  }
});
