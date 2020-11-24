using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace PowerLine.Upnp
{
    public static class StreamExtentions
    {
        public static void WriteLine(this Stream stream, string message) => stream.Write($"{message}\r\n");
        public static void Write(this Stream stream, string message) => stream.Write(System.Text.Encoding.UTF8.GetBytes(message));
        public static void Write(this Stream stream, byte[] message) => stream.Write(message, 0, message.Length);
    }
}
