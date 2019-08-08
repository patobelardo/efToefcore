using System;
using System.Collections.Generic;

namespace EfPoc.Models
{
    public partial class Spans
    {
        public long Id { get; set; }
        public long MemberId { get; set; }
        public string SpanType { get; set; }
        public string SpanValue { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual Members Member { get; set; }
    }
}
