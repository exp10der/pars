using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using HtmlAgilityPack;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main()
        {
            const string @base = "http://www.cds.spb.ru";
            string str = string.Empty;
            using (var client = new HttpClient())
            {
                str = client.GetStringAsync("http://www.cds.spb.ru/novostroiki-peterburga/").Result;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(str);


           #region tmp
            //var q = doc.DocumentNode.SelectNodes("//a[@class='b-object ']").Select(n =>
            //{
            //    var title_tmp = n.Element("header").Element("h2").InnerHtml.Trim();

            //    return new
            //    {
            //        url = @base + n.GetAttributeValue("href", ""),
            //        title = title_tmp
            //    };
            //});
           #endregion


            // С текст сканером
            // 
            var result = doc.DocumentNode.SelectNodes("//a[@class='b-object ']").Select(n =>
            {
                var title_tmp = n.Element("header").Element("h2").InnerHtml.Trim();
                var scan = new TextScanner(title_tmp);
                scan.Skip("&laquo;");
                return new
                {
                    url = @base + n.GetAttributeValue("href", ""),
                    title = scan.ReadTo("&raquo;") 
                };
            });

            var list = new List<string>(result.Select(n=>n.title));

            // TODO Дома которые сданны лежат в классе "b-object b-object_finish"
        }
    }
}