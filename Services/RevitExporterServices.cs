using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;


namespace PNCA_BIM_Suite_Library.Services
{
    public class RevitExporterServices

    {
        public static void ApplyPrintSettings(Document Document, PrintSettingsItem printSettings)
        {
            PrintManager pm = Document.PrintManager;
            const string setupName = "PNCA Exporter Setup";
            PrintSetting tempPrintsetting = new FilteredElementCollector(Document)
                .OfClass(typeof(PrintSetting))
                .Cast<PrintSetting>()
                .FirstOrDefault(ps => ps.Name == setupName);

            using (Transaction t1 = new Transaction(Document, "Refresh Print Settings"))
            {
                t1.Start();

                // Get or create the print setting
                if (tempPrintsetting != null)
                {
                    pm.PrintSetup.CurrentPrintSetting = tempPrintsetting;
                    pm.Apply();
                    pm.PrintSetup.Delete();
                }
                t1.Commit();
            }

            using (Transaction t = new Transaction(Document, "Apply Print Settings"))
            {
                t.Start();

                // IMPORTANT: You need to apply the PrintSetting FIRST before modifying it
                // Switch to the PrintSetting we want to modify
                pm.PrintSetup.CurrentPrintSetting = pm.PrintSetup.InSession;
                pm.Apply();

                // Now get the PrintParameters from the CURRENT print setting
                PrintParameters pp = pm.PrintSetup.CurrentPrintSetting.PrintParameters;

                // Apply parameters
                pp.ColorDepth = printSettings.ColorDepth;
                pp.HiddenLineViews = printSettings.HiddenLineViewsType;
                pp.HideCropBoundaries = printSettings.HideCropBoundaries;
                pp.HideReforWorkPlanes = printSettings.HideReferencePlanes;
                pp.HideScopeBoxes = printSettings.HideScopeBoxes;
                pp.HideUnreferencedViewTags = printSettings.HideUnreferencedViewTags;
                pp.PaperPlacement = printSettings.PaperPlacement;
                pp.MaskCoincidentLines = printSettings.MaskCoincidentLines;
                pp.PageOrientation = printSettings.PaperOrientation;
                pp.ReplaceHalftoneWithThinLines = printSettings.ReplaceHalftoneLines;
                pp.ViewLinksinBlue = printSettings.ViewLinksInBlue;
                pp.ZoomType = printSettings.ZoomType;

                if (pp.ZoomType == ZoomType.Zoom)
                    pp.Zoom = printSettings.ZoomPercentage;

                if (pp.PaperPlacement == PaperPlacementType.Margins)
                {
                    pp.MarginType = printSettings.MarginType;                    
                }

                // Save the modified print setting
                try
                {
                    pm.PrintSetup.SaveAs(setupName);                    
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Error", ex.Message);
                    t.RollBack();
                    return;
                }

                t.Commit();
            }

        }
    }
}