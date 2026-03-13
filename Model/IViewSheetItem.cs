using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    public interface IViewSheetItem
    {
        string Name { get; set; }
        string DisplayName { get; }
        bool IsSelected { get; set; }
        ElementId ItemElementId { get; set; }
    }
}
