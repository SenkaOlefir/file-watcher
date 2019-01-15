using System;
using System.IO;
using System.Threading;
using ReactivePlatform;

namespace FileWatcher
{
    public class FileWatcherObservable : BroadcastObservable<FileSystemEntity>
    {
        private readonly string _path;
        private readonly Lazy<CompositeObservableDisposer<FileSystemEntity>> _disposer;

        public FileWatcherObservable(string path)
        {
            _path = path;
            _disposer = new Lazy<CompositeObservableDisposer<FileSystemEntity>>(() =>
                new CompositeObservableDisposer<FileSystemEntity>(this));

            WorkingThread = new Thread(PublishInternal);
        }

        public override IDisposable Subscribe(IObserver<FileSystemEntity> observer)
        {
            Subscribers.Add(new ObserverWrapper(observer));

            return _disposer.Value;
        }

        private void BrowseDirectory(string directoryPath, int parentId, ref int id)
        {
            try
            {
                foreach (var item in Directory.GetDirectories(directoryPath))
                {
                    if (State == ObservableState.Stopping)
                    {
                        break;
                    }

                    id++;
                    var directoryInfo = new DirectoryInfo(item);
                    EnqueueNext(FileSystemEntity.FromDirectory(directoryInfo, id, parentId));
                    BrowseDirectory(item, id, ref id);
                }

                foreach (var fileItem in Directory.GetFiles(directoryPath))
                {
                    if (State == ObservableState.Stopping)
                    {
                        break;
                    }

                    var fileInfo = new FileInfo(fileItem);
                    EnqueueNext(FileSystemEntity.FromFile(fileInfo, parentId));
                }
            }
            catch (Exception ex)
            {
                EnqueueError(ex);
            }
        }

        public override void Publish()
        {
            WorkingThread.Start();
        }

        private void PublishInternal()
        {
            int id = 0;
            BrowseDirectory(_path, -1, ref id);
            EnqueueLast();
        }
    }
}