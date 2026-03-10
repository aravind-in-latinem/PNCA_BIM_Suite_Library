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
    public class LookupField

    {
        public string FieldName { get; set; } = string.Empty;        
        public string ParameterType { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string ForgeTypeId { get; set; } = string.Empty;
        public Dictionary<string, long> ElementElementIdPairs { get; set; } = new Dictionary<string, long>();        
        public int FieldIndex { get; set; }
        public Parameter ParameterElement { get; set; }
        public ElementId SelectedElementId { get; set; }

    }
}
