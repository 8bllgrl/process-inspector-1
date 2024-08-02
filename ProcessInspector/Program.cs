using ProcessInspector.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector
{
    class Program
    {
        static void Main(string[] args)
        {
            //This is meant to work on ffxiv_dx11 but can work for any program.
            string processName = "ffxiv_dx11";

            PEParser parser = new PEParser(processName);

            ProcessInfo processInfo = parser.ParseProcess();

            if (processInfo != null)
            {
                Console.WriteLine($"Handle: {ProcessInfo.Handle}");
                Console.WriteLine($"Base Address: {ProcessInfo.BaseAddress}");
                Console.WriteLine($"PE Header Address: {ProcessInfo.PEHeaderAddress}");
                Console.WriteLine($"Number of Sections: {ProcessInfo.NumberOfSections}");
                Console.WriteLine($"Optional Header Size: {ProcessInfo.OptionalHeaderSize}");
                Console.WriteLine($"Text Section Start: {ProcessInfo.TextSectionStart}");
                Console.WriteLine($"Text Section End: {ProcessInfo.TextSectionEnd}");
                Console.WriteLine($"Data Section Start: {ProcessInfo.DataSectionStart}");
                Console.WriteLine($"Data Section End: {ProcessInfo.DataSectionEnd}");
                Console.WriteLine($"Is 32-Bit Process: {ProcessInfo.Is32BitProcess}");
            }
            else
            {
                Console.WriteLine("Failed to parse process information.");
            }
        }
    }
}
