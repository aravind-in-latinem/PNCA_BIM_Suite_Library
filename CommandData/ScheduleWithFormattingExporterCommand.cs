using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Services;
using PNCA_BIM_Suite_Library.Views;
using PNCA_BIM_Suite_Library.ViewModel;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ScheduleWithFormattingExporterCommand : IExternalCommand

    {
        private Document _document;
        private ILogger _logger;
        private UserLogData _userLogData;
        public ScheduleWithFormattingExporterCommand()
        {
            _logger = new ProgressLoggerViewModel();
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
                _userLogData.AddinName = "ScheduleWithFormattingExporter";
                var uiApplication = commandData.Application;
                var application = uiApplication.Application;
                var uiDocument = uiApplication.ActiveUIDocument;
                _document = uiDocument.Document;


                var sheetLinkWithFormatting = new ScheduleWithFormattingExporterView(_document, uiDocument, _logger);

                // Set owner to Revit window so it stays on top and modal
                System.Windows.Interop.WindowInteropHelper helper =
                    new System.Windows.Interop.WindowInteropHelper(sheetLinkWithFormatting);
                helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                sheetLinkWithFormatting.ShowDialog();
                _userLogData.Status = "Success";
                _userLogData.Message = "Schedule exported successfully";
                _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
                UserLogRecorder.SendLog(_userLogData, _document);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", $"Task Failed. Error: {ex.Message}");
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
