using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RactivePlatform;
//using RactivePlatform;

namespace FileWatcher.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var observable = new Observable();

            observable.Subscribe(x => {
                Console.Write(x);
            });

            Thread.Sleep(5000);
        }
    }
}
