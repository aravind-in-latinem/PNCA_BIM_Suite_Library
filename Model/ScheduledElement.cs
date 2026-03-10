using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    public class ScheduledElement

    {
        public ElementId RowElementId { get; set; }
        public List<ScheduledField> ScheduledFields { get; set; } = new List<ScheduledField>();

    }

}
