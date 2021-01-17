using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BaitTranslator.Helpers
{
    public static class Notifications
    {
        public static MessageDialog CreateYesNoDialog(string title, string content)
        {
            var c = new MessageDialog(content, title);
            var res = ResourceLoader.GetForCurrentView();
            c.Commands.Add(new UICommand(res.GetString("YesAnswer")));
            c.Commands.Add(new UICommand(res.GetString("NoAnswer")));
            return c;
        }
        public static async Task CatchUnauthorizedAccessException(string deniedObject)
        {
            var dialog = CreateYesNoDialog($"App needs access to the {deniedObject}", $"The app needs to access the {deniedObject}. " +
                               "Press OK to open system settings and give this app permission. " +
                               "If the app closes, reopen it afterwards. ");
            var result = await dialog.ShowAsync();
            var res = ResourceLoader.GetForCurrentView();
            var no = res.GetString("NoAnswer");
            if (result.Label.Equals(no))
            {
                Application.Current.Exit();
            }
            else
            {
                switch (deniedObject)
                {
                    case "microphone":
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-microphone"));
                        break;
                    case "file system":
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
                        break;
                    case "camera":
                        await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-webcam"));
                        break;
                }
                // var okDialog = CreateOKDialog("App needs access to the file system", "Please give this app the file system permission " +
                //             "in the Settings app which has now opened.");
                // await okDialog.ShowAsync();
            }

        }
        public static ContentDialog CreateOKDialog(string title, string content)
        {
            var c = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = "OK",
                IsPrimaryButtonEnabled = true,
                DefaultButton = ContentDialogButton.Primary
            };
            return c;
        }
    }
}
