using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    public static class ListBoxSelectedItemsBehavior
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ListBoxSelectedItemsBehavior),
                new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject element, IList value)
            => element.SetValue(SelectedItemsProperty, value);

        public static IList GetSelectedItems(DependencyObject element)
            => (IList)element.GetValue(SelectedItemsProperty);

        private static void OnSelectedItemsChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ListBox))
                return;
            ListBox listBox = (ListBox)d;
            listBox.SelectionChanged -= ListBox_SelectionChanged;
            listBox.SelectionChanged += ListBox_SelectionChanged;
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            var boundCollection = GetSelectedItems(listBox);
            if (boundCollection == null)
                return;

            boundCollection.Clear();
            foreach (var item in listBox.SelectedItems)
                boundCollection.Add(item);
        }
    }
}
