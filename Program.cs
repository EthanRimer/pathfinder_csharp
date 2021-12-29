/**********
 * TODO:
 * 1. Switch from Hierarchy class to sorted collection
 * 2. Serialize Hierarchy object to XML after populated
 * 3. Make XSL file
 *********/
using HtmlAgilityPack;

public class Pathfinder {
    public class Page {
        public string? title { get; set; }
        public string link { get; }
        public List<string>? children { get; set; }

        public Page(string link) {
            this.link = link;
        }

        public Page() {}
    }

    public class Hierarchy {
        public string[] links { get; set; } 
        public Dictionary<string, Page> pages { get; set; }

        public Hierarchy() {
            pages = new Dictionary<string, Page>();
            links = new string[pages.Count];
        }
    }

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

            currentPage.title = GetPageTitle(htmlDoc);
            currentPage.children = FindLinks(currentPage.link, htmlDoc);

            h.pages.Add(currentPage.link, currentPage);
        }

        Console.WriteLine($"\n\nFound {visitedLinks.Count} total pages\n");

        h.links = visitedLinks.ToArray();
        Array.Sort(h.links);

        System.Xml.Serialization.XmlSerializer writer = 
            new System.Xml.Serialization.XmlSerializer(typeof(Page[]));
        System.IO.FileStream file = System.IO.File.Create("./hierarchy.xml");

        /*
        foreach(string link in h.links) {
            Console.WriteLine($"{h.pages[link].title}\n{h.pages[link].link}\n");
            writer.Serialize(file, h.pages[link]);
        }
        */
        writer.Serialize(file, h.pages.Values.ToArray());
        file.Close();
    }

    public static string GetPageTitle(HtmlDocument doc) {
        try {
            return doc.DocumentNode.SelectSingleNode("//head/title").InnerText;
        } catch(System.NullReferenceException) {
            return "";
        }
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
        if(link.Contains("#") 
                || unvisitedLinks.Contains(link) 
                || visitedLinks.Contains(link)) {

            return false;
        } 

        return true;
    }
}

