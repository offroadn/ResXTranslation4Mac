namespace ResxTranslation4Mac;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell())
        {
            Title = "ResX Translations for macOS",
            Width = 800,
            Height = 800
        };
#if MACCATALYST
        // Set minimum window size for Mac
        window.MinimumWidth = 800;
        window.MinimumHeight = 800;
        window.MaximumWidth = 1400;
        window.MaximumHeight = 1400;
#endif
        
        return window;
    }
}