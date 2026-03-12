using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.Views;
using PNCA_BIM_Suite_Library.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ScheduleWithElementIdExporterCommand : IExternalCommand

    {
        private ILogger _logger;

        private Document _document;
        private string _status;
        private UserLogData _userLogData;

        public ScheduleWithElementIdExporterCommand()
        {
            _logger = new ProgressLoggerViewModel();
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
                
                //var scheduleDataFromElements = new ScheduleDataFromElementsExtractor();


                // Create and show your window
                var mainWindow = new SheetLinkExport(_document, uiDocument, _logger);

                // Set owner to Revit window so it stays on top and modal
                System.Windows.Interop.WindowInteropHelper helper = new System.Windows.Interop.WindowInteropHelper(mainWindow);
                helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                mainWindow.ShowDialog();


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
                UserLogRecorder.SendLog(_userLogData, _document);
                return Result.Failed;
            }
            
        }
    }
}
