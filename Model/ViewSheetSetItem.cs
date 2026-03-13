using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PNCA_BIM_Suite_Library.Model
{
    internal class ViewSheetSetItem:ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }        
        public bool IsInSession { get; }

        public IViewSheetSet ViewSheetSet { get; set; }
        public ViewSheetSetItem(string name)
        {
            Name = name;
            IsInSession = true;
        }
        public ViewSheetSetItem(ViewSheetSet viewSheetSet)
        {
            Name = viewSheetSet.Name;
            IsInSession = false;
            ViewSheetSet=viewSheetSet;
        }
    }
}
