using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDMXUSBClient
{
    class Program
    {
        static void Main(string[] args)
        {
            DmxClient c = new DmxClient();
            string s;
            do
            {
                s = Console.ReadLine();
            } while (s != "exit");
            c.Close();
        }
    }
}
