using Azure;
using Azure.AI.Translation.Text;
using CommunityToolkit.Maui.Storage;
using System.Xml.Linq;
using System.Xml;

namespace ResxTranslation4Mac;

public partial class MainPage : ContentPage
{
    private TextTranslationClient? client;
    private string _resxLocation = "Please select Resx Location ... ";
    private string _resxName = String.Empty;
    private string _keytoadd = string.Empty;
    private string _valuetoadd = string.Empty;

    private const string AzureKeyStorageKey = "AzureTranslatorKey";
    private const string AzureEndpointStorageKey = "AzureTranslatorEndpoint";
    private const string AzureRegionStorageKey = "AzureTranslatorRegion";
    private const string DefaultRegion = "Global";
    
    // Dictionary to store dynamically created checkboxes
    private Dictionary<string, (CheckBox checkBox, string languageCode, string fileName)> _languageCheckboxes = new();

    public string ResxName
    {
        get => _resxName;
        set
        {
            _resxName = value;
            OnPropertyChanged(nameof(ResxName));
        }
    }
    
    public string ResxLocation
    {
        get => _resxLocation;
        set
        {
            _resxLocation = value;
            OnPropertyChanged(nameof(ResxLocation));
        }
    }

    public string KeyToAdd
    {
        get => _keytoadd;
        set
        {
            _keytoadd = value;
            OnPropertyChanged(nameof(KeyToAdd));
        }
    }

