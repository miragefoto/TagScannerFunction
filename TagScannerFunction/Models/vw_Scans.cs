using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TagScannerFunction.Models.data;
using Microsoft.WindowsAzure.Storage.Table;

namespace TagScannerFunction.Models
{
    public class vw_Scans : TableEntity
    {
        public string TagNo { get; set; }
        public string Location { get; set; }
        public int? Status { get; set; }
        public string EditBy { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public int GlobalPkey { get; set; }
        public string GlobalValue { get; set; }

    }
}
