using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector.Memory.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MemoryOffsetAttribute : Attribute
    {
        public int Offset { get; }
        public MemoryOffsetAttribute(int offset)
        {
            Offset = offset;
        }
    }
}
