using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class SheetCreatorExcelInputCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            var win = new PNCA_BIM_Suite_Library.Views.SheetCreatorExcelInput(commandData);
            win.ShowDialog();
            return Result.Succeeded;
        }
    }
}

