namespace CloudflareResolver.Models
{
    public class ResolverRequest
    {
        public string Url { get; set; }

        public string UserAgent { get; set; }

        public string SelectorForWait { get; set; }
    }
}
