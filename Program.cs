using System.Text;
using HtmlAgilityPack;

public static class StringExtension
{
    public static string StripPunctuation(this string s)
    {
        var sb = new StringBuilder();
        foreach (char c in s)
        {
            if (char.IsPunctuation(c)) {
                // n-dash and m-dash should be delimiter
                if (c == '-' || c == '—') {
                    sb.Append("|");
                }
            } else {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}

class Program
{
    private static HtmlNode parsePoem(HtmlNode poem) {
        /*
            Manipulate HTML to remove br characters since removing it
            from the list directly is not a straightforward option
        */
        string poemHtml = poem.OuterHtml;

        // Convert <br> and newline character to delimiter
        poemHtml = poemHtml.Replace("<br>", "|").Replace("\n", "|");

        var poemDoc = new HtmlDocument();
        poemDoc.LoadHtml(poemHtml);

        return poemDoc.DocumentNode.SelectSingleNode("//div");
    }

    static void Main(string[] args)
    {
        var doc = new HtmlDocument();
        doc.Load(@"the_file.html");
    
        HtmlNode body = doc.DocumentNode.SelectSingleNode("//body");
        HtmlNode title = body.SelectSingleNode("//h1");
        HtmlNode byLine = body.SelectSingleNode("//h2");
        HtmlNode poem = parsePoem(body.SelectSingleNode("//div[@class='chapter']"));

        string fullPoem = $"{title.InnerText} {byLine.InnerText} {poem.InnerText}";

        // Convert space between words to delimiter
        fullPoem = fullPoem.StripPunctuation().Replace(" ", "|");

        // Split on delimiter and remove any empty values
        string[] words = fullPoem.Split("|").Where(s => !string.IsNullOrEmpty(s)).ToArray();

        // Create a hashmap to track instances of words
        Dictionary<string, int> wordsMap = new Dictionary<string, int>();

        foreach (string word in words) {
            string wordKey = word.ToLower();

            if (wordsMap.ContainsKey(wordKey)) {
                wordsMap[wordKey] += 1;
            } else {
                wordsMap.Add(wordKey, 1);
            }
        }

        // Get the top 20 results
        var topResults = (from word in wordsMap orderby word.Value descending select word).Take(20);

        foreach (var kv in topResults) {
            Console.WriteLine(kv.Key + ", " + kv.Value);
        }
    }
}