using System;
using ADSK = Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using Autodesk.Revit.DB;
namespace PNCA_BIM_Suite_Library.Model
{
    public partial class SheetItem : ObservableObject, IViewSheetItem
    {

        private bool _isSelected;
        public Action SheetSelectionChanged { get; set; }
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    // Notify parent VM (if needed)
                    SheetSelectionChanged?.Invoke();
                }
            }
        }
        public ADSK.View SheetElement { get; set; }
        public ElementId ItemElementId { get; set; }

        private string _sheetNumber;
        public string SheetNumber
        {
            get => _sheetNumber;
            set => SetProperty(ref _sheetNumber, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string DisplayName
        {
            get => $"{SheetNumber} - {Name}";            
        }
        private string _revision;
        public string RevisionNo
        {
            get => _revision;
            set => SetProperty(ref _revision, value);
        }

        private string _size;
        public string Size
        {
            get => _size;
            set => SetProperty(ref _size, value);
        }

        private string _orientation;
        public string Orientation
        {
            get => _orientation;
            set => SetProperty(ref _orientation, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
    }
}