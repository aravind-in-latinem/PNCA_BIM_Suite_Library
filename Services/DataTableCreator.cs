using System.Collections.Generic;
using System.Data;
using System.Linq;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.Services
{
    public class DataTableCreator

    {
        public DataTable PrepareTableData(List<ScheduledElement> scheduledElements)
        {
            DataTable dataTable = new DataTable();

            // Build columns dynamically
            dataTable.Columns.Add("ElementId");
            var orderedFields = scheduledElements
            .SelectMany(e => e.ScheduledFields)
            .GroupBy(f => f.FieldName)                // Avoid duplicate column names
            .Select(g => g.First())                   // Take the first occurrence
            .OrderBy(f => f.FieldIndex)               // Sort by FieldIndex
            .ToList();

            foreach (var field in orderedFields)
                dataTable.Columns.Add(field.FieldName);

            // Fill rows
            foreach (var element in scheduledElements)
            {
                var row = dataTable.NewRow();
                row["ElementId"] = element.RowElementId.ToString();

                foreach (var field in element.ScheduledFields)
                    row[field.FieldName] = field.FieldValue ?? string.Empty;

                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

    }
}
