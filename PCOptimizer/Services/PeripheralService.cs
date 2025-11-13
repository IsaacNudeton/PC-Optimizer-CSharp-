using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace PCOptimizer.Services
{
    // Peripheral capability types
    public enum PeripheralCapability
    {
        RGB,           // RGB lighting
        DPI,           // Mouse DPI
        PollingRate,   // Keyboard/Mouse polling rate
        EQ,            // Headset EQ
        Mic,           // Microphone settings
        Battery,       // Battery level
        Buttons,       // Programmable buttons
        Temperature,   // Device temperature monitoring
        FirmwareUpdate // Firmware updates
    }

    public class PeripheralDevice
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Brand { get; set; } = string.Empty;        // Corsair, Razer, Logitech, etc.
        public string Type { get; set; } = string.Empty;         // Mouse, Headset, Keyboard, etc.
        public string Model { get; set; } = string.Empty;
        public List<PeripheralCapability> Capabilities { get; set; } = new();
        public bool IsConnected { get; set; }
        public int BatteryLevel { get; set; } = -1;              // -1 = N/A
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class PeripheralService
    {
        private Dictionary<string, PeripheralDevice> _connectedDevices = new();
        private Dictionary<string, Action<PeripheralDevice, Dictionary<string, object>>> _brandDrivers;

        public PeripheralService()
        {
            _brandDrivers = InitializeBrandDrivers();
            DetectPeripherals();
            System.Console.WriteLine("[PeripheralService] Initialized - scanning for peripherals...");
        }

        /// <summary>
        /// Initializes brand-specific driver actions
        /// </summary>
        private Dictionary<string, Action<PeripheralDevice, Dictionary<string, object>>> InitializeBrandDrivers()
        {
            return new Dictionary<string, Action<PeripheralDevice, Dictionary<string, object>>>
            {
                // Corsair iCUE
                ["Corsair"] = (device, settings) =>
                {
                    try
                    {
                        // Corsair devices are controlled via iCUE SDK or command-line
                        ApplyPeripheralSettings(device, settings, "corsair");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[PeripheralService] Corsair error: {ex.Message}");
                    }
                },

                // Razer Synapse
                ["Razer"] = (device, settings) =>
                {
                    try
                    {
                        ApplyPeripheralSettings(device, settings, "razer");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[PeripheralService] Razer error: {ex.Message}");
                    }
                },

                // Logitech G HUB
                ["Logitech"] = (device, settings) =>
                {
                    try
                    {
                        ApplyPeripheralSettings(device, settings, "logitech");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[PeripheralService] Logitech error: {ex.Message}");
                    }
                },

                // SteelSeries GG
                ["SteelSeries"] = (device, settings) =>
                {
                    try
                    {
                        ApplyPeripheralSettings(device, settings, "steelseries");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[PeripheralService] SteelSeries error: {ex.Message}");
                    }
                },

                // Generic USB HID
                ["Generic"] = (device, settings) =>
                {
                    // Fallback for standard USB HID devices
                    ApplyPeripheralSettings(device, settings, "generic");
                }
            };
        }

        /// <summary>
        /// Detects all connected USB peripherals
        /// </summary>
        public void DetectPeripherals()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_USBHub");
                var usbDevices = searcher.Get();

                foreach (var device in usbDevices)
                {
                    try
                    {
                        var deviceName = device["Name"]?.ToString() ?? "Unknown";
                        var deviceId = device["DeviceID"]?.ToString() ?? "Unknown";

                        // Parse device info
                        var peripheral = ParseUSBDevice(deviceName, deviceId);
                        if (peripheral != null)
                        {
                            _connectedDevices[peripheral.Id] = peripheral;
                            System.Console.WriteLine($"[PeripheralService] Detected: {peripheral.Brand} {peripheral.Type}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[PeripheralService] Error parsing device: {ex.Message}");
                    }
                }

                System.Console.WriteLine($"[PeripheralService] Total peripherals detected: {_connectedDevices.Count}");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[PeripheralService] Failed to detect peripherals: {ex.Message}");
            }
        }

        /// <summary>
        /// Parses USB device info to identify brand and type
        /// </summary>
        private PeripheralDevice? ParseUSBDevice(string deviceName, string deviceId)
        {
            // Simplistic parsing - can be enhanced with vendor IDs
            var peripheral = new PeripheralDevice { IsConnected = true };

            // Identify brand
            if (deviceName.Contains("Corsair", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Brand = "Corsair";
            }
            else if (deviceName.Contains("Razer", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Brand = "Razer";
            }
            else if (deviceName.Contains("Logitech", StringComparison.OrdinalIgnoreCase) ||
                     deviceName.Contains("Logi", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Brand = "Logitech";
            }
            else if (deviceName.Contains("SteelSeries", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Brand = "SteelSeries";
            }
            else
            {
                peripheral.Brand = "Generic";
            }

            // Identify type
            if (deviceName.Contains("Mouse", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Type = "Mouse";
                peripheral.Capabilities.AddRange(new[] { PeripheralCapability.DPI, PeripheralCapability.RGB, PeripheralCapability.PollingRate });
            }
            else if (deviceName.Contains("Keyboard", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Type = "Keyboard";
                peripheral.Capabilities.AddRange(new[] { PeripheralCapability.RGB, PeripheralCapability.PollingRate });
            }
            else if (deviceName.Contains("Headset", StringComparison.OrdinalIgnoreCase) ||
                     deviceName.Contains("Headphone", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Type = "Headset";
                peripheral.Capabilities.AddRange(new[] { PeripheralCapability.EQ, PeripheralCapability.Mic, PeripheralCapability.Battery });
            }
            else if (deviceName.Contains("Pad", StringComparison.OrdinalIgnoreCase) ||
                     deviceName.Contains("Mousepad", StringComparison.OrdinalIgnoreCase))
            {
                peripheral.Type = "Mousepad";
                peripheral.Capabilities.Add(PeripheralCapability.RGB);
            }
            else
            {
                peripheral.Type = "Unknown";
            }

            peripheral.Model = deviceName;
            return peripheral;
        }

        /// <summary>
        /// Gets all connected peripherals
        /// </summary>
        public List<PeripheralDevice> GetConnectedPeripherals()
        {
            return _connectedDevices.Values.ToList();
        }

        /// <summary>
        /// Gets a specific peripheral by ID
        /// </summary>
        public PeripheralDevice? GetPeripheral(string peripheralId)
        {
            return _connectedDevices.TryGetValue(peripheralId, out var device) ? device : null;
        }

        /// <summary>
        /// Gets peripherals by type (Mouse, Headset, etc.)
        /// </summary>
        public List<PeripheralDevice> GetPeripheralsByType(string type)
        {
            return _connectedDevices.Values
                .Where(d => d.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Gets peripherals by brand (Corsair, Razer, etc.)
        /// </summary>
        public List<PeripheralDevice> GetPeripheralsByBrand(string brand)
        {
            return _connectedDevices.Values
                .Where(d => d.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Apply settings to a peripheral (brand-agnostic)
        /// </summary>
        public async Task<bool> ApplyPeripheralSettings(string peripheralId, Dictionary<string, object> settings)
        {
            if (!_connectedDevices.TryGetValue(peripheralId, out var device))
            {
                System.Console.WriteLine($"[PeripheralService] Peripheral not found: {peripheralId}");
                return false;
            }

            return await ApplyPeripheralSettings(device, settings);
        }

        /// <summary>
        /// Internal method to apply settings
        /// </summary>
        private Task<bool> ApplyPeripheralSettings(PeripheralDevice device, Dictionary<string, object> settings)
        {
            try
            {
                // Update local settings
                foreach (var (key, value) in settings)
                {
                    device.Settings[key] = value;
                }

                // Route to appropriate brand driver
                if (_brandDrivers.TryGetValue(device.Brand, out var driverAction))
                {
                    driverAction(device, settings);
                    System.Console.WriteLine($"[PeripheralService] Applied settings to {device.Brand} {device.Type}");
                    return Task.FromResult(true);
                }

                System.Console.WriteLine($"[PeripheralService] No driver for brand: {device.Brand}");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[PeripheralService] Error applying settings: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Set DPI for all mice (universal)
        /// </summary>
        public async Task<bool> SetMouseDPI(int dpi)
        {
            var mice = GetPeripheralsByType("Mouse");
            var allSuccess = true;

            foreach (var mouse in mice)
            {
                var settings = new Dictionary<string, object> { { "DPI", dpi } };
                var success = await ApplyPeripheralSettings(mouse, settings);
                allSuccess = allSuccess && success;
            }

            return allSuccess;
        }

        /// <summary>
        /// Set RGB color for all RGB-capable devices
        /// </summary>
        public async Task<bool> SetRGBColor(int r, int g, int b)
        {
            var rgbDevices = _connectedDevices.Values
                .Where(d => d.Capabilities.Contains(PeripheralCapability.RGB))
                .ToList();

            var allSuccess = true;

            foreach (var device in rgbDevices)
            {
                var settings = new Dictionary<string, object>
                {
                    { "RGB_Red", r },
                    { "RGB_Green", g },
                    { "RGB_Blue", b }
                };
                var success = await ApplyPeripheralSettings(device, settings);
                allSuccess = allSuccess && success;
            }

            return allSuccess;
        }

        /// <summary>
        /// Set headset EQ preset
        /// </summary>
        public async Task<bool> SetHeadsetEQ(string preset)
        {
            var headsets = GetPeripheralsByType("Headset");
            var allSuccess = true;

            foreach (var headset in headsets)
            {
                var settings = new Dictionary<string, object> { { "EQ_Preset", preset } };
                var success = await ApplyPeripheralSettings(headset, settings);
                allSuccess = allSuccess && success;
            }

            return allSuccess;
        }

        /// <summary>
        /// Apply a gaming profile to all peripherals
        /// </summary>
        public async Task<bool> ApplyGamingProfile()
        {
            System.Console.WriteLine("[PeripheralService] Applying Gaming profile to all peripherals...");

            // Gaming profile settings
            var gamingSettings = new Dictionary<string, object>
            {
                { "DPI", 800 },                          // Lower DPI for better control
                { "PollingRate", 1000 },                 // Max polling rate
                { "RGB_Brightness", 50 },               // Reduce RGB brightness (less distraction)
                { "EQ_Preset", "Gaming" },              // Gaming headset EQ
                { "PollingRate", 8000 }                 // 8kHz for gaming mice
            };

            var mice = GetPeripheralsByType("Mouse");
            var headsets = GetPeripheralsByType("Headset");
            var allSuccess = true;

            foreach (var mouse in mice)
            {
                var success = await ApplyPeripheralSettings(mouse, gamingSettings);
                allSuccess = allSuccess && success;
            }

            foreach (var headset in headsets)
            {
                var success = await ApplyPeripheralSettings(headset, gamingSettings);
                allSuccess = allSuccess && success;
            }

            return allSuccess;
        }

        /// <summary>
        /// Helper for brand-specific implementations
        /// </summary>
        private void ApplyPeripheralSettings(PeripheralDevice device, Dictionary<string, object> settings, string brand)
        {
            // This would be extended to actually call the brand's SDK or CLI tools
            // For now, we're just logging the action
            var settingsStr = string.Join(", ", settings.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            System.Console.WriteLine($"[PeripheralService] [{brand.ToUpper()}] {device.Type}: {settingsStr}");

            // Example: Corsair integration
            if (brand == "corsair" && device.Brand == "Corsair")
            {
                // Would call: C:\Program Files\Corsair\CORSAIR iCUE\iCUE.exe (with COM/IPC)
                // Or use Corsair SDK DLL imports
            }

            // Example: Razer integration
            if (brand == "razer" && device.Brand == "Razer")
            {
                // Would call Razer Synapse SDK or CLI
            }
        }
    }
}
