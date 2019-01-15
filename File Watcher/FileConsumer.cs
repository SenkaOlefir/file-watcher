using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace File_Watcher
{
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
                    var newNode = FileSystemItem.FromDirectory(x);
                    node.SubItems.Add(newNode);
                    if (x.Type == FileSystemType.Directory)
                        _rootStore.Add(x.Id, newNode);
                }
                else
                {
                    var newNode = FileSystemItem.FromDirectory(x);
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
}