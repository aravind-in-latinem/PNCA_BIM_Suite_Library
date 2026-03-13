using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Services;
using System;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    public class SheetCreatorExcelInputCommand : IExternalCommand
    {
        private UserLogData _userLogData;
        private Document _document;
        public SheetCreatorExcelInputCommand()
        {
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
            _userLogData.AddinName = "SheetCreatorExcelInput";
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            _document = uidoc.Document;
            try
            {
                var win = new PNCA_BIM_Suite_Library.Views.SheetCreatorExcelInput(commandData);
                win.ShowDialog();
                _userLogData.Status = "Success";
                _userLogData.Message = "Sheets created successfully";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Task Failed. Error: {ex.Message}");
                // user long record creation on failure
                _userLogData.Status = "Fail";
                _userLogData.Message = "Sheet creation failed";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, _document);
                return Result.Failed;
            }
        }
    }
}

