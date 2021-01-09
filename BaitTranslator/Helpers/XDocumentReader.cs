using BaitTranslator.Core.Models;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Windows.Storage;

namespace BaitTranslator.Helpers
{
    public static class XDocumentReader
    {
        public static List<Node> ReadXlfAsync(Stream stream)
        {
            XDocument doc = XDocument.Load(stream);
            XNamespace df = doc.Root.Name.Namespace;
            var nodeList = new List<Node>();
            foreach (XElement transUnitNode in doc.Descendants(df + "trans-unit"))
            {
                XElement sourceNode = transUnitNode.Element(df + "source");
                XElement targetNode = transUnitNode.Element(df + "target");
                nodeList.Add(new Node(sourceNode.Value, targetNode.Value));
            }

            return nodeList;
        }
        public static void WriteXlf(List<Node> list, Stream stream)
        {
            XDocument doc = XDocument.Load(stream);
            XNamespace df = doc.Root.Name.Namespace;
            foreach (XElement transUnitNode in doc.Descendants(df + "trans-unit"))
            {
                XElement targetNode = transUnitNode.Element(df + "target");
                foreach (var item in list)
                {
                    if (targetNode.Value.Equals(item.sourceNode))
                    {
                        targetNode.SetValue(item.targetNode);
                    }
                }
            }
            doc.Save(stream);
            stream.Flush();
        }
        public static List<Node> ReadXlsx(Stream stream)
        {
            var list = new List<Node>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage xlPackage = new ExcelPackage(stream))
            {
                var myWorksheet = xlPackage.Workbook.Worksheets.First(); //select sheet here
                var totalRows = myWorksheet.Dimension.End.Row;
                var totalColumns = myWorksheet.Dimension.End.Column;
                for (int rowNum = 1; rowNum <= totalRows; rowNum++) //select starting row here
                {
                    var row = myWorksheet.Cells[rowNum, 1, rowNum, totalColumns].Select(c => c.Value == null ? string.Empty : c.Value.ToString());
                    var first = string.Join(",", row.First());
                    var last = string.Join(",", row.Last());
                    list.Add(new Node(first, last));
                }
            }
            return list;
        }
    }
}
