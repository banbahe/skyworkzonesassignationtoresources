using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkZoneLoad.Models
{
    public class Resource
    {
        public string parentId { get; set; }
        public string externalId { get; set; }
        public string name { get; set; }
        public string resourceType { get; set; }
        public string resourceStatus { get; set; }
        public string language { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string timezone { get; set; }
        public string calendar { get; set; }
        public string time_format { get; set; }
        public string date_format { get; set; }
        public string resource_workzones { get; set; }
        public string XR_PartnerID { get; set; }
        public string XR_MasterID { get; set; }
        // public List<string> work_skills { get; set; } = new List<string>();
        public string capacity_categories { get; set; }
        public string XR_RPID { get; set; }
        public string organization_id { get; set; }
        // public User user { get; set; }
        public WorkZone workZone { get; set; }
        // public List<WorkSkill> workSkills { get; set; } = new List<WorkSkill>();
    }
}
