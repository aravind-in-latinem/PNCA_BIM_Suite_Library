using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    public class SettingsProfile
    {
        public string ProfileName { get; set; }
        public string PDFExportSettingItemName => $"Profile Sett. - {ProfileName}";
        public bool IsInSession { get; set; } = false;
        public string Version { get; set; } = "1.0";        

        public PDFOptionsDto PdfOptions { get; set; }
        public FileNamingDto FileNamingParameters { get; set; }
        public PrintOrderDto PrintOrder { get; set; }
    }
    public class PDFOptionsDto
    {
        public string FileName { get; set; }
        public double OriginOffsetX { get; set; }
        public double OriginOffsetY { get; set; }
        public int ZoomPercentage { get; set; }

        public ExportPaperFormat PaperFormat { get; set; }
        public RasterQualityType RasterQuality { get; set; }
        public ColorDepthType ColorDepth { get; set; }
        public PDFExportQualityType ExportQuality { get; set; }

        public PaperPlacementType PaperPlacement { get; set; }
        public bool AlwaysUseRaster { get; set; }
        public ZoomType ZoomType { get; set; }
        public PageOrientationType PaperOrientation { get; set; }

        public bool Combine { get; set; }
        public bool HideCropBoundaries { get; set; }
        public bool HideReferencePlanes { get; set; }
        public bool HideScopeBoxes { get; set; }
        public bool HideUnreferencedViewTags { get; set; }
        public bool MaskCoincidentLines { get; set; }
        public bool ReplaceHalftoneLines { get; set; }
        public bool ViewLinksInBlue { get; set; }
    }
    public class FileNamingDto
    {
        public string SheetNamingSeparator { get; set; }
        public string ViewNamingSeparator { get; set; }

        public List<NamingTokenDto> Tokens { get; set; }
    }

    public class NamingTokenDto
    {
        public string Name { get; set; }        // Sheet Number, Sheet Name, AX, etc.
        public bool IsCustom { get; set; }
        public bool IsSheet { get; set; }
        public long? BuiltInParameterId { get; set; } // nullable
    }
    public class PrintOrderDto
    {
        public List<long> OrderedElementIds { get; set; } = new List<long>();
    }


}
