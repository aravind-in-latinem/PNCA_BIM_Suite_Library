using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using System;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class SheetCreatorUserInputCommand : IExternalCommand
    {
        private UserLogData _userLogData;
        private Document _document;

        public SheetCreatorUserInputCommand()
        {
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
            _userLogData.AddinName = "SheetCreatorUserInput";
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            _document = uidoc.Document;

            // Show the User Input Window
            try
            {
                var win = new PNCA_BIM_Suite_Library.Views.SheetCreatorUserInput(commandData);
                win.ShowDialog();
                _userLogData.Status = "Success";
                _userLogData.Message = "Sheets Created successfully";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, _document);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Task Failed. Error: {ex.Message}");
                // user long record creation on failure
                _userLogData.Status = "Fail";
                _userLogData.Message = "Sheets Creation failed";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, _document);
                return Result.Failed;
            }
        }


        
    }
}
