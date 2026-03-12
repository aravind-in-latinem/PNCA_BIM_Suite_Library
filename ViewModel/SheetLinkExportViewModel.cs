using System.Collections.ObjectModel;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Linq;
using Microsoft.Win32;
//using Autodesk.Revit.Creation;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.Views;

namespace PNCA_BIM_Suite_Library.ViewModel
{
    public class SheetLinkExportViewModel : ViewModelBase
    {
        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly System.Windows.Window _yourWindowReference;
        private ILogger _progressLogger;
        // Properties for data binding
        private bool _isActiveViewSelected;
        private bool _isSelectScheduleSelected;
        private ScheduleViewItem _selectedSchedule;
        private string _saveLocation;
        private ObservableCollection<ScheduleViewItem> _availableSchedules;
        private string _scheduleSearchText;
        private ObservableCollection<ScheduleViewItem> _filteredSchedules;
        private bool _shouldOpenDropDown;
        public SheetLinkExportViewModel(Document document, UIDocument uiDocument, System.Windows.Window yourWindowReference, ILogger progressLogger)
        {
            _document = document;
            _uiDocument = uiDocument;
            _progressLogger = progressLogger;
            // Initialize commands
            ExportCommand = new RelayCommand(ExecuteExport, CanExecuteExport);
            CancelCommand = new RelayCommand(ExecuteCancel);
            BrowseSaveLocationCommand = new RelayCommand(ExecuteBrowseSaveLocation);

            // Initialize properties
            IsActiveViewSelected = true;
            LoadAvailableSchedules();
            FilteredSchedules = new ObservableCollection<ScheduleViewItem>(AvailableSchedules);
            _yourWindowReference = yourWindowReference;
        }

        #region Properties for Binding

        // Radio Button Bindings
        public bool IsActiveViewSelected
        {
            get => _isActiveViewSelected;
            set
            {
                if (SetProperty(ref _isActiveViewSelected, value))
                {
                    // When active view is selected, auto-select the current view if it's a schedule
                    if (value && _uiDocument?.ActiveView is ViewSchedule activeSchedule)
                    {
                        SelectedSchedule = AvailableSchedules?
                            .FirstOrDefault(s => s.ViewId == activeSchedule.Id);
                    }
                }
            }
        }

        public bool IsSelectScheduleSelected
        {
            get => _isSelectScheduleSelected;
            set 
            { 
                SetProperty(ref _isSelectScheduleSelected, value);
                ShouldOpenDropDown = true;
            }
        }

        // ComboBox Binding
        public string ScheduleSearchText
        {
            get => _scheduleSearchText;
            set
            {
                if (SetProperty(ref _scheduleSearchText, value))
                {

                    // Auto-open dropdown when text changes and there are filtered items
                    if (!string.IsNullOrEmpty(value))
                    {
                        ShouldOpenDropDown = true;
                        FilterSchedules();
                    }
                }
            }
        }
        public bool ShouldOpenDropDown
        {
            get => _shouldOpenDropDown;
            set => SetProperty(ref _shouldOpenDropDown, value);
        }

        public ObservableCollection<ScheduleViewItem> FilteredSchedules
        {
            get => _filteredSchedules;
            set => SetProperty(ref _filteredSchedules, value);
        }

        public ObservableCollection<ScheduleViewItem> AvailableSchedules
        {
            get => _availableSchedules;
            set => SetProperty(ref _availableSchedules, value);
        }

