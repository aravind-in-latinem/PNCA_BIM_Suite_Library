using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PNCA_BIM_Suite_Library.Model;
using PNCA_BIM_Suite_Library.Services;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    internal class ViewSelectionViewModel : ObservableObject
    {

        private bool _selectAllChecked;

        [ObservableProperty]
        private ObservableCollection<Model.ViewItem> _viewItems;
        private IViewSheetSet _selectedViewSheetSet;
        private ObservableCollection<Model.ViewItem> _filteredViewItems;
        private string _searchText;
        private bool _headerCheckState;        
        private CustomNamingRuleViewModel _customNamingRuleVM;




        public ViewSelectionViewModel()
        {

            ViewItems = new ObservableCollection<Model.ViewItem>();
            ViewFileNameRule = new List<TableCellCombinedParameterData>();

            ViewItems.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (ViewItem newItem in e.NewItems)
                        newItem.ViewSelectionChanged = OnViewSelectionChanged;
                }
                if (e.OldItems != null)
                {
                    foreach (ViewItem oldItem in e.OldItems)
                        oldItem.ViewSelectionChanged = null;
                }

                // sync after collection change
                OnViewSelectionChanged();
            };

            LoadSheetsCommand = new RelayCommand(LoadViews);
            ToggleSelectAllCommand = new RelayCommand<object>(OnToggleSelectAll);
            CustomizeFileName = new RelayCommand(ExecuteCustomFileName);
            ViewFileNameRule = PdfSettingsStorage.GetDefaultViewNamingRule();
            ApplyFilter();
        }
        public RelayCommand LoadSheetsCommand { get; }
        public RelayCommand<object> ToggleSelectAllCommand { get; }
        public RelayCommand CustomizeFileName { get; }



        public ObservableCollection<ViewItem> SelectedViewItems { get; } = new ObservableCollection<ViewItem>();
        public ObservableCollection<Model.ViewItem> ViewItems
        {
            get => _viewItems;
            set => SetProperty(ref _viewItems, value); // This raises PropertyChanged
        }
        public ObservableCollection<Model.ViewItem> FilteredViewItems
        {
            get => _filteredViewItems;
            set => SetProperty(ref _filteredViewItems, value); // This raises PropertyChanged
        }
        public Document Document { get; set; }
        public List<TableCellCombinedParameterData> ViewFileNameRule { get; set; }
        

        public IViewSheetSet SelectedViewSheetSet
        {
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

        public CustomNamingRuleViewModel CustomFileNameVM
        {
            get => _customNamingRuleVM;
        }

        public void SetFilterState(bool enabled)
        {
            if (enabled)
                ApplyIsolate();
            else
            {
                FilteredViewItems.Clear();
                foreach (var view in ViewItems)
                {
                    FilteredViewItems.Add(view);
                }
            }
        }




        public void ApplyFilter()
        {
            SearchText = "";
            OnPropertyChanged(nameof(SearchText));
            var allowedViewIds = GetViewSheetSetIds();

            IEnumerable<ViewItem> filtered = ViewItems;

            // 1️⃣ Apply ViewSheetSet filter first
            if (allowedViewIds != null && allowedViewIds.Count > 0)
            {
                filtered = filtered.Where(v =>
                    v.IsSelected &&
                    allowedViewIds.Contains(v.ItemElementId));
            }

            

            // 3️⃣ Assign result
            FilteredViewItems = new ObservableCollection<ViewItem>(filtered);
        }

        public void ApplyIsolate()
        {
            SearchText = "";
            OnPropertyChanged(nameof(SearchText));
            IEnumerable<ViewItem> filtered = ViewItems;
            if (SelectedViewItems != null && SelectedViewItems.Count > 0)
            {
                filtered = filtered.Where(s => s.IsSelected);
                FilteredViewItems = new ObservableCollection<ViewItem>(filtered);
            }
        }

        public void RemoveFilter()
        {
            FilteredViewItems = new ObservableCollection<ViewItem>(ViewItems);
        }

        public void ApplySearchFilter()
        {
            RemoveFilter();
            IEnumerable<ViewItem> filtered = ViewItems;
            // 2️⃣ Apply text filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var lowerSearchText = SearchText.ToLower();

                filtered = filtered.Where(s =>
                    (s.Name != null && s.Name.ToLower().Contains(lowerSearchText)) ||
                    (s.ViewType != null && s.ViewType.ToLower().Contains(lowerSearchText)) ||
                    (s.ViewScale != null && s.ViewScale.ToLower().Contains(lowerSearchText)) ||
                    (s.DetailLevel != null && s.DetailLevel.ToLower().Contains(lowerSearchText)) ||
                    (s.Discipline != null && s.Discipline.ToLower().Contains(lowerSearchText))
                );
            }
            FilteredViewItems = new ObservableCollection<ViewItem>(filtered);
        }

        public void ApplySelection()
        {
            var allowedSheetIds = GetViewSheetSetIds();
            IEnumerable<ViewItem> selected = ViewItems;


            // 1️⃣ Apply ViewSheetSet filter first
            if (allowedSheetIds != null && allowedSheetIds.Count > 0)
            {
                selected = selected.Where(s =>
                    s.ItemElementId != null &&
                    allowedSheetIds.Contains(s.ItemElementId));
            }
            SelectedViewItems.Clear();
            foreach (var item in ViewItems)
            {
                if (selected.Contains(item))
                    item.IsSelected = true;
            }
        }

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

        public void LoadViews()
        {   // Load sheets logic here
            var viewCollection = RevitCollectorServices.GetAllViews(Document);
            _customNamingRuleVM = new CustomNamingRuleViewModel(Document, false);
            ViewItems = new ObservableCollection<ViewItem>(viewCollection);

            foreach (var view in viewCollection)
            {
                view.ViewSelectionChanged = OnViewSelectionChanged;
            }

            OnViewSelectionChanged();

        }

        private void UpdateHeaderState()
        {
            HeaderCheckState = ViewItems.Any() && ViewItems.All(s => s.IsSelected);
        }

        private void OnToggleSelectAll(object parameter)
        {
            if (parameter is bool isChecked)
            {
                SetSelection(isChecked);
            }
        }

        public void SetSelection(bool state)
        {
            foreach (var sheet in ViewItems)
                sheet.IsSelected = state;

            HeaderCheckState = state;
            UpdateSelectedViews();
        }
        public void OnViewSelectionChanged()
        {
            UpdateHeaderState();
            UpdateSelectedViews();
        }
        private void UpdateSelectedViews()
        {
            SelectedViewItems.Clear();
            foreach (var sheet in ViewItems.Where(x => x.IsSelected))
                SelectedViewItems.Add(sheet);
        }
        
        private void ExecuteCustomFileName()
        {
            
            var customFileNameWindow = new Views.PNCAExporter.CustomNamingRuleView
            {
                DataContext = _customNamingRuleVM
            };
            bool? result = customFileNameWindow.ShowDialog();

            if (!string.IsNullOrWhiteSpace(_customNamingRuleVM.Seperator)&&!PdfSettingsStorage.IsValidFileNameSeparator(_customNamingRuleVM.Seperator))
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

        public void CreateNamingRule(
    List<ParameterItem> selectedItems,
    string separator)
        {
            ViewFileNameRule.Clear();

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

                ViewFileNameRule.Add(parameterData);
                lastParameterData = parameterData;
            }

            // 🔥 FIX: If trailing custom text exists, append it to last parameter
            if (!string.IsNullOrEmpty(pendingText) && lastParameterData != null)
            {
                lastParameterData.Separator =
                    (lastParameterData.Separator ?? string.Empty) + pendingText;
            }           


            UpdateFileName(selectedItems, separator);
        }
        private void UpdateFileName(List<ParameterItem> SelectedSheetParameters, string seperator)
        {
            foreach (var viewItem in ViewItems)
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
                        var parameterData = viewItem.ViewElement.get_Parameter(parameterItem.Parameter.Definition);
                        var paramValue = "";
                        if (parameterData == null)
                            paramValue = "None";
                        else
                        paramValue = parameterData?.AsString() ?? parameterData.AsValueString() ?? "";
                        appendString.Add($"{paramValue}");
                    }
                    fileNameBase = string.Join(seperator ?? "", appendString);
                    viewItem.FileName = fileNameBase;
                }

            }

        }
        public void UpdateFileName()
        {
            foreach (var viewItem in ViewItems)
            {
                viewItem.FileName = $"{viewItem.Name}_{viewItem.ViewType}";
            }

        }
    }
}
