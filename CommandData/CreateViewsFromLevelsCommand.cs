using Autodesk.Revit.DB;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Views;
using PNCA_BIM_Suite_Library.Model;
using System;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.ViewModel;


namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class CreateViewsFromLevelsCommand : IExternalCommand
    {
        private UserLogData _userLogData;

        public CreateViewsFromLevelsCommand()
        {
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
            _userLogData.AddinName = "CreateViewsFromLevels";
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            try
            {
                CreateViewsFromLevelsWindow createViewsFromLevelsWindow = new CreateViewsFromLevelsWindow(doc);
                bool? result = createViewsFromLevelsWindow.ShowDialog();

                _userLogData.Status = "Success";
                _userLogData.Message = "Views Created successfully";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, doc);
                return Result.Succeeded;

            }
            catch (Exception e)
            {
                _userLogData.Status = "Fail";
                _userLogData.Message = "View Creation failed";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, doc);
                return Result.Cancelled;
            }
            
            
            
        }
    }
}
