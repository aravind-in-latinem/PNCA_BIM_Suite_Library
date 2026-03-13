using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Drawing;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.ViewModel.PNCAExporter;
using PNCA_BIM_Suite_Library.Views.PNCAExporter;

namespace PNCA_BIM_Suite_Library.ViewModel
{
    public class PDFSettingsViewModel : ObservableObject
    {
        private ObservableCollection<PDFExportOptionsItem> _pdfExportSettingsCollection;
        private PDFExportOptionsItem _selectedPDFExportSettingItem;
        private bool _alwaysUseRaster;
        private ObservableCollection<TableCellCombinedParameterData> _sheetNamingRule;
        private ObservableCollection<TableCellCombinedParameterData> _viewNamingRule;


        public PDFExportOptionsItem CurrentPDFExportSettingItem { get; set; }

        // ENUM OPTIONS FOR UI
        public EnumItem<ColorDepthType>[] ColorDepthOptions { get; }


        public EnumItem<PDFExportQualityType>[] ExportQualityOptions { get; }


        public EnumItem<ExportPaperFormat>[] PaperFormatOptions { get; }


        public EnumItem<PageOrientationType>[] PaperOrientationOptions { get; }


        public EnumItem<PaperPlacementType>[] PaperPlacementOptions { get; }


        public EnumItem<RasterQualityType>[] RasterQualityOptions { get; }


        public EnumItem<ZoomType>[] ZoomTypeOptions { get; }


        public PDFSettingsViewModel()
        {
            CurrentPDFExportSettingItem = new PDFExportOptionsItem();
            ColorDepthOptions = RevitCollectorServices.GetEnumItems<ColorDepthType>();
            ExportQualityOptions = RevitCollectorServices.GetEnumItems<PDFExportQualityType>();
            PaperFormatOptions = RevitCollectorServices.GetEnumItems<ExportPaperFormat>();
            PaperOrientationOptions = RevitCollectorServices.GetEnumItems<PageOrientationType>();
            PaperPlacementOptions = RevitCollectorServices.GetEnumItems<PaperPlacementType>();
            RasterQualityOptions = RevitCollectorServices.GetEnumItems<RasterQualityType>();
            ZoomTypeOptions = RevitCollectorServices.GetEnumItems<ZoomType>();
            SelectedColorDepth = ColorDepthOptions.First(e => e.Value == ColorDepthType.Color);

            SelectedExportQuality = ExportQualityOptions.First(e => e.Value == PDFExportQualityType.DPI600);

            SelectedPaperFormatOption = PaperFormatOptions.First(e => e.Value == ExportPaperFormat.ISO_A1);

            SelectedRasterQuality = RasterQualityOptions.First(e => e.Value == RasterQualityType.High);
            SavePDFOption = new RelayCommand(ExecuteSavePDFOptions);
            SaveAsPDFOption = new RelayCommand(ExecuteSaveAsPDFOption);
            RenamePDFOption = new RelayCommand(ExecuteRenamePDFOption,CanRenamePDFOption);
            DeletePDFOption = new RelayCommand(ExecuteDeletePDFOption, CanDeletePDFOption);
            PDFExportSettingsCollection = new ObservableCollection<PDFExportOptionsItem>();
            PrintSettingsItem = new PrintSettingsItem();
            PrinterNames = new ObservableCollection<string>();
            foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                PrinterNames.Add(printer);
            }
            SelectedPrinterName = PrinterNames.FirstOrDefault(a=>a=="diroots.prosheets");
        }

        #region Commands
        //Commands

        public RelayCommand SavePDFOption { get; }
        public RelayCommand SaveAsPDFOption { get; }
        public RelayCommand RenamePDFOption { get; }
        public RelayCommand DeletePDFOption { get; }
        #endregion

        #region properties for binding
        //properties
        public Document Document { get; set; }

        private string _selectedPrinterName;
        public string SelectedPrinterName 
        { get=>_selectedPrinterName; 
          set=>SetProperty(ref _selectedPrinterName,value); 
        }
        public ObservableCollection<string> PrinterNames { get; set; }

        

        public ObservableCollection<PDFExportOptionsItem> PDFExportSettingsCollection
        {
            get => _pdfExportSettingsCollection;
            set => SetProperty(ref _pdfExportSettingsCollection, value);
        }
        public ObservableCollection<TableCellCombinedParameterData> SheetNamingRule
        {
            get => _sheetNamingRule;
            set => SetProperty(ref _sheetNamingRule, value);
        }
        public ObservableCollection<TableCellCombinedParameterData> ViewNamingRule
        {
            get => _viewNamingRule;
            set => SetProperty(ref _viewNamingRule, value);
        }
        
