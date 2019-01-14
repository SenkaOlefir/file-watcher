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

        public static FileSystemEntity FromDirectory(DirectoryInfo directory)
        {
            return new FileSystemEntity
            {
                Name = directory.Name,
                CreationDate = directory.CreationTime,
                ModificationType = directory.LastWriteTime,
                LastAccessType = directory.LastAccessTime,
                LastAccessType = directory.LastAccessTime,
                Attributes = directory.Attributes,
                Owner = System.IO.File.GetAccessControl(directory.FullName).GetOwner(typeof(System.Security.Principal.NTAccount)).ToString(),
                Owner = directory.GetAccessControl()
            };
        }

        public int Id { get; private set; }
        public int ParentId { get; private set; }
        public string Name { get; private set; }
        public FileSystemType Type { get; private set; }
    }

    public class FileWatcher : BroadcastObservable<FileSystemEntity>
    {
        private readonly string _path;
        private readonly Lazy<CompositeObservableDisposer<FileSystemEntity>> _disposer;

        public FileWatcher(string path)
        {
            _path = path;
            _disposer = new Lazy<CompositeObservableDisposer<FileSystemEntity>>(() =>
                new CompositeObservableDisposer<FileSystemEntity>(this));

            WorkingThread = new Thread(new ThreadStart(PublishInternal));
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
                    var info = new DirectoryInfo(item);
                    EnqueueNext(new FileSystemEntity(id, parentId, info.Name, FileSystemType.Directory));
                    BrowseDirectory(item, id, ref id);
                }

                foreach (var fileItem in Directory.GetFiles(directoryPath))
                {
                    if (State == ObservableState.Stopping)
                    {
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