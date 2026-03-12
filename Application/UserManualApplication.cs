using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Application
{
    public class UserManualApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "PNCA® BIM Suite";
            string panelNameSheet = "Knowledge Base";

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
            PushButtonData buttonDataUserManual = new PushButtonData("UserManual",
                "User Manual", assemblyPath, "PNCA_BIM_Suite_Library.CommandData.UserManualCommand");


            // Icon Path
            Uri uriUserManual = new Uri("pack://application:,,,/PNCA_BIM_Suite_Library;component/Resources/UserManual.png", UriKind.Absolute);
            
            
            // To add Large Image for Button
            BitmapImage iconUserManual = new BitmapImage(uriUserManual);
            buttonDataUserManual.LargeImage = iconUserManual;
            


            // Adding Button to the Tab
            panel.AddItem(buttonDataUserManual);
            



            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}