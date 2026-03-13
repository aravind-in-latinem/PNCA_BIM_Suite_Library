using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    public class SettingsViewModel : ObservableObject
    {
        // Sub-view models
        public PDFSettingsViewModel PDFSettingsViewModel { get; }
        public DWGSettingsViewModel DWGSettingsViewModel { get; }
        public DWFSettingsViewModel DWFSettingsViewModel { get; }

        public SettingsViewModel(
            PDFSettingsViewModel pdfSettingsViewModel,
            DWGSettingsViewModel dwgSettingsViewModel,
            DWFSettingsViewModel dWFSettingsViewModel)
        {
            PDFSettingsViewModel = pdfSettingsViewModel;
            DWGSettingsViewModel = dwgSettingsViewModel;     
            DWFSettingsViewModel = dWFSettingsViewModel;
        }
    }
}
