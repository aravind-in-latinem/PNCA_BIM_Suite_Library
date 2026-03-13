using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PNCA_BIM_Suite_Library.Model
{
    public class PDFExportOptionsItem: ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public ExportPDFSettings DbSetting { get; }        
        public bool IsInSession { get; }
        public PDFExportOptionsItem()
        {
            PaperOrientation = PageOrientationType.Landscape;
        }
        public PDFExportOptionsItem(string name) 
        {
            Name = name;
            IsInSession = true;
        }

        public PDFExportOptionsItem(ExportPDFSettings setting) : this()
        {
            Name = setting?.Name;
            DbSetting = setting;
            IsInSession = false;            
        }
        
        
        public string FileName { get; set; }          
        public double OriginOffsetX { get; set; } 
        public double OriginOffsetY { get; set; } 
        public int ZoomPercentage { get; set; } 
        //ComboBoxes
        public ExportPaperFormat PaperFormat { get; set; } 
        public RasterQualityType RasterQuality { get; set; } 
        public ColorDepthType ColorDepth { get; set; } 
        public PDFExportQualityType ExportQuality { get; set; } 




        public string Version { get; set; } = "1.0";

        //radiobuttons
        public PaperPlacementType PaperPlacement { get; set; } 
        public bool AlwaysUseRaster { get; set; } 
        public ZoomType ZoomType { get; set; } 
        public PageOrientationType PaperOrientation { get; set; } 


        //checkboxes
        public bool Combine { get; set; }
        public bool HideCropBoundaries { get; set; }
        public bool HideReferencePlanes { get; set; }
        public bool HideScopeBoxes { get; set; }
        public bool HideUnreferencedViewTags { get; set; }
        public bool MaskCoincidentLines { get; set; }
        public bool ReplaceHalftoneLines { get; set; }

        public bool StopOnError = false; 
        public bool ViewLinksInBlue { get; set; } 
    }
}
