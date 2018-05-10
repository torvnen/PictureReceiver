using System;
using System.Collections.Generic;
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
    public class MessageFrame
    {
        public static readonly byte PaddingByte = 0;
        public static readonly int LengthMessageLength = 32;
        public static readonly int LengthFileName = 256;
        public static readonly int LengthFileType = 1;
        public static readonly int LengthTotal = 1024;
        public static int LengthPadding => 
            LengthTotal - LengthFileType - LengthFileName - LengthMessageLength;

        public int MessageLength { get; set; }
        public string FileName { get; set; }
        public  FileTypes FileType { get; set; }

        public MessageFrame(byte[] bytes)
        {
            System.Diagnostics.Debug.Assert(bytes.Length == LengthTotal);

            MessageLength = Convert.ToInt32(bytes.Take(LengthMessageLength).ToArray());
            FileName = Encoding.ASCII.GetString(bytes.Take(LengthFileName).ToArray());
            FileType = (FileTypes)Convert.ToInt16(bytes.Take(LengthFileType));
        }

        public byte[] AsBytes()
        {
            var bytesTotal = new List<byte>();

            var bytesFileType = BitConverter.GetBytes((int)FileType)[0];
            var bytesMessageLength = BitConverter.GetBytes(MessageLength);
            var bytesFileName = new List<byte>(Encoding.ASCII.GetBytes(FileName));
            
            // Pad filename byte array
            while (bytesFileName.Count < MessageFrame.LengthFileName)
            {
                bytesFileName.Add(MessageFrame.PaddingByte);
            }

            // Add all data to total bytes
            bytesTotal.Add(bytesFileType);
            bytesTotal.AddRange(bytesMessageLength);
            bytesTotal.AddRange(bytesFileName);

            while (bytesTotal.Count < MessageFrame.LengthFileName)
            {
                bytesFileName.Add(MessageFrame.PaddingByte);
            }

            return bytesTotal.ToArray();
        }
    }
}
