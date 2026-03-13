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
    public class ParameterItem

    {
        private readonly string _name;
        private readonly Parameter _parameter;
        public string Name 
        { 
            get=>_name; }
        public bool IsCustom { get; set; }
        public Parameter Parameter
        {
            get => _parameter;
        }
        public ParameterItem(string name)
        {
            _name = name;
            IsCustom = true;
        }
        public ParameterItem(Parameter parameter)
        {
            _name = parameter.Definition.Name;
            _parameter = parameter;
            IsCustom = false;
        }

    }
}
