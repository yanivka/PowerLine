using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PowerLine.Upnp
{
    public static class XElementExntetions
    {

        public static XElement AddSame(this XElement rootNamespace, string name, params object[] objects)
        {
            XElement item = rootNamespace.Create(name, objects);
            rootNamespace.Add(item);
            return item;
        }
        public static XElement AddSame(this XElement rootNamespace, XElement other)
        {
            XElement item = rootNamespace.Create( other);
            rootNamespace.Add(item);
            return item;
        }
        public static XElement AddSame(this XElement rootNamespace, string name, object singleObject)
        {
            XElement item = rootNamespace.Create(name, singleObject);
            rootNamespace.Add(item);
            return item;
        }


        public static XElement FixNamespace(this XElement source, XNamespace defaultNamespace = null, IEnumerable<XNode> firstElements = null)
        {
            string currentNameSpace = source.Name.NamespaceName?.Trim();
            defaultNamespace = (currentNameSpace != null && currentNameSpace.Trim() != "") ? source.Name.Namespace : defaultNamespace;
            IEnumerable<XObject> attrs = source.Attributes();
            IEnumerable<XNode> childElements = (firstElements == null) ? source.Nodes() : firstElements.Concat(source.Nodes());
            XObject[] elementObject = attrs.Concat(childElements.Select((item) => (item is XElement) ? ((XElement)item).FixNamespace(defaultNamespace) : item )).ToArray();
            return new XElement((((currentNameSpace == null || currentNameSpace == "") && defaultNamespace != null) ? defaultNamespace : source.Name.Namespace) + source.Name.LocalName, elementObject);
        }
        public static XElement Create(this XElement rootNamespace, XElement other)
        {
            XElement item = new XElement(rootNamespace.GetDefaultNamespace() + other.Name.ToString(), other.Elements().ToArray());
            return item;
        }
        public static XElement Create(this XElement rootNamespace, string name, params object[] objects)
        {
            XElement item = new XElement(rootNamespace.GetDefaultNamespace() + name, objects);
            return item;
        }
        public static XElement Create(this XElement rootNamespace, string name, object singleObject)
        {
            XElement item = new XElement(rootNamespace.GetDefaultNamespace() + name, singleObject);
            return item;
        }
    }
}
