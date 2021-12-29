/**********
 * TODO:
 * 1. Switch from Hierarchy class to sorted collection
 * 2. Serialize Hierarchy object to XML after populated
 * 3. Make XSL file
 *********/
using HtmlAgilityPack;

class Page {
    public string? title { get; set; }
    public string link { get; }
    public List<string>? children { get; set; }

    public Page(string link) {
        this.link = link;
    }
}

class Hierarchy {
    public string[] links { get; set; } 
    public Dictionary<string, Page> pages { get; set; }

    public Hierarchy() {
        pages = new Dictionary<string, Page>();
        links = new string[pages.Count];
    }

    public Hierarchy(string[] links, Dictionary<string, Page> pages) {
        this.links = links;
        this.pages = pages;
    }
}

public class Pathfinder {

    private static HashSet<string> visitedLinks = new HashSet<string>();
    private static Queue<string> unvisitedLinks = new Queue<string>();

    public static string rootPage = "https://www.peanuts.com";

    static Page currentPage = new Page(rootPage);
    static Hierarchy h = new Hierarchy();

    public static void Main(string[] args) {

        HtmlWeb web = new HtmlWeb();
        unvisitedLinks.Enqueue(rootPage);
        
        while(unvisitedLinks.Count > 0) {

            currentPage = new Page(unvisitedLinks.Dequeue());

            var htmlDoc = web.Load(currentPage.link);
            visitedLinks.Add(currentPage.link);
            Console.WriteLine($"Visited page: {currentPage.link}");

            string pageTitle = GetPageTitle(htmlDoc);
            currentPage.title = pageTitle != "" ? pageTitle : "";

            currentPage.children = FindLinks(currentPage.link, htmlDoc);
            h.pages.Add(currentPage.link, currentPage);
        }

        Console.WriteLine($"\n\nFound {visitedLinks.Count} total pages\n");

        h.links = visitedLinks.ToArray();
        Array.Sort(h.links);
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
                    Console.WriteLine($"Found page: {link}");
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
        if(link.Contains("#") || unvisitedLinks.Contains(link) || visitedLinks.Contains(link)) {
            return false;
        } 

        return true;
    }
}

