using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;
using DialogBoxResult = System.Windows.Forms.DialogResult;

namespace FileWatcher.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<FileSystemItem> _nodes;
        private readonly FolderBrowserDialog _folderBrowserDialog;
        private readonly SaveFileDialog _saveFileDialog;
        private IDisposable _disposable;

        public MainWindow()
        {
            InitializeComponent();

            _nodes = new ObservableCollection<FileSystemItem>();

            _folderBrowserDialog = new FolderBrowserDialog();
            _saveFileDialog = new SaveFileDialog();

            FilesTree.ItemsSource = _nodes;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openDialogResult = _folderBrowserDialog.ShowDialog();
            if (openDialogResult == DialogBoxResult.OK)
            {
                var saveDialogResult = _saveFileDialog.ShowDialog();
                if (saveDialogResult == DialogBoxResult.OK)
                {
                    _nodes.Clear();
                    //we actually don't need to dispose observers after 
                    _disposable?.Dispose();
                    var fileWatcher = new FileWatcherObservable(_folderBrowserDialog.SelectedPath);
                    _disposable = fileWatcher.Subscribe(new FileConsumer(_nodes, new UiLogger(LogContainer)));
                    _disposable = fileWatcher.Subscribe(new FileXmlConsumer(_saveFileDialog.FileName));
                    fileWatcher.Publish();
                }
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