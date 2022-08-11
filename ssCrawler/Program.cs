using System.Net;
using HtmlAgilityPack;

const string homepageUrl = "https://www.sahibinden.com";

List<string> vitrinList = GetVitrinListLinks(homepageUrl);
List<AdvertDataModel> advertData = GetAdvertData(vitrinList);
string[] txtFileLines = new string[advertData.Count];
int counter = 0;
// Konsol ekranına yazdır ve aynı zamanda TXT dosyası için array'i doldur.
foreach (AdvertDataModel advert in advertData)
{
    txtFileLines[counter] = $"{advert.Title} --- {advert.Price}";
    Console.WriteLine($"Advert's Title: {advert.Title}");
    Console.WriteLine($"Advert's Price: {advert.Price}");
    Console.WriteLine("-------------------------------");
}

Console.WriteLine($"Adverts AVG Price: {advertData.Average(x => x.Price)}");
await WriteToTxtFile(txtFileLines);

// String olarak gelen parametreye request yolla ve contentini geri döndür.
HtmlDocument GetDocument(string url)
{
    WebClient client = new();
    string htmlCode = client.DownloadString(url);
    HtmlDocument document = new();
    document.LoadHtml(htmlCode);
    return document;
}

// Vitrin alanındaki ilanların linklerini alır ve geri döndür. 
List<string> GetVitrinListLinks(string url)
{
    HtmlDocument document = GetDocument(url);
    List<string> vitrinUrls = new();
    List<HtmlNode> ulElementList = document
        .DocumentNode.Descendants("ul")
        .Where(x => x.GetAttributeValue("class", false).Equals("vitrin-list clearfix"))
        .ToList();
    foreach (HtmlNode ulElement in ulElementList)
    {
        List<HtmlNode> liElementList = ulElement
            .Descendants("li")
            .ToList();
        foreach (HtmlNode liElement in liElementList)
        {
            string virtinUrl = liElement
                .Descendants("a").FirstOrDefault()?
                .ChildAttributes("href")?.FirstOrDefault()?
                .Value ?? "";
            vitrinUrls.Add(virtinUrl);
        }
        break;
    }
    return vitrinUrls;
}

// İlanın detaylarını oku ve geri döndür.
List<AdvertDataModel> GetAdvertData(List<string> links)
{
    List<AdvertDataModel> advertData = new();
    foreach (string link in links)
    {
        HtmlDocument document = GetDocument(link);
        string advertPriceStr = document.DocumentNode.Descendants("input")
            .Where(x => x.GetAttributeValue("id", false).Equals("favoriteClassifiedPrice"))
            .FirstOrDefault()?
            .ChildAttributes("value")?.FirstOrDefault()?
            .Value ?? "";
        advertPriceStr = advertPriceStr.Replace("TL", "").Replace(".", "");
        int advertPrice = int.Parse(advertPriceStr);
        List<HtmlNode> advertTitleElementList = document.DocumentNode.Descendants("class")
            .Where(x => x.GetAttributeValue("class", false).Equals("classifiedDetailTitle"))
            .ToList();
        string advertTitleStr = "";
        foreach (HtmlNode element in advertTitleElementList)
        {
            advertTitleStr = element.Descendants("h1").FirstOrDefault()?.InnerText ?? "";
            break;
        }

        advertData.Add(new AdvertDataModel
        {
            Price = advertPrice,
            Title = advertTitleStr
        });
    }
    return advertData;
}

// Toplanan datayı txt dosyasına bas.
async Task WriteToTxtFile(string[] lines)
{
    await File.WriteAllLinesAsync("Advert.txt", lines);
}

// Data model
class AdvertDataModel
{
    public int Price { get; set; }
    public string Title { get; set; }
}