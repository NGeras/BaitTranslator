using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using BaitTranslator.Core.Models;
using BaitTranslator.Helpers;
using BaitTranslator.ViewModels;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BaitTranslator.Views
{
    public sealed partial class MainPage : Page
    {
        private ObservableCollection<Node> _nodeList = new ObservableCollection<Node>();
        private ObservableCollection<Node> _xlList = new ObservableCollection<Node>();
        private ObservableCollection<Node> _result = new ObservableCollection<Node>();
        private StorageFile _xlfFile;
        private StorageFolder _folder;

        private MainViewModel ViewModel
        {
            get { return ViewModelLocator.Current.MainViewModel; }
        }

        public MainPage()
        {
            InitializeComponent();
        }

        private async void XlfAppBar_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail, SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add(".xlf");
            var file = await picker.PickSingleFileAsync();
            await GetXlfFile(file);
        }

        private async Task GetXlfFile(StorageFile file)
        {
            _xlfFile = file;
            if (_xlfFile != null)
            {
                _result.Clear();
                using (var stream = await _xlfFile.OpenStreamForReadAsync())
                {
                    _nodeList = XDocumentReader.ReadXlfAsync(stream);
                    if (_nodeList.Count > 0)
                    {
                        XlfList.ItemsSource = _nodeList;
                        CommandAppBar.Content = XlfList.Items.Count.ToString();
                    }
                }
            }
        }

        private async void ExcelAppBar_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail, SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add(".xlsx");
            picker.FileTypeFilter.Add(".xlsm");
            picker.FileTypeFilter.Add(".xls");
            StorageFile file = await picker.PickSingleFileAsync();

            await GetExcelFile(file);
        }

        private async Task GetExcelFile(StorageFile file)
        {
            if (file != null)
            {
                _result.Clear();
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    _xlList = XDocumentReader.ReadXlsx(stream);
                    if (_xlList.Count > 0)
                    {
                        ExcelList.ItemsSource = _xlList;
                        CommandAppBar2.Content = ExcelList.Items.Count.ToString();
                    }
                }
            }
        }

        private async void ConvertBtn_Click(object sender, RoutedEventArgs e)
        {
            GetComparison(false);
            _folder = await FileHelper.TryGetFolder(UserDataPaths.GetDefault().Desktop);
            var newfile = await _folder.CreateFileAsync(_xlfFile.Name,
                CreationCollisionOption.GenerateUniqueName);
            if (_xlfFile != null)
            {
                using (var stream = await _xlfFile.OpenStreamForWriteAsync())
                {
                    var xlfDoc = XDocumentReader.WriteXlf(_result, stream);
                    await FileIO.WriteTextAsync(newfile, xlfDoc);
                }
            }
        }

        private void CompareBtn_OnClick(object sender, RoutedEventArgs e)
        {
            GetComparison(true);
            ResultList.ItemsSource = _result;
        }

        private void GetComparison(bool isCompare)
        {
            _result.Clear();
            if (_xlList.Count < 1 || _nodeList.Count < 1)
            {
                _result.Clear();
            }

            var list = _xlList.Select(c => c.sourceNode);
            foreach (var node in _nodeList)
            {
                if (node.sourceNode.Equals(node.targetNode))
                {
                    if (list.Contains(node.sourceNode))
                    {
                        var nodeToadd =
                            _xlList.First(c => c.sourceNode.Equals(node.sourceNode));
                        if (isCompare)
                        {
                            _result.Add(new Node(node.sourceNode, nodeToadd.targetNode));
                        }
                        else
                        {
                            node.targetNode = nodeToadd.targetNode;
                            _result.Add(node);
                        }
                    }
                }
            }
        }

        private void ClearList_OnClick(object sender, RoutedEventArgs e)
        {
            string appBarName = ((AppBarButton) sender).Name;
            if (appBarName.Equals("ClearXlf"))
            {
                _nodeList.Clear();
                CommandAppBar.Content = 0;
            }
            else if (appBarName.Equals("ClearExcel"))
            {
                _xlList.Clear();
                CommandAppBar2.Content = 0;
            }
        }

        private void SortList_OnClick(object sender, RoutedEventArgs e)
        {
            var toggle = sender as AppBarToggleButton;
            var notDoneList = new ObservableCollection<Node>();
            var doneList = new ObservableCollection<Node>();
            if (toggle.IsChecked == null)
            {
                foreach (var node in _nodeList)
                {
                    if (!node.sourceNode.Equals(node.targetNode))
                    {
                        doneList.Add(node);
                    }
                }

                XlfList.ItemsSource = doneList;
                SortXlf.Label = "Done";
            }
            else if ((bool) toggle.IsChecked)
            {
                foreach (var node in _nodeList)
                {
                    if (node.sourceNode.Equals(node.targetNode))
                    {
                        notDoneList.Add(node);
                    }
                }

                SortXlf.Label = "Not done";
                XlfList.ItemsSource = notDoneList;
            }
            else if ((bool) !toggle.IsChecked)
            {
                XlfList.ItemsSource = _nodeList;
                SortXlf.Label = "All";
            }

            CommandAppBar.Content = XlfList.Items.Count.ToString();
        }

        private void Grid_OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Grid_OnDrop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var storageItems = await e.DataView.GetStorageItemsAsync();
                if (storageItems.Count > 2)
                {
                    var tooManyDialog = Notifications.CreateOKDialog("Max 2 files supported", $"You have dropped {storageItems.Count} item(s)!");
                    await tooManyDialog.ShowAsync();
                    return;
                }
                foreach (StorageFile storageItem in storageItems)
                {
                    var file = await FileHelper.TryGetFile(storageItem.Path);
                    if (file.FileType.Equals(".xlf"))
                    {
                        await GetXlfFile(file);
                    }
                    else if (file.FileType.Equals(".xlsx")|| file.FileType.Equals(".xls") || file.FileType.Equals(".xlsm"))
                    {
                        await GetExcelFile(file);
                    }
                    else
                    {
                        var dialog = Notifications.CreateOKDialog("File not supported", $"This file type '{file.FileType}' is not supported!");
                        await dialog.ShowAsync();
                    }
                }
            }
        }
    }
}
