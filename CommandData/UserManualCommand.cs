using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UserManualCommand : IExternalCommand
        
    {
        private UserLogData _userLogData;

        public UserManualCommand()
        {
            _userLogData = new UserLogData();
        }
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _userLogData.StartTime = DateTime.Now.ToString("HH:mm:ss");
            _userLogData.AddinName = "User Manual";
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
             var document = uidoc.Document;

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://sobha-my.sharepoint.com/:p:/g/personal/aravind_krishnan_latinem_in/IQAXNyUkqMnJR7YSMcaP78eBAVKcbwj4Z55Mrzw8H600Oac?e=1qFqTX",
                UseShellExecute = true
            });
            _userLogData.Status = "Success";
            _userLogData.Message = "User Manual was referred";
            _userLogData.StopTime = DateTime.Now.ToString("HH:mm:ss");
            UserLogRecorder.SendLog(_userLogData, document);
            return Result.Succeeded;
        }
    }
}
