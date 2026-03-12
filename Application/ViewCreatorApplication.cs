using System;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Application
{
    public class ViewCreatorApplication : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "PNCA® BIM Suite";
            string panelNameSheet = "Views";

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
            panel.AddItem(buttonDataViewCreator);
            panel.AddItem(buttonDataViewDuplicator);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}