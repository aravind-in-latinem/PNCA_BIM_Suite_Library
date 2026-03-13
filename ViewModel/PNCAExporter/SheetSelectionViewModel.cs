using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    public class SheetSelectionViewModel : ObservableObject
    {
        #region fields        
        private bool _selectAllChecked;
        [ObservableProperty]
        private ObservableCollection<SheetItem> _sheets;        
        private bool _headerCheckState;
        private string _searchText;
        private IViewSheetSet _selectedViewSheetSet;
        private ObservableCollection<SheetItem> _filteredSheets;        
        private CustomNamingRuleViewModel _customNamingRuleVM;
        


        #endregion
        #region constructor
        public SheetSelectionViewModel()
        {
            
            Sheets = new ObservableCollection<SheetItem>();
            SheetFileNameRule = new List<TableCellCombinedParameterData>();
            

            Sheets.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (SheetItem newItem in e.NewItems)
                        newItem.SheetSelectionChanged = OnSheetSelectionChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (SheetItem oldItem in e.OldItems)
                        oldItem.SheetSelectionChanged = null;
                }
                OnSheetSelectionChanged();                
            };

            LoadSheetsCommand = new RelayCommand(LoadSheets);
            ToggleSelectAllCommand = new RelayCommand<object>(OnToggleSelectAll);
            CustomizeFileName = new RelayCommand(ExecuteCustomFileName);
            SheetFileNameRule = PdfSettingsStorage.GetDefaultSheetNamingRule();
            ApplyFilter();            
        }
        #endregion
        #region Commands
        public RelayCommand LoadSheetsCommand { get; }
        public RelayCommand<object> ToggleSelectAllCommand { get; }
        public RelayCommand CustomizeFileName { get; }
        #endregion
        #region properties
        public ObservableCollection<SheetItem> SelectedSheets { get; } = new ObservableCollection<SheetItem>();
        public ObservableCollection<SheetItem> Sheets
        {
            get => _sheets;
            set => SetProperty(ref _sheets, value);
        }
        public ObservableCollection<SheetItem> FilteredSheets
        {
            get => _filteredSheets;
            set => SetProperty(ref _filteredSheets, value);
        }
        public Document Document { get; set; }
        public List<TableCellCombinedParameterData> SheetFileNameRule { get; set; }
        

        public IViewSheetSet SelectedViewSheetSet {
            get => _selectedViewSheetSet;
            set
            {
                if (SetProperty(ref _selectedViewSheetSet, value))
                {
                    ApplySelection();
                    ApplyFilter();
                }
            }

        }       

        public bool HeaderCheckState
        {
            get => _headerCheckState;
            set => SetProperty(ref _headerCheckState, value);
        }
        

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplySearchFilter();
                }
            }
        }
        public CustomNamingRuleViewModel CustomNamingRuleVM 
        { 
            get=>_customNamingRuleVM;            
        }
        #endregion
        #region public methods
        public void SetFilterState(bool enabled)
        {
            if (enabled)
                ApplyIsolate();
            else
            {
                FilteredSheets.Clear();
                foreach (var sheet in Sheets)
                {
                    FilteredSheets.Add(sheet);
                }
            }
        }
        

        public void ApplyFilter()
        {
            SearchText = "";
            OnPropertyChanged(nameof(SearchText));
            var allowedSheetIds = GetViewSheetSetIds();

            IEnumerable<SheetItem> filtered = Sheets;


            //ViewSheetSet filter
            if (allowedSheetIds != null && allowedSheetIds.Count > 0)
            {
                filtered = filtered.Where(s =>
                    s.IsSelected &&
                    allowedSheetIds.Contains(s.ItemElementId));}         

            

            
            FilteredSheets = new ObservableCollection<SheetItem>(filtered);
        }
        public void RemoveFilter()
        {
            FilteredSheets = new ObservableCollection<SheetItem>(Sheets);
        }

        public void ApplySearchFilter()
        {
            RemoveFilter();
            IEnumerable<SheetItem> filtered = Sheets;
            //Apply text filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lowerSearchText = SearchText.ToLower();

                filtered = filtered.Where(s =>
                    (s.SheetNumber != null && s.SheetNumber.ToLower().Contains(lowerSearchText)) ||
                    (s.Name != null && s.Name.ToLower().Contains(lowerSearchText)) ||
                    (s.RevisionNo != null && s.RevisionNo.ToLower().Contains(lowerSearchText)) ||
                    (s.Size != null && s.Size.ToLower().Contains(lowerSearchText)) ||
                    (s.Orientation != null && s.Orientation.ToLower().Contains(lowerSearchText))
                );
            }
            FilteredSheets = new ObservableCollection<SheetItem>(filtered);
        }


        public void ApplyIsolate()
        {
            SearchText = "";
            OnPropertyChanged(nameof(SearchText));
            IEnumerable<SheetItem> filtered = Sheets;
            if (SelectedSheets != null && SelectedSheets.Count > 0)
            {
                filtered = filtered.Where(s => s.IsSelected);
                FilteredSheets = new ObservableCollection<SheetItem>(filtered);                
            }            
        }

        public void ApplySelection()
        {
            SetSelection(false);
            var allowedSheetIds = GetViewSheetSetIds();
            IEnumerable<SheetItem> selected = Sheets;
            
            if (allowedSheetIds != null && allowedSheetIds.Count > 0)
            {
                selected = selected.Where(s =>
                    s.ItemElementId != null &&
                    allowedSheetIds.Contains(s.ItemElementId));
            }
            SelectedSheets.Clear();
            foreach(var item in Sheets)
            {
                if (selected.Contains(item))
                    item.IsSelected = true;
            }
        }
        public void LoadSheets()
        {   
            var newSheets = new RevitCollectorServices(Document).GetSheets();
            _customNamingRuleVM = new CustomNamingRuleViewModel(Document, true);            

            Sheets = new ObservableCollection<SheetItem>(newSheets);

            foreach (var sheet in newSheets)
            {
                sheet.SheetSelectionChanged = OnSheetSelectionChanged;
            }

            OnSheetSelectionChanged();

        }
        public void UpdateSelectedSheets()
        {
            SelectedSheets.Clear();
            foreach (var sheet in Sheets.Where(x => x.IsSelected))
                SelectedSheets.Add(sheet);
        }
        public void SetSelection(bool state)
        {
            foreach (var sheet in Sheets)
                sheet.IsSelected = state;

            HeaderCheckState = state;
            UpdateSelectedSheets();
        }
        
        #endregion
        #region private methods

        private HashSet<ElementId> GetViewSheetSetIds()
        {
            if (SelectedViewSheetSet == null)
                return null;

            // In-session set is also valid
            var viewsets = SelectedViewSheetSet.OrderedViewList;
            var views = viewsets.Select(a => a.Id);

            if (views == null)
                return null;

            return new HashSet<ElementId>(views.Cast<ElementId>());
        }       
        

        private void UpdateHeaderState()
        {
            HeaderCheckState = Sheets.Any() && Sheets.All(s => s.IsSelected);
        }

        private void OnToggleSelectAll(object parameter)
        {
            if (parameter is bool isChecked)
            {
                SetSelection(isChecked);
            }
        }
        

        private void OnSheetSelectionChanged()
        {
            UpdateHeaderState();
            UpdateSelectedSheets();
        }
        
        private void ExecuteCustomFileName()
        {
            var customFileNameWindow = new Views.PNCAExporter.CustomNamingRuleView
            {
                DataContext = _customNamingRuleVM
            };
            bool? result = customFileNameWindow.ShowDialog();
            if (!string.IsNullOrWhiteSpace(_customNamingRuleVM.Seperator) && !PdfSettingsStorage.IsValidFileNameSeparator(_customNamingRuleVM.Seperator))
            {
                TaskDialog.Show("Illegal Characters Error",
                        "The following characters are not allowed in file names: \\ / : * ? \" < > |.");
                _customNamingRuleVM.Seperator = "";
                UpdateFileName();
            }
            if (_customNamingRuleVM.SelectedParameters.Count == 0)
                UpdateFileName();
            else
                CreateNamingRule(_customNamingRuleVM.SelectedParameters.ToList(),
                    _customNamingRuleVM.Seperator ?? "");


        }
        #endregion
        public void CreateNamingRule(
    List<ParameterItem> selectedItems,
    string separator)
        {
            SheetFileNameRule.Clear();

            string pendingText = string.Empty;
            TableCellCombinedParameterData lastParameterData = null;

            for (int i = 0; i < selectedItems.Count; i++)
            {
                var item = selectedItems[i];

                if (item.IsCustom)
                {
                    pendingText += item.Name;

                    // Add separator between tokens
                    if (i < selectedItems.Count - 1)
                        pendingText += separator;

                    continue;
                }

                // Parameter item
                var parameterData = TableCellCombinedParameterData.Create();
                parameterData.ParamId = item.Parameter.Id;

                // Attach any accumulated custom text BEFORE parameter
                if (!string.IsNullOrEmpty(pendingText))
                {
                    parameterData.Prefix = pendingText;
                    pendingText = string.Empty;
                }

                // Default separator after parameter
                parameterData.Separator = separator;

                SheetFileNameRule.Add(parameterData);
                lastParameterData = parameterData;
            }

            // If trailing custom text exists, append it to last parameter
            if (!string.IsNullOrEmpty(pendingText) && lastParameterData != null)
            {
                lastParameterData.Separator =
                    (lastParameterData.Separator ?? string.Empty) + pendingText;
            }           

            UpdateFileName(selectedItems, separator);
        }



        private void UpdateFileName(List<ParameterItem> SelectedSheetParameters, string seperator)
        {          

            foreach (var sheetItem in Sheets)
            {
                List<string> appendString = new List<string>();
                var fileNameBase = "";
                foreach (var parameterItem in SelectedSheetParameters)
                {

                    if (parameterItem.IsCustom)
                    {
                        appendString.Add(parameterItem.Name);
                    }
                    else
                    {
                        var parameterData = sheetItem.SheetElement.get_Parameter(parameterItem.Parameter.Definition);
                        var paramValue = "";
                        if (parameterData == null)
                            paramValue = "None";
                        else
                            paramValue = parameterData?.AsString() ?? parameterData.AsValueString() ?? "";
                        appendString.Add($"{paramValue}");                      
                        
                    }
                }
                fileNameBase = string.Join(seperator ?? "", appendString);
                sheetItem.FileName = fileNameBase;
            }
            
        }
        public void UpdateFileName()
        {
            foreach (var sheetItem in Sheets)
            {
                sheetItem.FileName = $"{sheetItem.SheetNumber}_{sheetItem.Name}";
            }

        }
    }
}
