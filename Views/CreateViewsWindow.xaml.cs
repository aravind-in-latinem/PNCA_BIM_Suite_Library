using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;


namespace PNCA_BIM_Suite_Library.Views
{
    /// <summary>
    /// Interaction logic for CreateViewsWindow.xaml
    /// </summary>
    public partial class CreateViewsWindow : Window
    {
        private Document _document;
        private List<Level> _alllevels;
        public ICommand Close_Click { get; }
        public ICommand Minimize_Click { get; }
        public CreateViewsWindow(Document document)
        {
            InitializeComponent();
            _document = document;
            Close_Click = new RelayCommand(_ => this.Close());
            Minimize_Click = new RelayCommand(_ => this.WindowState = WindowState.Minimized);
            LoadData();
            DataContext = this;
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
            public void Execute(object parameter) => _execute(parameter);
            public event EventHandler CanExecuteChanged;
        }
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }


        private void LoadData()
        {
            LoadAvailableLevels();
            LoadViewTypes();
            LoadDisciplines();
        }


        private void LoadAvailableLevels()
        {
            try
            {
                FilteredElementCollector levelcollector = new FilteredElementCollector(_document);
                _alllevels = levelcollector.OfCategory(BuiltInCategory.OST_Levels)
                 .WhereElementIsNotElementType()
                 .Cast<Level>()
                 .OrderBy(l => l.Elevation)
                 .ToList();

                foreach (Level level in _alllevels)
                {
                    CheckBox checkBox = new CheckBox
                    {
                        Content = level.Name,
                        Tag = level
                    };
                    AvailableLevelsListBox.Items.Add(checkBox);
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error loading levels: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }

        //private void LoadViewTypes()
        //{
        //    TypeComboBox.Items.Clear();
        //    TypeComboBox.Items.Add("Floor Plan");
        //    TypeComboBox.Items.Add("Ceiling Plan");

        //    TypeComboBox.SelectedIndex = 0;
        //}

        private void LoadDisciplines()
        {
            DisciplineComboBox.Items.Clear();
            DisciplineComboBox.Items.Add("Architecture");
            DisciplineComboBox.Items.Add("Structure");
            DisciplineComboBox.Items.Add("Mechanical");
            DisciplineComboBox.Items.Add("Electrical");
            DisciplineComboBox.Items.Add("Plumbing");
            DisciplineComboBox.Items.Add("Coordination");
            DisciplineComboBox.SelectedIndex = 0;
        }

        private void GetSubDisciplineValue()
        {
            string subDiscipline = SubDisciplineTextBox.Text;
        }

        private void SetSubDisciplineValue(string value)
        {
            SubDisciplineTextBox.Text = value;
        }

        private void ClearSubDiscipline()
        {
            SubDisciplineTextBox.Clear();
            // or
            SubDisciplineTextBox.Text = string.Empty;
        }

        private void CheckAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox checkBox in AvailableLevelsListBox.Items.OfType<CheckBox>())
            {

                checkBox.IsChecked = true;

            }
        }

