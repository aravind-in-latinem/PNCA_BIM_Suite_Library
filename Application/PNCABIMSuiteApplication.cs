using System;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Application
{
    public class PNCABIMSuiteApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "PNCA® BIM Suite";

            // try creating tab
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch{}

            // Assembly path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // creating panel 
            RibbonPanel panelPNCA = GetOrCreatePanel(application, tabName, "PNCA");
            RibbonPanel panelViews = GetOrCreatePanel(application, tabName, "Views");
            RibbonPanel panelSheets = GetOrCreatePanel(application, tabName, "Sheets");
            RibbonPanel panelSchedules = GetOrCreatePanel(application, tabName, "Schedules");

            // create buttons 
            PushButtonData userManual = CreateButton(panelPNCA, "UserManual", "User Manual", assemblyPath, "PNCA_BIM_Suite_Library.CommandData.UserManualCommand", "UserManual16.png");
            PushButtonData lmsPortal = CreateButton(panelPNCA, "LMSPortal", "LMS Portal", assemblyPath, "PNCA_BIM_Suite_Library.CommandData.LMSPortalCommand", "UserManual16.png");
            PushButtonData ViewCreatorFromLevels = CreateButton(panelViews,"ViewCreatorFromLevels","Create Views \r\n from Levels",assemblyPath, "PNCA_BIM_Suite_Library.CommandData.CreateViewsFromLevelsCommand", "CreateViewsFromLevels-Light.ico");
            PushButtonData DuplicateViews = CreateButton(panelViews,"DuplicateViews","Mass Duplicate \r\n Views",assemblyPath,"PNCA_BIM_Suite_Library.CommandData.DuplicateViewsCommand", "DuplicateViews-Light.ico");
            PushButtonData SheetCreatorUserInput = CreateButton(panelSheets,"SheetCreatorUserInput","Create Sheets \r\n User Input",assemblyPath,"PNCA_BIM_Suite_Library.CommandData.SheetCreatorUserInputCommand", "SheetCreatorUserInput-Light.ico");
            PushButtonData SheetCreatorExcelInput = CreateButton(panelSheets,"SheetCreatorExcelInput","Create Sheets \r\n Excel Input",assemblyPath,"PNCA_BIM_Suite_Library.CommandData.SheetCreatorExcelInputCommand", "SheetCreatorExcelInput-Light.ico");
            PushButtonData ScheduleExportWEId = CreateButton(panelSchedules,"ScheduleExportWEId", "Export Excel \r\n With Elem-ID",assemblyPath, "PNCA_BIM_Suite_Library.CommandData.ScheduleWithElementIdExporterCommand", "ScheduleExportwEID-Light.ico");
            PushButtonData ScheduleExportWFormat = CreateButton(panelSchedules, "ScheduleExportWFormat","Export Excel \r\n With Formatting",assemblyPath,"PNCA_BIM_Suite_Library.CommandData.ScheduleWithFormattingExporterCommand", "ScheduleExportwFormatting-Light.ico");
            PushButtonData ScheduleImport = CreateButton(panelSchedules, "ScheduleImport","Import Schedule", assemblyPath, "PNCA_BIM_Suite_Library.CommandData.ImportDataFromExcelCommand", "ScheduleImport-Light.ico");

            // stack buttons
            //panelPNCA.AddStackedItems(userManual, lmsPortal);

            return Result.Succeeded;
        }


        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        // Create panel on tab 
        private RibbonPanel GetOrCreatePanel(UIControlledApplication app, string tabName, string panelName)
        {
            RibbonPanel panel = app.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelName);
            if(panel == null)
            {
                panel = app.CreateRibbonPanel(tabName, panelName);
            }
            return panel;
        }

        private PushButtonData CreateButton(RibbonPanel panel, string buttonName, string buttonText, string assemblyPath, string commandClass, string iconName)
        {
            PushButtonData button = new PushButtonData(buttonName, buttonText, assemblyPath, commandClass);
            button.LargeImage = GetIcon(iconName);
            panel.AddItem(button);

            return button;
        }

        private BitmapImage GetIcon(string iconName)
        {
            Uri uri = new Uri($"pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/{iconName}", UriKind.Absolute);
            return new BitmapImage(uri);
        }
    }
}