using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class SheetCreatorUserInputCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Show the User Input Window
            var win = new PNCA_BIM_Suite_Library.Views.SheetCreatorUserInput(commandData);
            win.ShowDialog();
            return Result.Succeeded;
        }


        
    }
}
