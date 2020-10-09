using System;
using System.Collections.Generic;
using System.Text;

namespace Kurisu.Scan.PE
{
    public class Module
    {
        public string Name { get; set; }
        public List<Import> Imports { get; set; }
    }
}
