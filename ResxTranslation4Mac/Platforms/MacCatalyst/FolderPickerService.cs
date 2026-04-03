using Foundation;
using UIKit;
using UniformTypeIdentifiers;

namespace ResxTranslation4Mac.Platforms.MacCatalyst;

public class FolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            try
            {
                var documentPicker = new UIDocumentPickerViewController(new[] { UTTypes.Folder }, false);
                documentPicker.AllowsMultipleSelection = false;

                documentPicker.DidPickDocumentAtUrls += (sender, e) =>
                {
                    if (e.Urls != null && e.Urls.Length > 0)
                    {
                        var url = e.Urls[0];
                        url.StartAccessingSecurityScopedResource();
                        tcs.TrySetResult(url.Path);
                    }
                    else
                    {
                        tcs.TrySetResult(null);
                    }
                };

                documentPicker.WasCancelled += (sender, e) =>
                {
                    tcs.TrySetResult(null);
                };

                var viewController = GetCurrentViewController();
                if (viewController != null)
                {
                    await viewController.PresentViewControllerAsync(documentPicker, true);
                }
                else
                {
                    tcs.TrySetResult(null);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing folder picker: {ex.Message}");
                tcs.TrySetResult(null);
            }
        });

        return await tcs.Task;
    }

    private static UIViewController? GetCurrentViewController()
    {
        var window = UIApplication.SharedApplication.KeyWindow 
                     ?? UIApplication.SharedApplication.Windows.FirstOrDefault();
        
        if (window == null)
            return null;

        var viewController = window.RootViewController;

        while (viewController?.PresentedViewController != null)
        {
            viewController = viewController.PresentedViewController;
        }

        return viewController;
    }
}
