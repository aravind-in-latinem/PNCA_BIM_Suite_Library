using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using MVVM = CommunityToolkit.Mvvm.Input;
using PNCA_BIM_Suite_Library.Model;

namespace PNCA_BIM_Suite_Library.ViewModel.PNCAExporter
{
    public class CustomPrintOrderViewModel: ObservableObject
    {
        private ObservableCollection<IViewSheetItem> _sortableViews;        
        private ObservableCollection<IViewSheetItem> _selectedViews;
        public CustomPrintOrderViewModel(List<IViewSheetItem> sortableViews)
        {
            SortableViews = new ObservableCollection<IViewSheetItem>(sortableViews);
            MoveUpCommand = new MVVM.RelayCommand(ExecuteMoveUp, CanMoveUp);
            MoveDownCommand = new MVVM.RelayCommand(ExecuteMoveDown, CanMoveDown);
            MoveToFirstCommand = new MVVM.RelayCommand(ExecuteMoveToFirst, CanMoveToFirst);
            MoveToLastCommand = new MVVM.RelayCommand(ExecuteMoveToLast, CanMoveToLast);
            SelectedViews = new ObservableCollection<IViewSheetItem>();
            SelectedViews.CollectionChanged += (sender, e) =>
            {
                MoveUpCommand.NotifyCanExecuteChanged();
                MoveDownCommand.NotifyCanExecuteChanged();
                MoveToFirstCommand.NotifyCanExecuteChanged();
                MoveToLastCommand.NotifyCanExecuteChanged();
            };
        }
        public MVVM.IRelayCommand MoveUpCommand { get; }
        public MVVM.IRelayCommand MoveDownCommand { get; }
        public MVVM.IRelayCommand MoveToFirstCommand { get; }
        public MVVM.IRelayCommand MoveToLastCommand { get; }

        public ObservableCollection<IViewSheetItem> SelectedViews
        {
            get => _selectedViews;
            set => SetProperty(ref _selectedViews, value);            
        }

        public ObservableCollection<IViewSheetItem> SortableViews { 
            get=>_sortableViews;
            set => SetProperty(ref _sortableViews, value);
        }
        private void ExecuteMoveUp()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return;

            // Get selected items ordered by their current index (top → bottom)
            var orderedSelection = SelectedViews
                .Select(item => new
                {
                    Item = item,
                    Index = SortableViews.IndexOf(item)
                })
                .Where(x => x.Index > 0)   // can't move items already at top
                .OrderBy(x => x.Index)
                .ToList();

            foreach (var entry in orderedSelection)
            {
                SortableViews.Move(entry.Index, entry.Index - 1);
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }
        private bool CanMoveUp()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return false;

            return SelectedViews
                .Any(item => SortableViews.IndexOf(item) > 0);
        }


        private void ExecuteMoveDown()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return;

            var orderedSelection = SelectedViews
                .Select(item => new
                {
                    Item = item,
                    Index = SortableViews.IndexOf(item)
                })
                .Where(x => x.Index < SortableViews.Count - 1)
                .OrderByDescending(x => x.Index)
                .ToList();

            foreach (var entry in orderedSelection)
            {
                SortableViews.Move(entry.Index, entry.Index + 1);
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }
        private bool CanMoveDown()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return false;

            int lastIndex = SortableViews.Count - 1;

            return SelectedViews
                .Any(item => SortableViews.IndexOf(item) < lastIndex);
        }

        private void ExecuteMoveToFirst()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return;

            var orderedSelection = SelectedViews
                .OrderBy(item => SortableViews.IndexOf(item))
                .ToList();

            int insertIndex = 0;

            foreach (var item in orderedSelection)
            {
                int currentIndex = SortableViews.IndexOf(item);
                SortableViews.Move(currentIndex, insertIndex);
                insertIndex++;
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }
        private bool CanMoveToFirst()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return false;

            return SelectedViews
                .Any(item => SortableViews.IndexOf(item) > 0);
        }

        private void ExecuteMoveToLast()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return;

            var orderedSelection = SelectedViews
                .OrderByDescending(item => SortableViews.IndexOf(item))
                .ToList();

            int lastIndex = SortableViews.Count - 1;

            foreach (var item in orderedSelection)
            {
                int currentIndex = SortableViews.IndexOf(item);
                SortableViews.Move(currentIndex, lastIndex);
            }
            MoveUpCommand.NotifyCanExecuteChanged();
            MoveDownCommand.NotifyCanExecuteChanged();
            MoveToFirstCommand.NotifyCanExecuteChanged();
            MoveToLastCommand.NotifyCanExecuteChanged();
        }

        private bool CanMoveToLast()
        {
            if (SelectedViews == null || SelectedViews.Count == 0)
                return false;

            int lastIndex = SortableViews.Count - 1;

            return SelectedViews
                .Any(item => SortableViews.IndexOf(item) < lastIndex);
        }
    }
}
