using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using ReactivePlatform;

namespace File_Watcher
{
    public enum FileSystemType
    {
        File = 0,
        Directory = 1
    }

    public struct FileSystemEntity
    {
        public static FileSystemEntity FromDirectory(DirectoryInfo directory, int id, int parentId)
        {
            return new FileSystemEntity
            {
                Id = id,
                ParentId = parentId,
                Name = directory.Name,
                CreationDate = directory.CreationTime,
                ModificationTime = directory.LastWriteTime,
                LastAccessTime = directory.LastAccessTime,
                Attributes = directory.Attributes,
                Owner = File.GetAccessControl(directory.FullName).GetOwner(typeof(NTAccount)).ToString(),
                Permissions = DirectoryHasPermission(directory.FullName),
                Type = FileSystemType.Directory
            };
        }

        public static FileSystemEntity FromFile(FileInfo fileInfo, int parentId)
        {
            return new FileSystemEntity
            {
                ParentId = parentId,
                Name = fileInfo.Name,
                CreationDate = fileInfo.CreationTime,
                ModificationTime = fileInfo.LastWriteTime,
                LastAccessTime = fileInfo.LastAccessTime,
                Attributes = fileInfo.Attributes,
                Owner = File.GetAccessControl(fileInfo.FullName).GetOwner(typeof(NTAccount)).ToString(),
                Permissions = DirectoryHasPermission(fileInfo.FullName),
                Type = FileSystemType.File
            };
        }

        public int Id { get; private set; }
        public int ParentId { get; private set; }
        public string Name { get; private set; }
        public FileSystemType Type { get; private set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public FileAttributes Attributes { get; set; }
        public string Owner { get; set; }
        public FileSystemRights Permissions { get; set; }

        public static FileSystemRights DirectoryHasPermission(string directoryPath)
        {
            FileSystemRights rights = 0x0;
            AuthorizationRuleCollection rules = Directory.GetAccessControl(directoryPath)
                .GetAccessRules(true, true, typeof(SecurityIdentifier));
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            foreach (FileSystemAccessRule rule in rules)
            {
                if (identity.Groups != null && identity.Groups.Contains(rule.IdentityReference))
                {
                    rights |= rule.FileSystemRights;
                }
            }

            return rights;
        }
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