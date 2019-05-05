using System.Text.RegularExpressions;

namespace Scraper
{
    class Record
    {
        private readonly string urlFirst = "<a class=\"secondary-btn website-link\" href=\"";
        private readonly string nameFirst = "<div class=\"sales-info\"><h1>";
        private readonly string nameSecond = "<";
        private readonly string mailFirst = "<a class=\"email-business\" href=\"mailto:";
        private readonly Regex mailRegex = new Regex("<a class=\"email-business\" href=\"mailto:[a-zA-Z0-9._-]+@[a-zA-Z0-9._-]+\\.[a-zA-Z0-9._-]+\"");
        private readonly string UnknownPlaceholder = "Unknown";
        public Record(string data)
        {
            try
            {
                Name = data.Split(nameFirst)[1].Split(nameSecond)[0];
            }
            catch
            {
                Name = UnknownPlaceholder;
            }
            try
            {
                SiteUrl = data.Split(urlFirst)[1].Split('"')[0];
            }
            catch
            {
                SiteUrl = UnknownPlaceholder;
            }
            try
            {
                Email = mailRegex.IsMatch(data)
                    ? mailRegex.Match(data).Value.Replace(mailFirst, string.Empty).Replace("\"", string.Empty)
                    : UnknownPlaceholder;
            }
            catch
            {
                Email = UnknownPlaceholder;
            }

        }

        public string Name { get; set; }
        public string SiteUrl { get; set; }
        public string Email { get; set; }
    }
}
