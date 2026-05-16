using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWPetrovskogo.Data
{
    internal class ApplicationItem
    {
        public int ApplicationID { get; set; }
        public string TransferName { get; set; }
        public string CompanyName { get; set; }
        public decimal SumInRubles { get; set; }
        public string StatusName { get; set; }
        public string StatusColor { get; set; }
        public string DisplayText { get; set; }
    }
}