        private void CheckNoneButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox checkBox in AvailableLevelsListBox.Items.OfType<CheckBox>())
            {
                checkBox.IsChecked = false;
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedLevels = GetSelectedLevels();

                if (selectedLevels.Count == 0)
                {
                    MessageBox.Show("Please select at least one level.", "No Level Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string selectedViewType = TypeComboBox.SelectedItem.ToString();
                if (string.IsNullOrEmpty(selectedViewType))
                {
                    MessageBox.Show("Please select a view type.", "No View Type Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                // Get selected discipline
                string selectedDiscipline = DisciplineComboBox.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(selectedDiscipline))
                {
                    MessageBox.Show("Please select a discipline.", "No Discipline Selected",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get sub discipline (optional)
                string selectedSubDiscipline = SubDisciplineTextBox.Text?.Trim();
                var selectedViewTypeWrapper = GetSelectedViewTypeWrapper();
                // Create views
                CreateViews(selectedLevels, selectedViewTypeWrapper, selectedDiscipline, selectedSubDiscipline);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating views: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<Level> GetSelectedLevels()
        {
            List<Level> selectedLevels = new List<Level>();
            foreach (CheckBox checkBox in AvailableLevelsListBox.Items.OfType<CheckBox>())
            {
                if (checkBox.IsChecked == true && checkBox.Tag is Level level)
                {
                    selectedLevels.Add(level);
                }
            }
            return selectedLevels;
        }

        private int GetDisciplineEnumValue(string disclipline)
        {
            switch (disclipline)
            {
                case "Architecture":
                    return 1;
                case "Structure":
                    return 2;
                case "Mechanical":
                    return 4;
                case "Electrical":
                    return 8;
                case "Plumbing":
                    return 16;
                case "Coordination":
                    return 4095;
            }

            return 4095;
        }

        private void CreateViews(List<Level> levels, ViewTypeWrapper viewTypeWrapper, string discipline, string subDiscipline)
        {
            using (Transaction transaction = new Transaction(_document, "Create Views"))
            {
                transaction.Start();
                try
                {
                    int successCount = 0;
                    int failCount = 0;
                    List<string> errors = new List<string>();

                    foreach (Level level in levels)
                    {
                        try
                        {
                            ViewPlan newView = null;

                            if (viewTypeWrapper.ViewType == "FloorPlan" || viewTypeWrapper.ViewType == "CeilingPlan")
                            {
                                ViewFamilyType floorPlanType = viewTypeWrapper.ViewFamilyType;

                                if (floorPlanType != null)
                                {
                                    newView = ViewPlan.Create(_document, floorPlanType.Id, level.Id);
                                }

                            }
                            //else if (viewTypeWrapper.ViewType == "Ceiling Plan")
                            //{
                            //    ViewFamilyType ceilingPlanType = GetViewFamilyType(ViewFamily.CeilingPlan);
                            //    if (ceilingPlanType != null)
                            //    {
                            //        newView = ViewPlan.Create(_document, ceilingPlanType.Id, level.Id);
                            //    }
                            //}

                            if (newView != null)
                            {
                                string viewName = GenerateViewName(level.Name, discipline, subDiscipline);
                                newView.Name = viewName;
                                newView.ViewTemplateId = ElementId.InvalidElementId;

                                Parameter disciplineParam = newView.get_Parameter(BuiltInParameter.VIEW_DISCIPLINE);
                                if (disciplineParam != null && !disciplineParam.IsReadOnly)
                                {
                                    int disciplineEnum = GetDisciplineEnumValue(discipline);
                                    disciplineParam.Set(disciplineEnum);
                                }

                                if (!string.IsNullOrEmpty(subDiscipline))
                                {
                                    Parameter subDisciplineParam = newView.LookupParameter("Sub-Discipline");
                                    if (subDisciplineParam != null && !subDisciplineParam.IsReadOnly)
                                    {
                                        subDisciplineParam.Set(subDiscipline);
                                    }
                                }
                                successCount++;
                            }
                            else
                            {
                                failCount++;
                                errors.Add($"Failed to create {viewTypeWrapper} for level {level.Name}: View family type not found");
                            }
                        }

                        catch (Exception ex)
                        {
                            failCount++;
                            errors.Add($"Failed to create {viewTypeWrapper} for level {level.Name}: {ex.Message}\n{ex.StackTrace}");


                        }
                    }

                    transaction.Commit();

                    string message = $"Views created successfully: {successCount}\n";
                    if (failCount > 0)
                    {
                        message += $"Failed to create: {failCount}\n\nErrors:\n";
                        message += string.Join("\n", errors.Take(5)); // Show first 5 errors
                        if (errors.Count > 5)
                        {
                            message += $"\n... and {errors.Count - 5} more errors.";
                        }
                    }

                    MessageBox.Show(message, "Create Views Result",
                        MessageBoxButton.OK,
                        failCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);

                    if (successCount > 0)
                    {
                        this.DialogResult = true;
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    transaction.RollBack();
                    throw new Exception($"Transaction failed: {ex.Message}");
                }
            }

        }

        private ViewFamilyType GetViewFamilyType(ViewFamily viewFamily)
        {
            FilteredElementCollector collector = new FilteredElementCollector(_document);
            ViewFamilyType viewFamilyType = collector
              .OfClass(typeof(ViewFamilyType))
              .Cast<ViewFamilyType>()
              .FirstOrDefault(vft => vft.ViewFamily == viewFamily);

            return viewFamilyType;
        }

        private string GenerateViewName(string levelName, string disclipline, string subDiscipline)
        {
            string viewName = $"{levelName} - {disclipline} - {subDiscipline}";
            return viewName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public ViewTypeWrapper TypeCollection { get; set; }
        private void LoadViewTypes()
        {
            try
            {
                FilteredElementCollector viewtypeCollector = new FilteredElementCollector(_document);
                ICollection<ViewFamilyType> viewTypes = viewtypeCollector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().ToList();

                TypeComboBox.Items.Clear();

                foreach (ViewFamilyType viewType in viewTypes)
                {
                    if (viewType.ViewFamily == ViewFamily.FloorPlan ||
                       viewType.ViewFamily == ViewFamily.CeilingPlan)
                    {

                        var check = new ComboBoxItem()
                        {
                            Content = viewType.Name,
                            Tag = new ViewTypeWrapper()
                            {
                                Name = viewType.Name,
                                ViewFamilyType = viewType,
                                ViewType = viewType.ViewFamily.ToString()
                            }
                        };

                        TypeComboBox.Items.Add(check);
                    }

                }
                TypeComboBox.SelectedIndex = 0;
            }




            catch (Exception ex)
            {
                MessageBox.Show($"Error loading view types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private ViewTypeWrapper GetSelectedViewTypeWrapper()
        {
            if (TypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag as ViewTypeWrapper;
            }
            return null;
        }

    }




}


