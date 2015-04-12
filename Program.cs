using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main()
        {

            const string @base = "http://www.cds.spb.ru";

            string html;
            using (var client = new HttpClient())
            {
                html = client.GetStringAsync("http://www.cds.spb.ru/novostroiki-peterburga/").Result;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Заполняем Первые 3 свойства
            List<RealEstate> realState = doc.DocumentNode.SelectNodes(".//*[@id='outer']/div[2]/div/table/tr/td[2]/div[2]/a").AsParallel()
                .Select(item =>
                {
                    var titleTmp = item.SelectSingleNode("header/h2");
                    var shortAddressTmp = item.SelectSingleNode("header/p/em");
                    var uriTmp = item.GetAttributeValue("href", "");

                    // Вот тут 3
                    return new RealEstate
                        {
                            title = titleTmp != null ? titleTmp.InnerText.Trim().FixString() : "",
                            shortAddress = shortAddressTmp != null ? shortAddressTmp.InnerText.Trim().FixString() : "",
                            Uri = new Uri(uriTmp, UriKind.RelativeOrAbsolute).IsAbsoluteUri ? uriTmp : @base + uriTmp
                        };
                }).ToList();

            //foreach (var realEstate in realState)
            //{
            //    Console.WriteLine(realEstate.title);
            //}

            // Узнаем корпуса и uri уже на квартиры в домах
            realState.AsParallel().Select(n =>
            {
                n.HousingEstates = new List<HousingEstate>();
                string site;
                using (var client = new HttpClient())
                {
                    site = client.GetStringAsync(n.Uri).Result;
                }
                var htmldoc = new HtmlDocument();
                htmldoc.LoadHtml(site);

                // Если равно Null то делаем вывод то текущий юрл уже привел нас где список квартир!
                // Т.е на текущей страницы уже идет список квартир
                var housing =
                    htmldoc.DocumentNode.SelectNodes(".//*[@id='outer']/div[2]/div/table/tr/td[2]/div/div/div[2]/a");

                if (housing != null)
                {
                    n.HousingEstates = housing
                        .Select(x =>
                        {
                            // Тут лежит Название корпуса
                            var tt = x.SelectSingleNode("div/span");
                            var urlsite = x.GetAttributeValue("href", "");
                            return new HousingEstate()
                            {
                                NameHousing = tt.InnerText.Trim(),
                                Uri = urlsite
                            };
                        }).ToList();
                }
                else  // А вот если нулл то надо сохранить Исходный код для дальнейшего парсинга наверно надо!
                {
                    n.htmlcode = site;
                }
                return n;
            }).ToList();

            realState.Last().title = "Последнии квартиры в сданных домах!";

            foreach (var realEstate in realState)
            {
                Console.WriteLine(realEstate.shortAddress);
                Console.WriteLine("  " + realEstate.title);
                foreach (var estate in realEstate.HousingEstates)
                {

                    Console.WriteLine("\t" + estate.NameHousing);

                }
                Console.WriteLine();
            }

            List<ImportHouseInfo> list = realState
                .Select(n =>
                    new ImportHouseInfo()
                    {
                        strHouseName = n.title
                    }).ToList();

            var listKorpus = realState.SelectMany(n =>
                n.HousingEstates.Select(x => new ImportHouseInfo()
                {
                    strHouseName = n.title+" "+n.shortAddress + " " + x.NameHousing
                })).ToList();

            foreach (var importHouseInfo in listKorpus)
            {
                Console.WriteLine(importHouseInfo.strHouseName);
            }

        }
    }
}

/// <summary>
/// Недвижимость
/// </summary>
class RealEstate
{
    /// <summary>
    /// Короткое название
    /// </summary>
    public string title { get; set; }
    /// <summary>
    /// Адрес
    /// </summary>
    public string shortAddress { get; set; }
    /// <summary>
    /// Сылка на Жилой комплекс
    /// </summary>
    public string Uri { get; set; }

    /// <summary>
    /// Тут идет вторая вложенность страницы где название корпусов
    /// некотрые страницы могут иметь сразу сылку на табличку для квартир
    /// </summary>
    public List<HousingEstate> HousingEstates { get; set; }

    /// <summary>
    /// Если тут есть исходный код то данный объект во второй вложености имеет уже табличку с квартирами
    /// </summary>
    public string htmlcode { get; set; }

}

/// <summary>
/// Жилой коплекс
/// </summary>
class HousingEstate
{
    // Название корпуса
    public string NameHousing { get; set; }
    // Сылка на таблицу
    public string Uri { get; set; }
}
static class ExMethod
{
    // Фикс Строк где может находится мусор
    static public string FixString(this string str)
    {
        return Regex.Replace(str, @"[a-zA-z&;<>]*", "");
    }
}
class ImportHouseInfo
{
    public string strHouseName;
    public string HouseName
    {
        get { return strHouseName; }
        set { strHouseName = value; }
    }
}