using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;

namespace FileWatcher.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<FileSystemItem> _nodes;
        IDisposable _disposable;

        public MainWindow()
        {
            InitializeComponent();

            _nodes = new ObservableCollection<FileSystemItem>();

            FilesTree.ItemsSource = _nodes;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog();
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                _nodes.Clear();
                _disposable?.Dispose();
                var fileWatcher = new FileWatcherObservable(openDialog.SelectedPath);
                _disposable = fileWatcher.Subscribe(new FileConsumer(_nodes, new UiLogger(LogContainer)));
                _disposable = fileWatcher.Subscribe(new FileXmlConsumer("D:/File.xml"));
                fileWatcher.Publish();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _disposable?.Dispose();
        }
    }
}