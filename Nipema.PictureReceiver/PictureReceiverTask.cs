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
            var fileBytes = new List<byte>();

            try
            {                
                Console.Write("Waiting for a connection... ");
                var handler = _listener.Accept();
                Console.WriteLine("Connected!");

                while (handler.Connected)
                {
                    buffer = new byte[1024];
                    var receiveLength = handler.Receive(buffer);
                    var bytesReceived = buffer.Take(receiveLength);
                    var charactersReceived = Encoding.ASCII.GetString(bytesReceived.ToArray());

                    Console.WriteLine($"Received {receiveLength} bytes.");
                    if (charactersReceived.Contains("<EOF>"))
                    {
                        var eofLength = Encoding.ASCII.GetByteCount("<EOF>");
                        var interestingBytesCount = bytesReceived.Count() - eofLength;
                        fileBytes.AddRange(bytesReceived.Take(interestingBytesCount));

                        Console.WriteLine("Received <EOF>, removed {0} bytes. Added {1}", eofLength, interestingBytesCount);
                        break;
                    }

                    fileBytes.AddRange(bytesReceived);

                    Console.WriteLine($"Received {receiveLength} bytes.");
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