    public string ValueToAdd
    {
        get => _valuetoadd;
        set
        {
            _valuetoadd = value;
            OnPropertyChanged(nameof(ValueToAdd));
        }
    }

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        InitializeClient();
    }

    private void InitializeClient()
    {
        try
        {
            var apiKey = Preferences.Get(AzureKeyStorageKey, string.Empty);
            var endpointAzure = Preferences.Get(AzureEndpointStorageKey, string.Empty);
            var region = Preferences.Get(AzureRegionStorageKey, DefaultRegion);

            if (!string.IsNullOrEmpty(apiKey))
            {
                client = new TextTranslationClient(new AzureKeyCredential(apiKey), region);
                System.Diagnostics.Debug.WriteLine($"Azure Translation client initialized with region: {region}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Azure API key not found in preferences");
                client = null;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing client: {ex.Message}");
            client = null;
        }
    }

    private async void SettingsBtn_OnClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("SettingsPage");
    }

    private async void ResxBtn_OnClicked(object? sender, EventArgs e)
    {
        CancellationToken cancellationToken = default;
        FolderPickerResult result;
        if (string.IsNullOrWhiteSpace(ResxName))
        {
            await DisplayAlert("Error", "Please provide Resx File Name.", "OK");
            return;
        }
        
#if MACCATALYST
        var folderPicker = new Platforms.MacCatalyst.FolderPickerService();
        var folderPath = await folderPicker.PickFolderAsync();
        
        if (!string.IsNullOrEmpty(folderPath))
        {
            ResxLocation = folderPath;
            BuildCheckboxesFromResxFiles(folderPath);
        }
#else
        // For other platforms, implement accordingly
        await DisplayAlert("Error", "Folder picker not implemented for this platform", "OK");
#endif
    }

    private void BuildCheckboxesFromResxFiles(string folderPath)
    {
        // Clear existing checkboxes
        MyGrid.Clear();
        _languageCheckboxes.Clear();

        if (!Directory.Exists(folderPath))
            return;

        // Get all .resx files
        var resxFiles = Directory.GetFiles(folderPath, "*.resx");
        
        if (resxFiles.Length == 0)
            return;

        // Parse language info from file names
        var languages = new List<(string fileName, string languageCode, string displayName)>();
        
        foreach (var filePath in resxFiles)
        {
            if (filePath.Contains(ResxName + ".") && filePath.EndsWith(".resx"))
            {
                var fileName = Path.GetFileName(filePath);
                var (languageCode, displayName) = ParseLanguageFromFileName(fileName);
                languages.Add((fileName, languageCode, displayName));
            }
            else
            {
                ResxLocation = "No Resx file found in this folder with that name. Please select a valid Resx location or change the name of the Resx file.";
                return; 
            }
        }

        // Sort by display name for better organization
        languages = languages.OrderBy(x => x.displayName).ToList();

        // Calculate grid layout (4 columns)
        int columnsCount = 4;
        int itemsPerColumn = (int)Math.Ceiling((double)languages.Count / columnsCount);

        // Create checkboxes in grid layout
        for (int i = 0; i < languages.Count; i++)
        {
            var (fileName, languageCode, displayName) = languages[i];
            int column = i / itemsPerColumn;
            
            var checkBox = new CheckBox
            {
                IsChecked = true,
                Color = Colors.Blue
            };
            
            var label = new Label
            {
                Text = displayName,
                VerticalOptions = LayoutOptions.Center
            };

            var stackLayout = new HorizontalStackLayout
            {
                Spacing = 10
            };
            stackLayout.Children.Add(checkBox);
            stackLayout.Children.Add(label);

            var verticalStack = GetOrCreateVerticalStackForColumn(column);
            verticalStack.Children.Add(stackLayout);
            
            // Store the checkbox reference
            _languageCheckboxes[languageCode] = (checkBox, languageCode, fileName);
        }
    }
    
    private VerticalStackLayout GetOrCreateVerticalStackForColumn(int column)
    {
        // Check if column already has a VerticalStackLayout
        foreach (var child in MyGrid.Children)
        {
            if (child is VerticalStackLayout vsl && Grid.GetColumn(vsl) == column)
                return vsl;
        }

        // Create new VerticalStackLayout for this column
        var newStack = new VerticalStackLayout();
        Grid.SetColumn(newStack, column);
        MyGrid.Children.Add(newStack);
        return newStack;
    }

    private (string languageCode, string displayName) ParseLanguageFromFileName(string fileName)
    {
        var tempResxName = ResxName + ".";
        // Remove "Resources." prefix and ".resx" suffix
        var baseName = fileName.Replace(tempResxName, "").Replace(".resx", "");

        if (string.IsNullOrEmpty(baseName) || baseName.Equals("resx"))
        {
            // This is the default file (Resources.resx)
            return ("en-US", "English (en-US)");
        }

        // Map language codes to display names
        var languageMap = new Dictionary<string, string>
        {
            { "ar", "Arabic (ar)" },
            { "ar-SA", "Arabic (ar-SA)" },
            { "ba", "Bashkir (ba)" },
            { "bg", "Bulgarian (bg)" },
            { "bn", "Bengali (bn)" },
            { "bs", "Bosnian (bs)" },
            { "ca", "Catalan (ca)" },
            { "ca-ES", "Catalan (ca-ES)" },
            { "cs", "Czech (cs)" },
            { "cs-CZ", "Czech (cs-CZ)" },
            { "cy", "Welsh (cy)" },
            { "da", "Danish (da)" },
            { "da-DK", "Danish (da-DK)" },
            { "de", "German (de)" },
            { "de-DE", "German (de-DE)" },
            { "el", "Greek (el)" },
            { "el-GR", "Greek (el-GR)" },
            { "en", "English (en)" },
            { "en-AU", "English (en-AU)" },
            { "en-CA", "English (en-CA)" },
            { "en-GB", "English (en-GB)" },
            { "en-IN", "English (en-IN)" },
            { "en-NZ", "English (en-NZ)" },
            { "en-US", "English (en-US)" },
            { "en-ZA", "English (en-ZA)" },
            { "es", "Spanish (es)" },
            { "es-ES", "Spanish (es-ES)" },
            { "es-MX", "Spanish (es-MX)" },
            { "et", "Estonian (et)" },
            { "eu", "Basque (eu)" },
            { "fa", "Persian (fa)" },
            { "fi", "Finnish (fi)" },
            { "fr", "French (fr)" },
            { "fr-BE", "French (fr-BE)" },
            { "fr-CA", "French-Canada (fr-CA)"},
            { "fr-FR", "French (fr-FR)" },
            { "gl", "Galician (gl)" },
            { "gl-ES", "Galician (gl-ES)" },
            { "gu", "Gujarati (gu)" },
            { "gu-IN", "Gujarati (gu-IN)" },
            { "he", "Hebrew (he)" },
            { "he-IL", "Hebrew (he-IL)" },
            { "hi", "Hindi (hi)" },
            { "hi-IN", "Hindi (hi-IN)" },
            { "hr", "Croatian (hr)" },
            { "hr-HR", "Croatian (hr-HR)" },
            { "hu", "Hungarian (hu)" },
            { "hu-HU", "Hungarian (hu-HU)" },
            { "id", "Indonesian (id)" },
            { "id-ID", "Indonesian (id-ID)" },
            { "is", "Icelandic (is)" },
            { "is-IS", "Icelandic (is-IS)"},
            { "it", "Italian (it)" },
            { "it-IT", "Italian (it-IT)" },
            { "ja-JP", "Japanese-Japan (ja-JP)" },
            { "ka", "Georgian (ka)" },
            { "kk-KZ", "Kazakh (kk-KZ)" },
            { "km", "Khmer (km)" },
            { "kn", "Kannada (kn)" },
            { "kn-IN", "Kannada (kn-IN)" },
            { "ko", "Korean (ko)" },
            { "ko-KR", "Korean (ko-KR)" },
            { "lt", "Lithuanian (lt)" },
            { "lt-LT", "Lithuanian (lt-LT)" },
            { "lv", "Latvian (lv)" },
            { "lv-LV", "Latvian (lv-LV)" },
            { "mk", "Macedonian (mk)" },
            { "mk-MK", "Macedonian (mk-MK)" },
            { "mr", "Marathi (mr)" },
            { "mr-IN", "Marathi (mr-IN)" },
            { "ms", "Malay (ms)" },
            { "ms-MY", "Malay (ms-MY)" },
            { "nb", "Norwegian (nb)" },
            { "nb-NO", "Norwegian (nb-NO)" },
            { "ne", "Nepali (ne)" },
            { "ne-NP", "Nepali (ne-NP)" },
            { "nl", "Dutch (nl)" },
            { "nl-BE", "Dutch (nl-BE)" },
            { "nl-NL", "Dutch (nl-NL)" },
            { "pa", "Punjabi (pa)" },
            { "pa-IN", "Punjabi (pa-IN)" },
            { "pl", "Polish (pl)" },
            { "pl-PL", "Polish (pl-PL)" },
            { "pt", "Portuguese (pt)" },
            { "pt-BR", "Brazilian Portuguese (pt-BR)" },
            { "pt-PT", "Portuguese (pt-PT)" },
            { "ro", "Romanian (ro)" },
            { "ro-RO", "Romanian (ro-RO)" },
            { "ru", "Russian (ru)" },
            { "ru-RU", "Russian (ru-RU)" },
            { "ru-UA", "Russian (ru-UA)" },
            { "sk", "Slovak (sk)" },
            { "sk-SK", "Slovak (sk-SK)" },
            { "sl", "Slovenian (sl)" },
            { "sl-SI", "Slovenian (sl-SI)" },
            { "sr", "Serbian (sr)" },
            { "sr-RS", "Serbian (sr-RS)" },
            { "sv", "Swedish (sv)" },
            { "sv-FI", "Swedish (sv-FI)" },
            { "sv-SE", "Swedish (sv-SE)" },
            { "ta", "Tamil (ta)" },
            { "ta-IN", "Tamil (ta-IN)" },
            { "te", "Telugu (te)" },
            { "te-IN", "Telugu (te-IN)" },
            { "th", "Thai (th)" },
            { "th-TH", "Thai (th-TH)" },
            { "tl", "Tagalog (tl)" },
            { "tl-PH", "Tagalog (tl-PH)" },
            { "tr", "Turkish (tr)" },
            { "tr-TR", "Turkish (tr-TR)" },
            { "uk", "Ukrainian (uk)" },
            { "uk-UA", "Ukrainian (uk-UA)" },
            { "ur", "Urdu (ur)" },
            { "ur-PK", "Urdu (ur-PK)" },
            { "uz-UZ", "Uzbek (uz-UZ)" },
            { "vi", "Vietnamese (vi)" },
            { "vi-VN", "Vietnamese (vi-VN)" },
            { "zh-CN", "Chinese Mainland (zh-CN)" },
            { "zh-HanT-HK", "Chinese Hong Kong (zh-HanT-HK)" },
            { "zh-HanT-TW", "Chinese Taiwan (zh-HanT-TW)" },
        };

        if (languageMap.TryGetValue(baseName, out var displayName))
        {
            return (baseName, displayName);
        }

        // If not in our map, create a generic display name
        return (baseName, $"{baseName}");
    }
    
    private async void ApplyBtn_OnClicked(object? sender, EventArgs e)
    {
        this.LanguagePB.Progress = 0;

        if (string.IsNullOrWhiteSpace(ResxName))
        {
            await DisplayAlert("Error", "Please provide Resx Name.", "OK");
            return;
        }
        
        if (!Directory.Exists(ResxLocation))
        {
            await DisplayAlert("Error", "Please select a valid Resx location.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(KeyToAdd) || string.IsNullOrWhiteSpace(ValueToAdd))
        {
            await DisplayAlert("Error", "Please provide both Key and Value.", "OK");
            return;
        }

        if (_languageCheckboxes.Count == 0)
        {
            await DisplayAlert("Error", "No language files found. Please select a valid Resx location.", "OK");
            return;
        }

        try
        {
            // Reset progress bar at start
            LanguagePB.Progress = 0;

            var originalValue = ValueToAdd; // Keep original for all translations

            // Get checked languages
            var checkedLanguages = _languageCheckboxes.Values.Where(x => x.checkBox.IsChecked).ToList();
            int totalLanguages = checkedLanguages.Count;
            int completedLanguages = 0;

            foreach (var (checkBox, languageCode, fileName) in checkedLanguages)
            {
                var translatedValue = await TranslateTextAsync(originalValue, languageCode);
                var resxFilePath = Path.Combine(ResxLocation, fileName);
                AddOrUpdateResxEntry(resxFilePath, KeyToAdd.Trim(), translatedValue, languageCode);

                // Increment progress based on completed languages
                completedLanguages++;
                LanguagePB.Progress = (double)completedLanguages / totalLanguages;
            }

            await DisplayAlert("Success", $"Please run 'Generate Resources' by right clicking on '{ResxName}.resx'",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to update Resx files: {ex.Message}", "OK");
        }
        finally
        {
            LanguagePB.Progress = 0;
        }
    }

    private async Task<string> TranslateTextAsync(string text, string targetLanguage)
    {
        var result = await client.TranslateAsync(targetLanguage, text, cancellationToken: CancellationToken.None);
        return result.Value[0].Translations[0].Text;
    }

    private static void AddOrUpdateResxEntry(string resxFilePath, string key, string value, string languageCode)
    {
        XDocument document;

        if (File.Exists(resxFilePath))
        {
            document = XDocument.Load(resxFilePath, LoadOptions.PreserveWhitespace);
        }
        else
        {
            document = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("root"));
        }

        var root = document.Element("root")
                   ?? throw new InvalidOperationException("Resx is missing a root element in: " + languageCode);

        var dataElement = root.Elements("data")
            .FirstOrDefault(x => string.Equals((string?)x.Attribute("name"), key, StringComparison.Ordinal));

        if (dataElement is null)
        {
            dataElement =
                new XElement("data",
                    new XAttribute("name", key),
                    new XAttribute(XNamespace.Xml + "space", "preserve"),
                    new XText("\r\n    "),
                    new XElement("value", value),
                    new XText("\r\n"));

            root.Add(new XText("\r\n"));
            root.Add(dataElement);
            root.Add(new XText("\r\n"));
        }
        else
        {
            dataElement.SetAttributeValue(XNamespace.Xml + "space", "preserve");
            var valueElement = dataElement.Element("value");
            if (valueElement is null)
            {
                dataElement.Add(new XText("\r\n    "));
                dataElement.Add(new XElement("value", value));
                dataElement.Add(new XText("\r\n"));
            }
            else
            {
                valueElement.Value = value;
            }
        }

        var writerSettings = new XmlWriterSettings
        {
            Indent = true,
            NewLineChars = "\r\n",
            NewLineHandling = NewLineHandling.Replace,
            NewLineOnAttributes = true,
        };

        using var writer = XmlWriter.Create(resxFilePath, writerSettings);
        document.Save(writer);
    }
}