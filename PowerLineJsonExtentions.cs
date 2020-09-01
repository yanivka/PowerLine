using Newtonsoft.Json.Linq;
using System.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Reflection.Metadata.Ecma335;

namespace PowerLine
{
    public static class PowerLineJsonExtentions
    {


        public static Dictionary<string, string> ReadHeaders(this JObject obj)
        {
            if(obj.TryGetValue("headers", out JObject value))
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                foreach (KeyValuePair<string, JToken> item in value)
                {
                    headers.Add(item.Key, item.Value.Value<string>());
                }
                return headers;
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        public static T GetValue<T>(this JObject token, string propertyName)
        {
            if (token.TryGetValue<T>(propertyName, out T innerValue))
            {
                return innerValue;
            }
            else
            {
                throw new Exception($"Unable to locate the field \"{propertyName}\"");
            }
        }
        public static T GetValue<T>(this JObject token, string[] propertyPath)
        {
            if (token.TryGetValue<T>(propertyPath, out int exceptionIndex ,out T innerValue))
            {
                return innerValue;
            }
            else
            {
                throw new Exception($"Unable to locate the field \"{string.Join("->", propertyPath.Take(exceptionIndex + 1))}\" out of \"{string.Join("->", propertyPath)}\"");
            }
        }
        public static bool TryGetValue<T>(this JObject token, string propertyName, out T value) 
        {
            if(token.TryGetValue(propertyName, out JToken innerValue))
            {
                value = innerValue.Value<T>();
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
        private static bool TryGetValue<T>(this JObject token, string[] propertyPath,  int currentIndex, out int exceptionIndex, out T value)
        {
            if (token.TryGetValue(propertyPath[currentIndex], out JToken innerValue))
            {
                if(currentIndex < propertyPath.Length -1)
                {
                    JObject innerObject = innerValue.Value<JObject>();
                    if(innerObject == null)
                    {
                        exceptionIndex = currentIndex;
                        value = default(T);
                        return false;
                    }
                    return innerObject.TryGetValue(propertyPath, currentIndex + 1, out exceptionIndex, out value);
                }
                else
                {
                    value = innerValue.Value<T>();
                    exceptionIndex = -1;
                    return true;
                }
            }
            else
            {
                exceptionIndex = currentIndex;
                value = default(T);
                return false;
            }
        }
        public static bool TryGetValue<T>(this JObject token, string[] propertyPath, out int expcetionIndex, out T value) => token.TryGetValue<T>(propertyPath, 0, out expcetionIndex , out value);
        public static JObject GetJson(this Dictionary<string, string> dict)
        {
            JObject obj = new JObject();
            foreach(KeyValuePair<string, string> item in dict)
            {
                obj.Add(item.Key, item.Value);
            }
            return obj;
        }
    }
}
