using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using Autodesk.Revit.DB;

using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.ViewModel.PNCAExporter;
using PNCA_BIM_Suite_Library.Views.PNCAExporter;
using PNCA_BIM_Suite_Library.Views;
//using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using RevitUI = Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.ViewModel
{

    internal class MainViewModel : ObservableObject
    {
        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly int _revitYear;
        private string _saveLocation;
        private string _exportFileName;
        private PDFExportOptions _pdfExportOptionsSheet;
        private PDFExportOptions _pdfExportOptionsView;
        [ObservableProperty]
        private ObservableObject _currentView;
        private JsonSettingsStorage _jsonSettingsStorage;
        private bool _isPDFExportChecked;
        private bool _isDWGExportChecked;
        private bool _isDWFExportChecked;
        public ObservableObject CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        private List<IViewSheetItem> _combinedViews;
        private bool _isSheetsTabActive;
        private bool _isViewsTabActive;
        private bool _isInitializing = true;




        private readonly SheetSelectionViewModel _sheetSelectionVM = new SheetSelectionViewModel();
        private readonly ViewSelectionViewModel _viewSelectionVM = new ViewSelectionViewModel();

        private readonly PDFSettingsViewModel _pDFSettingsVM = new PDFSettingsViewModel();
        private readonly DWGSettingsViewModel _dWGSettingsVM = new DWGSettingsViewModel();
        private readonly DWFSettingsViewModel _dWFSettingsVM = new DWFSettingsViewModel();
        private ViewSheetSetVM _viewSheetSetVM;
        private List<ElementId> _combinedSortedViews = new List<ElementId>();
        private IList<ElementId> _selectedSheetsId = new List<ElementId>();
        private IList<ElementId> _selectedViewsId = new List<ElementId>();
        private IList<ElementId> _combinedUnsortedViews = new List<ElementId>();
        private SettingsViewModel _settingsVM;



        public MainViewModel(Document document, UIDocument uiDocument)
        {
            _document = document;
            _uiDocument = uiDocument;
            _exportFileName = "Default FileName";
            bool success = int.TryParse(document.Application.VersionNumber, out int intValue);
            _revitYear = intValue;
            _isPDFExportChecked = true;
            _isDWGExportChecked = false;

            _pDFSettingsVM.Document = document;
            _pDFSettingsVM.LoadSettings();

            _dWGSettingsVM.Document = document;
            _dWGSettingsVM.LoadDWGExportSettings();

            _dWFSettingsVM.Document = document;
            _dWFSettingsVM.LoadDWFExportSettings();

            _settingsVM = new SettingsViewModel(_pDFSettingsVM, _dWGSettingsVM, _dWFSettingsVM);

            _sheetSelectionVM.Document = document;
            _sheetSelectionVM.LoadSheets();
            _sheetSelectionVM.ApplyFilter();


            _viewSelectionVM.Document = document;
            _viewSelectionVM.LoadViews();
            _viewSelectionVM.ApplyFilter();

            _viewSheetSetVM = new ViewSheetSetVM(document);
            SelectedViewSheetSet = _viewSheetSetVM.ViewSheetSetCollection.Where(a => a.Name == "<In Session>").First();
            _combinedViews = new List<IViewSheetItem>();
            CustomPrintOrderViewModel = new CustomPrintOrderViewModel(_combinedViews);

            InitializeProfileSettings();

            MinimizeCommand = new RelayCommand<Window>(w =>
            {
                if (w != null) w.WindowState = WindowState.Minimized;
            });

            CloseCommand = new RelayCommand<Window>(w =>
            {
                if (w != null) w.Close();
            });
            CurrentView = _sheetSelectionVM; // default
            NavigateCommand = new RelayCommand<string>(Navigate);
            ExportCommand = new RelayCommand(ExecuteExport, CanExecuteExport);
            BrowseSaveLocationCommand = new RelayCommand(ExecuteBrowseSaveLocation);
            SaveVSSet = new RelayCommand(ExecuteSaveVSSet, CanExecuteSaveVSSet);
            SaveAsVSSet = new RelayCommand(ExecuteSaveAsVSSet, CanExecuteSaveVSSet);
            RenameVSSet = new RelayCommand(ExecuteRenameVSSet, CanExecuteRenameVSSet);
            DeleteVSSet = new RelayCommand(ExecuteDeleteVSSet, CanExecuteDeleteVSSet);
            CustomPrintOrderCommand = new RelayCommand(ExecuteCustomPrintOrder, CanExecuteCustomPrintOrder);
            SaveAsProfileCommand = new RelayCommand(ExecuteSaveAsProfile);
            SaveProfileCommand = new RelayCommand(ExecuteSaveProfile);
            CustomFileNameCommand = new RelayCommand(ExecuteCustomFileName, CanExecuteCustomFileName);
            OpenProfilesFolderCommand = new RelayCommand(ExecuteOpenProfilesFolder);
            IsSheetsTabActive = true;
            IsViewsTabActive = false;
            _isInitializing = false;
        }

        public RelayCommand<Window> MinimizeCommand { get; }
        public RelayCommand<Window> CloseCommand { get; }
        public RelayCommand<string> NavigateCommand { get; }




        

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    _sheetSelectionVM.SearchText = value;
                    _viewSelectionVM.SearchText = value;
                }
            }
        }
        private bool _isFilterChecked;
        public bool IsFilterChecked
        {
            get => _isFilterChecked;
            set
            {
                if (SetProperty(ref _isFilterChecked, value))
                {
                    _sheetSelectionVM.SetFilterState(value);
                    _viewSelectionVM.SetFilterState(value);
                }
            }
        }

        public bool IsPDFExportChecked
        {
            get => _isPDFExportChecked;
            set => SetProperty(ref _isPDFExportChecked, value);
        }
        public bool IsDWGExportChecked
        {
            get => _isDWGExportChecked;
            set => SetProperty(ref _isDWGExportChecked, value);
        }
        public bool IsDWFExportChecked
        {
            get => _isDWFExportChecked;
            set => SetProperty(ref _isDWFExportChecked, value);
        }

        public bool IsCombinedChecked
        {
            get => _pDFSettingsVM.IsCombinedChecked;
            set
            {
                _pDFSettingsVM.IsCombinedChecked = value;
                OnPropertyChanged();
            }
        }




        public bool IsSheetsTabActive
        {
            get => _isSheetsTabActive;
            set
            {
                if (SetProperty(ref _isSheetsTabActive, value) && value && !_isInitializing)
                {
                    IsViewsTabActive = false;
                }
            }
        }

        public bool IsViewsTabActive
        {
            get => _isViewsTabActive;
            set
            {
                if (SetProperty(ref _isViewsTabActive, value) && value && !_isInitializing)
                {
                    IsSheetsTabActive = false;
                }
            }
        }


        public PDFSettingsViewModel PDFSettingsVM => _pDFSettingsVM;
        public SheetSelectionViewModel SheetSelectionVM => _sheetSelectionVM;
        public ViewSelectionViewModel ViewSelectionVM => _viewSelectionVM;
        public SettingsViewModel SettingsVM => _settingsVM;


        private void Navigate(string viewName)
        {
            switch (viewName)
            {
                case "SheetSelectionView":
                    CurrentView = _sheetSelectionVM;
                    break;

                case "ViewSelectionView":
                    CurrentView = _viewSelectionVM;
                    break;

                case "SettingsView":
                    CurrentView = _settingsVM;
                    break;

                default:
                    CurrentView = _sheetSelectionVM;
                    break;
            }
        }

        #region Export Methods
        public RelayCommand ExportCommand { get; }
        public RelayCommand BrowseSaveLocationCommand { get; }
        public RelayCommand CustomFileNameCommand { get; }
        public string SaveLocation
        {
            get => _saveLocation;
            set => SetProperty(ref _saveLocation, value);
        }
        private bool _isHeavyModeChecked;
        public bool IsHeavyModeChecked
        {
            get => _isHeavyModeChecked;
            set => SetProperty(ref _isHeavyModeChecked, value);
        }

        private void ExecuteExport()
        {
            _selectedSheetsId = _sheetSelectionVM.SelectedSheets.Select(a => a.ItemElementId).ToList();
            _selectedViewsId = _viewSelectionVM.SelectedViewItems.Select(v => v.ViewElement.Id).ToList();
            _combinedUnsortedViews = _selectedSheetsId.Concat(_selectedViewsId).ToList();
            var resultPDF = Result.Cancelled;
            var resultCAD = Result.Cancelled;
            var stringBuilder = new StringBuilder();
            if (IsPDFExportChecked)
            {
                if(IsHeavyModeChecked)
                {
                    resultPDF = ExecuteExportPDFSafeMode();
                    if(PDFSettingsVM.SelectedPrinterName=="diroots.prosheets")
                    PostProcessFiles();

                }
                else
                    resultPDF = ExecuteExportPDFFastMode();
            }

            if (IsDWGExportChecked)
                resultCAD = ExecuteExportDWG();
            if (IsDWFExportChecked)
            {
                var resultDWF = ExecuteExportDWF();
                if (resultDWF == Result.Succeeded)
                {
                    stringBuilder.AppendLine("DWF Export Succeeded.");
                }
                if (resultDWF == Result.Failed)
                {
                    stringBuilder.AppendLine("DWF Export Failed.");
                }
            }

            if (resultPDF == Result.Succeeded)
            {
                stringBuilder.AppendLine("PDF Export Succeeded.");
            }
            if (resultCAD == Result.Succeeded)
            {
                stringBuilder.AppendLine("CAD Export Succeeded.");
            }
            if (resultPDF == Result.Failed)
            {
                stringBuilder.AppendLine("PDF Export Failed.");
            }
            if (resultCAD == Result.Failed)
            {
                stringBuilder.AppendLine("CAD Export Failed.");
            }
            RevitUI.TaskDialog.Show("Export Status:", stringBuilder.ToString());

        }
        private Result ExecuteExportDWG()
        {


            if (SettingsVM.DWGSettingsViewModel.SelectedDWGExportSetup.IsInSession)
            {
                var options = new DWGExportOptions() { MergedViews = !SettingsVM.DWGSettingsViewModel.IsExternalReferencesChecked };

                InternalExportDWG(options);
                return Result.Succeeded;
            }
            else
            {
                var dWGExportOption = SettingsVM.DWGSettingsViewModel.SelectedDWGExportSetup.ExportDWGSettings.GetDWGExportOptions();
                dWGExportOption.MergedViews = !SettingsVM.DWGSettingsViewModel.IsExternalReferencesChecked;
                InternalExportDWG(dWGExportOption);
                return Result.Succeeded;
            }
        }

        private void InternalExportDWG(DWGExportOptions dWGExportOption)
        {
            foreach (var sheet in _sheetSelectionVM.SelectedSheets)
            {
                var stat = _document.Export(SaveLocation, sheet.FileName, new List<ElementId>() { sheet.ItemElementId }, dWGExportOption);
                Thread.Sleep(2000);
            }
            foreach (var view in _viewSelectionVM.SelectedViewItems)
            {
                var stat = _document.Export(SaveLocation, view.FileName, new List<ElementId>() { view.ItemElementId }, dWGExportOption);
                Thread.Sleep(2000);
            }
        }

        private Result ExecuteExportDWF()
        {
            using (var t = new Transaction(_document))
            {


                _dWFSettingsVM.ApplyPrintSettings(_dWFSettingsVM.CurrentPrintSettingItem);

                t.Start("Export DWF");
                DWFExportOptions options = new DWFExportOptions();
                options.MergedViews = _dWFSettingsVM.ViewsAsExternalReferences;
                options.ExportObjectData = _dWFSettingsVM.ExportObjectData;
                options.ExportingAreas = _dWFSettingsVM.ExportingAreas;
                options.ExportTexture = _dWFSettingsVM.ExportTexture;
                options.ImageFormat = _dWFSettingsVM.SelectedDWFImageFormat.Value;
                options.ImageQuality = _dWFSettingsVM.SelectedDWFImageQuality.Value;


                foreach (var sheet in _sheetSelectionVM.SelectedSheets)
                {
                    var view = new ViewSet();
                    view.Insert(sheet.SheetElement);
                    var stat = _document.Export(SaveLocation, sheet.FileName, view, options);
                    Thread.Sleep(2000);

                }
                foreach (var viewItem in _viewSelectionVM.SelectedViewItems)
                {
                    var view = new ViewSet();
                    view.Insert(viewItem.ViewElement);
                    var stat = _document.Export(SaveLocation, viewItem.FileName, view, options);
                    Thread.Sleep(2000);
                }

                t.Commit();
                return Result.Succeeded;

            }
        }

        private Result ExecuteExportPDFSafeMode()
        {
            
            var view = new ViewSet();
                ViewSheetSet newViewSheetSet = null;
                var tempSetName = "PNCA Exporter Temporary ViewSet";
                PDFSettingsVM.ApplyPrintSettings(PdfSettingsStorage.ToRevitOptions(_pDFSettingsVM.CurrentPDFExportSettingItem));
                var printManager = ViewSheetSetVM.PrintManager;
                using (Transaction t = new Transaction(_document, "Apply Print Settings"))
                {
                    t.Start();
                    //printManager.PrintRange = PrintRange.Select;
                    printManager.SelectNewPrintDriver(PDFSettingsVM.SelectedPrinterName);
                    printManager.PrintRange = PrintRange.Select;
                    printManager.CombinedFile = IsCombinedChecked;
                    printManager.Apply();
                    t.Commit();
                }

                if (IsCombinedChecked)
                {
                    foreach (var sheet in _sheetSelectionVM.SelectedSheets)
                    {
                        view.Insert(sheet.SheetElement);
                    }
                    ViewSheetSetVM.SaveAsVSSetRevitDB(tempSetName, view);

                    printManager.SubmitPrint();
                    Thread.Sleep(2000);
                    ViewSheetSetVM.DeleteVSSet();
                    return Result.Succeeded;
                }
                else
                {
                    foreach (var sheet in _sheetSelectionVM.SelectedSheets)
                    {
                        printManager.SubmitPrint(sheet.SheetElement);
                        Thread.Sleep(2000); 
                    }
                    foreach (var viewItem in _viewSelectionVM.SelectedViewItems)
                    {
                        printManager.SubmitPrint(viewItem.ViewElement);
                        Thread.Sleep(2000);
                    }

                    return Result.Succeeded;
                }



        }

        private void PostProcessFiles()
        {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string diroots = @"\DiRoots\ProSheets\Temp\PDF";
                var tempPath = string.Concat(localAppData, diroots);
                string[] pdfFiles = Directory.GetFiles(tempPath, "*.pdf");
            if (!IsCombinedChecked)
            {
                foreach (var sheet in _sheetSelectionVM.SelectedSheets)
                {
                    //var oldNameNewNamePair = new Dictionary<string, string>();
                    var oldName = Path.Combine(tempPath, pdfFiles.Where(a => a.Contains(sheet.SheetNumber)).FirstOrDefault());
                    if (!File.Exists(oldName))
                        Thread.Sleep(5000);
                    var newName = Path.Combine(SaveLocation, $"{sheet.FileName}.pdf");
                    FileInfo fileInfo = new FileInfo(oldName);
                    if (File.Exists(newName))
                    {
                        File.Delete(newName);
                    }
                    fileInfo.MoveTo(newName);
                }
            }
            else
            {
                foreach(var filePath in pdfFiles )
                {
                    var newName = Path.Combine(SaveLocation, Path.GetFileName(filePath));
                    FileInfo fileInfo = new FileInfo(filePath);
                    if (File.Exists(newName))
                    {
                        File.Delete(newName);
                    }
                    fileInfo.MoveTo(newName);

                }

            }
        }

        private Result ExecuteExportPDFFastMode()
        {
            var continueExport = AskContinueWithoutSaving();
            if (!continueExport)
                return Result.Cancelled;

            IList<ElementId> selSheets = _sheetSelectionVM.SelectedSheets.Select(a => a.ItemElementId).ToList();
            IList<ElementId> selViews = _viewSelectionVM.SelectedViewItems.Select(v => v.ViewElement.Id).ToList();

            using (var t = new Transaction(_document))
            {
                t.Start("Export PDFs");

                _pdfExportOptionsSheet = PdfSettingsStorage.ToRevitOptions(_pDFSettingsVM.CurrentPDFExportSettingItem);
                _pdfExportOptionsSheet.Combine = IsCombinedChecked;

                _pdfExportOptionsSheet.SetNamingRule(_sheetSelectionVM.SheetFileNameRule);

                _pdfExportOptionsView = PdfSettingsStorage.ToRevitOptions(_pDFSettingsVM.CurrentPDFExportSettingItem);
                _pdfExportOptionsView.Combine = IsCombinedChecked;

                _pdfExportOptionsView.SetNamingRule(_viewSelectionVM.ViewFileNameRule);

                _pdfExportOptionsSheet.FileName = _exportFileName;

                _pdfExportOptionsView.FileName = _exportFileName;
                _document.Regenerate();

                t.Commit();
            }
            #region commented
            var path = SaveLocation;
            if (!IsCombinedChecked)
            {
                if (_selectedSheetsId.Count > 0)
                {
                    try
                    {
                        RegenerateGraphicsCombinedViews(_selectedSheetsId);
                        Thread.Sleep(5000);
                        var exportStatSheets = _document.Export(path, _selectedSheetsId, _pdfExportOptionsSheet);
                        Thread.Sleep(5000);

                    }
                    catch (Exception)
                    {
                        var newRule = _sheetSelectionVM.SheetFileNameRule;
                        var viewId = TableCellCombinedParameterData.Create();
                        viewId.ParamId = new ElementId(BuiltInParameter.ID_PARAM);
                        newRule.Add(viewId);

                        _pdfExportOptionsSheet.SetNamingRule(newRule);
                        //_document.Regenerate();

                        try
                        {

                            var exportStatViews = _document.Export(path, _selectedSheetsId, _pdfExportOptionsSheet);
                            Thread.Sleep(1000);
                        }
                        catch (Exception ex)
                        {
                            RevitUI.TaskDialog.Show("Error", ex.Message);
                            return Result.Failed;
                        }

                    }
                }
                if (_selectedViewsId.Count > 0)
                {
                    try
                    {
                        RegenerateGraphicsCombinedViews(_selectedViewsId);
                        Thread.Sleep(5000);
                        var exportStatViews = _document.Export(path, _selectedViewsId, _pdfExportOptionsView);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        RevitUI.TaskDialog.Show("Export Error", $"{ex.Message}");
                        var newRule = PdfSettingsStorage.GetDefaultViewNamingRule();

                        _pdfExportOptionsView.SetNamingRule(newRule);
                        //_document.Regenerate();

                        try
                        {
                            var exportStatViews = _document.Export(path, _selectedViewsId, _pdfExportOptionsView);
                            Thread.Sleep(1000);
                        }
                        catch (Exception exc)
                        {
                            RevitUI.TaskDialog.Show("Error", exc.Message);
                            return Result.Failed;
                        }
                    }
                }
            }
            if (IsCombinedChecked)
            {
                try
                {
                    if (_combinedSortedViews.Count > 0)
                    {
                        var exportStatCombined = _document.Export(path, _combinedSortedViews, _pdfExportOptionsSheet);
                    }
                    else
                    {
                        var exportStatCombined = _document.Export(path, _combinedUnsortedViews, _pdfExportOptionsSheet);
                        Thread.Sleep(1000);

                    }
                }
                catch (Exception ex)
                {
                    RevitUI.TaskDialog.Show("Export Error", $"{ex.Message}");
                }
            }
            return Result.Succeeded;

        }


        private void RegenerateGraphicsCombinedViews(IList<ElementId> viewCollector)
        {
            var containingViews = new List<Element>();
            
            foreach (var viewId in viewCollector)
            {
                var collector = new FilteredElementCollector(_document, viewId)
                .OfClass(typeof(ImportInstance)).Cast<ImportInstance>().ToList();                
            }
            
        }
        #endregion
        private bool CanExecuteExport()
        {
            if (_sheetSelectionVM.SelectedSheets.Count > 0 || _viewSelectionVM.SelectedViewItems.Count > 0)
            {
                if (!(string.IsNullOrEmpty(SaveLocation)))
                    return true;
                else
                    return false;
            }
            else return false;
        }


        private bool AskContinueWithoutSaving()
        {
            RevitUI.TaskDialog td = new RevitUI.TaskDialog("Confirm Export");
            td.MainInstruction = "Caution, Do you want to continue exporting without saving the file?";
            td.MainContent = "There are chances that Revit may crash if exporting large number of sheets or sheets with high vector details. Please proceed after saving your progress.";
            td.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
            td.DefaultButton = RevitUI.TaskDialogResult.Yes;

            RevitUI.TaskDialogResult result = td.Show();
            return result == RevitUI.TaskDialogResult.Yes;
        }
        #region Print Manager Helper for future
        //    public void OpenViews(    
        //IList<ElementId> viewIds)
        //    {
        //        if (viewIds == null)
        //            return;

        //        int openedCount = 0;
        //        foreach (ElementId viewId in viewIds)
        //        {
        //            try
        //            {
        //                RevitDB.View view = _document.GetElement(viewId) as RevitDB.View;

        //                if (view == null || view.IsTemplate)
        //                    continue;

        //                // Check if view is already open
        //                bool isAlreadyOpen = false;
        //                foreach (UIView openView in _uiDocument.GetOpenUIViews())
        //                {
        //                    if (openView.ViewId == viewId)
        //                    {
        //                        isAlreadyOpen = true;
        //                        break;
        //                    }
        //                }

        //                // If not already open, open it
        //                if (!isAlreadyOpen)
        //                {
        //                    // Check if we need to switch active document for certain view types
        //                    if (view is ViewSchedule || view is ViewDrafting)
        //                    {
        //                        _uiDocument.ActiveView = view;
        //                        Thread.Sleep(500);
        //                    }
        //                    else
        //                    {

        //                        _uiDocument.ActiveView = view;
        //                        Thread.Sleep(500);
        //                    }
        //                    openedCount++;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                RevitUI.TaskDialog.Show("Error", ex.Message);
        //            }
        //        }
        //    }
        //    public static void ApplyPdfOptionsToPrintParameters(
        //Document doc,
        //PDFExportOptions pdfOptions,
        //PrintParameters printParams)
        //    {
        //        if (pdfOptions == null)
        //            throw new ArgumentNullException(nameof(pdfOptions));

        //        if (printParams == null)
        //            throw new ArgumentNullException(nameof(printParams));
        //        // Color depth
        //        printParams.ColorDepth = pdfOptions.ColorDepth;

        //        // Orientation
        //        printParams.PageOrientation =
        //            pdfOptions.PaperOrientation == PageOrientationType.Landscape
        //                ? PageOrientationType.Landscape
        //                : PageOrientationType.Portrait;

        //        // Zoom
        //        printParams.ZoomType = pdfOptions.ZoomType;
        //        if (pdfOptions.ZoomType == ZoomType.Zoom)
        //        {
        //            printParams.Zoom = pdfOptions.ZoomPercentage;
        //        }

        //        // Placement
        //        printParams.PaperPlacement = pdfOptions.PaperPlacement;



        //        // Raster quality
        //        printParams.RasterQuality = pdfOptions.RasterQuality;

        //    }
        #endregion

        private void ExecuteBrowseSaveLocation()
        {
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Export Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                SaveLocation = dialog.FileName;
            }
        }


        private void ExecuteCustomFileName()
        {
            var dialog = new TextBoxDialogue
            {
                TitleText = "Custom File Name",
                Message = "Enter Custom File Name:"
            };
            dialog.TextValue = _exportFileName ?? "Default File Name";
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                _exportFileName = dialog.TextValue;
            }
            else
            {
                var random = new Random();
                _exportFileName = $"Default File Name {random}";
            }
        }
        private bool CanExecuteCustomFileName()
        {
            if (IsCombinedChecked)
                return true;
            else return false;
        }
        #endregion
        #region ViewSheetSet Management
        public RelayCommand SaveVSSet { get; }
        public RelayCommand SaveAsVSSet { get; }
        public RelayCommand RenameVSSet { get; }
        public RelayCommand DeleteVSSet { get; }

        private ViewSheetSetItem _selectedViewSheetSet;
        public ViewSheetSetVM ViewSheetSetVM => _viewSheetSetVM;
        public ViewSheetSetItem SelectedViewSheetSet
        {
            get => _selectedViewSheetSet;
            set
            {
                if (SetProperty(ref _selectedViewSheetSet, value))
                {
                    if (value != null && value.Name != "<In Session>")
                    {
                        _sheetSelectionVM.SelectedViewSheetSet = value.ViewSheetSet;
                        _viewSelectionVM.SelectedViewSheetSet = value.ViewSheetSet;
                        ViewSheetSetVM.SelectedViewSheetSet = value;
                        IsFilterChecked = true;
                    }
                    else
                    {
                        _sheetSelectionVM.RemoveFilter();
                        _sheetSelectionVM.HeaderCheckState = false;
                        _sheetSelectionVM.SetSelection(false);
                        _viewSelectionVM.RemoveFilter();
                        _viewSelectionVM.HeaderCheckState = false;
                        _viewSelectionVM.SetSelection(false);
                        IsFilterChecked = false;
                    }
                }
            }
        }
        private void ExecuteSaveVSSet()
        {
            ViewSet views = new ViewSet();
            foreach (var item in _viewSelectionVM.SelectedViewItems)
            {
                views.Insert(item.ViewElement);
            }
            foreach (var item in _sheetSelectionVM.SelectedSheets)
            {
                views.Insert(item.SheetElement);
            }
            _viewSheetSetVM.SaveVSSet(views);
            SelectedViewSheetSet = _viewSheetSetVM.SelectedViewSheetSet;
            OnPropertyChanged(nameof(SelectedViewSheetSet));
        }
        private bool CanExecuteSaveVSSet()
        {
            if ((_viewSelectionVM.SelectedViewItems.Count > 0 || _sheetSelectionVM.SelectedSheets.Count > 0))
                return true;
            else
                return false;
        }
        private void ExecuteSaveAsVSSet()
        {
            ViewSet views = new ViewSet();
            foreach (var item in _viewSelectionVM.SelectedViewItems)
            {
                views.Insert(item.ViewElement);
            }
            foreach (var item in _sheetSelectionVM.SelectedSheets)
            {
                views.Insert(item.SheetElement);
            }
            _viewSheetSetVM.SaveAsVSSet(views);
            SelectedViewSheetSet = _viewSheetSetVM.SelectedViewSheetSet;
            OnPropertyChanged(nameof(SelectedViewSheetSet));
        }
        private void ExecuteRenameVSSet()
        {
            ViewSheetSetVM.RenameVSSet();
            SelectedViewSheetSet = ViewSheetSetVM.SelectedViewSheetSet;
            OnPropertyChanged(nameof(SelectedViewSheetSet));
        }
        private bool CanExecuteRenameVSSet()
        {
            if (SelectedViewSheetSet.Name != "<In Session>")
                return true;
            else
                return false;
        }
        private void ExecuteDeleteVSSet()
        {
            ViewSheetSetVM.DeleteVSSet();
            SelectedViewSheetSet = ViewSheetSetVM.SelectedViewSheetSet;
        }
        private bool CanExecuteDeleteVSSet()
        {
            if (SelectedViewSheetSet.Name != "<In Session>")
                return true;
            else
                return false;
        }
        #endregion
        #region Custom Print Order
        public RelayCommand CustomPrintOrderCommand { get; }
        private CustomPrintOrderViewModel CustomPrintOrderViewModel { get; set; }
        private void ExecuteCustomPrintOrder()
        {
            IList<IViewSheetItem> selSheets = _sheetSelectionVM.SelectedSheets.ToList().Select(a => a as IViewSheetItem).ToList();
            IList<IViewSheetItem> selViews = _viewSelectionVM.SelectedViewItems.ToList().Select(a => a as IViewSheetItem).ToList();

            List<IViewSheetItem> combinedViews = selSheets.Concat(selViews).ToList();
            if (!HaveSameItems(combinedViews, CustomPrintOrderViewModel.SortableViews.ToList()))
            {
                CustomPrintOrderViewModel = new CustomPrintOrderViewModel(combinedViews);
                _combinedViews.Clear();
                _combinedViews = combinedViews;
            }


            var printOrderView = new CustomPrintOrderView
            {
                DataContext = CustomPrintOrderViewModel
            };
            bool? result = printOrderView.ShowDialog();
            if (result == true)
            {
                // Update the sheet order based on the ViewModel's SortedSheets
                var sortedSheets = CustomPrintOrderViewModel.SortableViews;
                // Clear the current selection and re-add sheets in the new order
                _combinedSortedViews.Clear();
                foreach (var sheet in sortedSheets)
                {
                    _combinedSortedViews.Add(sheet.ItemElementId);
                }
            }
        }
        private bool CanExecuteCustomPrintOrder()
        {
            if (_sheetSelectionVM.SelectedSheets.Count + _viewSelectionVM.SelectedViewItems.Count > 0 && IsCombinedChecked)
                return true;
            else
                return false;
        }
        private static bool HaveSameItems(
        IList<IViewSheetItem> first,
        IList<IViewSheetItem> second)
        {
            if (first == null || second == null)
                return false;

            if (first.Count != second.Count)
                return false;

            var firstIds = new HashSet<ElementId>(
                first.Select(x => x.ItemElementId));

            return second.All(x => firstIds.Contains(x.ItemElementId));
        }

        #endregion


        #region Profile Management
        private SettingsProfile _selectedSettingsProfile;
        public bool IncludePrintOrderChecked { get; set; }
        public ObservableCollection<SettingsProfile> SettingsProfiles { get; set; }
        public SettingsProfile SelectedSettingsProfile
        {
            get => _selectedSettingsProfile;
            set
            {
                if (SetProperty(ref _selectedSettingsProfile, value))
                {
                    if (value.IsInSession)
                    {
                        _pDFSettingsVM.SetInSessionPDFSettings();
                    }
                    if (value != null)
                    {
                        ApplyPDFOptionsDTO(value);
                        ApplyNamingTokenDTOs(value);
                        ApplyPrintOrderDTO(value);
                    }
                }
            }
        }

        public RelayCommand SaveAsProfileCommand { get; }
        public RelayCommand SaveProfileCommand { get; }
        public RelayCommand OpenProfilesFolderCommand { get; }
        private void InitializeProfileSettings()
        {
            SettingsProfiles = new ObservableCollection<SettingsProfile>(JsonSettingsStorage.LoadProfiles(_revitYear).ToList());
            SettingsProfiles.Add(new SettingsProfile() { ProfileName = "In Session", IsInSession = true });
            if (SettingsProfiles.Count > 0)
            {
                SelectedSettingsProfile = SettingsProfiles.Where(a => a.IsInSession).FirstOrDefault();
                LoadPDFOptionsDTO(SettingsProfiles.ToList());
            }
        }
        private void ExecuteSaveAsProfile()
        {
            var dialog = new ProfileSaveAsDialog
            {
                DataContext = this
            };
            bool? result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dialog.TextValue))
                throw new InvalidOperationException("A name is required for the Settings Profile Creation.");
            string newName = dialog.TextValue.Trim();

            List<NamingTokenDto> tokens = GetNamingTokenDTOs();


            var fileNamingDTO = new FileNamingDto()
            {
                SheetNamingSeparator = _sheetSelectionVM.CustomNamingRuleVM.Seperator,
                ViewNamingSeparator = _viewSelectionVM.CustomFileNameVM.Seperator,
                Tokens = tokens
            };

            var options = GetPdfOptionsDto();
            var pdfExportProfile = new SettingsProfile()
            {
                ProfileName = newName,
                FileNamingParameters = fileNamingDTO,
                PdfOptions = options
            };
            if (IncludePrintOrderChecked)
            {
                var printOrderDTO = GetPrintOrderDTO();
                pdfExportProfile.PrintOrder = printOrderDTO;
            }

            JsonSettingsStorage.SaveProfile(_revitYear, pdfExportProfile);
            InitializeProfileSettings();
            OnPropertyChanged(nameof(SettingsProfiles));
            SelectedSettingsProfile = SettingsProfiles
                .Where(a => a.ProfileName == pdfExportProfile.ProfileName).FirstOrDefault();
            OnPropertyChanged(nameof(SelectedSettingsProfile));
        }
        private void ExecuteSaveProfile()
        {
            if (SelectedSettingsProfile.IsInSession)
                ExecuteSaveAsProfile();
            else
            {
                PDFSettingsVM.ExecuteSavePDFOptions();
                List<NamingTokenDto> tokens = GetNamingTokenDTOs();

                var fileNamingDTO = new FileNamingDto()
                {
                    SheetNamingSeparator = SheetSelectionVM.CustomNamingRuleVM.Seperator,
                    ViewNamingSeparator = ViewSelectionVM.CustomFileNameVM.Seperator,
                    Tokens = tokens
                };

                var options = GetPdfOptionsDto();
                var pdfExportProfile = new SettingsProfile()
                {
                    ProfileName = SelectedSettingsProfile.ProfileName,
                    FileNamingParameters = fileNamingDTO,
                    PdfOptions = options
                };
                if (IncludePrintOrderChecked)
                {
                    var printOrderDTO = GetPrintOrderDTO();
                    pdfExportProfile.PrintOrder = printOrderDTO;
                }

                JsonSettingsStorage.SaveProfile(_revitYear, pdfExportProfile);
                InitializeProfileSettings();
                OnPropertyChanged(nameof(SettingsProfiles));
                SelectedSettingsProfile = SettingsProfiles
                .Where(a => a.ProfileName == pdfExportProfile.ProfileName).FirstOrDefault();
                OnPropertyChanged(nameof(SelectedSettingsProfile));
            }
        }
        private List<NamingTokenDto> GetNamingTokenDTOs()
        {
            List<NamingTokenDto> tokens = new List<NamingTokenDto>();
            foreach (var parameterItem in _sheetSelectionVM.CustomNamingRuleVM.SelectedParameters)
            {
                tokens.Add(new NamingTokenDto()
                {
                    Name = parameterItem.Name,
                    BuiltInParameterId = parameterItem.Parameter?.Id.Value,
                    IsCustom = parameterItem.IsCustom,
                    IsSheet = true
                });
            }
            foreach (var parameterItem in _viewSelectionVM.CustomFileNameVM.SelectedParameters)
            {
                tokens.Add(new NamingTokenDto()
                {
                    Name = parameterItem.Name,
                    BuiltInParameterId = parameterItem.Parameter?.Id.Value,
                    IsCustom = parameterItem.IsCustom,
                    IsSheet = false
                });
            }
            return tokens;
        }
        private PrintOrderDto GetPrintOrderDTO()
        {
            var printOrderDTO = new PrintOrderDto();
            if (_combinedSortedViews.Count > 0)
            {
                foreach (var id in _combinedSortedViews)
                {
                    printOrderDTO.OrderedElementIds.Add(id.Value);
                }
            }
            else
            {
                IList<ElementId> selSheets = _sheetSelectionVM.SelectedSheets.Select(a => a.ItemElementId).ToList();
                IList<ElementId> selViews = _viewSelectionVM.SelectedViewItems.Select(v => v.ViewElement.Id).ToList();
                var combinedList = selSheets.Concat(selViews).ToList();
                foreach (var id in combinedList)
                {
                    printOrderDTO.OrderedElementIds.Add(id.Value);
                }
            }
            return printOrderDTO;
        }
        private PDFOptionsDto GetPdfOptionsDto()
        {
            var options = new PDFOptionsDto()
            {
                FileName = _pDFSettingsVM.CurrentPDFExportSettingItem.FileName,
                OriginOffsetX = _pDFSettingsVM.CurrentPDFExportSettingItem.OriginOffsetX,
                OriginOffsetY = _pDFSettingsVM.CurrentPDFExportSettingItem.OriginOffsetY,
                ZoomPercentage = _pDFSettingsVM.CurrentPDFExportSettingItem.ZoomPercentage,
                PaperFormat = _pDFSettingsVM.CurrentPDFExportSettingItem.PaperFormat,
                RasterQuality = _pDFSettingsVM.CurrentPDFExportSettingItem.RasterQuality,
                ColorDepth = _pDFSettingsVM.CurrentPDFExportSettingItem.ColorDepth,
                ExportQuality = _pDFSettingsVM.CurrentPDFExportSettingItem.ExportQuality,
                PaperPlacement = _pDFSettingsVM.CurrentPDFExportSettingItem.PaperPlacement,
                AlwaysUseRaster = _pDFSettingsVM.CurrentPDFExportSettingItem.AlwaysUseRaster,
                ZoomType = _pDFSettingsVM.CurrentPDFExportSettingItem.ZoomType,
                PaperOrientation = _pDFSettingsVM.CurrentPDFExportSettingItem.PaperOrientation,
                Combine = _pDFSettingsVM.CurrentPDFExportSettingItem.Combine,
                HideCropBoundaries = _pDFSettingsVM.CurrentPDFExportSettingItem.HideCropBoundaries,
                HideReferencePlanes = _pDFSettingsVM.CurrentPDFExportSettingItem.HideReferencePlanes,
                HideScopeBoxes = _pDFSettingsVM.CurrentPDFExportSettingItem.HideScopeBoxes,
                HideUnreferencedViewTags = _pDFSettingsVM.CurrentPDFExportSettingItem.HideUnreferencedViewTags,
                MaskCoincidentLines = _pDFSettingsVM.CurrentPDFExportSettingItem.MaskCoincidentLines,
                ReplaceHalftoneLines = _pDFSettingsVM.CurrentPDFExportSettingItem.ReplaceHalftoneLines,
                ViewLinksInBlue = _pDFSettingsVM.CurrentPDFExportSettingItem.ViewLinksInBlue,
            };
            return options;
        }
        public void LoadPDFOptionsDTO(List<SettingsProfile> profiles)
        {
            List<PDFExportOptionsItem> profilePDFExportSettingItems = new List<PDFExportOptionsItem>();
            foreach (SettingsProfile profile in profiles)
            {
                if (profile.IsInSession)
                    continue;
                var profilePDFExportSettingItem = new PDFExportOptionsItem();
                profilePDFExportSettingItem.Name = profile.PDFExportSettingItemName;
                profilePDFExportSettingItem.FileName = $"{profile.ProfileName} - Default";
                profilePDFExportSettingItem.OriginOffsetX = profile.PdfOptions.OriginOffsetX;
                profilePDFExportSettingItem.OriginOffsetY = profile.PdfOptions.OriginOffsetY;
                profilePDFExportSettingItem.ZoomPercentage = profile.PdfOptions.ZoomPercentage;
                profilePDFExportSettingItem.PaperFormat = profile.PdfOptions.PaperFormat;
                profilePDFExportSettingItem.RasterQuality = profile.PdfOptions.RasterQuality;
                profilePDFExportSettingItem.ColorDepth = profile.PdfOptions.ColorDepth;
                profilePDFExportSettingItem.ExportQuality = profile.PdfOptions.ExportQuality;
                profilePDFExportSettingItem.PaperPlacement = profile.PdfOptions.PaperPlacement;
                profilePDFExportSettingItem.AlwaysUseRaster = profile.PdfOptions.AlwaysUseRaster;
                profilePDFExportSettingItem.ZoomType = profile.PdfOptions.ZoomType;
                profilePDFExportSettingItem.PaperOrientation = profile.PdfOptions.PaperOrientation;
                profilePDFExportSettingItem.Combine = profile.PdfOptions.Combine;
                profilePDFExportSettingItem.HideCropBoundaries = profile.PdfOptions.HideCropBoundaries;
                profilePDFExportSettingItem.HideReferencePlanes = profile.PdfOptions.HideReferencePlanes;
                profilePDFExportSettingItem.HideScopeBoxes = profile.PdfOptions.HideScopeBoxes;
                profilePDFExportSettingItem.HideUnreferencedViewTags = profile.PdfOptions.HideUnreferencedViewTags;
                profilePDFExportSettingItem.MaskCoincidentLines = profile.PdfOptions.MaskCoincidentLines;
                profilePDFExportSettingItem.ReplaceHalftoneLines = profile.PdfOptions.ReplaceHalftoneLines;
                profilePDFExportSettingItem.ViewLinksInBlue = profile.PdfOptions.ViewLinksInBlue;
                profilePDFExportSettingItems.Add(profilePDFExportSettingItem);
            }
            PDFSettingsVM.AddProfileSettings(profilePDFExportSettingItems);
        }
        public void ApplyPDFOptionsDTO(SettingsProfile profile)
        {
            PDFSettingsVM.ApplyPDFOptionsDTO(profile.PDFExportSettingItemName);
        }
        public void ApplyNamingTokenDTOs(SettingsProfile profile)
        {
            if (profile.FileNamingParameters == null || profile.FileNamingParameters.Tokens == null)
                return;
            var sheetProfileParamsCount = profile.FileNamingParameters.Tokens.Where(a => a.IsSheet).Count();
            var viewProfileParamsCount = profile.FileNamingParameters.Tokens.Where(a => !a.IsSheet).Count();
            if (sheetProfileParamsCount > 0)
            {
                SheetSelectionVM.CustomNamingRuleVM.SelectedParameters.Clear();
                SheetSelectionVM.CustomNamingRuleVM.Seperator = profile.FileNamingParameters.SheetNamingSeparator ?? "";
            }
            if (viewProfileParamsCount > 0)
            {
                ViewSelectionVM.CustomFileNameVM.SelectedParameters.Clear();
                ViewSelectionVM.CustomFileNameVM.Seperator = profile.FileNamingParameters.ViewNamingSeparator ?? "";
            }
            var sheetParams = SheetSelectionVM.CustomNamingRuleVM.SelectedParameters;
            var viewParams = ViewSelectionVM.CustomFileNameVM.SelectedParameters;


            foreach (var token in profile.FileNamingParameters.Tokens)
            {
                if (token.IsSheet)
                {
                    if (token.IsCustom)
                    {
                        sheetParams.Add(new ParameterItem(token.Name));
                    }
                    else if (token.BuiltInParameterId != null || token.BuiltInParameterId == -1)
                    {
                        var Id = token.BuiltInParameterId ?? -1;

                        var param = SheetSelectionVM.CustomNamingRuleVM.AvailableParameters
                            .FirstOrDefault(a => a.Parameter.Id == new ElementId(Id));
                        sheetParams.Add(param);
                    }
                }
                if (!token.IsSheet)
                {
                    if (token.IsCustom)
                    {
                        viewParams.Add(new ParameterItem(token.Name));
                    }
                    else if (token.BuiltInParameterId != null || token.BuiltInParameterId == -1)
                    {
                        var Id = token.BuiltInParameterId ?? -1;

                        var param = ViewSelectionVM.CustomFileNameVM.AvailableParameters
                            .FirstOrDefault(a => a.Parameter.Id == new ElementId(Id));
                        viewParams.Add(param);
                    }
                }
            }
            if (sheetProfileParamsCount > 0)
                SheetSelectionVM.CreateNamingRule(SheetSelectionVM.CustomNamingRuleVM.SelectedParameters.ToList()
                , SheetSelectionVM.CustomNamingRuleVM.Seperator ?? "");
            else
            {
                SheetSelectionVM.CustomNamingRuleVM.ExecuteClear();
                SheetSelectionVM.SheetFileNameRule = PdfSettingsStorage.GetDefaultSheetNamingRule();
                SheetSelectionVM.UpdateFileName();
            }
            if (viewProfileParamsCount > 0)
                ViewSelectionVM.CreateNamingRule(ViewSelectionVM.CustomFileNameVM.SelectedParameters.ToList()
                , ViewSelectionVM.CustomFileNameVM.Seperator ?? "");
            else
            {
                ViewSelectionVM.CustomFileNameVM.ExecuteClear();
                ViewSelectionVM.ViewFileNameRule = PdfSettingsStorage.GetDefaultViewNamingRule();
                ViewSelectionVM.UpdateFileName();
            }
        }
        public void ApplyPrintOrderDTO(SettingsProfile profile)
        {
            if (profile.PrintOrder == null || profile.PrintOrder.OrderedElementIds.Count == 0)
                return;
            List<IViewSheetItem> combinedViews = new List<IViewSheetItem>();
            foreach (var id in profile.PrintOrder.OrderedElementIds)
            {
                _sheetSelectionVM.Sheets
                    .Where(a => a.ItemElementId == new ElementId(id))
                    .ToList()
                    .ForEach(a => a.IsSelected = true);
                OnPropertyChanged(nameof(_sheetSelectionVM.SelectedSheets));
                _viewSelectionVM.ViewItems
                    .Where(a => a.ItemElementId == new ElementId(id))
                    .ToList()
                    .ForEach(a => a.IsSelected = true);
                OnPropertyChanged(nameof(_viewSelectionVM.SelectedViewItems));
                var selectedSheetIds = _sheetSelectionVM.SelectedSheets.ToList().Select(a => a.ItemElementId).ToList();
                var selectedViewIds = _viewSelectionVM.SelectedViewItems.ToList().Select(a => a.ItemElementId).ToList();
                if (selectedSheetIds.Contains(new ElementId(id)))
                    combinedViews.Add(_sheetSelectionVM.SelectedSheets.Where(a => a.ItemElementId == new ElementId(id))
                        .FirstOrDefault());
                if (selectedViewIds.Contains(new ElementId(id)))
                    combinedViews.Add(_viewSelectionVM.SelectedViewItems.Where(a => a.ItemElementId == new ElementId(id))
                        .FirstOrDefault());

                IList<IViewSheetItem> selSheets = _sheetSelectionVM.SelectedSheets.ToList().Select(a => a as IViewSheetItem).ToList();
                IList<IViewSheetItem> selViews = _viewSelectionVM.SelectedViewItems.ToList().Select(a => a as IViewSheetItem).ToList();
                CustomPrintOrderViewModel = new CustomPrintOrderViewModel(combinedViews);
            }
        }

        public void ExecuteOpenProfilesFolder()
        {
            var folder = RevitAddinPaths.GetProfilesFolder(_revitYear);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            Process.Start("explorer.exe", folder);
        }


        #endregion
    }

}
