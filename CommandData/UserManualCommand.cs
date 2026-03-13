using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.CommandData
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class UserManualCommand : IExternalCommand

    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://sobha-my.sharepoint.com/:p:/g/personal/aravind_krishnan_latinem_in/IQAXNyUkqMnJR7YSMcaP78eBAVKcbwj4Z55Mrzw8H600Oac?e=1qFqTX",
                UseShellExecute = true
            });

            return Result.Succeeded;
        }
    }
}
