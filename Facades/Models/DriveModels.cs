using System;
using System.Collections.Generic;
using System.Text;

namespace Facades.Models
{
    public class MSGraphLink
    {
        public string id { get; set; }
        public List<string> roles { get; set; }
        public LinkType link { get; set; }
    }
    public class LinkType
    {
        public string type { get; set; }
        public string scope { get; set; }
        public string webUrl { get; set; }
        public ApplicationType application { get; set; }
    }
    public class ApplicationType
    {
        public string id { get; set; }
        public string displayName { get; set; }
    }
}
