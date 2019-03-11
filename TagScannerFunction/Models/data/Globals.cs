using System;
using System.Collections.Generic;

namespace TagScannerFunction.Models.data
{
    public partial class Globals
    {

        public int GlobalPkey { get; set; }
        public string GlobalGroup { get; set; }
        public string GlobalValue { get; set; }
        public bool? Active { get; set; }
        public int? EditBy { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
