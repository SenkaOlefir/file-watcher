using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.AccessControl;

namespace FileWatcher.UI
{
    public class FileSystemItem
    {
        public static FileSystemItem FromDirectory(FileSystemEntity entity)
        {
            return new FileSystemItem
            {
                Name = entity.Name,
                CreationDate = entity.CreationDate,
                ModificationTime = entity.CreationDate,
                LastAccessTime = entity.LastAccessTime,
                Attributes = entity.Attributes,
                Owner = entity.Owner,
                Permissions = entity.Permissions,
            };
        }

        public string Name { get; private set; }
        public FileSystemType Type { get; private set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public FileAttributes Attributes { get; set; }
        public string Owner { get; set; }
        public FileSystemRights Permissions { get; set; }
        public ObservableCollection<FileSystemItem> SubItems { get; set; }

        public FileSystemItem()
        {
            SubItems = new ObservableCollection<FileSystemItem>();
        }
    }
}