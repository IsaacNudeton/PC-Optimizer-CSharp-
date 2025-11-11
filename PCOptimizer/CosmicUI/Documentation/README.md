# CosmicUI - Universal Theme Library

A reusable WPF theme library with dynamic profile-based themes for desktop applications.

## 📁 Structure

```
CosmicUI/
├── Themes/              # Theme resource dictionaries
│   ├── VoidTheme.xaml   # Default - Cosmic/Black Hole monochrome
│   ├── GamingTheme.xaml # Gaming - Red/Black aggressive cosmic
│   ├── WorkTheme.xaml   # Work - Daylight Cosmos ergonomic
│   └── Overlays/        # Color accent overlays
│       ├── PinkAccent.xaml   # Pink/Rose accents
│       ├── PurpleAccent.xaml # Purple/Nebula accents
│       └── BlueAccent.xaml   # Blue/Aquatic accents
├── Controls/            # Custom controls (to be implemented)
│   ├── AccretionDiskGauge.xaml
│   └── (more controls...)
├── Resources/           # Shared resources (icons, images)
└── Documentation/       # This file

```

## 🎨 Themes

### Profile-Based Themes (Auto-Switch)

### 1. **VoidTheme** (Default/Balanced)
**Aesthetic:** Cosmic/Black Hole/Event Horizon Monochrome
- Pure blacks (#000000) to dark grays (#262626)
- White/silver accents (#FFFFFF, #E0E0E0)
- Hawking Radiation glow (soft blue-white: #B8D4E8)
- Minimal color, maximum contrast
- **Profile:** Default, Balanced, Neutral

### 2. **GamingTheme** (Gaming Profile)
**Aesthetic:** Red/Black Aggressive
- Pure black backgrounds with red tints
- Crimson/Blood Red accents (#FF0000, #DC143C, #8B0000)
- Fire glow effects
- Sharp, aggressive styling
- **Profile:** Gaming, Performance

### 3. **WorkTheme** (Work/Eco Profile)
**Aesthetic:** Ergonomic/Minimal/Professional
- Light backgrounds (#F5F5F7, #FFFFFF)
- Soft blues (#007AFF, #5AC8FA)
- Reduced blue light, eye-friendly
- Calm animations, subtle transitions
- **Profile:** Work, Eco, Productivity

---

## 🎨 Color Customization (Optional Overlays)

Users can customize the accent colors of any of the 3 core themes through settings:

### Accent Color Options:
- **Default**: Uses the theme's default cosmic accent (Hawking Radiation blue-white glow)
- **Pink Overlay**: Replaces accent brushes with soft pink tones (#FF69B4, #FFB6D9) - Hello Kitty aesthetic
- **Purple Overlay**: Replaces accent brushes with nebula purple/violet (#9D4EDD, #C77DFF) - Mystical cosmic
- **Blue Overlay**: Replaces accent brushes with deep ocean blue (#00B4D8, #48CAE4) - Calm aquatic

**Note**: Overlays only modify accent colors (HawkingRadiationBrush, QuantumGlowBrush) while maintaining the theme's core universe aesthetic. Background and text colors remain unchanged.

## 🔧 Usage

### Basic Integration

1. **Copy CosmicUI folder** to your project directory or reference as external library

2. **Add theme to App.xaml**:
```xaml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Default theme -->
            <ResourceDictionary Source="/CosmicUI/Themes/VoidTheme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Dynamic Theme Switching (C#)

```csharp
public void SwitchTheme(string profile, string accentOverlay = "Default")
{
    // Clear existing dictionaries
    Application.Current.Resources.MergedDictionaries.Clear();

    // Load base theme
    var themeDict = new ResourceDictionary();
    switch (profile)
    {
        case "Gaming":
            themeDict.Source = new Uri("/CosmicUI/Themes/GamingTheme.xaml", UriKind.Relative);
            break;
        case "Work":
            themeDict.Source = new Uri("/CosmicUI/Themes/WorkTheme.xaml", UriKind.Relative);
            break;
        default:
            themeDict.Source = new Uri("/CosmicUI/Themes/VoidTheme.xaml", UriKind.Relative);
            break;
    }
    Application.Current.Resources.MergedDictionaries.Add(themeDict);

    // Apply accent overlay if specified
    if (accentOverlay != "Default")
    {
        var overlayDict = new ResourceDictionary();
        switch (accentOverlay)
        {
            case "Pink":
                overlayDict.Source = new Uri("/CosmicUI/Themes/Overlays/PinkAccent.xaml", UriKind.Relative);
                break;
            case "Purple":
                overlayDict.Source = new Uri("/CosmicUI/Themes/Overlays/PurpleAccent.xaml", UriKind.Relative);
                break;
            case "Blue":
                overlayDict.Source = new Uri("/CosmicUI/Themes/Overlays/BlueAccent.xaml", UriKind.Relative);
                break;
        }
        Application.Current.Resources.MergedDictionaries.Add(overlayDict);
    }
}
```

**Example Usage:**
```csharp
// Gaming profile with default red accents
SwitchTheme("Gaming");

// Work profile with pink accents (Hello Kitty vibe)
SwitchTheme("Work", "Pink");

// Universal/Void theme with purple nebula accents
SwitchTheme("Default", "Purple");
```

## 🎯 Color Reference

### Shared Brush Keys (all themes)
```xaml
<!-- Backgrounds -->
VoidBlackBrush
EventHorizonBrush
DeepSpaceBrush
DarkMatterBrush

<!-- Text -->
StarlightBrush
SilverGlowBrush
GrayNebulaBrush
DimStarBrush

<!-- Accents -->
HawkingRadiationBrush
QuantumGlowBrush

<!-- Semantic -->
SuccessVoidBrush
WarningVoidBrush
DangerVoidBrush

<!-- Gradients -->
AccretionDiskGradient
```

## 🚀 Custom Controls

### AccretionDiskGauge
Circular gauge with rotating accretion disk effect

**Properties:**
- `Value` (double): Current value (0-100)
- `Label` (string): Bottom label text
- `Unit` (string): Unit display ("%", "°C", etc.)

**Usage:**
```xaml
<cosmic:AccretionDiskGauge Value="75" Label="CPU" Unit="%" />
```

## 📦 Installation in New Projects

1. Copy entire `CosmicUI/` folder to project root
2. Reference in App.xaml
3. Use brush keys in your XAML
4. Implement profile-based theme switching

## 🎨 Customization

To create new themes:
1. Copy VoidTheme.xaml as template
2. Modify color values while keeping brush key names
3. Add new theme to switch logic

## 📝 License

Reusable across all Event Horizon / Universal EDA projects
