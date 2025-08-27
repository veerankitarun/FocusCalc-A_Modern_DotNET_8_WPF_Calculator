using System.Collections.ObjectModel;
using System.Windows;

namespace Calculator
{
    public partial class HistoryWindow : Window
    {
        public HistoryWindow() { InitializeComponent(); }

        public void Bind(ObservableCollection<HistoryEntry> items)
        {
            List.ItemsSource = items;
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (List.ItemsSource is ObservableCollection<HistoryEntry> items)
                items.Clear();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
