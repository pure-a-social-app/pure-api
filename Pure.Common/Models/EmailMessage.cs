using System;
using System.Collections.Generic;
using System.Text;

namespace Pure.Common.Models
{
    public class EmailMessage
    {
        public string Destination { get; set; }
        public string Subject { get; set; }
        public string FirstName { get; set; }
        public Dictionary<string, string> PresetSubstitutions { get; set; }
    }
}
