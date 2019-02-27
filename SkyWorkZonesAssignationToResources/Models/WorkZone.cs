using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkZoneLoad.Models
{
    public class WorkZone
    {
        public List<string> id { get; set; }
        public string source { get; set; }
        public string workZoneName { get; set; }
        public string workZoneLabel { get; set; }
        public string travelArea { get; set; }
        public string status { get; set; } = "active";
        public List<string> label { get; set; } = new List<string>();
        public string workZoneItemId { get; set; }
        public string workZone { get; set; }
        public string startDate { get; set; }
        public DateTime endDate { get; set; }
        public string ratio { get; set; }
        public string recurrence { get; set; }
        public string recurEvery { get; set; }
        public string type { get; set; }
        public string country { get; set; }
        public bool replace { get; set; } = true;

    }
}
