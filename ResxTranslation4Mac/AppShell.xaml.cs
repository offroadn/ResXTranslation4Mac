namespace ResxTranslation4Mac;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("SettingsPage", typeof(SettingsPage));

    }  
}