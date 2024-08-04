using ProcessInspector.Memory.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector.Memory.FF14
{
    public class FF14Entity
    {
        [MemoryOffset(0x80)]
        public IntPtr TargetPtr { get; set; }

        [MemoryOffset(0x30)]
        public string Name { get; set; }

        //[MemoryOffset(0x8E)]
        //public byte Sex { get; set; }
    }

}
