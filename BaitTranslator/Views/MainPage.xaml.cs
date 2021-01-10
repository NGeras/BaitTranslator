using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
            _xlfFile = await picker.PickSingleFileAsync();
            if (_xlfFile != null)
            {
                _result.Clear();
                using (var stream = await _xlfFile.OpenStreamForReadAsync())
                {
                    _nodeList = XDocumentReader.ReadXlfAsync(stream);
                    if (_nodeList.Count > 0)
                    {
                        XlfList.ItemsSource = _nodeList;
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

            if (file != null)
            {
                _result.Clear();
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    _xlList = XDocumentReader.ReadXlsx(stream);
                    if(_xlList.Count > 0)
                    {
                        ExcelList.ItemsSource = _xlList;
                    }
                }
            }
        }

        private async void ConvertBtn_Click(object sender, RoutedEventArgs e)
        {
            GetComparison();
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
            GetComparison();
            ResultList.ItemsSource = _result;
        }

        private void GetComparison()
        {
            _result.Clear();
            if (_xlList.Count < 1 || _nodeList.Count < 1)
            {
                _result.Clear();
            }

            foreach (var node in _xlList)
            {
                node.sourceNode.TrimEnd(':');
            }

            var list = _xlList.Select(c => c.sourceNode);

            foreach (var node in _nodeList)
            {
                if (node.sourceNode.Equals(node.targetNode))
                {
                    if (list.Contains(node.sourceNode.TrimEnd(':')))
                    {
                        var nodeToadd =
                            _xlList.First(c => c.sourceNode.ToLower().Equals(node.sourceNode.TrimEnd(':').ToLower()));
                        node.targetNode = nodeToadd.targetNode;
                        _result.Add(node);
                    }
                }
            }
        }

        private void ClearList_OnClick(object sender, RoutedEventArgs e)
        {
            string appBarName = ((AppBarButton)sender).Name;
            if (appBarName.Equals("ClearXlf"))
            {
                _nodeList.Clear();
            }
            else if (appBarName.Equals("ClearExcel"))
            {
                _xlList.Clear();
            }
        }
    }
}
