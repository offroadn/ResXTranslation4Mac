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
            Title = "RESX Translation Tool for the Mac"
        };
#if MACCATALYST
        // Set minimum window size for Mac
        window.MinimumWidth = 800;
        window.MinimumHeight = 600;
#endif
        
        return window;
    }
}