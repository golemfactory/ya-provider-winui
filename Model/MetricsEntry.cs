using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GolemUI.Model
{
    public class MetricsEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public MetricsEntry(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
