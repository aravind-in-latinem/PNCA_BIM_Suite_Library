using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Services
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ScheduleWithFormattingExtractor

    {
        private ILogger _progressLogger;
        public DataTable GetDataTableWithRevitFormatting(Document document, ViewSchedule schedule, ILogger progressLogger)
        {
            
            var dataTable = new System.Data.DataTable();
            _progressLogger = progressLogger;

            if (schedule == null)
            {
                TaskDialog.Show("Info", "No schedules found.");
                return null;
            }

            // Access table data
            TableData tableData = schedule.GetTableData();
            _progressLogger.LogTaskCompleted("Retrieved Table Data");
            TableSectionData bodyData = tableData.GetSectionData(SectionType.Body);
            _progressLogger.LogTaskCompleted("Retrieved Section Data");
            var coln = bodyData.NumberOfColumns;
            var rown = bodyData.NumberOfRows;

            List<string[]> rowCollection = new List<string[]>();
            for (int row = 0; row < rown; row++)
            {
                string[] cellTexts = new string[coln];
                for (int col = 0; col < coln; col++)
                {

                    var scheduleTexts = schedule.GetCellText(SectionType.Body, row, col).ToString();
                    if (!string.IsNullOrEmpty(scheduleTexts) && !string.IsNullOrWhiteSpace(scheduleTexts))
                        cellTexts[col] = scheduleTexts;
                }
                //if (!cellTexts.All(string.IsNullOrEmpty))
                rowCollection.Add(cellTexts);
            }
            dataTable = GenerateDataTable(rowCollection);
            _progressLogger.LogTaskCompleted("Generated Data Table");
            return dataTable;
        }

        public DataTable GenerateDataTable(List<string[]> rowCollection)
        {
            DataTable dataTable = new DataTable();
            if (rowCollection == null || rowCollection.Count == 0)
                return dataTable;
            // Add columns based on the first row
var firstRow = rowCollection[0];
for (int col = 0; col < firstRow.Length; col++)
{
    string header = string.IsNullOrWhiteSpace(firstRow[col]) 
        ? $"Column{col + 1}" 
        : firstRow[col].Trim();

    // Handle duplicate headers
    string uniqueHeader = header;
    int suffix = 1;
    while (dataTable.Columns.Contains(uniqueHeader))
    {
        uniqueHeader = $"{header}_{suffix}";
        suffix++;
    }

    dataTable.Columns.Add(uniqueHeader);
}
            // Add rows
            foreach (var rowArray in rowCollection.Skip(1))
            {
                DataRow dataRow = dataTable.NewRow();
                for (int col = 0; col < rowArray.Length; col++)
                {
                    dataRow[col] = rowArray[col] ?? string.Empty;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

    }
}
