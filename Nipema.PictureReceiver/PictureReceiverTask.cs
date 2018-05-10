using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nipema.PictureReceiver
{
    public class PictureReceiverTask
    {
        private readonly Socket _listener = null;
        
        public PictureReceiverTask(string ipAddr = "192.168.1.50", int portNumber = 7359)
        {
            var localIp = IPAddress.Parse(ipAddr);
            var endPoint = new IPEndPoint(localIp, portNumber);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endPoint);
            _listener.Listen(100);
        }

        public void Run()
        {
            // Buffer for reading data
            byte[] buffer = null;
            int receivedBytesSingle = 0;
            int receivedBytesTotal = 0;
            var fileBytes = new List<byte>();
            
            try
            {                
                Console.Write("Waiting for a connection... ");
                var handler = _listener.Accept();
                Console.WriteLine("Connected!");

                // Empty buffer for receiving the first message (meta data)
                buffer = new byte[1024];
                receivedBytesSingle = handler.Receive(buffer);

                System.Diagnostics.Debug.Assert(receivedBytesSingle == 1024);
                System.Diagnostics.Debug.Assert(receivedBytesTotal == 1024);

                var frame = new MessageFrame(buffer);

                while (handler.Connected && receivedBytesTotal < frame.MessageLength)
                {
                    buffer = new byte[1024];
                    receivedBytesSingle = handler.Receive(buffer);
                    var bytesReceived = buffer.Take(receivedBytesSingle);
                    var charactersReceived = Encoding.ASCII.GetString(bytesReceived.ToArray());

                    Console.WriteLine($"Received {receivedBytesSingle} bytes.");
                    fileBytes.AddRange(bytesReceived);
                }

                Console.WriteLine("End of transfer.");
                System.IO.File.WriteAllBytes(@"D:\kuva.png", fileBytes.ToArray());

                byte[] msg = Encoding.ASCII.GetBytes("<EOF>");
                handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}