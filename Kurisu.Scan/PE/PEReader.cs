using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace Kurisu.Scan.PE
{
    class PEReader
    {
        public Stream Stream { get; private set; }

        public List<Module> Imports { get; set; }

        // headers
        public ImageSectionHeader[] Sections { get; private set; }

        public ImageDosHeader ImageDosHeader { get; private set; }

        public ImageFileHeader ImageFileHeader { get; private set; }

        public ImageOptionalHeader64 ImageOptionalHeader { get; private set; }

        public PEReader(Stream file)
        {
            Stream = file;
            Imports = new List<Module>();
        }

        public void Parse()
        {
            Stream.Position = 0;

            ImageDosHeader = Read<ImageDosHeader>();
            Stream.Position = ImageDosHeader.e_lfanew;

            var ImageFileHeader = Read<ImageFileHeader>();

            // if not x64
            if (ImageFileHeader.Machine != 0x8664)
            {
                throw new NotImplementedException();
            }

            ImageOptionalHeader = Read<ImageOptionalHeader64>();

            // read all sections
            Sections = new ImageSectionHeader[ImageFileHeader.NumberOfSections];
            for(int i = 0; i < ImageFileHeader.NumberOfSections; i++)
            {
                Sections[i] = Read<ImageSectionHeader>();
            }

            // get the import table and find the section
            var importTable = ImageOptionalHeader.DataDirectory[1];
            var dataSection = FindSectionByRVA(importTable.VirtualAddress);

            // jump to import table section
            Stream.Position = importTable.VirtualAddress - dataSection.VirtualAddress + dataSection.PointerToRawData;

            // read all import directories
            var entries = new List<ImportDirectoryTable>();
            do
            {
                var entry = Read<ImportDirectoryTable>();
                if (entry.AddressRVA == 0) break;

                entries.Add(entry);
            } while (true);

            foreach(var entry in entries)
            {
                var module = new Module();
                module.Imports = new List<Import>();

                Stream.Position = entry.Name - dataSection.VirtualAddress + dataSection.PointerToRawData;
                module.Name = ReadString();

                Stream.Position = entry.LookupTableRVA - dataSection.VirtualAddress + dataSection.PointerToRawData;

                do
                {
                    Import import;
                    var read = ReadImportLookupTable(out import);
                    if (!read) break;

                    module.Imports.Add(import);
                } while (true);

                Imports.Add(module);
            }
        }

        private T Read<T>()
        {
            byte[] buffer = new byte[Marshal.SizeOf<T>()];
            Stream.Read(buffer, 0, buffer.Length);

            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        private bool ReadImportLookupTable(out Import import)
        {
            var lookup = Read<ulong>();
            if (lookup == 0)
            {
                import = null;
                return false;
            }

            import = new Import();

            var isOrdinal = (lookup & 0x8000000000000000) != 0;
            if (isOrdinal)
            {
                // TODO
                return false;
            }
            else
            {
                var oldPosition = Stream.Position;
                var section = FindSectionByRVA((uint) lookup);

                Stream.Position = (long)(lookup - section.VirtualAddress + section.PointerToRawData) + 2;
                import.Name = ReadString();

                Stream.Position = oldPosition;
            };

            return true;
        }

        private ImageSectionHeader FindSectionByRVA(uint rva)
        {
            return Sections.First(x => rva >= x.VirtualAddress && rva < x.VirtualAddress + x.VirtualSize);
        }

        /// <summary>
        /// Reads a null-terminated string
        /// </summary>
        private string ReadString()
        {
            var name = new List<byte>();

            do
            {
                var c = Read<byte>();
                if (c == 0) break;

                name.Add(c);
            } while (true);

            return Encoding.ASCII.GetString(name.ToArray());
        }
    }
}
