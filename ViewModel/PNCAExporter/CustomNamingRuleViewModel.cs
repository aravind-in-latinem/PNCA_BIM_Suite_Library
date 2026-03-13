using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PNCA_BIM_Suite_Library.Model;
using MVVM = CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    public class CustomNamingRuleViewModel: ObservableObject

    {
        #region fields
        private Document _document { get; set; }
        private ObservableCollection<ParameterItem> _availableParameters;
        private ObservableCollection<ParameterItem> _selectedParameters;
        private string _customKeyword;
        private string _seperator;
        
        private bool _isSheets;
        
        #endregion
        #region constructor
        public CustomNamingRuleViewModel(Document document, bool isSheets)
        {
            _document = document;
            _isSheets = isSheets;
            AvailableParameters = new ObservableCollection<ParameterItem>();
            SelectedParameters = new ObservableCollection<ParameterItem>();
            SelectedLHSParameters = new ObservableCollection<ParameterItem>();
            SelectedRHSParameters = new ObservableCollection<ParameterItem>();
            if (_isSheets)
                LoadSheetParameters();            
            else            
                LoadViewParameters();
            AddParameterCommand = new RelayCommand(AddParameter);
            RemoveParameterCommand = new RelayCommand(RemoveParameter);
            ClearCommand = new RelayCommand(ExecuteClear);
            MoveUpCommand = new MVVM.RelayCommand(ExecuteMoveUp, CanMoveUp);
            MoveDownCommand = new MVVM.RelayCommand(ExecuteMoveDown, CanMoveDown);
            MoveToFirstCommand = new MVVM.RelayCommand(ExecuteMoveToFirst, CanExecuteMoveToFirst);
            MoveToLastCommand = new MVVM.RelayCommand(ExecuteMoveToLast, CanExecuteMoveToLast);
            SelectedRHSParameters.CollectionChanged += (sender, e) =>
            {
                MoveUpCommand.NotifyCanExecuteChanged();
                MoveDownCommand.NotifyCanExecuteChanged();
                MoveToFirstCommand.NotifyCanExecuteChanged();
                MoveToLastCommand.NotifyCanExecuteChanged();
            };

        }
        #endregion



        #region observable properties
        public ObservableCollection<ParameterItem> AvailableParameters
        {
            get=> _availableParameters;

            set => SetProperty(ref _availableParameters, value);

        }
        public ObservableCollection<ParameterItem> SelectedParameters
        {
            get => _selectedParameters;
            set => SetProperty(ref _selectedParameters, value);
        }
        public ObservableCollection<ParameterItem> SelectedLHSParameters { get; }
        public ObservableCollection<ParameterItem> SelectedRHSParameters { get; }
        public string CustomKeyword
        {
            get => _customKeyword;
            set => SetProperty(ref _customKeyword, value);
        }
        public string Seperator
        {
            get => _seperator;
            set => SetProperty(ref _seperator, value);
        }
        public string PreviewText { get; set; }
        #endregion
        #region commands
        public RelayCommand AddParameterCommand { get; set; }
        public RelayCommand RemoveParameterCommand { get; set; }
        public RelayCommand ClearCommand { get; set; }
        public MVVM.IRelayCommand MoveUpCommand { get; }
        public MVVM.IRelayCommand MoveDownCommand { get; }
        public MVVM.IRelayCommand MoveToFirstCommand { get; }
        public MVVM.IRelayCommand MoveToLastCommand { get; }




        #endregion
        #region private methods
        private void LoadSheetParameters()
        {
            var parameters = new FilteredElementCollector(_document).OfClass(typeof(ViewSheet))
                .FirstOrDefault()
                .GetOrderedParameters()
                .Where(p => !ExcludedSheetParameterNames.Contains(p.Definition.Name));
            var projectparameters = GetProjectParameters();

            foreach (Parameter param in parameters)
            {
                if (param!=null)
                {                    
                    AvailableParameters.Add(new ParameterItem(param));
                }
            }
            foreach (Parameter param in projectparameters)
            {
                if (param != null)
                {
                    AvailableParameters.Add(new ParameterItem(param));
                }
            }
        }
        private void LoadViewParameters()
        {
            var containingViews = new List<Element>();
            var viewPlanCollector = new FilteredElementCollector(_document).OfClass(typeof(ViewPlan)).Where(v => v is Autodesk.Revit.DB.View view && !view.IsTemplate)
                .FirstOrDefault();
            var viewSectionCollector = new FilteredElementCollector(_document).OfClass(typeof(ViewSection))
                .Where(v => v is Autodesk.Revit.DB.View view && !view.IsTemplate)
                .FirstOrDefault();
            var view3DCollector = new FilteredElementCollector(_document).OfClass(typeof(View3D))
                .Where(v => v is Autodesk.Revit.DB.View view && !view.IsTemplate)
                .FirstOrDefault();
            var viewDraftingCollector = new FilteredElementCollector(_document).OfClass(typeof(ViewDrafting))
                .Where(v => v is Autodesk.Revit.DB.View view && !view.IsTemplate)
                .FirstOrDefault();
            containingViews = new List<Element>
            {
                viewPlanCollector,
                viewSectionCollector,
                view3DCollector,
                viewDraftingCollector
            };
            var parameters = containingViews
            .Where(v => v != null)
            .SelectMany(v => v.GetOrderedParameters() ?? Enumerable.Empty<Parameter>())
            .Where(p => p != null && p.Definition != null)
            .GroupBy(p => p.Definition.Name)
            .Select(g => g.First())
            .Where(p => !ExcludedViewParameterNames.Contains(p.Definition.Name))
            .Select(p => new ParameterItem(p))
            .ToList();
            var viewTypeParam = containingViews.FirstOrDefault().get_Parameter(BuiltInParameter.VIEW_TYPE_SCHEDULES);
            parameters.Add(new ParameterItem(viewTypeParam));
            GetProjectParameters().Where(a=>a.StorageType==StorageType.String)
                .ToList().ForEach(p => parameters.Add(new ParameterItem(p)));

            AvailableParameters = new ObservableCollection<ParameterItem>(parameters);
        }
        public List<Parameter> GetProjectParameters()
        {
        var projparams = new FilteredElementCollector(_document)
                .OfClass(typeof(ProjectInfo)).FirstElement()
                .GetOrderedParameters().Where(a=>a.StorageType==StorageType.String);
            return projparams.ToList();
        }
        private void AddParameter()
        {
            if (SelectedLHSParameters == null || SelectedLHSParameters.Count == 0)
            {
                if (CustomKeyword != null && CustomKeyword != "")
                {
                    var customParam = new ParameterItem(CustomKeyword);                    
                    SelectedParameters.Add(customParam);
                    CustomKeyword = "";                    
                }
                return;
            }
            if (CustomKeyword != null && CustomKeyword != "")
            {
                var customParam = new ParameterItem(CustomKeyword);
                SelectedParameters.Add(customParam);
                CustomKeyword = "";
            }

            foreach (var param in SelectedLHSParameters)
            {
                if (!SelectedParameters.Contains(param))
                {
                    SelectedParameters.Add(param);
                }
            }
            UpdatePreview();
        }

        private void RemoveParameter()
        {
            if (SelectedRHSParameters == null || SelectedRHSParameters.Count == 0)
                return;

            // copy first to avoid modifying collection while iterating
            var toRemove = SelectedRHSParameters.ToList();

            foreach (var param in toRemove)
            {
                SelectedParameters.Remove(param);
            }
        }
        private void UpdatePreview()
        {
            List<string> parts = new List<string>();
            foreach (var param in SelectedParameters)
            {
                parts.Add(param.Name);
            }
            PreviewText = string.Join(Seperator ?? "", parts);
            OnPropertyChanged(nameof(PreviewText));
        }
        public void ExecuteClear()
        {
            SelectedParameters.Clear();
            Seperator = "";
            UpdatePreview();
        }

        private void ExecuteMoveUp()
        {
            var orderedSelection = SelectedRHSParameters
                .OrderBy(p => SelectedParameters.IndexOf(p))
                .ToList();

            foreach (var item in orderedSelection)
            {
                int index = SelectedParameters.IndexOf(item);
                if (index > 0)
                {
                    SelectedParameters.Move(index, index - 1);
                }
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }

        private bool CanMoveUp()
        {
            return SelectedRHSParameters?.Any() == true &&
                   SelectedRHSParameters.Min(p => SelectedParameters.IndexOf(p)) > 0;
        }

        private void ExecuteMoveDown()
        {
            var orderedSelection = SelectedRHSParameters
                .OrderByDescending(p => SelectedParameters.IndexOf(p))
                .ToList();

            foreach (var item in orderedSelection)
            {
                int index = SelectedParameters.IndexOf(item);
                if (index < SelectedParameters.Count - 1)
                {
                    SelectedParameters.Move(index, index + 1);
                }
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }


        private bool CanMoveDown()
        {
            return SelectedRHSParameters?.Any() == true &&
                   SelectedRHSParameters.Max(p => SelectedParameters.IndexOf(p)) < SelectedParameters.Count - 1;
        }


        private void ExecuteMoveToFirst()
        {
            var item = SelectedRHSParameters[0];
            SelectedParameters.Move(
                SelectedParameters.IndexOf(item),
                0);
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteMoveToFirst()
        {
            return SelectedRHSParameters?.Count == 1 &&
                   SelectedParameters.IndexOf(SelectedRHSParameters[0]) > 0;
        }
        private void ExecuteMoveToLast()
        {
            var item = SelectedRHSParameters[0];
            SelectedParameters.Move(
                SelectedParameters.IndexOf(item),
                SelectedParameters.Count - 1);
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }

        private bool CanExecuteMoveToLast()
        {
            return SelectedRHSParameters?.Count == 1 &&
                   SelectedParameters.IndexOf(SelectedRHSParameters[0]) <
                   SelectedParameters.Count - 1;
        }


        #endregion
        static readonly HashSet<string> ExcludedViewParameterNames = new HashSet<string>()
        {
            "Orientation",
            "Phase",
            "Far Clipping",
            "Far Clip Offset",
            "Hide at scales coarser than",
            "Far Clip Active",
            "Rendering Settings",
            "Section Box",
            "Show Grids",
            "Locked Orientation",
            "Projection Mode",
            "Eye Elevation",
            "Target Elevation",
            "Camera Position",
            "View Scale",
            "Scale Value    1:",
            "Color Scheme",
            "Color Scheme Location",
            "Crop Region Visible",
            "Crop View",
            "Default Analysis Display Style",
            "Dependency",
            "Depth Clipping",
            "Display Model",
            "Graphic Display Options",
            "None",
            "Parts Visibility",
            "Phase Filter",
            "Show Hidden Lines",
            "Sun Path",
            "System Color Schemes",
            "Underlay Orientation",
            "View Range",
            "Annotation Crop",
            "Scope Box",
            "View Template",
            "Temporary Hide/Isolate",
            "Visibility/Graphics Overrides",
            "Wall Join Display",
            "Parts Visibility",            
            "Design Option",
            "Appears In Sheet List",
            "Dependency",
            "Visibility/Graphics Overrides",
            "Sheet Sort",
            "File Path",
            "Guide Grid",
            "Sheet Name",
            "Sheet Number",
            "Rotation on Sheet",
            "Detail Level"
        };
        static readonly HashSet<string> ExcludedSheetParameterNames = new HashSet<string>()
        {
            "Orientation",
            "Phase",
            "Far Clipping",
            "Far Clip Offset",
            "Hide at scales coarser than",
            "Far Clip Active",
            "Rendering Settings",
            "Section Box",
            "Show Grids",
            "Locked Orientation",
            "Projection Mode",
            "Eye Elevation",
            "Target Elevation",
            "Camera Position",
            "View Scale",
            "Scale Value    1:",
            "Color Scheme",
            "Color Scheme Location",
            "Crop Region Visible",
            "Crop View",
            "Default Analysis Display Style",
            "Dependency",
            "Depth Clipping",
            "Display Model",
            "Graphic Display Options",
            "None",
            "Parts Visibility",
            "Phase Filter",
            "Show Hidden Lines",
            "Sun Path",
            "System Color Schemes",
            "Underlay Orientation",
            "View Range",
            "Annotation Crop",
            "Scope Box",
            "View Template",
            "Temporary Hide/Isolate",
            "Visibility/Graphics Overrides",
            "Wall Join Display",
            "Parts Visibility",
            "Design Option",
            "Appears In Sheet List",
            "Dependency",
            "Visibility/Graphics Overrides",
            "Sheet Sort",
            "File Path",
            "Guide Grid"
        };

    }
}
