using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Views
{
    /// <summary>
    /// Interaction logic for SheetCreatorUserInput.xaml
    /// </summary>
    public partial class SheetCreatorUserInput : Window
    {
        private UIDocument _uiDoc;
        private Document _doc;

        public ICommand Close_Click { get; }
        public ICommand Minimize_Click { get; }
        public SheetCreatorUserInput(ExternalCommandData commandData)
        {
            InitializeComponent();
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;

            UserTitleBlockDropDown.DisplayMemberPath = "Name";
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
            var collector = new FilteredElementCollector(_doc)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .OrderBy(tb => tb.FamilyName)
                .ThenBy(tb => tb.Name)
                .ToList();

            foreach (var tb in collector)
            {
                UserTitleBlockDropDown.Items.Add(tb);

                if(UserTitleBlockDropDown.Items.Count > 0)
                    UserTitleBlockDropDown.SelectedIndex = 0;
            }

        }
        private void UserCreateButton_Click(object sender, RoutedEventArgs e)
        {
            string numberOfSheetsText = NumberOfSheetsTextBox.Text?.Trim();
            string sheetName = SheetNameTextBox.Text?.Trim();
            string sheetNumber = SheetNumberTextBox.Text?.Trim();
            var errorCollection = new StringBuilder();
            var selectedTitleBlock = UserTitleBlockDropDown.SelectedItem as FamilySymbol;

            if(!int.TryParse(numberOfSheetsText, out int numberOfSheets) || numberOfSheets <= 0)
            {
                MessageBox.Show("Please enter a valid positive integer for the number of sheets.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(sheetName))
            { 
                if(MessageBox.Show("Sheet Name is empty. Do you want to proceed?", "Empty Sheet Name", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                {
                    return;
                }
            }
            if(string.IsNullOrWhiteSpace(sheetNumber))
            {
                MessageBox.Show("Please enter a starting Sheet Number.", "Empty Sheet Number", MessageBoxButton.OK, MessageBoxImage.Warning);       
                    return;
                
            }
            if(selectedTitleBlock == null)
            {
                MessageBox.Show("Please select a Title Block.", "No Title Block Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string prefix = "";
            string numberPart = "";
            
            //int padding = sheetNumber.Length;

            foreach(char c in sheetNumber)
            {
                if (char.IsDigit(c))
                {
                    numberPart += c;
                }
                else
                {
                    prefix += c;
                }
            }

            int startNumber = 1;
            int padding = 0;

            if(!string.IsNullOrEmpty(numberPart))
            {
                startNumber = int.Parse(numberPart);
                padding = numberPart.Length;
            }
            
            try
            {
                using (Transaction t = new Transaction(_doc, "Create Sheets"))
                {
                    t.Start();

                    if (!selectedTitleBlock.IsActive)
                    {
                        selectedTitleBlock.Activate();
                        _doc.Regenerate();
                    }

                    var createdSheets = new List<ViewSheet>();

                    for (int i =0; i < numberOfSheets; i++)
                    {
                        int nextNumber = startNumber + i;

                        string formattedSheetNumber = prefix + nextNumber.ToString(new string('0', padding));
                        

                        ViewSheet vs = ViewSheet.Create(_doc, selectedTitleBlock.Id);

                        vs.SheetNumber = formattedSheetNumber;
                        //vs.Name = $"{sheetName}_{formattedSheetNumber}";
                        vs.Name = $"{sheetName}_{nextNumber}";


                        createdSheets.Add(vs);
                    }

                    t.Commit();

                    MessageBox.Show($"{createdSheets.Count} sheets created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }

            catch (Autodesk.Revit.Exceptions.ArgumentException ex)
            {
                // Commonly thrown if a duplicate Sheet Number sneaks in
                errorCollection.AppendLine("Revit rejected a value (often due to duplicate Sheet Numbers).\n\n" +
                                           "Details:\n" + ex.Message);
            }
            catch (Exception ex)
            {
                errorCollection.AppendLine(ex.Message);
            }
            if(errorCollection.Length > 0)
            {
                MessageBox.Show("Some sheets may not have been created due to the following errors:\n\n" + errorCollection.ToString(), "Errors Occurred", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void UserCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
