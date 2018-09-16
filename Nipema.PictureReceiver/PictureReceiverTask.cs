using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Nipema.PictureReceiver
{
    public class PictureReceiverTask
    {
        private readonly Socket _listener = null;

        public PictureReceiverTask(int portNumber = 7359)
        {
            string ipAddr = GetLocalIPAddress();
            var localIp = IPAddress.Parse(ipAddr);
            var endPoint = new IPEndPoint(localIp, portNumber);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(endPoint);
            _listener.Listen(100);
        }

        public static string GetLocalIPAddress()
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var i in interfaces)
            {
                if (i.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                    && i.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProps = i.GetIPProperties();
                    foreach (var ac in ipProps.UnicastAddresses)
                    {
                        if (ac.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            return ac.Address.ToString();
                        }
                    }
                }
            }
            throw new Exception("Pitää olla wifi yhteys tai sitten koodia pitää muuttaa (PictureReceiverTask.cs:35)");
        }

        public void Run()
        {
            // Buffer for reading data
            byte[] buffer = null;
            int receivedBytesSingle = 0;
            var fileBytes = new List<byte>();

            try
            {
                Console.Write("Waiting for a connection... ");
                var handler = _listener.Accept();
                Console.WriteLine("Connected!");

                // Empty buffer for receiving the first message (meta data)
                buffer = new byte[1024];
                receivedBytesSingle = handler.Receive(buffer);

                System.Diagnostics.Debug.Assert(receivedBytesSingle == 1024, "RECEIVED NO HEADER.");

                var header = new MessageHeader(buffer);
                Console.WriteLine(header.ToString());

                while (handler.Connected && fileBytes.Count < header.MessageLength)
                {
                    buffer = new byte[1024];
                    receivedBytesSingle = handler.Receive(buffer);

                    if (receivedBytesSingle == 0) break;

                    var bytesReceived = buffer.Take(receivedBytesSingle);
                    var charactersReceived = Encoding.ASCII.GetString(bytesReceived.ToArray());

                    Console.WriteLine($"Received {receivedBytesSingle} bytes. Total received: {fileBytes.Count} / {header.MessageLength}");
                    fileBytes.AddRange(bytesReceived);
                }

                Console.WriteLine("End of transfer.");
                foreach (var fileType in header.FileTypes)
                {
                    System.IO.File.WriteAllBytes
                    (
                        "C:\\Nipema\\tuotekuvat\\" + header.FileName + "_" + (int)fileType + ".JPEG",
                        fileBytes.ToArray()
                    );
                }

                //byte[] msg = Encoding.ASCII.GetBytes("<EOF>");
                //handler.Send(msg);
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

                Run();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}