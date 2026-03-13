using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Application
{
    public class SheetCreatorApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "PNCA® BIM Suite";
            string panelNameSheet = "Sheets";

            // Create Tab (ignore if it already exists)
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch { }

            // Check the Existance and Create Panel
            RibbonPanel panel = application.GetRibbonPanels(tabName).FirstOrDefault(p => p.Name == panelNameSheet);
            if (panel == null)
            {
                panel = application.CreateRibbonPanel(tabName, panelNameSheet);
            }

            // DLL Path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Button
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
            panel.AddItem(buttonDataSheetCreatorUser);
            panel.AddItem(buttonDataSheetCreatorExcel);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}