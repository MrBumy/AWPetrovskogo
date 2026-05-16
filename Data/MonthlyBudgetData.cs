using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWPetrovskogo.Data
{
    internal class MonthlyBudgetData
    {
        /// <summary>
        /// Класс для данных по бюджету по месяцам
        /// </summary>
        public string MonthName { get; set; } // Название месяца
        public decimal Limit { get; set; } // Лимит за месяц
        public decimal Spent { get; set; } // Потрачено за месяц
        public decimal Remaining { get; set; } // Остаток на месяц
        public decimal Percent { get; set; } // Процент использования
    }
}
