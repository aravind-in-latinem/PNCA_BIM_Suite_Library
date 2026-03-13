using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.Views.PNCAExporter;


namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class OpenExportSheetDialogCommand : IExternalCommand

    {
        private UserLogData _userLogData;
        private Document _document;

        public OpenExportSheetDialogCommand()
        {
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
                _userLogData.AddinName = "ScheduleWithElementIdExporter";
                var uiApplication = commandData.Application;
                var application = uiApplication.Application;
                var uiDocument = uiApplication.ActiveUIDocument;
                _document = uiDocument.Document;
                int year = int.Parse(application.VersionNumber);

                Directory.CreateDirectory(
                    RevitAddinPaths.GetProfilesFolder(year));


                var mainViewWindow = new MainView(_document, uiDocument);
                RevitWindowHelper.SetRevitOwner(mainViewWindow, uiApplication);
                mainViewWindow.ShowDialog();

                // user long record creation on success
                _userLogData.Status = "Success";
                _userLogData.Message = "Schedule exported successfully";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, _document);

                return Result.Succeeded;

            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Failed to save schedule. Error: {ex.Message}");
                // user long record creation on failure
                _userLogData.Status = "Fail";
                _userLogData.Message = "Schedule export failed";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, _document);
                return Result.Failed;
            }
        }
    }
}
