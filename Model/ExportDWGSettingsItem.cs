using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Model
{
    public class ExportDWGSettingsItem
    {
        public ExportDWGSettingsItem(ExportDWGSettings exportDWGSettings)
        {
            ExportDWGSettings = exportDWGSettings;
            Name = exportDWGSettings.Name;
            IsInSession = false;
        }
        public ExportDWGSettingsItem(string name)
        {
            Name = name;
            IsInSession = true;
        }
        public String Name { get; set; }
        public bool IsInSession { get; set; }
        public ExportDWGSettings ExportDWGSettings { get; set; }

    }
}