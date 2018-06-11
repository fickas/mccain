using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Transparity.C2C.Client.Example
{
    /// <summary>
    /// Class to hold information about the phase info
    /// </summary>
    public class PhaseInfo
    {
        public int PhaseID { get; set; }
        public int MinGreen { get; set; }
        public int MaxGreen { get; set; }
        public float LastActiveTime { get; set; }
        public float CurrentActiveTime { get; set; }
        public DateTime BecameActiveTimestap { get; set; }
        public bool CurrentlyActive { get; set; }
    }
}
