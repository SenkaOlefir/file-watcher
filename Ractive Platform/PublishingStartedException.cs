using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactivePlatform
{
    public class PublishingStartedException : Exception
    {
        public PublishingStartedException(string message) : base(message)
        {

        }
    }
}