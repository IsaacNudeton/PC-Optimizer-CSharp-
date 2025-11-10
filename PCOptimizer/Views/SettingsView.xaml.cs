using System.Windows;
using System.Windows.Controls;
using PCOptimizer.Services;

namespace PCOptimizer.Views
{
    public partial class SettingsView : UserControl
    {
        private string _currentProfile = "Universal";
        private string _currentAccent = "Default";

        public SettingsView()
        {
            InitializeComponent();
        }

        private void OnThemeProfileChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                // Determine which profile was selected
                if (radioButton == UniversalThemeRadio)
                    _currentProfile = "Universal";
                else if (radioButton == GamingThemeRadio)
                    _currentProfile = "Gaming";
                else if (radioButton == WorkThemeRadio)
                    _currentProfile = "Work";

                // Apply the theme with current accent
                ApplyCurrentTheme();
            }
        }

        private void OnAccentOverlayChanged(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                // Determine which accent was selected
                if (radioButton == DefaultAccentRadio)
                    _currentAccent = "Default";
                else if (radioButton == PinkAccentRadio)
                    _currentAccent = "Pink";
                else if (radioButton == PurpleAccentRadio)
                    _currentAccent = "Purple";
                else if (radioButton == BlueAccentRadio)
                    _currentAccent = "Blue";

                // Apply the theme with new accent
                ApplyCurrentTheme();
            }
        }

        private void ApplyCurrentTheme()
        {
            ThemeManager.Instance.ApplyTheme(_currentProfile, _currentAccent);
        }
    }
}