        public PDFExportOptionsItem SelectedPDFExportSettingItem
        {
            get => _selectedPDFExportSettingItem;
            set
            {
                if (SetProperty(ref _selectedPDFExportSettingItem, value))
                {
                    if (value != null&&value.Name!="<In Session>")
                    {
                        ApplyRevitPDFSettings(value.DbSetting.GetOptions());
                    }
                }
            }
        }

        public bool IsCombinedChecked
        {
            get => CurrentPDFExportSettingItem.Combine;
            set
            {
                if (CurrentPDFExportSettingItem.Combine != value)
                {
                    CurrentPDFExportSettingItem.Combine = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsVectorProcessing
        {
            get => !CurrentPDFExportSettingItem.AlwaysUseRaster;
            set
            {
                if (value)
                    CurrentPDFExportSettingItem.AlwaysUseRaster = false;

                OnPropertyChanged(nameof(IsVectorProcessing));
                OnPropertyChanged(nameof(IsRasterProcessing));
                
            }
        }

        public bool IsRasterProcessing
        {
            get => CurrentPDFExportSettingItem.AlwaysUseRaster;
            set
            {
                if (value)
                    CurrentPDFExportSettingItem.AlwaysUseRaster = true;

                OnPropertyChanged(nameof(IsVectorProcessing));
                OnPropertyChanged(nameof(IsRasterProcessing));
                
            }
        }        

        public bool AlwaysUseRaster
        {
            get => _alwaysUseRaster;
            set
            {
                if (SetProperty(ref _alwaysUseRaster, value))
                {
                    CurrentPDFExportSettingItem.AlwaysUseRaster = value;
                    OnPropertyChanged(nameof(IsVectorProcessing));
                    OnPropertyChanged(nameof(IsRasterProcessing));
                    
                }
            }
        }


        private EnumItem<ExportPaperFormat> _selectedPaperFormatOption;
        public EnumItem<ExportPaperFormat> SelectedPaperFormatOption
        {
            get => _selectedPaperFormatOption;
            set
            {
                SetProperty(ref _selectedPaperFormatOption, value);
                CurrentPDFExportSettingItem.PaperFormat = value.Value;
                
            }
        }

        private EnumItem<RasterQualityType> _selectedRasterQuality;
        public EnumItem<RasterQualityType> SelectedRasterQuality
        {
            get => _selectedRasterQuality;
            set
            {
                SetProperty(ref _selectedRasterQuality, value);
                if (value != null)
                    CurrentPDFExportSettingItem.RasterQuality = value.Value;
                
            }
        }

        private EnumItem<ColorDepthType> _selectedColorDepth;
        public EnumItem<ColorDepthType> SelectedColorDepth
        {
            get => _selectedColorDepth;
            set
            {
                SetProperty(ref _selectedColorDepth, value);
                CurrentPDFExportSettingItem.ColorDepth = value.Value;
                
            }
        }

        private EnumItem<PDFExportQualityType> _selectedExportQuality;
        public EnumItem<PDFExportQualityType> SelectedExportQuality
        {
            get => _selectedExportQuality;
            set
            {
                SetProperty(ref _selectedExportQuality, value);
                if (value != null)
                    CurrentPDFExportSettingItem.ExportQuality = value.Value;
                
            }
        }

        //text boxes
        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set
            {
                if (SetProperty(ref _fileName, value))
                {
                    CurrentPDFExportSettingItem.FileName = value;
                    
                }
            }
        }
        public bool IsCustomOffset => PaperPlacement != PaperPlacementType.Center;

        private double _originOffsetX;
        public double OriginOffsetX
        {
            get => _originOffsetX;
            set
            {
                if (SetProperty(ref _originOffsetX, value))
                {
                    CurrentPDFExportSettingItem.OriginOffsetX = value;
                    
                }
            }
        }

        private double _originOffsetY;
        public double OriginOffsetY
        {
            get => _originOffsetY;
            set
            {
                if (SetProperty(ref _originOffsetY, value))
                {
                    CurrentPDFExportSettingItem.OriginOffsetY = value;
                    
                }
            }
        }
        public bool IsZoomPercent => ZoomType == ZoomType.Zoom;

        private int _zoomPercentage;
        public int ZoomPercentage
        {
            get => _zoomPercentage;
            set
            {
                if (SetProperty(ref _zoomPercentage, value))
                {
                    CurrentPDFExportSettingItem.ZoomPercentage = value;
                    
                }
            }
        }

        //radio buttons
        private PaperPlacementType _paperPlacement;
        public PaperPlacementType PaperPlacement
        {
            get => _paperPlacement;
            set
            {
                if (SetProperty(ref _paperPlacement, value))
                {
                    CurrentPDFExportSettingItem.PaperPlacement = value;
                    OnPropertyChanged(nameof(IsCustomOffset));
                    
                }
            }
        }

        private ZoomType _zoomType;
        public ZoomType ZoomType
        {
            get => _zoomType;
            set
            {
                if (SetProperty(ref _zoomType, value))
                {
                    CurrentPDFExportSettingItem.ZoomType = value;
                    OnPropertyChanged(nameof(IsZoomPercent));
                    
                }
            }
        }

        private PageOrientationType _paperOrientation;
        public PageOrientationType PaperOrientation
        {
            get => _paperOrientation;
            set
            {
                if (SetProperty(ref _paperOrientation, value))
                {
                    CurrentPDFExportSettingItem.PaperOrientation = value;
                    
                }
            }
        }
        //Check boxes        

        private bool _hideCropBoundaries;
        public bool HideCropBoundaries
        {
            get => _hideCropBoundaries;
            set
            {
                if (SetProperty(ref _hideCropBoundaries, value))
                {
                    CurrentPDFExportSettingItem.HideCropBoundaries = value;
                    
                }
            }
        }

        private bool _hideReferencePlanes;
        public bool HideReferencePlanes
        {
            get => _hideReferencePlanes;
            set
            {
                if (SetProperty(ref _hideReferencePlanes, value))
                {
                    CurrentPDFExportSettingItem.HideReferencePlanes = value;
                    
                }
            }
        }

        private bool _hideScopeBoxes;
        public bool HideScopeBoxes
        {
            get => _hideScopeBoxes;
            set
            {
                if (SetProperty(ref _hideScopeBoxes, value))
                {
                    CurrentPDFExportSettingItem.HideScopeBoxes = value;
                    
                }
            }
        }

        private bool _hideUnreferencedViewTags;
        public bool HideUnreferencedViewTags
        {
            get => _hideUnreferencedViewTags;
            set
            {
                if (SetProperty(ref _hideUnreferencedViewTags, value))
                {
                    CurrentPDFExportSettingItem.HideUnreferencedViewTags = value;
                    
                }
            }
        }

        private bool _maskCoincidentLines;
        public bool MaskCoincidentLines
        {
            get => _maskCoincidentLines;
            set
            {
                if (SetProperty(ref _maskCoincidentLines, value))
                {
                    CurrentPDFExportSettingItem.MaskCoincidentLines = value;
                    
                }
            }
        }

        private bool _replaceHalftoneLines;
        public bool ReplaceHalftoneLines
        {
            get => _replaceHalftoneLines;
            set
            {
                if (SetProperty(ref _replaceHalftoneLines, value))
                {
                    CurrentPDFExportSettingItem.ReplaceHalftoneLines = value;
                    
                }
            }
        }        

        private bool _viewLinksInBlue;
        public bool ViewLinksInBlue
        {
            get => _viewLinksInBlue;
            set
            {
                if (SetProperty(ref _viewLinksInBlue, value))
                {
                    CurrentPDFExportSettingItem.ViewLinksInBlue = value;
                    
                }
            }
        }
        public PrintSettingsItem PrintSettingsItem { get; set; }
        #endregion
        //methods
        #region methods

        public void ExecuteSavePDFOptions()
        {
            // CASE 1: <In Session> → must prompt for name and CREATE new setting
            if (SelectedPDFExportSettingItem.IsInSession)
            {
                var dialog = new TextBoxDialogue
                {
                    TitleText = "Save PDF Export Settings",
                    Message = "PDF Export Setting Name:"
                };
                //dialog.Owner = System.Windows.Application.Current.MainWindow;

                bool? result = dialog.ShowDialog();

                if (result != true || string.IsNullOrWhiteSpace(dialog.TextValue))
                    throw new InvalidOperationException("A name is required to save PDF export settings.");

                string newName = dialog.TextValue.Trim();

                using (var t = new Transaction(Document, "Save PDF Export Settings"))
                {
                    t.Start();

                    // Create new Revit PDFExportSettings
                    var newSetting = ExportPDFSettings.Create(
                        Document,
                        newName,
                        PdfSettingsStorage.ToRevitOptions(CurrentPDFExportSettingItem)
                    );

                    t.Commit();

                    // Update UI collection
                    var wrapper = new PDFExportOptionsItem(newSetting);
                    

                    PDFExportSettingsCollection.Add(wrapper);
                    SelectedPDFExportSettingItem = wrapper;
                }

                return;
            }
            using (var t = new Transaction(Document))
            {
                t.Start("Save Selected Option");
                SelectedPDFExportSettingItem.DbSetting.SetOptions(PdfSettingsStorage.ToRevitOptions(CurrentPDFExportSettingItem));
                t.Commit();
            }

        }
        private void ExecuteSaveAsPDFOption()
        {
            var dialog = new TextBoxDialogue
            {
                TitleText = "New PDF Export Settings",
                Message = "PDF Export Setting Name:"
            };
            bool? result = dialog.ShowDialog();
            if (result != true || string.IsNullOrWhiteSpace(dialog.TextValue))
                throw new InvalidOperationException("A name is required to rename PDF export settings.");
            string newName = dialog.TextValue.Trim();
            using (var t = new Transaction(Document, "New PDF Export Settings"))
            {
                t.Start();

                // Create new Revit PDFExportSettings
                var newSetting = ExportPDFSettings.Create(
                    Document,
                    newName,
                    PdfSettingsStorage.ToRevitOptions(CurrentPDFExportSettingItem)
                );

                t.Commit();

                // Update UI collection
                var wrapper = new PDFExportOptionsItem(newSetting);


                PDFExportSettingsCollection.Add(wrapper);
                SelectedPDFExportSettingItem = wrapper;
            }

        }
        private bool CanRenamePDFOption()
        {
            return SelectedPDFExportSettingItem != null && !SelectedPDFExportSettingItem.IsInSession;
        }
        private void ExecuteRenamePDFOption()
        {
            if(SelectedPDFExportSettingItem.IsInSession)
            {
                throw new InvalidOperationException("Cannot rename the in-session PDF export settings.");
            }
            else 
            {
                var dialog = new TextBoxDialogue
                {
                    TitleText = "Rename PDF Export Settings",
                    Message = "New PDF Export Setting Name:"
                };
                
                bool? result = dialog.ShowDialog();
                if (result != true || string.IsNullOrWhiteSpace(dialog.TextValue))
                    throw new InvalidOperationException("A name is required to rename PDF export settings.");
                string newName = dialog.TextValue.Trim();
                using (var t = new Transaction(Document, "Rename PDF Export Settings"))
                {
                    t.Start();
                    // Rename Revit PDFExportSettings
                    SelectedPDFExportSettingItem.DbSetting.Name = newName;
                    SelectedPDFExportSettingItem.Name = newName;
                    
                    t.Commit();
                }
            }
        }
        private bool CanDeletePDFOption()
        {
            return SelectedPDFExportSettingItem != null && !SelectedPDFExportSettingItem.IsInSession;
        }

        private void ExecuteDeletePDFOption()
        {
            if (SelectedPDFExportSettingItem.IsInSession)
            {
                throw new InvalidOperationException("Cannot delete the in-session PDF export settings.");
            }
            else
            {
                using (var t = new Transaction(Document, "Delete PDF Export Settings"))
                {
                    t.Start();
                    // Delete Revit PDFExportSettings
                    Document.Delete(SelectedPDFExportSettingItem.DbSetting.Id);
                    // Update UI collection
                    PDFExportSettingsCollection.Remove(SelectedPDFExportSettingItem);
                    SelectedPDFExportSettingItem = PDFExportSettingsCollection.First();
                    t.Commit();
                }
            }
        }

        

        public void SetInSessionPDFSettings()
        {
            SelectedPDFExportSettingItem = PDFExportSettingsCollection.Where(a => a.Name == "<In Session>").First();
        }

        public void LoadSettings()
        {
            LoadPDFSettings();
        }
        private void LoadPDFSettings()
        {
            var names = ExportPDFSettings.ListNames(Document);

            // Create/reuse "In Session"
            PDFExportSettingsCollection.Add(new PDFExportOptionsItem("<In Session>"));            

            // Load all other user settings
            foreach (var name in names)
            {
                var item = new PDFExportOptionsItem(ExportPDFSettings.FindByName(Document, name));
                PDFExportSettingsCollection.Add(item);
            }
            SelectedPDFExportSettingItem = PDFExportSettingsCollection.Where(a=>a.Name=="<In Session>").First();
        }
        private void ApplyRevitPDFSettings(PDFExportOptions opt)
        {
            // Update ViewModel model properties
            IsCombinedChecked = opt.Combine;
            AlwaysUseRaster = opt.AlwaysUseRaster;
            HideCropBoundaries = opt.HideCropBoundaries;
            HideReferencePlanes = opt.HideReferencePlane;
            HideScopeBoxes = opt.HideScopeBoxes;
            HideUnreferencedViewTags = opt.HideUnreferencedViewTags;
            MaskCoincidentLines = opt.MaskCoincidentLines;
            ReplaceHalftoneLines = opt.ReplaceHalftoneWithThinLines;        
            ViewLinksInBlue = opt.ViewLinksInBlue;
            ZoomPercentage = opt.ZoomPercentage;
            
            // Update UI-selectable wrapper properties
            SelectedColorDepth = ColorDepthOptions.First(e => e.Value == opt.ColorDepth);
            SelectedExportQuality = ExportQualityOptions.First(e => e.Value == opt.ExportQuality);
            SelectedPaperFormatOption = PaperFormatOptions.First(e => e.Value == opt.PaperFormat);
            SelectedRasterQuality = RasterQualityOptions.First(e => e.Value == opt.RasterQuality);
            OriginOffsetX = UnitUtils.ConvertFromInternalUnits(opt.OriginOffsetX, UnitTypeId.Inches);
            OriginOffsetY = UnitUtils.ConvertFromInternalUnits(opt.OriginOffsetY, UnitTypeId.Inches);
            PaperOrientation = opt.PaperOrientation;
            PaperPlacement = opt.PaperPlacement;
            ZoomType = opt.ZoomType;
            FileName = opt.FileName;            
        }

        public void ApplyPrintSettings(PDFExportOptions setting)
        {
            PrintSettingsItem.PaperFormat = setting.PaperFormat;
            PrintSettingsItem.PaperOrientation = setting.PaperOrientation;
            PrintSettingsItem.OriginOffsetX = UnitUtils.ConvertFromInternalUnits(setting.OriginOffsetX, UnitTypeId.Inches);
            PrintSettingsItem.OriginOffsetY = UnitUtils.ConvertFromInternalUnits(setting.OriginOffsetY, UnitTypeId.Inches);
            PrintSettingsItem.ZoomPercentage = setting.ZoomPercentage;
            PrintSettingsItem.ColorDepth = setting.ColorDepth;
            //PrintSettingsItem.HiddenLineViewsType = setting.;
            PrintSettingsItem.PaperFormat = setting.PaperFormat;
            PrintSettingsItem.RasterQuality = setting.RasterQuality;
            //PrintSettingsItem.MarginType = setting.;
            PrintSettingsItem.PaperPlacement = setting.PaperPlacement;
            PrintSettingsItem.AlwaysUseRaster = setting.AlwaysUseRaster;
            PrintSettingsItem.ZoomType = setting.ZoomType;
            PrintSettingsItem.HideCropBoundaries = setting.HideCropBoundaries;
            PrintSettingsItem.HideReferencePlanes = setting.HideReferencePlane;
            PrintSettingsItem.HideScopeBoxes = setting.HideScopeBoxes;
            PrintSettingsItem.HideUnreferencedViewTags = setting.HideUnreferencedViewTags;
            PrintSettingsItem.MaskCoincidentLines = setting.MaskCoincidentLines;
            PrintSettingsItem.ReplaceHalftoneLines = setting.ReplaceHalftoneWithThinLines;
            PrintSettingsItem.ViewLinksInBlue = setting.ViewLinksInBlue;
            RevitExporterServices.ApplyPrintSettings(Document, PrintSettingsItem);
        }


        public void AddProfileSettings(List<PDFExportOptionsItem> pDFExportSettingsItems)
        {
            foreach (var sourceItem in pDFExportSettingsItems)
            {
                var names = ExportPDFSettings.ListNames(Document);
                if (names.Contains(sourceItem.Name))
                    continue;

                ExportPDFSettings newSetting;

                using (var t = new Transaction(Document))
                {
                    t.Start("Create PDF Export Setting");

                    newSetting = ExportPDFSettings.Create(
                        Document,
                        sourceItem.Name,
                        PdfSettingsStorage.ToRevitOptions(sourceItem)
                    );

                    t.Commit();
                }

                var newItem = new PDFExportOptionsItem(newSetting);
                PDFExportSettingsCollection.Add(newItem);
            }
        }
        public void ApplyPDFOptionsDTO(string profileName)
        {
            foreach(var pdfExportSettingsItem in PDFExportSettingsCollection)
            {
                if (pdfExportSettingsItem.Name == profileName)
                {
                    SelectedPDFExportSettingItem = pdfExportSettingsItem;
                    OnPropertyChanged(nameof(SelectedPDFExportSettingItem));
                }                
            }
        }
        #endregion
    }
}
