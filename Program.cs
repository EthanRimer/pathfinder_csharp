/**********
 * TODO:
 * 1. Serialize Hierarchy object to XML after populated
 * 2. Make XSL file
 *********/

using System.Text.RegularExpressions;

using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

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
    private static HashSet<string> guidPages = new HashSet<string>();
    private static Regex guidRegex 
        = new Regex(@"[0-9A-F]{8}-([0-9A-F]{4}-){3}[0-9A-F]{12}",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static string rootPage = "https://onrealm.t.ac.st";

    static Page currentPage = new Page(rootPage);
    static Hierarchy h = new Hierarchy();

    static IWebDriver driver = new FirefoxDriver(@"./");
    static HtmlDocument htmlDoc = new HtmlDocument();

    public static void Main(string[] args) {

        unvisitedLinks.Enqueue(rootPage);

        try {
            while(unvisitedLinks.Count > 0) {
                if(unvisitedLinks.Peek().Contains("Account/SignOut") 
                        && unvisitedLinks.Count > 1) {
                    unvisitedLinks.Enqueue(unvisitedLinks.Dequeue());
                    continue;
                }

                currentPage = new Page(unvisitedLinks.Dequeue());

                driver.Navigate().GoToUrl(currentPage.link);
                if(!driver.Url.StartsWith(rootPage)) {
                    continue;
                }
                visitedLinks.Add(currentPage.link);
                Console.WriteLine($"Visited page: {currentPage.link}");

                htmlDoc.LoadHtml(driver.PageSource);
                currentPage.title = driver.Title;
                currentPage.children = FindLinks(currentPage.link, htmlDoc);

                h.pages.Add(currentPage.link, currentPage);

                Thread.Sleep(2000);
                string path = CaptureScreenshot(currentPage.link);
                if(currentPage.link.EndsWith("Site/SignIn")) {
                    LogIn();
                } 
            }
        }
        catch(Exception e) {
            Console.WriteLine("Exception encountered: {0}\n\nLast link visited: {1}\n{2} Links visited\n\n", e, currentPage.link, visitedLinks.Count);
            System.Environment.Exit(1);
        }

        driver.Quit();

        Console.WriteLine($"\n\nFound {visitedLinks.Count} total pages\n");

        /*
        h.links = visitedLinks.ToArray();
        Array.Sort(h.links);
        */
    }

    public static void LogIn() {
        driver.FindElement(By.Id("emailAddress")).SendKeys("anneconley@example.org");
        driver.FindElement(By.Id("password")).SendKeys("RealmAcs#2018");
        driver.FindElement(By.Id("signInButton")).Click();
        
        Thread.Sleep(750);
        driver.FindElement(By.Id("siteList")).Click();
        Thread.Sleep(500);
        driver.FindElement(By.XPath("//*[@id='siteDialog']/div[1]/ul/div/li[26]")).Click();
        Thread.Sleep(500);
        driver.FindElement(By.Id("selectSite")).Click();
        Thread.Sleep(500);

        unvisitedLinks.Enqueue(driver.Url);
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

                bool isGuid = guidRegex.IsMatch(link);

                link = FormatLink(link);

                if(ValidLink(link, isGuid)) {

                    links.Add(link);
                    if(isGuid) {
                        guidPages.Add(guidRegex.Replace(link, ""));
                    } else {
                        unvisitedLinks.Enqueue(link);
                    }
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

    public static bool ValidLink(string link, bool isGuid) {
        if(link.Contains("#") 
                || unvisitedLinks.Contains(link) 
                || visitedLinks.Contains(link)
                || guidPages.Contains(guidRegex.Replace(link, ""))
                || link.Contains("Help/LMS")) { // This Page breaks the HTML parser

            return false;
        } 

        return true;
    }

    public static string CaptureScreenshot(string link) {
        string screenshotPath = $"./screens/screen_{visitedLinks.Count}.png";
        Screenshot ss = ((ITakesScreenshot)driver).GetScreenshot();
        ss.SaveAsFile(screenshotPath, ScreenshotImageFormat.Png);

        return screenshotPath;
    }
}

