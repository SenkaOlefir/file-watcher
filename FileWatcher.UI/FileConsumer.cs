using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace FileWatcher.UI
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
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new Action(delegate
                {
                    AddNode(_rootStore.TryGetValue(x.ParentId, out FileSystemItem node) ? node.SubItems : _nodes,
                        x);
                }), DispatcherPriority.ApplicationIdle);
        }

        private void AddNode(ObservableCollection<FileSystemItem> collection, FileSystemEntity entity)
        {
            var newNode = FileSystemItem.FromEntity(entity);
            collection.Add(newNode);
            if (entity.Type == FileSystemType.Directory)
                _rootStore.Add(entity.Id, newNode);
        }

        public void OnError(Exception error)
        {
            //{DateTime.Now} will make an additional allocation in heap
            //we need write {DateTime.Now.ToString()} to avoid it
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new Action(delegate { _logger.Error($"{DateTime.Now} - en error occured with message", error); }),
                DispatcherPriority.ApplicationIdle);
        }

        public void OnCompleted()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(
                new Action(delegate { _logger.Info($"{DateTime.Now} - Completed!"); }),
                DispatcherPriority.ApplicationIdle);
        }
    }
}