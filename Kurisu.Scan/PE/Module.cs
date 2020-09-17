using System;
using System.Collections.Generic;
using System.Text;

namespace Kurisu.Scan.PE
{
    class Module
    {
        public string Name { get; set; }
        public List<Import> Imports { get; set; }
    }
}
