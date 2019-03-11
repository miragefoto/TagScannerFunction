using System;
using System.Collections.Generic;

namespace TagScannerFunction.Models.data
{
    public partial class Scans
    {
        public string TagNo { get; set; }
        public string Location { get; set; }
        public int? Status { get; set; }
        public string EditBy { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}
