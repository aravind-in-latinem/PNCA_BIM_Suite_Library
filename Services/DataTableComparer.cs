using System;
using System.Data;
using System.Linq;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.Services
{
    public class DataTableComparer
    {
        public static DataTable GetDifferenceReport(DataTable checkTable, DataTable referenceTable,ScheduleDataFromElementsExtractor sourceData)
        {
            // Prepare the result table
            DataTable result = new DataTable("Differences");
            result.Columns.Add("ElementId", typeof(string));
            result.Columns.Add("ParameterName", typeof(string));
            result.Columns.Add("UnitType", typeof(string));
            result.Columns.Add("ValueInTable1", typeof(string));
            result.Columns.Add("ValueInTable2", typeof(string));

            // Create lookup for table2 by ElementId for fast access
            var dt2Lookup = referenceTable.AsEnumerable()
                .ToDictionary(r => r["ElementId"].ToString(), r => r);

            foreach (DataRow row1 in checkTable.Rows)
            {
                string elementId = row1["ElementId"].ToString();

                // Check if same ElementId exists in dt2
                if (!dt2Lookup.TryGetValue(elementId, out DataRow row2))
                    continue; // skip if not present

                // Go through every column except ElementId
                foreach (DataColumn col in checkTable.Columns)
                {
                    if (col.ColumnName == "ElementId") continue;

                    if (!referenceTable.Columns.Contains(col.ColumnName))
                        continue; // skip parameters not present in both

                    string val1 = row1[col.ColumnName]?.ToString() ?? "";
                    string val2 = row2[col.ColumnName]?.ToString() ?? "";

                    if (!val1.Equals(val2, StringComparison.OrdinalIgnoreCase))
                    {
                        // Add to result table
                        result.Rows.Add(elementId, col.ColumnName, GetUnitTypeForField(sourceData,col.ColumnName),val1, val2);
                    }
                }
            }
            //result.PrimaryKey = new DataColumn[] { result.Columns["ElementId"]};
            return result;
        }

        public static bool AreSchemasEqual(DataTable table1, DataTable table2)
        {
            // 1. Check for nulls
            if (table1 == null || table2 == null)
                throw new ArgumentNullException("One or both DataTables are null.");

            // 2. Check column count
            if (table1.Columns.Count != table2.Columns.Count)
                return false;

            // 3. Check each column name and type (and optionally order)
            for (int i = 0; i < table1.Columns.Count; i++)
            {
                var col1 = table1.Columns[i];
                var col2 = table2.Columns[i];

                // Compare name and type
                if (col1.ColumnName != col2.ColumnName ||
                    col1.DataType != col2.DataType)
                    return false;
            }

            // 4. If everything matches
            return true;
        }
        public static string GetUnitTypeForField(ScheduleDataFromElementsExtractor scheduleData, string fieldName)
        {
            if (scheduleData == null || string.IsNullOrWhiteSpace(fieldName))
                return string.Empty;

            // Search through all scheduled elements
            foreach (var scheduledElement in scheduleData.ScheduledElements)
            {
                if (scheduledElement == null || scheduledElement.ScheduledFields == null)
                    continue;

                // Look for the field name match (case-insensitive)
                foreach (var field in scheduledElement.ScheduledFields)
                {
                    if (field != null && string.Equals(field.FieldName, fieldName, StringComparison.OrdinalIgnoreCase))
                    {
                        return field.UnitType ?? string.Empty;
                    }
                }
            }

            // If nothing found, return empty string
            return string.Empty;
        }


    }



}
