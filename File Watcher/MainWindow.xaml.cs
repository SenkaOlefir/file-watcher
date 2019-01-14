using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Threading;

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

    public interface ILog
    {
        void Info(string message);
        void Error(string message, Exception exception);
    }

    public class UiLogger : ILog
    {
        private readonly TextBlock _textBlock;

        public UiLogger(TextBlock textBlock)
        {
            _textBlock = textBlock;
        }

        public void Info(string message) => AddMessage(message);

        public void Error(string message, Exception exception) => AddMessage(message, exception.Message);

        //array allocation here
        //we need methods overloading for every parameters count
        private void AddMessage(params string[] messages)
        {
            _textBlock.Inlines.Add(string.Join(" ", messages));
            _textBlock.Inlines.Add(new LineBreak());
        }
    }

    public class FileConsumer : IObserver<FileSystemEntity>
    {
        private readonly ILog _logger;
        private readonly ObservableCollection<FileSystemItem> _nodes;
        private readonly Dictionary<int, FileSystemItem> _rootStore = new Dictionary<int, FileSystemItem>();

        public FileConsumer(ObservableCollection<FileSystemItem> nodes, ILog logger)
        {
            _nodes = nodes;
            _logger = logger;
        }

        public void OnNext(FileSystemEntity x)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                if (_rootStore.TryGetValue(x.ParentId, out FileSystemItem node))
                {
                    var newNode = new FileSystemItem {Name = x.Name, Duration = x.Type.GetHashCode()};
                    node.SubItems.Add(newNode);
                    if (x.Type == FileSystemType.Directory)
                        _rootStore.Add(x.Id, newNode);
                }
                else
                {
                    var newNode = new FileSystemItem {Name = x.Name, Duration = x.Type.GetHashCode()};
                    _nodes.Add(newNode);
                    if (x.Type == FileSystemType.Directory)
                        _rootStore.Add(x.Id, newNode);
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        public void OnError(Exception error)
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new Action(delegate { _logger.Error($"{DateTime.Now} - en error occured with message", error); }),
                DispatcherPriority.ApplicationIdle);
            //{DateTime.Now} will make an additional allocation in heap
            //we need write {DateTime.Now.ToString()} to avoid it
        }

        public void OnCompleted()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new Action(delegate { _logger.Info($"{DateTime.Now} - Completed!"); }),
                DispatcherPriority.ApplicationIdle);
        }
    }

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

            filesTree.ItemsSource = _nodes;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new FolderBrowserDialog();
            var dialogResult = openDialog.ShowDialog();
            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                _nodes.Clear();
                var fileWatcher = new FileWatcher(openDialog.SelectedPath);
                _disposable = fileWatcher.Subscribe(new FileConsumer(_nodes, new UiLogger(LogContainer)));
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