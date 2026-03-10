using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PNCA_BIM_Suite_Library.View
{
    public partial class PNCATitleBar : UserControl
    {
        public PNCATitleBar()
        {
            InitializeComponent();
        }

        // --------------------------
        // Dependency Properties
        // --------------------------

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PNCATitleBar), new PropertyMetadata(""));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleIconProperty =
            DependencyProperty.Register("TitleIcon", typeof(ImageSource), typeof(PNCATitleBar), new PropertyMetadata(null));

        public ImageSource TitleIcon
        {
            get { return (ImageSource)GetValue(TitleIconProperty); }
            set { SetValue(TitleIconProperty, value); }
        }

        private Window GetWindow()
            => Window.GetWindow(this);

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            var window = GetWindow();
            if (window != null)
                window.WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            GetWindow()?.Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                return;

            GetWindow()?.DragMove();
        }
    }
}
