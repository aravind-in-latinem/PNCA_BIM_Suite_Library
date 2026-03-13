using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.ViewModel.PNCAExporter;


namespace PNCA_BIM_Suite_Library.Model
{
    public class DWFSettingsViewModel : ObservableObject
    {
        private string _name;
        private bool _cropBoxVisible;
        private bool _exportingAreas;
        private bool _exportObjectData;
        private bool _exportOnlyViewId;
        private bool _exportTexture;
        private EnumItem<DWFImageFormat> _selectedDWFImageFormat;
        private EnumItem<DWFImageQuality> _selectedDWFImageQuality;
        private bool _viewsAsExternalReferences;
        private EnumItem<ExportPaperFormat> _selectedPaperFormatOption;
        private EnumItem<RasterQualityType> _selectedRasterQuality;
        private EnumItem<ColorDepthType> _selectedColorDepth;
        private bool _portraitLayout;
        private PrintSettingsItem _currentPrintSettingItem;
        private ObservableCollection<PrintSettingsItem> _printSettingsCollection;
        private PrintSettingsItem _selectedPrintSettingItem;


        // ENUM OPTIONS FOR UI
        public EnumItem<ColorDepthType>[] ColorDepthOptions { get; }


        public EnumItem<ExportPaperFormat>[] PaperFormatOptions { get; }

        public EnumItem<PageOrientationType>[] PaperOrientationOptions { get; }

        public EnumItem<PaperPlacementType>[] PaperPlacementOptions { get; }


        public EnumItem<RasterQualityType>[] RasterQualityOptions { get; }


        public EnumItem<ZoomType>[] ZoomTypeOptions { get; }

        public EnumItem<HiddenLineViewsType>[] HiddenLineViewTypeOptions { get; }

        public EnumItem<DWFImageQuality>[] DWFImageQualityOptions { get; }

        public EnumItem<DWFImageFormat>[] DWFImageFormatOptions { get; }


        public DWFSettingsViewModel()
        {
            ColorDepthOptions = RevitCollectorServices.GetEnumItems<ColorDepthType>();
            PaperFormatOptions = RevitCollectorServices.GetEnumItems<ExportPaperFormat>();
            PaperOrientationOptions = RevitCollectorServices.GetEnumItems<PageOrientationType>();
            PaperPlacementOptions = RevitCollectorServices.GetEnumItems<PaperPlacementType>();
            RasterQualityOptions = RevitCollectorServices.GetEnumItems<RasterQualityType>();
            DWFImageQualityOptions = RevitCollectorServices.GetEnumItems<DWFImageQuality>();
            DWFImageFormatOptions = RevitCollectorServices.GetEnumItems<DWFImageFormat>();

        }
        
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        
        public PrintSettingsItem CurrentPrintSettingItem 
        {
            get;set;
        }
        
        public ObservableCollection<PrintSettingsItem> PrintSettingsCollection
        {
            get => _printSettingsCollection;
            set => SetProperty(ref _printSettingsCollection, value);
        }
        //public PrintSettingsItem SelectedPrintSettingItem
        //{
        //    get => _selectedPrintSettingItem;
        //    set
        //    {
        //        if (SetProperty(ref _selectedPrintSettingItem, value))
        //        {
        //            if (value != null && value.Name != "<In Session>")
        //            {
        //                ApplyPrintSettings(value);
        //            }
        //        }
        //    }
        //}
        public bool IsInSession { get; }

        public Document Document { get; set; }

        public bool ExportingAreas
        {
            get => _exportingAreas;
            set => SetProperty(ref _exportingAreas, value);
        }

        public bool ExportObjectData
        {
            get => _exportObjectData;
            set => SetProperty(ref _exportObjectData, value);
        }

        public bool ExportOnlyViewId
        {
            get => _exportOnlyViewId;
            set => SetProperty(ref _exportOnlyViewId, value);
        }

        public bool ExportTexture
        {
            get => _exportTexture;
            set => SetProperty(ref _exportTexture, value);
        }

        private bool _hideCropBoundaries;
        public bool HideCropBoundaries
        {
            get => _hideCropBoundaries;
            set
            {
                if (SetProperty(ref _hideCropBoundaries, value))
                {
                    CurrentPrintSettingItem.HideCropBoundaries = value;

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
                    CurrentPrintSettingItem.HideReferencePlanes = value;

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
                    CurrentPrintSettingItem.HideScopeBoxes = value;

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
                    CurrentPrintSettingItem.HideUnreferencedViewTags = value;

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
                    CurrentPrintSettingItem.MaskCoincidentLines = value;

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
                    CurrentPrintSettingItem.ReplaceHalftoneLines = value;

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
                    CurrentPrintSettingItem.ViewLinksInBlue = value;

                }
            }
        }

        public bool IsVectorProcessing
        {
            get => !CurrentPrintSettingItem.AlwaysUseRaster;
            set
            {
                if (value)
                    CurrentPrintSettingItem.AlwaysUseRaster = false;

                OnPropertyChanged(nameof(IsVectorProcessing));
                OnPropertyChanged(nameof(IsRasterProcessing));

            }
        }

