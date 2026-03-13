using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Application
{
    public class PNCABIMSuiteApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "PNCA® BIM Suite";
            // Create Tab (ignore if it already exists)
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { }

            // DLL Path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            //----------------------Knowledge Base Panel----------------------

            string panelNameKnowledgeBase = "Knowledge Base";

            
            // Check the Existance and Create Panel
            RibbonPanel panelKnowledgeBase = application.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelNameKnowledgeBase);
            if (panelKnowledgeBase == null)
            {
                panelKnowledgeBase = application.CreateRibbonPanel(tabName, panelNameKnowledgeBase);
            }

            // Button
            PushButtonData buttonDataUserManual = new PushButtonData("UserManual",
                "User Manual", assemblyPath, "PNCA_BIM_Suite_Library.CommandData.UserManualCommand");


            // Icon Path
            Uri uriUserManual = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/UserManual.png", UriKind.Absolute);
            
            
            // To add Large Image for Button
            BitmapImage iconUserManual = new BitmapImage(uriUserManual);
            buttonDataUserManual.LargeImage = iconUserManual;
            


            // Adding Button to the Tab
            panelKnowledgeBase.AddItem(buttonDataUserManual);

            //----------------------View Creator Panel-------------------------
            
            string panelNameViews = "Views";

            // Check the Existance and Create Panel
            RibbonPanel panelViews = application.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelNameViews);
            if (panelViews == null)
            {
                panelViews = application.CreateRibbonPanel(tabName, panelNameViews);
            }

            // Button
            PushButtonData buttonDataViewCreator = new PushButtonData("ViewCreatorFromLevels",
                "Create Views \r\n from Levels", assemblyPath,
                "PNCA_BIM_Suite_Library.CommandData.CreateViewsFromLevelsCommand");
            PushButtonData buttonDataViewDuplicator = new PushButtonData("DuplicateViews",
                "Mass Duplicate\r\n Views", assemblyPath,
                "PNCA_BIM_Suite_Library.CommandData.DuplicateViewsCommand");


            // Icon Path
            Uri uriViewCreator = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/CreateViewsFromLevels-Light.ico", UriKind.Absolute);
            Uri uriViewDuplicator = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/DuplicateViews-Light.ico", UriKind.Absolute);


            // To add Large Image for Button
            BitmapImage iconViewCreator = new BitmapImage(uriViewCreator);
            buttonDataViewCreator.LargeImage = iconViewCreator;
            BitmapImage iconViewDuplicator = new BitmapImage(uriViewDuplicator);
            buttonDataViewDuplicator.LargeImage = iconViewDuplicator;


            // Adding Button to the Tab
            panelViews.AddItem(buttonDataViewCreator);
            panelViews.AddItem(buttonDataViewDuplicator);

            //----------------------Sheet Creator Panel-------------------------
            string panelNameSheet = "Sheets";

            RibbonPanel panelSheet = application.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelNameSheet);
            if (panelSheet == null)
            {
                panelSheet = application.CreateRibbonPanel(tabName, panelNameSheet);
            }

            PushButtonData buttonDataSheetCreatorUser = new PushButtonData("SheetCreatorUserInput",
                "Create Sheets \r\n User Input", assemblyPath,
                "PNCA_BIM_Suite_Library.CommandData.SheetCreatorUserInputCommand");
            PushButtonData buttonDataSheetCreatorExcel = new PushButtonData("SheetCreatorExcelInput",
                "Create Sheets \r\n Excel Input", assemblyPath,
                "PNCA_BIM_Suite_Library.CommandData.SheetCreatorExcelInputCommand");


            // Icon Path
            Uri uriSheetCreatorUser = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/SheetCreatorUserInput-Light.ico", UriKind.Absolute);
            Uri uriSheetCreatorExcel = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/SheetCreatorExcelInput-Light.ico", UriKind.Absolute);


            // To add Large Image for Button
            BitmapImage iconSheetCreatorUser = new BitmapImage(uriSheetCreatorUser);
            buttonDataSheetCreatorUser.LargeImage = iconSheetCreatorUser;
            BitmapImage iconSheetCreatorExcel = new BitmapImage(uriSheetCreatorExcel);
            buttonDataSheetCreatorExcel.LargeImage = iconSheetCreatorExcel;


            // Adding Button to the Tab
            panelSheet.AddItem(buttonDataSheetCreatorUser);
            panelSheet.AddItem(buttonDataSheetCreatorExcel);


            //----------------------Schedule Link Panel-------------------------

            string panelNameSchedule = "Schedules & Take-offs";

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