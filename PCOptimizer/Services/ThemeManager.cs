using System;
using System.Windows;

namespace PCOptimizer.Services
{
    /// <summary>
    /// Manages CosmicUI theme switching with profile-based themes and accent overlays
    /// </summary>
    public class ThemeManager
    {
        private static ThemeManager? _instance;
        private string _currentProfile = "Universal";
        private string _currentAccent = "Default";

        public static ThemeManager Instance => _instance ??= new ThemeManager();

        public string CurrentProfile => _currentProfile;
        public string CurrentAccent => _currentAccent;

        /// <summary>
        /// Applies a theme based on profile and optional accent overlay
        /// </summary>
        /// <param name="profile">Theme profile: Universal, Gaming, or Work</param>
        /// <param name="accentOverlay">Optional accent: Default, Pink, Purple, or Blue</param>
        public void ApplyTheme(string profile, string accentOverlay = "Default")
        {
            try
            {
                // Clear existing CosmicUI dictionaries
                var mergedDicts = Application.Current.Resources.MergedDictionaries;

                // Remove old CosmicUI theme dictionaries (keep other resources)
                for (int i = mergedDicts.Count - 1; i >= 0; i--)
                {
                    var dict = mergedDicts[i];
                    if (dict.Source != null &&
                        (dict.Source.OriginalString.Contains("CosmicUI/Themes/") ||
                         dict.Source.OriginalString.Contains("CosmicUI\\Themes\\")))
                    {
                        mergedDicts.RemoveAt(i);
                    }
                }

                // Load base theme
                var themeDict = new ResourceDictionary();
                switch (profile)
                {
                    case "Gaming":
                        themeDict.Source = new Uri("pack://application:,,,/CosmicUI/Themes/GamingTheme.xaml", UriKind.Absolute);
                        break;
                    case "Work":
                        themeDict.Source = new Uri("pack://application:,,,/CosmicUI/Themes/WorkTheme.xaml", UriKind.Absolute);
                        break;
                    default:
                        themeDict.Source = new Uri("pack://application:,,,/CosmicUI/Themes/VoidTheme.xaml", UriKind.Absolute);
                        profile = "Universal";
                        break;
                }

                // Insert base theme at the beginning (so it can be overridden)
                mergedDicts.Insert(0, themeDict);

                // Apply accent overlay if specified
                if (accentOverlay != "Default")
                {
                    var overlayDict = new ResourceDictionary();
                    switch (accentOverlay)
                    {
                        case "Pink":
                            overlayDict.Source = new Uri("pack://application:,,,/CosmicUI/Themes/Overlays/PinkAccent.xaml", UriKind.Absolute);
                            break;
                        case "Purple":
                            overlayDict.Source = new Uri("pack://application:,,,/CosmicUI/Themes/Overlays/PurpleAccent.xaml", UriKind.Absolute);
                            break;
                        case "Blue":
                            overlayDict.Source = new Uri("pack://application:,,,/CosmicUI/Themes/Overlays/BlueAccent.xaml", UriKind.Absolute);
                            break;
                        default:
                            accentOverlay = "Default";
                            break;
                    }

                    if (accentOverlay != "Default")
                    {
                        mergedDicts.Insert(1, overlayDict);
                    }
                }

                _currentProfile = profile;
                _currentAccent = accentOverlay;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Theme application failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets available theme profiles
        /// </summary>
        public string[] GetAvailableProfiles()
        {
            return new[] { "Universal", "Gaming", "Work" };
        }

        /// <summary>
        /// Gets available accent overlays
        /// </summary>
        public string[] GetAvailableAccents()
        {
            return new[] { "Default", "Pink", "Purple", "Blue" };
        }
    }
}