        public ScheduleViewItem SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                if (SetProperty(ref _selectedSchedule, value))
                {
                    // Update commands when selection changes
                    (ExportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        // TextBox Binding
        public string SaveLocation
        {
            get => _saveLocation;
            set
            {
                if (SetProperty(ref _saveLocation, value))
                {
                    // Update commands when save location changes
                    (ExportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands for Button Binding

        public ICommand ExportCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand BrowseSaveLocationCommand { get; }

        #endregion

        #region Private Methods

        private void LoadAvailableSchedules()
        {
            // Get all schedule views from the document
            var scheduleCollector = new FilteredElementCollector(_document)
                .OfClass(typeof(ViewSchedule))
                .WhereElementIsNotElementType()
                .Cast<ViewSchedule>()
                .Where(vs => !vs.IsTemplate && vs.Name != null);

            AvailableSchedules = new ObservableCollection<ScheduleViewItem>(
                scheduleCollector.Select(schedule => new ScheduleViewItem
                {
                    ViewId = schedule.Id,
                    Name = schedule.Name,
                    Schedule = schedule
                }).OrderBy(x => x.Name)
            );
        }
        private void FilterSchedules()
        {
            if (string.IsNullOrWhiteSpace(ScheduleSearchText))
            {
                FilteredSchedules = new ObservableCollection<ScheduleViewItem>(AvailableSchedules);
            }
            else
            {
                var lowerText = ScheduleSearchText.ToLower();
                var filtered = AvailableSchedules
                    .Where(s => s.Name.ToLower().Contains(lowerText))
                    .OrderBy(s => s.Name)
                    .ToList();

                FilteredSchedules = new ObservableCollection<ScheduleViewItem>(filtered);
            }
        }

        private bool CanExecuteExport()
        {
            // Export can only execute when:
            // 1. A schedule is selected OR active view is selected and current view is a schedule
            // 2. Save location is specified
            // 3. Save location path is valid

            bool hasValidSchedule = false;
            if (IsActiveViewSelected && !(_uiDocument?.ActiveView is ViewSchedule))
            {
                TaskDialog.Show("Warning", "Open the intended schedule view for easier export");
            }

            if (IsActiveViewSelected && _uiDocument?.ActiveView is ViewSchedule)
            {
                hasValidSchedule = true;
            }
            else if (IsSelectScheduleSelected && SelectedSchedule != null)
            {
                hasValidSchedule = true;
            }

            return hasValidSchedule &&
                   !string.IsNullOrWhiteSpace(SaveLocation) &&
                   System.IO.Path.HasExtension(SaveLocation);
        }

        private void ExecuteExport()
        {
            try
            {
                ViewSchedule targetSchedule = null;

                // Determine which schedule to export
                if (IsActiveViewSelected && _uiDocument.ActiveView is ViewSchedule activeSchedule)
                {
                    targetSchedule = activeSchedule;
                }
                else if (IsSelectScheduleSelected && SelectedSchedule != null)
                {
                    targetSchedule = SelectedSchedule.Schedule;
                }

                if (targetSchedule == null)
                {
                    TaskDialog.Show("Error", "No valid schedule selected for export.");
                    return;
                }

                // Export logic
                ExportScheduleToExcel(targetSchedule, SaveLocation);
                System.Diagnostics.Process.Start(SaveLocation);
                _progressLogger = new ProgressLoggerViewModel();



            }
            catch (System.Exception ex)
            {
                TaskDialog.Show("Export Error", $"Failed to export schedule: {ex.Message}");
            }

        }

        private void ExecuteCancel()
        {
            // Close the window
            System.Windows.Window.GetWindow(_yourWindowReference)?.Close();
        }

        private void ExecuteBrowseSaveLocation()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                DefaultExt = "xlsx",
                FileName = SelectedSchedule?.Name ?? "Schedule",
                Title = "Select Export Location"
            };
            bool? success = saveFileDialog.ShowDialog();
            if (success == true )
            {
                SaveLocation = saveFileDialog.FileName;
            }
        }

        private void ExportScheduleToExcel(ViewSchedule schedule, string filePath)
        {
            var progressLoggerView = new ProgressLoggerView(_progressLogger);
            progressLoggerView.Show();
            ScheduleDataFromElementsExtractor scheduleDataFromElementsExtractor = new ScheduleDataFromElementsExtractor(schedule, _document, _progressLogger);
            var dataTableData = scheduleDataFromElementsExtractor.CreateScheduleDataTable();
            _progressLogger.LogTaskCompleted("Schedule data extraction complete. Proceed for writing..");
            ExcelWriter writer = new ExcelWriter(filePath);
            writer.CreateExcelFile(dataTableData);
            _progressLogger.LogTaskCompleted("Excel file created successfully");
        }

        #endregion
    }
    
}