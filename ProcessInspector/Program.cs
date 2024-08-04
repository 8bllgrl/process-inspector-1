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
            processInfo.PrintProcessInfo();

            // find current target and print out details
            SignatureScanner signatureScanner = new SignatureScanner();
            AddressService addressService = new AddressService(signatureScanner);

        }
    }
}
