using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Views
{
    public class ViewTypeWrapper
    {
        public string Name { get; set; }
        public ViewFamilyType ViewFamilyType { get; set; }
        public string ViewType { get; set; }
    }
}
