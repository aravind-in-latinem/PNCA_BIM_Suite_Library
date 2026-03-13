using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;
using CommunityToolkit.Mvvm.ComponentModel;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.ViewModel
{
    public class DWGSettingsViewModel : ObservableObject
    {
        private Document _document;
        private bool _isExternalReferencesChecked = false;
        private ExportDWGSettingsItem _selectedDWGExportSetup;
        private ObservableCollection<ExportDWGSettingsItem> _dWGExportSetups = new ObservableCollection<ExportDWGSettingsItem>();
        public DWGSettingsViewModel()
        {

            
        }
        public Document Document 
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }
        public bool IsExternalReferencesChecked
        {
            get => _isExternalReferencesChecked;
            set => SetProperty(ref _isExternalReferencesChecked, value);
        }
        public ExportDWGSettingsItem SelectedDWGExportSetup 
        { 
            get=>_selectedDWGExportSetup; 
            set=> SetProperty(ref _selectedDWGExportSetup,value); 
        }
        public ObservableCollection<ExportDWGSettingsItem> DWGExportSetups 
        { 
            get=>_dWGExportSetups; 
            set=> SetProperty(ref _dWGExportSetups,value); 
        }

        public void LoadDWGExportSettings()
        {
            var ExportSettingsCollector = new FilteredElementCollector(_document)
                .OfClass(typeof(ExportDWGSettings)).ToList();
            DWGExportSetups.Clear();
            DWGExportSetups.Add(new ExportDWGSettingsItem("<In Session>"));
            foreach (ExportDWGSettings setting in ExportSettingsCollector)
            {
                DWGExportSetups.Add(new ExportDWGSettingsItem(setting));
            }
            SelectedDWGExportSetup = DWGExportSetups.Where(a => a.Name == "<In Session>").First();
            OnPropertyChanged(nameof(SelectedDWGExportSetup));
        }

    }
}
