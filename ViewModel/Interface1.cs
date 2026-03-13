using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using PNCA_SheetExporter.Model;

namespace PNCA_SheetExporter.ViewModel
{
    public interface IPdfExportSettingsConsumer
    {
        PdfExportSettingsItem SelectedPDFExportSetting { get; set; }
    }
}
