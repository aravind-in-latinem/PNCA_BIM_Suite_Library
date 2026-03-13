using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.Views;
using System;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class DuplicateViewsCommand : IExternalCommand
    {
        private UserLogData _userLogData;

        public DuplicateViewsCommand()
        {
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
            _userLogData.AddinName = "DuplicateViews";
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                DuplicateViewsWindow duplicateViewsWindow = new DuplicateViewsWindow(doc);
                bool? result = duplicateViewsWindow.ShowDialog();
                
                _userLogData.Status = "Success";
                _userLogData.Message = "Views Duplicated successfully";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, doc);
                return Result.Succeeded;
            }

            catch (Exception e)
            {
                // user long record creation on failure
                _userLogData.Status = "Fail";
                _userLogData.Message = "View Duplication failed";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, doc);
                return Result.Cancelled;
            }
            
            

        }



    }
}

