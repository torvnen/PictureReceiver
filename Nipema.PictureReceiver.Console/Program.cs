using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nipema.PictureReceiver.Console
{
    class Program
    {
        private static Thread _mainThread = null;
        private static PictureReceiverTask _receiver;

        static void Main(string[] args)
        {
            _receiver = new PictureReceiverTask(portNumber: 7362);

            _mainThread = new Thread(_receiver.Run);


            _mainThread.Start();

            System.Console.ReadKey();
        }
    }
}
