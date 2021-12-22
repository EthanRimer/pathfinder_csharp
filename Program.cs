using HtmlAgilityPack;

class Page {
    public string? title { get; set; }
    public string? link { get; set; }
    public string[]? children { get; set; }
}

class Hierarchy {
    public static string[] links { get; set; }
    public static Dictionary<string, Page> pages { get; set; }
}

public class Pathfinder {

    private static HashSet<string> visitedLinks = new HashSet<string>();
    private static HashSet<string> unvisitedLinks = new HashSet<string>();

    public static string rootPage = @"https://www.peanuts.com";
    static Page currentPage = new Page();

    public static void Main(string[] args) {

        Hierarchy h = new Hierarchy();

        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(rootPage);

        currentPage.link = rootPage;

        string pageTitle = GetPageTitle(htmlDoc);
        if(pageTitle != "") {
            currentPage.title = pageTitle;
        }

        visitedLinks.Add(rootPage);
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

    /*
    public static string[] FindLinks(HtmlDocument doc) {

        return {""};
    }
    */
}
