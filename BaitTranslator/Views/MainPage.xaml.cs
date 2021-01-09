using System;
using System.IO;
using BaitTranslator.Helpers;
using BaitTranslator.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace BaitTranslator.Views
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel ViewModel
        {
            get { return ViewModelLocator.Current.MainViewModel; }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private async void XlfAppBar_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".xlf");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var nodeList = XDocumentReader.ReadXlfAsync(stream);
                    if (nodeList.Count > 0)
                    {
                        XlfList.ItemsSource = nodeList;
                    }
                }
            }
        }

        private async void ExcelAppBar_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xlsm");
            picker.FileTypeFilter.Add(".xls");
            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    var xlList = XDocumentReader.ReadXlsx(stream);
                    if(xlList.Count > 0)
                    {
                        ExcelList.ItemsSource = xlList;
                    }
                }
            }
        }
    }
}
