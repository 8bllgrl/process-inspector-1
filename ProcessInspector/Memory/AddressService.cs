using ProcessInspector.Memory.Attributes;
using ProcessInspector.Memory.FF14;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProcessInspector.Memory
{
    public class AddressService
    {
        public static IntPtr PlayerTargetSystem { get; set; }

        public SignatureScanner signatureScanner { get; set; }  

        public AddressService(SignatureScanner signatureScanner) 
        {
            this.signatureScanner = signatureScanner;

            //scan
            findPlayerTargetSystem();
        }  
        public void findPlayerTargetSystem()
        {
            PlayerTargetSystem = FindAddressFromSignature("48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 3B C6 0F 95 C0");
            Console.WriteLine($"PlayerTargetSystem address: 0x{PlayerTargetSystem.ToInt64():X16}");

            PopulateCurrentTarget();
        }

        public void PopulateCurrentTarget()
        {
            FF14Entity entity = new FF14Entity();
            FindTargetPtr( entity , PlayerTargetSystem);
            PopulateByAttribute(entity, entity.TargetPtr);

            //Printing for debug purposes:
            if (entity.TargetPtr == IntPtr.Zero)
            {
                Console.WriteLine("Target pointer is zero. Aborting further processing.");
                return;
            }
            Console.WriteLine($"Possible 'Target' address: 0x{entity.TargetPtr.ToInt64():X16}");
            Console.WriteLine($"Possible 'Targeted' name address: 0x{IntPtr.Add(entity.TargetPtr, 0x30).ToInt64():X16}");
            Console.WriteLine($"Targeted Name: {entity.Name}");
        }

        private void FindTargetPtr(FF14Entity entity, IntPtr baseAddress)
        {
            var property = typeof(FF14Entity).GetProperty("TargetPtr");
            var attribute = property.GetCustomAttribute<MemoryOffsetAttribute>();

            if (attribute != null)
            {
                IntPtr propertyAddress = IntPtr.Add(baseAddress, attribute.Offset);
                if (property.PropertyType == typeof(IntPtr))
                {
                    property.SetValue(entity, MemoryReader.Read<IntPtr>(ProcessInfo.Handle, propertyAddress));
                }
            }
            else
            {
                throw new InvalidOperationException("The TargetPtr property does not have a MemoryOffset attribute.");
            }
        }

        private IntPtr FindAddressFromSignature(string signature)
        {
            try
            {
                return this.signatureScanner.GetStaticAddressFromSig(signature, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to scan memory for signature: {signature} (Have you tried restarting FFXIV?)");
                Console.WriteLine($"Error: {ex.Message}");
                return IntPtr.Zero;
            }
        }

        private void PopulateByAttribute<T>(T obj, IntPtr baseAddress)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var attribute = property.GetCustomAttribute<MemoryOffsetAttribute>();
                if (attribute != null)
                {
                    IntPtr propertyAddress = IntPtr.Add(baseAddress, attribute.Offset);
                    if (property.PropertyType == typeof(IntPtr))
                    {
                        property.SetValue(obj, MemoryReader.Read<IntPtr>(ProcessInfo.Handle, propertyAddress));
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(obj, MemoryReader.ReadStringFromMemory(ProcessInfo.Handle, propertyAddress, 25));
                    }
                    else if (property.PropertyType == typeof(byte))
                    {
                        property.SetValue(obj, MemoryReader.Read<byte>(ProcessInfo.Handle, propertyAddress));
                    }
                }
            }
        }
       
    }
}
