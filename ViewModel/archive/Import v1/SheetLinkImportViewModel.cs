using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System.Windows.Input;
using Microsoft.Win32;
using PNCA_SheetLink.SheetLink.Model;


namespace PNCA_SheetLink.SheetLink.ViewModel
{
    public class SheetLinkImportViewModel: ViewModelBase
    {
        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly System.Windows.Window _yourWindowReference;

        private string _fileLocation;
        public SheetLinkImportViewModel(Document document, UIDocument uiDocument, System.Windows.Window yourWindowReference)
        {
            _document = document;
            _uiDocument = uiDocument;
            _yourWindowReference = yourWindowReference;

            //intiialize properties and commands
            ImportCommand = new RelayCommand(ExecuteImport, CanExecuteImport);
            CancelCommand = new RelayCommand(ExecuteCancel);
            BrowseFileLocationCommand = new RelayCommand(ExecuteBrowseFileLocation);
        }
        public string FileLocation
        {
            get => _fileLocation;
            set
            {
                if (SetProperty(ref _fileLocation, value))
                {
                    // Update commands when save location changes
                    (ImportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }


        public ICommand ImportCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseFileLocationCommand { get; }

        private void ExecuteImport()
        {
            var excelReader = new ExcelReader(FileLocation);
            TaskDialog.Show("Import", $"Importing data from: {FileLocation}");
        }

        private void ExecuteCancel()
        {
            // Close the window
            System.Windows.Window.GetWindow(_yourWindowReference)?.Close();
        }
        private void ExecuteBrowseFileLocation()
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                DefaultExt = "xlsx",
                Title = "Select Export Location"
            };
            bool? success = openFileDialog.ShowDialog();
            if (success == true)
            {
                FileLocation = openFileDialog.FileName;
            }
        }

        private bool CanExecuteImport()
        {
            // Import can only execute when:
            // 1. Save location is specified
            // 2. Save location path is valid
            
            return !string.IsNullOrWhiteSpace(FileLocation) &&
                   System.IO.Path.HasExtension(FileLocation);
        }



    }
}