        public bool IsRasterProcessing
        {
            get => CurrentPrintSettingItem.AlwaysUseRaster;
            set
            {
                if (value)
                    CurrentPrintSettingItem.AlwaysUseRaster = true;

                OnPropertyChanged(nameof(IsVectorProcessing));
                OnPropertyChanged(nameof(IsRasterProcessing));

            }
        }
        
        public EnumItem<DWFImageFormat> SelectedDWFImageFormat
        {
            get => _selectedDWFImageFormat;
            set => SetProperty(ref _selectedDWFImageFormat, value);
        }


        public EnumItem<DWFImageQuality> SelectedDWFImageQuality
        {
            get => _selectedDWFImageQuality;
            set => SetProperty(ref _selectedDWFImageQuality, value);
        }

        public bool ViewsAsExternalReferences
        {
            get => _viewsAsExternalReferences;
            set => SetProperty(ref _viewsAsExternalReferences, value);
        }

        public EnumItem<ExportPaperFormat> SelectedPaperFormatOption
        {
            get => _selectedPaperFormatOption;
            set => SetProperty(ref _selectedPaperFormatOption, value);
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
                    CurrentPrintSettingItem.ZoomPercentage = value;
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
                    CurrentPrintSettingItem.ZoomType = value;
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
                    CurrentPrintSettingItem.PaperOrientation = value;

                }
            }
        }
        public EnumItem<RasterQualityType> SelectedRasterQuality
        {
            get => _selectedRasterQuality;
            set
            {
                SetProperty(ref _selectedRasterQuality, value);
                if (value != null)
                    CurrentPrintSettingItem.RasterQuality = value.Value;
            }
        }
        
        public EnumItem<ColorDepthType> SelectedColorDepth
        {
            get => _selectedColorDepth;
            set
            {
                SetProperty(ref _selectedColorDepth, value);
                CurrentPrintSettingItem.ColorDepth = value.Value;
            }
        }
        public bool IsCustomOffset => PaperPlacement != PaperPlacementType.Center;
        
        private PaperPlacementType _paperPlacement;
        public PaperPlacementType PaperPlacement
        {
            get => _paperPlacement;
            set
            {
                if (SetProperty(ref _paperPlacement, value))
                {
                    CurrentPrintSettingItem.PaperPlacement = value;
                    OnPropertyChanged(nameof(IsCustomOffset));
                }
            }
        }
        

        private double _originOffsetX;
        public double OriginOffsetX
        {
            get => _originOffsetX;
            set
            {
                if (SetProperty(ref _originOffsetX, value))
                {
                    CurrentPrintSettingItem.OriginOffsetX = value;

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
                    CurrentPrintSettingItem.OriginOffsetY = value;

                }
            }
        }

        

        public bool PortraitLayout
        {
            get => _portraitLayout;
            set => SetProperty(ref _portraitLayout, value);
        }

        public void ApplyPrintSettings(PrintSettingsItem printSettings)
        {
            RevitExporterServices.ApplyPrintSettings(Document, printSettings);
        }

        PrintSetup SetPrintSetupOptions()
        {
            var printManager = Document.PrintManager;
            var printSetup = printManager.PrintSetup;
            var printParameters = printSetup.CurrentPrintSetting.PrintParameters;

            // PaperPlacement & Zoom
            printParameters.PaperPlacement = PaperPlacementType.Center;
            printParameters.ZoomType = ZoomType.Zoom;
            printParameters.Zoom = 100;

            // Orientation, HiddenLineViews, Appearance
            printParameters.PageOrientation = PageOrientationType.Portrait;
            printParameters.HiddenLineViews = HiddenLineViewsType.VectorProcessing;

            // Appearance
            printParameters.RasterQuality = RasterQualityType.High;
            printParameters.ColorDepth = ColorDepthType.Color;

            // Options
            printParameters.ViewLinksinBlue = true;
            printParameters.HideCropBoundaries = true;
            printParameters.HideScopeBoxes = true;
            printParameters.HideReforWorkPlanes = true;
            printParameters.HideUnreferencedViewTags = true;
            printParameters.MaskCoincidentLines = false;
            printParameters.ReplaceHalftoneWithThinLines = false;            
            printSetup.Save();
            return printSetup;
        }
        public void LoadDWFExportSettings()
        { 
            CurrentPrintSettingItem = new PrintSettingsItem();            
            SelectedColorDepth = ColorDepthOptions.FirstOrDefault(c => c.Value == ColorDepthType.Color);
            SelectedPaperFormatOption = PaperFormatOptions.First(e => e.Value == ExportPaperFormat.ISO_A1);
            SelectedRasterQuality = RasterQualityOptions.First(e => e.Value == RasterQualityType.High);
            SelectedDWFImageFormat = DWFImageFormatOptions.First(e => e.Value == DWFImageFormat.Lossless);
            SelectedDWFImageQuality = DWFImageQualityOptions.First(e => e.Value == DWFImageQuality.High);            
        }
    }
}

