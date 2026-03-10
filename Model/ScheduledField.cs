using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    public class ScheduledField

    {
        public  string FieldName { get; set; } = string.Empty;
        public string FieldValue { get; set; } = string.Empty;
        public string ParameterType { get; set; } = string.Empty;
        public string UnitType { get; set; } = string.Empty;
        public string ForgeTypeId { get; set; } = string.Empty;
        public Dictionary<string,int> ElementElementIdPairs { get; set; } = new Dictionary<string, int>();
        public int FieldIndex { get; set; }
        public Parameter ParameterElement { get; set; }
        public ElementId SelectedElementId { get; set; }
    }
}
