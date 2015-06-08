using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFilter.Common.Util
{
    class AboutString
    {
        public static string specialStringConverter(string s)
        {
            // -- 정규 표현식을 알면 심플하게 할수 있음. 난 정규표현식 모름 ㅡㅡ;
            string returnValue;
            returnValue = s.Replace("<b>", "").Replace("</b>", "").Replace("&#39;", "'")
                .Replace("&lt;", "<").Replace("&gt;", ">").Replace("&nbsp;", " ").Replace("&middot;", "\"\"")
                .Replace("&amp;", "&").Replace("&quot;", "\"").Replace("&apos;", "'");
            return returnValue;
        }
    }
}
