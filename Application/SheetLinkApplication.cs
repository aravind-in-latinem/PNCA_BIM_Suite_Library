using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Application
{
    public class SheetLinkApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "PNCA® BIM Suite";
            string panelNameSchedule = "Schedules & Take-offs";

            // Create Tab (ignore if it already exists)
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { }

            // Check the Existance and Create Panel
            RibbonPanel panelSchedule = application.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelNameSchedule);
            if (panelSchedule == null)
            {
                panelSchedule = application.CreateRibbonPanel(tabName, panelNameSchedule);
            }

            

            // Button
            PushButtonData buttonDataScheduleExportWEId = new PushButtonData("ScheduleExportWEId",
                "Export Excel \r\n With Elem-ID", assemblyPath,
                "PNCA_BIM_Suite_Library.CommandData.ScheduleWithElementIdExporterCommand");
            PushButtonData buttonDataScheduleExportWFormat = new PushButtonData("ScheduleExportWFormat",
                "Export Excel \r\n With Formatting", assemblyPath,
                "PNCA_BIM_Suite_Library.CommandData.ScheduleWithFormattingExporterCommand");
            PushButtonData buttonDataScheduleImport = new PushButtonData("ScheduleImport", "Import Schedule",
                assemblyPath, "PNCA_BIM_Suite_Library.CommandData.ImportDataFromExcelCommand");

            // Icon Path
            Uri uriScheduleExportWEId = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/ScheduleExportwEID-Light.ico", UriKind.Absolute);
            Uri uriScheduleExportWFormat = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/ScheduleExportwFormatting-Light.ico", UriKind.Absolute);
            Uri uriScheduleImport = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/ScheduleImport-Light.ico", UriKind.Absolute);
            
            // To add Large Image for Button
            BitmapImage iconScheduleExportWEId = new BitmapImage(uriScheduleExportWEId);
            buttonDataScheduleExportWEId.LargeImage = iconScheduleExportWEId;
            BitmapImage iconScheduleExportWFormat = new BitmapImage(uriScheduleExportWFormat);
            buttonDataScheduleExportWFormat.LargeImage = iconScheduleExportWFormat;
            BitmapImage iconScheduleImport = new BitmapImage(uriScheduleImport);
            buttonDataScheduleImport.LargeImage = iconScheduleImport;


            // Adding Button to the Tab
            panelSchedule.AddItem(buttonDataScheduleExportWEId);
            panelSchedule.AddItem(buttonDataScheduleExportWFormat);
            panelSchedule.AddItem(buttonDataScheduleImport);




            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}