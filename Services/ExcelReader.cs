using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace PNCA_BIM_Suite_Library.Services
{
    public class ExcelReader
    {



        private FileInfo _existingFile;
        private string _filePath = string.Empty;
        public ExcelReader(string filePath)
        {
            this._filePath = filePath;
            _existingFile = new FileInfo(filePath);
        }
        public DataTable ReadExcelFile()
        {
            DataTable dataTable = new DataTable();

            if (!_existingFile.Exists)
                throw new FileNotFoundException("The specified Excel file does not exist.", _filePath);

            using (var wb = new XLWorkbook(_filePath))
            {
                var worksheet = wb.Worksheet(1); // Read the first worksheet

                // Determine number of columns based on the first row
                var firstRow = worksheet.FirstRowUsed();
                int columnCount = firstRow.LastCellUsed().Address.ColumnNumber;

                // Create columns
                for (int col = 1; col <= columnCount; col++)
                {
                    string header = firstRow.Cell(col).GetValue<string>().Trim();
                    if (string.IsNullOrEmpty(header))
                        header = $"Column{col}";
                    dataTable.Columns.Add(header);
                }

                // Read rows (starting from row 2)
                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    DataRow dataRow = dataTable.NewRow();
                    for (int col = 1; col <= columnCount; col++)
                    {
                        dataRow[col - 1] = row.Cell(col).GetValue<string>();
                    }
                    dataTable.Rows.Add(dataRow);
                }
            }

            return dataTable;
        }

    }
}
