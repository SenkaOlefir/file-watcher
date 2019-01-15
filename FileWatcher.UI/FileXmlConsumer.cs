using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace FileWatcher.UI
{
    public class FileXmlConsumer : IObserver<FileSystemEntity>
    {
        private readonly string _fileName;
        private readonly XDocument _document = new XDocument(new XElement("Root"));
        private readonly Dictionary<int, XElement> _rootStore = new Dictionary<int, XElement>();

        public FileXmlConsumer(string fileName)
        {
            _fileName = fileName;
        }

        public void OnNext(FileSystemEntity x)
        {
            AddNode(_rootStore.TryGetValue(x.ParentId, out XElement element) ? element : _document.Root, x);
        }

        private XElement CreateXElement(FileSystemEntity entity)
        {
            string name = XmlConvert.EncodeName(entity.Name);
            var element = new XElement(XName.Get(name));
            element.Add(new XAttribute("Owner", entity.Owner));
            element.Add(new XAttribute("LastAccessTime", entity.LastAccessTime));
            return element;
        }

        private void AddNode(XElement element, FileSystemEntity entity)
        {
            var newElement = CreateXElement(entity);
            element.Add(newElement);
            if (entity.Type == FileSystemType.Directory)
                _rootStore.Add(entity.Id, newElement);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
            using (var fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write))
            {
                _document.Save(fileStream);
            }
        }
    }
}