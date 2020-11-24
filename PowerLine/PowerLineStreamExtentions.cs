using System;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PowerLine
{
    public static class PowerLineStreamExtentions
    {
        public static async Task<byte[]> ReadToEndAsync(this Stream stream)
        {
            MemoryStream tempStream = new MemoryStream();
            await stream.CopyToAsync(tempStream);
            tempStream.Position = 0;
            return tempStream.ToArray();
        }
        public static async Task<string> ReadToEndStringAsync(this Stream stream) => System.Text.Encoding.UTF8.GetString(await stream.ReadToEndAsync());

        public static bool ReadBySize(this Stream stream, byte[] buffer, int startIndex, int length)
        {
            int readed = 0;
            int current;
            while(readed < length)
            {
                current = stream.Read(buffer, readed, length - readed);
                if (current == 0) return false;
                readed += current;
            }
            return true;
        }

    }
}
