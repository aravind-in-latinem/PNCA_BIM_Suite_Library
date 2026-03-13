using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using ADSK = Autodesk.Revit.DB;

namespace PNCA_BIM_Suite_Library.Model
{
    public class ViewItem : ObservableObject, IViewSheetItem
    {
        private bool _isSelected;
        public Action ViewSelectionChanged { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    // Notify parent VM (if needed)
                    ViewSelectionChanged?.Invoke();
                }
            }
        }
        private ADSK.View _viewElement;
        public ADSK.View ViewElement
        {
            get => _viewElement;
            set => SetProperty(ref _viewElement, value);
        }
        public ElementId ItemElementId { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string DisplayName
        {
            get => $"{Name} - {ViewType}";
        }

        private string _viewType;
        public string ViewType
        {
            get => _viewType;
            set => SetProperty(ref _viewType, value);
        }

        private string _viewScale;
        public string ViewScale
        {
            get => _viewScale;
            set => SetProperty(ref _viewScale, value);
        }

        private string _detailLevel;
        public string DetailLevel
        {
            get => _detailLevel;
            set => SetProperty(ref _detailLevel, value);
        }

        private string _discipline;
        public string Discipline
        {
            get => _discipline;
            set => SetProperty(ref _discipline, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
        
    }
}
