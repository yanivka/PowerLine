using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PowerLine
{
    public class PowerLineUriArgumentParser
    {

        public readonly Uri Url;
        public readonly Dictionary<string, string> Arguemnts;
        public PowerLineUriArgumentParser(Uri url)
        {
            this.Url = url;
            this.Arguemnts = PowerLineUriArgumentParser.ParseUrl(url);
        }

        public static Dictionary<string, string> ParseUrl(Uri uri)
        {
            string query = uri.Query.TrimStart('?');
            return new Dictionary<string, string>( query
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select((singleArg) => singleArg.Split('=', StringSplitOptions.RemoveEmptyEntries))
                .Where((singleSplittedArg) => singleSplittedArg.Length == 2)
                .Select((singleSplittedVaildArg) => 
                    new KeyValuePair<string, string>(
                        System.Net.WebUtility.UrlDecode(singleSplittedVaildArg[0]),
                        System.Net.WebUtility.UrlDecode((singleSplittedVaildArg[1]))
                    )
                ));
        }
    }
}
