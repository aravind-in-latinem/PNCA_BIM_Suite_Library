using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace PNCA_BIM_Suite_Library.Views
{

    public partial class DuplicateViewsWindow : Window
    {
        private Document _doc;
        public ICommand Close_Click { get; }
        public ICommand Minimize_Click { get; }
        public DuplicateViewsWindow(Document doc)
        {
            InitializeComponent();
            _doc = doc;
            Close_Click = new RelayCommand(_ => this.Close());
            Minimize_Click = new RelayCommand(_ => this.WindowState = WindowState.Minimized);
            LoadViews();
            LoadViewTypes();
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

        private void LoadViews()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(_doc);
                ICollection<View> views = collector.OfClass(typeof(View)).WhereElementIsNotElementType().Cast<View>().OrderBy(v => v.ViewType).ToList();

                List<View> filteredViews = new List<View>();

                foreach (View view in views)
                {
                    if (view.IsTemplate)
                        continue;


                    Parameter disciplineParam = view.get_Parameter(BuiltInParameter.VIEW_DISCIPLINE);
                    if (disciplineParam != null && disciplineParam.HasValue)
                    {
                        int disciplineValue = disciplineParam.AsInteger();
                        if (disciplineValue == 1 || disciplineValue == 2 || disciplineValue == 4 || disciplineValue == 8 || disciplineValue == 4095)
                        {
                            filteredViews.Add(view);
                        }
                        else
                        {
                            string viewName = view.Name.ToLower();
                            bool execute = false;

                            if (viewName.Contains("Analytical") || viewName.Contains("Isolated") || viewName.Contains("Unconnected") || viewName.Contains("Nodes") || viewName.Contains("Connections"))
                                execute = true;

                            if (!execute)
                            {
                                filteredViews.Add(view);
                            }
                        }

                    }

                }

                lstViews.Items.Clear();


                foreach (View view in filteredViews)
                {
                    if (!view.IsTemplate && view.ViewType == ViewType.FloorPlan || view.ViewType == ViewType.CeilingPlan || view.ViewType == ViewType.Elevation || view.ViewType == ViewType.Section || view.ViewType == ViewType.ThreeD)
                    {
                        var check = new CheckBox
                        {
                            Content = $"{view.ViewType} : {view.Name}",
                            Tag = view
                        };
                        lstViews.Items.Add(check);
                    }
                }


            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error loading views: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }



        private void LoadViewTypes()
        {
            try
            {
                FilteredElementCollector viewtypeCollector = new FilteredElementCollector(_doc);
                ICollection<ViewFamilyType> viewTypes = viewtypeCollector.OfClass(typeof(ViewFamilyType)).Cast<ViewFamilyType>().ToList();

                lstViewTypes.Items.Clear();

                foreach (ViewFamilyType viewType in viewTypes)
                {
                    if (viewType.ViewFamily == ViewFamily.FloorPlan ||
                       viewType.ViewFamily == ViewFamily.CeilingPlan ||
                       viewType.ViewFamily == ViewFamily.Section ||
                       viewType.ViewFamily == ViewFamily.Elevation ||
                       viewType.ViewFamily == ViewFamily.ThreeDimensional)
                    {

                        var check = new CheckBox
                        {
                            Content = viewType.Name,
                            Tag = viewType
                        };

                        lstViewTypes.Items.Add(check);
                    }

                }
            }




            catch (Exception ex)
            {
                MessageBox.Show($"Error loading view types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<View> selectedViews = lstViews.Items.OfType<CheckBox>().Where(cb => cb.IsChecked == true).Select(cb => cb.Tag as View).ToList();
                var selectedViewTypes = lstViewTypes.Items.OfType<CheckBox>().Where(cb => cb.IsChecked == true).Select(cb => cb.Tag as ViewFamilyType).ToList();

                if (!int.TryParse(DuplicateViewCount.Text.Trim(), out int duplicateCount) || duplicateCount < 1)
                {
                    MessageBox.Show("Please enter a valid number for duplicate");
                    return;
                }

                if (selectedViews.Count == 0)
                {
                    MessageBox.Show("Please select the views to duplicate");
                    return;
                }

                if (selectedViewTypes.Count != 1)
                {
                    MessageBox.Show("Please select exactly one correct view type for duplicated views");
                    return;
                }

                ViewFamilyType targetViewType = selectedViewTypes.FirstOrDefault();

                using (Transaction t = new Transaction(_doc, "Duplicate Views"))
                {
                    t.Start();

                    foreach (var cb in selectedViews)
                    {
                        ViewDuplicateOption option = ViewDuplicateOption.Duplicate;

                        if (rbDuplicateWithDetailing.IsChecked == true)
                        {
                            option = ViewDuplicateOption.WithDetailing;
                        }
                        else if (rbDuplicateAsDependent.IsChecked == true)
                        {
                            option = ViewDuplicateOption.AsDependent;
                        }

                        var existingViewNames = new FilteredElementCollector(_doc)
                            .OfClass(typeof(View))
                            .Cast<View>()
                            .Select(v => v.Name)
                            .ToHashSet();

                        for (int i = 0; i < duplicateCount; i++)
                        {
                            View dupView = _doc.GetElement(cb.Duplicate(option)) as View;

                            if (dupView != null)
                            {
                                dupView.ChangeTypeId(targetViewType.Id);

                                
                                    string newName = $"{cb.Name} - Copy {i + 1}";
                                   
                                
                                    int counter = 1;

                                    while (existingViewNames.Contains(newName))
                                    {
                                     
                                        newName = $"{newName} - Copy {i + 1} - Copy {counter}";
                                        counter++;
                                    }

                                dupView.Name = newName;
                                existingViewNames.Add(newName);

                            

                            }

                        }

                        

                    }

                    t.Commit();


                }

                

                MessageBox.Show("Views duplicated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();








            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating views: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();




        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
