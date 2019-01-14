using RactivePlatform;
using System;
using System.IO;
using System.Threading;

namespace File_Watcher
{
    public enum FileSystemType
    {
        File = 0,
        Directory = 1
    }
    public struct FileSystemEntity
    {
        public FileSystemEntity(int id, int parentId, string name, FileSystemType type) : this()
        {
            Id = id;
            ParentId = parentId;
            Name = name;
            Type = type;
        }

        public int Id { get; }
        public int ParentId { get; }
        public string Name { get; }
        public FileSystemType Type { get; }
    }

    public class FileWatcher : BroadcastObservable<FileSystemEntity>, IBroadcastObservable<FileSystemEntity>
    {
        private Lazy<CompositeObservableDisposer<FileSystemEntity>> disposer;
 

        private readonly string _path;

        public FileWatcher(string path)
        {
            _path = path;

            WorkingThread = new Thread(new ThreadStart(PublishInternal));
            disposer = new Lazy<CompositeObservableDisposer<FileSystemEntity>>(() => new CompositeObservableDisposer<FileSystemEntity>(this));
        }

        public override IDisposable Subscribe(IObserver<FileSystemEntity> observer)
        {
            Subscribers.Add(new ObserverWrapper(observer));

            return disposer.Value;
        }

        private void BrowseDirectory(string directoryPath, int parentId, ref int id)
        {
            try
            {
                foreach (var item in Directory.GetDirectories(directoryPath))
                {
                    if (State == ObservableState.Stopping)
                    {
                        EnqueueLast();
                        break;
                    }
                    id++;
                    EnqueueNext(new FileSystemEntity(id, parentId, new DirectoryInfo(item).Name, FileSystemType.Directory));
                    BrowseDirectory(item, id, ref id);
                }

                foreach (var fileItem in Directory.GetFiles(directoryPath))
                {
                    if (State == ObservableState.Stopping)
                    {
                        EnqueueLast();
                        break;
                    }
                    EnqueueNext(new FileSystemEntity(0, parentId, new FileInfo(fileItem).Name, FileSystemType.File));
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