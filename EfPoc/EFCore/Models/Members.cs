using System;
using System.Collections.Generic;

namespace EfPoc.Models
{
    public partial class Members
    {
        public Members()
        {
            Spans = new HashSet<Spans>();
        }

        public long Id { get; set; }
        public string HIC { get; set; }
        public string PlanID { get; set; }
        public string PBP { get; set; }
        public string SegmentID { get; set; }
        public DateTime? CurrentEffDate { get; set; }
        public string EnrollSource { get; set; }
        public string ProgramSource { get; set; }
        public string MemberStatus { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual ICollection<Spans> Spans { get; set; }
    }
}
