using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace PowerLine.Upnp.UpnpCustomPackets
{
    public class UpnpIcon
    {
        public static class IconMimeType
        {
            public const string Apng = "image/apng";

            public const string Bmp = "image/bmp";

            public const string Gif = "image/gif";

            public const string Ico = "image/x-icon";
            public const string Cur = "image/x-icon";

            public const string Jpeg = "image/jpeg";
            public const string Jpg = "image/jpeg";
            public const string Jfif = "image/jpeg";
            public const string Pjpeg = "image/jpeg";
            public const string Pjp = "image/jpeg";

            public const string Png = "image/png";

            public const string Svg = "image/svg";

            public const string Tif = "image/tiff";
            public const string Tiff = "image/tiff";

            public const string Webp = "image/webp";

        }

        public string MimeType { get; set; } = IconMimeType.Jpeg;
        public int Width { get; set; } 
        public int Depth { get; set; }
        public int Height { get; set; }
        public string Url { get; set; }

        public UpnpIcon(string mimeType, int width, int depth, int height, string url)
        {
            MimeType = mimeType;
            Width = width;
            Depth = depth;
            Height = height;
            Url = url;
        }

        public XElement GetXml()
        {
            XElement icon = new XElement("icon");
            if (this.MimeType != null) icon.Add(new XElement("mimetype", this.MimeType));
            icon.Add(new XElement("width", this.Width));
            icon.Add(new XElement("height", this.Height));
            icon.Add(new XElement("depth", this.Depth));
            if (this.Url != null) icon.Add(new XElement("url", this.Url));
            return icon;
        }
    }
}
