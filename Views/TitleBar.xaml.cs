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

namespace PNCA_BIM_Suite_Library.Views
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {
        public TitleBar()
        {
            InitializeComponent();
        }    
    
        public ICommand Minimize_Click
        {
            get => (ICommand)GetValue(Minimize_ClickProperty);
            set => SetValue(Minimize_ClickProperty, value);
        }
        public static readonly DependencyProperty Minimize_ClickProperty =
            DependencyProperty.Register(nameof(Minimize_Click), typeof(ICommand), typeof(TitleBar), new PropertyMetadata(null));

        public ICommand Close_Click
        {
            get => (ICommand)GetValue(Close_ClickProperty);
            set => SetValue(Close_ClickProperty, value);
        }
        public static readonly DependencyProperty Close_ClickProperty =
            DependencyProperty.Register(nameof(Close_Click), typeof(ICommand), typeof(TitleBar), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleBarTextProperty =
        DependencyProperty.Register(
            "TitleBarText",
            typeof(string),
            typeof(TitleBar),
            new PropertyMetadata(string.Empty)
        );

        // CLR wrapper
        public string TitleBarText
        {
            get => (string)GetValue(TitleBarTextProperty);
            set => SetValue(TitleBarTextProperty, value);
        }

        public string IconPath
        {
            get { return (string)GetValue(IconPathProperty); }
            set { SetValue(IconPathProperty, value); }
        }

        public static readonly DependencyProperty IconPathProperty =
            DependencyProperty.Register(
                nameof(IconPath),
        typeof(string),
                typeof(TitleBar),
                new PropertyMetadata(null)
            );

    }
}
