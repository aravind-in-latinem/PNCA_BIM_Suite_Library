using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Views;


namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class CreateViewsFromLevelsCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            CreateViewsWindow createViewsWindow = new CreateViewsWindow(doc);
            bool? result = createViewsWindow.ShowDialog();

            if (result == true)
            {
                // The user clicked OK, so we can proceed with the view creation
                return Result.Succeeded;
            }

            return Result.Cancelled;

        }
    }
}
