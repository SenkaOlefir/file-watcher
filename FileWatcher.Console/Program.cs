using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactivePlatform;

namespace FileWatcher.Consol
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main thread";
            Console.WriteLine($"Start execution in thread {Thread.CurrentThread.Name}");
            BroadcastObservable<int> observable = Enumerable.Range(0, 10).ToObservable();

            var disposable = observable.Subscribe(x => {
                Thread.Sleep(5000);
                Console.WriteLine($"Value received from {Thread.CurrentThread.ManagedThreadId}. Value {x}");
            });

            observable.Subscribe(x => {
                Thread.Sleep(3000);
                Console.WriteLine($"Value received from {Thread.CurrentThread.ManagedThreadId}. Value {x}");
            });

            observable.Publish();

            Console.ReadLine();
            disposable.Dispose();
        }
    }
}
