using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    public class ExportDWFSettingsItem
    {
        public ExportDWFSettingsItem(ExportDWGSettings exportDWGSettings)
        {
            ExportDWGSettings = exportDWGSettings;
            Name = exportDWGSettings.Name;
            IsInSession = false;
        }
        public ExportDWFSettingsItem(string name)
        {
            Name = name;
            IsInSession = true;
        }
        public String Name { get; set; }
        public bool IsInSession { get; set; }
        public ExportDWGSettings ExportDWGSettings { get; set; }
    }
}
