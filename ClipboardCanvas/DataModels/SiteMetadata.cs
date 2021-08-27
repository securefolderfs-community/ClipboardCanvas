using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels
{
    public sealed class SiteMetadata
    {
        public string SiteUrl { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Keywords { get; set; }

        public string IconUrl { get; set; }

        public string SiteName { get; set; }

        public List<string> ImageUrls { get; set; }

        public bool HasAnyData { get; set; }

        public SiteMetadata()
        {
            ImageUrls = new List<string>();
        }
    }
}
