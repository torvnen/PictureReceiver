using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nipema.PictureReceiver
{
    public enum FileTypes
    {
        Ripustusohje = 1,
        Maalausohje,
        Purkuohje
    }
    public class MessageHeader
    {
        public static readonly byte PaddingByte = 0;
        public static readonly int LengthMessageLength = 16;
        public static readonly int LengthFileName = 256;
        public static readonly int LengthFileType = 1;
        public static readonly int LengthTotal = 1024;
        public static int LengthPadding => 
            LengthTotal - LengthFileType - LengthFileName - LengthMessageLength;

        public int MessageLength { get; set; }
        public string FileName { get; set; }
        public  FileTypes FileType { get; set; }

        public MessageHeader(byte[] bytes)
        {
            // FILETYPE || MESSAGELENGTH || FILENAME
            System.Diagnostics.Debug.Assert(bytes.Length == LengthTotal, "MESSAGE HEADER IS TOO SHORT.");
            
            var bytesTaken = 0;

            var fileTypeBytes = bytes.Skip(bytesTaken).Take(LengthFileType).ToArray()[0];
            bytesTaken += LengthFileType;
            FileType = (FileTypes)fileTypeBytes;

            var messageLengthBytes = bytes.Skip(bytesTaken).Take(LengthMessageLength).ToArray();
            bytesTaken += LengthMessageLength;
            MessageLength = int.Parse(Encoding.ASCII.GetString(messageLengthBytes).Trim('a'));

            var fileNameBytes = bytes.Skip(bytesTaken).Take(LengthFileName).ToArray();
            FileName = Encoding.ASCII.GetString(fileNameBytes).TrimEnd((char)PaddingByte);
            bytesTaken += LengthFileName;
        }

        public override string ToString()
        {
            return 
                $"MessageFrame: File type: {FileType.ToString()}, " +
                $"Message length: {MessageLength}, " +
                $"Filename: {FileName}";
        }

        public byte[] AsBytes()
        {
            var bytesTotal = new List<byte>();

            var bytesFileType = BitConverter.GetBytes((int)FileType)[0];
            var bytesMessageLength = BitConverter.GetBytes(MessageLength);
            var bytesFileName = new List<byte>(Encoding.ASCII.GetBytes(FileName));
            
            // Pad filename byte array
            while (bytesFileName.Count < MessageHeader.LengthFileName)
            {
                bytesFileName.Add(MessageHeader.PaddingByte);
            }

            // Add all data to total bytes
            bytesTotal.Add(bytesFileType);
            bytesTotal.AddRange(bytesMessageLength);
            bytesTotal.AddRange(bytesFileName);

            while (bytesTotal.Count < MessageHeader.LengthFileName)
            {
                bytesFileName.Add(MessageHeader.PaddingByte);
            }

            return bytesTotal.ToArray();
        }
    }
}
