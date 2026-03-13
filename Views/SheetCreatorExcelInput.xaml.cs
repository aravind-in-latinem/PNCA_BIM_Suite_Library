#region Imported Libraries
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using System.Data;
using System.Data.OleDb;
using System.Text;

#endregion


namespace PNCA_BIM_Suite_Library.Views
{
    public partial class SheetCreatorExcelInput : Window
    {
        private UIDocument _uiDoc;
        private Document _doc;
        private string excelFilePath;
        private List<string> headers = new List<string>();

        // Using OleDB Connection Excel method

        private DataTable _excelData;
        public ICommand Close_Click { get; }
        public ICommand Minimize_Click { get; }


        public SheetCreatorExcelInput(ExternalCommandData commandData)
        {
            InitializeComponent();
            _uiDoc = commandData.Application.ActiveUIDocument;
             _doc = _uiDoc.Document;

            ExcelTitleBlockDropDown.DisplayMemberPath = "Name";

            Close_Click = new RelayCommand(_ => this.Close());
            Minimize_Click = new RelayCommand(_ => this.WindowState = WindowState.Minimized);
            LoadTitleBlocks();
            DataContext = this;
        }


        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }
        private void LoadTitleBlocks()
        {
            var titleBlocks = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .OrderBy(tb => tb.FamilyName)
                .ThenBy(tb => tb.Name)
                .ToList();


            foreach (var tb in titleBlocks)
            {
                ExcelTitleBlockDropDown.Items.Add(tb);
               
            }
            if (ExcelTitleBlockDropDown.Items.Count > 0)
                ExcelTitleBlockDropDown.SelectedIndex = 0;
        }

        private void BrowseExcel_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Excel Files|*.xls;*.xlsx;*.xlsm",
                Title = "Select an Excel File"
            };

            if (ofd.ShowDialog() == true)
            {

                


                string excelFilePath = ofd.FileName;

                try
                {
                    
                    _excelData = ReadExcel(excelFilePath);

                    SheetNumberColumnDropDown.Items.Clear();
                    SheetNameColumnDropDown.Items.Clear();

                    foreach (DataColumn col in _excelData.Columns)
                    {
                        SheetNumberColumnDropDown.Items.Add(col.ColumnName);
                        SheetNameColumnDropDown.Items.Add(col.ColumnName);
                    }

                    MessageBox.Show("Excel Loaded Successfully!");
                }

                catch(Exception ex)
                {
                    MessageBox.Show("Error reading Excel File:\n" + ex.Message);
                }
            }

            else
            {
                MessageBox.Show("Selected file path is invalid or file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

           

        }

        private static DataTable ReadExcel(string filepath)
        {
            string connStr = $@"Provider=Microsoft.ACE.OLEDB.12.0;
                                Data Source={filepath};
                                Extended Properties='Excel 12.0 Xml;HDR=YES;'";

            using (OleDbConnection conn = new OleDbConnection(connStr))
            {
                conn.Open();

                DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string sheetName = schemaTable.Rows[0]["TABLE_NAME"].ToString();

                OleDbDataAdapter adapter = new OleDbDataAdapter($"SELECT * FROM [{sheetName}]", conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                return dt;
            }
        }      

        private void ExcelCreateButton_Click(object sender, RoutedEventArgs e)
        {
            var result = true;
            if(_excelData == null)
            {
                MessageBox.Show("Please load an Excel file first");
                return;
            }

            if (SheetNumberColumnDropDown.SelectedItem == null || SheetNameColumnDropDown.SelectedItem == null || ExcelTitleBlockDropDown == null)
            {
                MessageBox.Show("Please select sheet number, name columns, and a title block.");
                return;
            }

            string sheetNumberColumn = SheetNumberColumnDropDown.SelectedItem.ToString();
            string sheetNameColumn = SheetNameColumnDropDown.SelectedItem.ToString();
            FamilySymbol titleblock = ExcelTitleBlockDropDown.SelectedItem as FamilySymbol;

            using (Transaction trans = new Transaction(_doc, "Create Sheet from an Excel"))
            {
                trans.Start();

                if (!titleblock.IsActive)
                titleblock.Activate();
                var errorCollection = new StringBuilder();
                foreach (DataRow row in _excelData.Rows)
                {
                    string sheetNumber = row[sheetNumberColumn].ToString();
                    string sheetName = row[sheetNameColumn].ToString();

                    if (string.IsNullOrEmpty(sheetNumber) || string.IsNullOrEmpty(sheetName))
                        continue;

                    try
                    {
                        ViewSheet newSheet = ViewSheet.Create(_doc, titleblock.Id);
                        newSheet.SheetNumber = sheetNumber;
                        newSheet.Name = sheetName;
                    }
                    catch(Exception ex)
                    {
                        errorCollection.AppendLine( $"Could not create sheet {sheetNumber}: {ex.Message}");
                    }
                }

                if (errorCollection.Length > 0)
                {
                    TaskDialog.Show("Error",errorCollection.ToString());
                    result = false;
                }

                trans.Commit();
            }

            if (result == true)
            {
                MessageBox.Show("Sheets Created Successfully");
                this.Close();
            }
                
        }
        private void ExcelCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        
        
    }

   

}
