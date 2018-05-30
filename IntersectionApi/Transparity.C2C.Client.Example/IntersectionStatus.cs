using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transparity.C2C.Client.Example
{
    public class IntersectionStatus
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public int GroupGreens { get; set; }
        public List<int> ActivePhases { get; set; }
        public List<PhaseInfo> AllPhases { get; set; }
    }
}
