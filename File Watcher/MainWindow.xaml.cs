using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Forms;

namespace File_Watcher
{
    public class FileSystemItem
    {
        public string Name { get; set; }
        public double Duration { get; set; }
        public string Notes { get; set; }
        public ObservableCollection<FileSystemItem> SubItems { get; set; }
        public FileSystemItem()
        {
            SubItems = new ObservableCollection<FileSystemItem>();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<FileSystemItem> nodes;
        IDisposable disposable;

        public MainWindow()
        {
            InitializeComponent();

            nodes = new ObservableCollection<FileSystemItem>();

            filesTree.ItemsSource = nodes;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog();
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                nodes.Clear();
                var rootStore = new Dictionary<int, FileSystemItem>();
                var fileWatcher = new FileWatcher(openDialog.SelectedPath);
                disposable = fileWatcher.Subscribe(x =>
                {
                    System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                    {
                        if (rootStore.TryGetValue(x.ParentId, out FileSystemItem node))
                        {
                            var newNode = new FileSystemItem { Name = x.Name, Duration = x.Type.GetHashCode() };
                            node.SubItems.Add(newNode);
                            if (x.Type == FileSystemType.Directory)
                                rootStore.Add(x.Id, newNode);
                        }
                        else
                        {
                            var newNode = new FileSystemItem { Name = x.Name, Duration = x.Type.GetHashCode() };
                            nodes.Add(newNode);
                            if (x.Type == FileSystemType.Directory)
                                rootStore.Add(x.Id, newNode);
                        }
                    }));
                });
                fileWatcher.Publish();
                //disposable.Dispose();
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            disposable.Dispose();
        }
    }
}