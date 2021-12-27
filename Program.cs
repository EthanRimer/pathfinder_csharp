using HtmlAgilityPack;

class Page {
    public string? title { get; set; }
    public string link { get; set; }
    public List<string>? children { get; set; }
}

class Hierarchy {
    public string[] links { get; set; } 
    public Dictionary<string, Page> pages { get; set; }
}

public class Pathfinder {

    private static HashSet<string> visitedLinks = new HashSet<string>();
    //private static HashSet<string> unvisitedLinks = new HashSet<string>();
    private static Queue<string> unvisitedLinks = new Queue<string>();

    public static string rootPage = @"https://www.peanuts.com";

    static Page currentPage = new Page();
    static Hierarchy h = new Hierarchy();

    public static void Main(string[] args) {

        HtmlWeb web = new HtmlWeb();
        var htmlDoc = web.Load(rootPage);

        visitedLinks.Add(rootPage);
        Console.WriteLine($"Visited page {rootPage}");

        currentPage.link = rootPage;

        string pageTitle = GetPageTitle(htmlDoc);
        if(pageTitle != "") {
            currentPage.title = pageTitle;
        }

        currentPage.children = FindLinks(rootPage, htmlDoc);
        h.pages = new Dictionary<string, Page>();
        h.pages.Add(currentPage.link, currentPage);

        currentPage = new Page();
        
        while(unvisitedLinks.Count > 0) {
            string link = unvisitedLinks.First();
            if(visitedLinks.Contains(link)) {
                visitedLinks.Remove(link);

                continue;
            }

            currentPage.link = link;

            htmlDoc = web.Load(link);
            visitedLinks.Add(link);
            Console.WriteLine($"Visited page {link}");

            pageTitle = GetPageTitle(htmlDoc);
            if(pageTitle != "") {
                currentPage.title = pageTitle;
            }

            currentPage.children = FindLinks(link, htmlDoc);
            h.pages.Add(currentPage.link, currentPage);

            unvisitedLinks.Dequeue();
            currentPage = new Page();
        }

        Console.WriteLine($"Found {visitedLinks.Count} total pages");
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
                    links.Add(link);
                    unvisitedLinks.Enqueue(link);
                    Console.WriteLine($"Found page {link}");
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
