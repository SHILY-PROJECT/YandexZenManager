using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yandex.Zen.Core.Models
{
    public class HE
    {
        public string XPath { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;

        public HE(string xpath) => XPath = xpath;
        public HE(string xpath, string title) : this(xpath) => Title = title;
    }
}
