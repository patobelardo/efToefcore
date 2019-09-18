using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfPoc
{
    class MemberDTO
    {
        public int ID { get; set; }
        public string FirstName{ get; set; }
        public List<SpanDTO> Spans { get; set; }
    }
    class SpanDTO
    {
        public string SpanValue { get; set; }
        public long Id { get; set; }
        public string SpanType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
