using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PNCA_BIM_Suite_Library.Views
{
    public partial class SheetLinkUserControl : UserControl
    {
        public SheetLinkUserControl()
        {
            InitializeComponent();
        }

        public string SaveLocation
        {
            get => (string)GetValue(SaveLocationProperty);
            set => SetValue(SaveLocationProperty, value);
        }
        public static readonly DependencyProperty SaveLocationProperty =
            DependencyProperty.Register(nameof(SaveLocation), typeof(string), typeof(SheetLinkUserControl), new PropertyMetadata(""));

        
        public IEnumerable FilteredSchedules
        {
            get => (IEnumerable)GetValue(FilteredSchedulesProperty);
            set => SetValue(FilteredSchedulesProperty, value);
        }
        public static readonly DependencyProperty FilteredSchedulesProperty =
            DependencyProperty.Register(nameof(FilteredSchedules), typeof(IEnumerable), typeof(SheetLinkUserControl), new PropertyMetadata(null));

        public object SelectedSchedule
        {
            get => GetValue(SelectedScheduleProperty);
            set => SetValue(SelectedScheduleProperty, value);
        }
        public static readonly DependencyProperty SelectedScheduleProperty =
            DependencyProperty.Register(nameof(SelectedSchedule), typeof(object), typeof(SheetLinkUserControl), new PropertyMetadata(null));

        public string ScheduleSearchText
        {
            get => (string)GetValue(ScheduleSearchTextProperty);
            set => SetValue(ScheduleSearchTextProperty, value);
        }
        public static readonly DependencyProperty ScheduleSearchTextProperty =
            DependencyProperty.Register(nameof(ScheduleSearchText), typeof(string), typeof(SheetLinkUserControl), new PropertyMetadata(""));
        public bool ShouldOpenDropDown
        {
            get => (bool)GetValue(ShouldOpenDropDownProperty);
            set => SetValue(ShouldOpenDropDownProperty, value);
        }
        public static readonly DependencyProperty ShouldOpenDropDownProperty =
            DependencyProperty.Register(nameof(ShouldOpenDropDown), typeof(bool), typeof(SheetLinkUserControl), new PropertyMetadata(false));
        public bool IsActiveViewSelected
        {
            get => (bool)GetValue(IsActiveViewSelectedProperty);
            set => SetValue(IsActiveViewSelectedProperty, value);
        }
        public static readonly DependencyProperty IsActiveViewSelectedProperty =
            DependencyProperty.Register(nameof(IsActiveViewSelected), typeof(bool), typeof(SheetLinkUserControl), new PropertyMetadata(false));

        public bool IsSelectScheduleSelected
        {
            get => (bool)GetValue(IsSelectScheduleSelectedProperty);
            set => SetValue(IsSelectScheduleSelectedProperty, value);
        }
        public static readonly DependencyProperty IsSelectScheduleSelectedProperty =
            DependencyProperty.Register(nameof(IsSelectScheduleSelected), typeof(bool), typeof(SheetLinkUserControl), new PropertyMetadata(false));


        public static readonly DependencyProperty IOButtonTextProperty =
        DependencyProperty.Register(
            "IOButtonText",
            typeof(string),
            typeof(SheetLinkUserControl),
            new PropertyMetadata(string.Empty)
        );

        // CLR wrapper
        public string IOButtonText
        {
            get => (string)GetValue(IOButtonTextProperty);
            set => SetValue(IOButtonTextProperty, value);
        }

        public ICommand ExportCommand
        {
            get => (ICommand)GetValue(ExportCommandProperty);
            set => SetValue(ExportCommandProperty, value);
        }
        public static readonly DependencyProperty ExportCommandProperty =
            DependencyProperty.Register(nameof(ExportCommand), typeof(ICommand), typeof(SheetLinkUserControl), new PropertyMetadata(null));

        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }
        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(nameof(CancelCommand), typeof(ICommand), typeof(SheetLinkUserControl), new PropertyMetadata(null));

        public ICommand BrowseSaveLocationCommand
        {
            get => (ICommand)GetValue(BrowseSaveLocationCommandProperty);
            set => SetValue(BrowseSaveLocationCommandProperty, value);
        }
        public static readonly DependencyProperty BrowseSaveLocationCommandProperty =
            DependencyProperty.Register(nameof(BrowseSaveLocationCommand), typeof(ICommand), typeof(SheetLinkUserControl), new PropertyMetadata(null));
    }
}
