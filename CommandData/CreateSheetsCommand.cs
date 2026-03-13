using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;



namespace ViewsCreation
{
    [Transaction(TransactionMode.Manual)]
    public class CreateSheetsCommand : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            CreateSheetsWindow createSheetsWindow = new CreateSheetsWindow(doc);
            bool? result = createSheetsWindow.ShowDialog();

            if (result == true)
            {
                // The user clicked OK, so we can proceed with the view creation
                return Result.Succeeded;
            }

            return Result.Cancelled;

        }
    }
}
