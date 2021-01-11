using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Text;
using Windows.UI.Xaml;
using BaitTranslator.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using BaitTranslator.Helpers;

namespace BaitTranslator.Views
{
    public sealed partial class BlankPage : Page
    {
        private StorageFile _file;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private BlankViewModel ViewModel
        {
            get { return ViewModelLocator.Current.BlankViewModel; }
        }

        public BlankPage()
        {
            InitializeComponent();
        }

        private async Task ReadLog()
        {
            if (_file != null)
            {
                try
                {
                    var x = await FileIO.ReadTextAsync(_file);
                    var bytes = System.Text.Encoding.Unicode.GetBytes(x);
                    InMemoryRandomAccessStream randomAccessStream = new InMemoryRandomAccessStream();
                    await randomAccessStream.WriteAsync(bytes.AsBuffer());
                    IRandomAccessStream stream2 = randomAccessStream;
                    RichEditTextBlock.Document.LoadFromStream(TextSetOptions.FormatRtf, stream2);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private void StopTextBtn_OnClick(object sender, RoutedEventArgs e)
        {
            if (_cts != null)
            {
                _cts.Cancel();
                StopTextBtn.IsEnabled = false;
            }
        }
        public async Task PeriodicMethodAsync(TimeSpan interval, CancellationToken cancellationToken)
        {
            while (true)
            {
                await ReadLog();
                await Task.Delay(interval, cancellationToken);
            }
        }

        private void RichEditTextBlock_OnTextChanged(object sender, RoutedEventArgs e)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(RichEditTextBlock, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f);
                break;
            }
        }

        private async void AddFileBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add(".txt");
            picker.FileTypeFilter.Add(".log");
            _file = await picker.PickSingleFileAsync();
            PeriodicMethodAsync(TimeSpan.FromSeconds(5), _cts.Token);
        }
    }
}
