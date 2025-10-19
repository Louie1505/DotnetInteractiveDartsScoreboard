using MudBlazor;

namespace DartsScoreboardGames.Theming;
public class CustomThemeProvider {

    private bool _darkMode { get; set; } = true;
    public bool DarkMode {
        get {
            return _darkMode;
        }
        set {
            _darkMode = value;
            OnThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler<EventArgs> OnThemeChanged;

    public readonly Lazy<MudTheme> Instance = new(() => new() {
        LayoutProperties = new() { 
            DefaultBorderRadius = "10px",
            AppbarHeight = "1px"
        }
    });
}

