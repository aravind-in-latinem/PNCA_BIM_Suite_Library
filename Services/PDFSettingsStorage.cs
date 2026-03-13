using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Autodesk.Revit.DB;
using PNCA_BIM_Suite_Library.Model;
namespace PNCA_BIM_Suite_Library.Services
{
    public static class PdfSettingsStorage
    {
        public static void Save(string path, PDFExportOptionsItem settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        public static PDFExportOptionsItem Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<PDFExportOptionsItem>(json);
        }

        public static PDFExportOptions ToRevitOptions(PDFExportOptionsItem s)
        {
            var rasterQuality = RasterQualityType.Low;
            var exportQuality = PDFExportQualityType.DPI300;
            if (s.RasterQuality == 0)
                rasterQuality = RasterQualityType.Low;
            else
                rasterQuality = s.RasterQuality;
            if (s.ExportQuality == 0)
                exportQuality = PDFExportQualityType.DPI300;
            else
                exportQuality = s.ExportQuality;

            var opts = new PDFExportOptions
            {
                AlwaysUseRaster = s.AlwaysUseRaster,
                FileName = "DefaultFileName.pdf",
                Combine = s.Combine,
                HideCropBoundaries = s.HideCropBoundaries,
                HideReferencePlane = s.HideReferencePlanes,
                HideScopeBoxes = s.HideScopeBoxes,
                HideUnreferencedViewTags = s.HideUnreferencedViewTags,
                MaskCoincidentLines = s.MaskCoincidentLines,
                RasterQuality = rasterQuality,
                ReplaceHalftoneWithThinLines = s.ReplaceHalftoneLines,
                StopOnError = s.StopOnError,
                ViewLinksInBlue = s.ViewLinksInBlue,
                ZoomPercentage = s.ZoomPercentage
            };

            // Units conversion
            opts.OriginOffsetX = UnitUtils.ConvertToInternalUnits(s.OriginOffsetX, UnitTypeId.Millimeters);
            opts.OriginOffsetY = UnitUtils.ConvertToInternalUnits(s.OriginOffsetY, UnitTypeId.Millimeters);

            // Enums (safe mapping)
            opts.ExportQuality = exportQuality;
            opts.ZoomType = (s.ZoomType);
            opts.ColorDepth = (s.ColorDepth);
            opts.PaperFormat = (s.PaperFormat);
            opts.PaperOrientation = (s.PaperOrientation);
            opts.PaperPlacement = (s.PaperPlacement);

            return opts;
        }
        public static List<TableCellCombinedParameterData> GetDefaultSheetNamingRule()
        {
            var defaultsheetRule = new List<TableCellCombinedParameterData>();

            // Sheet Number
            var number = TableCellCombinedParameterData.Create();
            number.ParamId = new ElementId(BuiltInParameter.SHEET_NUMBER);
            number.Suffix = "_";
            defaultsheetRule.Add(number);

            // Sheet Name
            var name = TableCellCombinedParameterData.Create();
            name.ParamId = new ElementId(BuiltInParameter.SHEET_NAME);
            defaultsheetRule.Add(name);
            return defaultsheetRule;
        }

        public static List<TableCellCombinedParameterData> GetDefaultViewNamingRule()
        {
            var viewRule = new List<TableCellCombinedParameterData>();
            var viewName = TableCellCombinedParameterData.Create();
            viewName.ParamId = new ElementId(BuiltInParameter.VIEW_NAME);
            viewName.Separator = "-";

            var viewId = TableCellCombinedParameterData.Create();
            viewId.ParamId = new ElementId(BuiltInParameter.VIEW_TYPE_SCHEDULES);

            viewRule.Add(viewName);
            viewRule.Add(viewId);
            return viewRule;

        }

        public static bool IsValidFileNameSeparator(string separator)
        {
            if (string.IsNullOrEmpty(separator))
                return false;

            // Windows invalid filename characters
            char[] invalidChars = Path.GetInvalidFileNameChars();

            // Check if separator contains any invalid character
            if (separator.Any(c => invalidChars.Contains(c)))
                return false;

            // Cannot end with space or dot
            if (separator.EndsWith(" ") || separator.EndsWith("."))
                return false;

            return true;
        }

    }
}