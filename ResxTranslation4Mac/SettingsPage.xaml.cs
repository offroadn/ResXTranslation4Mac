namespace ResxTranslation4Mac;

public partial class SettingsPage : ContentPage
{
    private const string AzureKeyStorageKey = "AzureTranslatorKey";
    private const string AzureEndpointStorageKey = "AzureTranslatorEndpoint";
    private const string AzureRegionStorageKey = "AzureTranslatorRegion";
    private const string DefaultEndpoint = "https://api.cognitive.microsofttranslator.com/";
    private const string DefaultRegion = "Global";

    public SettingsPage()
    {
        InitializeComponent();
        Loaded += OnPageLoaded;
    }

    private void OnPageLoaded(object? sender, EventArgs e)
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        try
        {
            var apiKey = Preferences.Get(AzureKeyStorageKey, string.Empty);
            var endpoint = Preferences.Get(AzureEndpointStorageKey, DefaultEndpoint);
            var region = Preferences.Get(AzureRegionStorageKey, DefaultRegion);

            AzureKeyEntry.Text = apiKey;
            AzureEndpointEntry.Text = endpoint;
            RegionEntry.Text = region;
        }
        catch (Exception ex)
        {
            DisplayAlert("Error", $"Failed to load settings: {ex.Message}", "OK");
        }
    }

    private void TogglePasswordBtn_OnClicked(object? sender, EventArgs e)
    {
        AzureKeyEntry.IsPassword = !AzureKeyEntry.IsPassword;
        TogglePasswordBtn.Text = AzureKeyEntry.IsPassword ? "👁️" : "🙈";
    }
    
    private async void SaveBtn_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            var apiKey = AzureKeyEntry.Text?.Trim();
            var endpoint = AzureEndpointEntry.Text?.Trim();
            var region = RegionEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                await DisplayAlert("Validation Error", "Please enter an Azure Translator Key.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                endpoint = DefaultEndpoint;
            }

            if (string.IsNullOrWhiteSpace(region))
            {
                region = DefaultRegion;
            }

            Preferences.Set(AzureKeyStorageKey, apiKey);
            Preferences.Set(AzureEndpointStorageKey, endpoint);
            Preferences.Set(AzureRegionStorageKey, region);

            StatusLabel.Text = "Settings saved successfully!";
            StatusLabel.IsVisible = true;

            // Hide status message after 3 seconds
            await Task.Delay(3000);
            StatusLabel.IsVisible = false;

            // Don't need to restart - just go back and it will re-initialize
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save settings: {ex.Message}", "OK");
        }
    }

    private async void BackBtn_OnClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}