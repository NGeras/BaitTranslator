﻿using System;
using System.Collections.Generic;
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
        List<Node> _nodeList = new List<Node>();
        List<Node> _xlList = new List<Node>();
        StorageFile _xlfFile;
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
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".xlf");
            _xlfFile = await picker.PickSingleFileAsync();

            if (_xlfFile != null)
            {
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
            var result = new List<Node>();
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
                        var nodeToadd = _xlList.First(c => c.sourceNode.ToLower().Equals(node.sourceNode.TrimEnd(':').ToLower()));
                        node.targetNode = nodeToadd.targetNode;
                        result.Add(node);
                    }
                }
            }
            ResultList.ItemsSource = result;
            if(_xlfFile != null)
            {
                using (var stream = await _xlfFile.OpenStreamForWriteAsync())
                {
                    XDocumentReader.WriteXlf(result, stream);
                }
            }
        }
    }
}
