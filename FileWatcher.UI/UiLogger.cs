using System;
using System.Windows.Controls;
using System.Windows.Documents;

namespace FileWatcher.UI
{
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
}