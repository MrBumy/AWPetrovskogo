using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWPetrovskogo.Data
{
    internal class MonthlyBudgetData
    {
        public string MonthName { get; set; }
        public decimal Limit { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal Percent { get; set; }
    }
}
