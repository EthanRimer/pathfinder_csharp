using HtmlAgilityPack;

class Page {
    public string? title { get; set; }
    public string? link { get; set; }
    public List<string>? children { get; set; }
}

class Hierarchy {
    public static string[] links; 
    public static Dictionary<string, Page> pages;
}

public class Pathfinder {

    private static HashSet<string> visitedLinks = new HashSet<string>();
    private static HashSet<string> unvisitedLinks = new HashSet<string>();

    public static string rootPage = @"https://www.peanuts.com";
    static Page currentPage = new Page();

    public static void Main(string[] args) {

        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(rootPage);

        visitedLinks.Add(rootPage);

        currentPage.link = rootPage;

        string pageTitle = GetPageTitle(htmlDoc);
        if(pageTitle != "") {
            currentPage.title = pageTitle;
        }

        currentPage.children = FindLinks(rootPage, htmlDoc);
    }

    public static string GetPageTitle(HtmlDocument doc) {

        string pageTitle = "";
        try {

            pageTitle = doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
        } catch(System.NullReferenceException) {

            pageTitle = "";
        }

        return pageTitle;
    }

    public static List<string> FindLinks(string rootPage, HtmlDocument doc) {
        List<string> links = new List<string>();

        foreach(var node in doc.DocumentNode.SelectNodes("//a[@href]")) {
            string link = node.Attributes["href"].Value;
            if(link.StartsWith(rootPage) || link.StartsWith("/")) {
                link = FormatLink(link);
                if(ValidLink(link)) {

                }
            }
        }

        return links;
    }

    public static string FormatLink(string link) {
        if(link.StartsWith("/")) {
            link = $"{rootPage}{link}";
        }
        if(link.EndsWith("/")) {
            link = link.Remove(link.Length - 1);
        }

        return link;
    }
    public static bool ValidLink(string link) {
        //Uri uri = new Uri(link);
        if(link.Contains("#") || unvisitedLinks.Contains(link) || visitedLinks.Contains(link)) {

            return false;
        } 

        return true;
    }
}
