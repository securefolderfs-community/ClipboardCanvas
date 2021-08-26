using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.DataModels
{
    public sealed class SiteMetadata
    {
        public string Title { get; set; }

        public string SiteName { get; set; }

        public string Description { get; set; }

        public string RawIcon { get; set; }

        public List<string> RawImages { get; set; }

        public SiteMetadata()
        {
            RawImages = new List<string>();
        }
    }
}
