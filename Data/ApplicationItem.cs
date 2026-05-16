using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWPetrovskogo.Data
{
    /// <summary>
    /// Класс для отображения заявки в списке
    /// </summary>
    internal class ApplicationItem
    {
        public int ApplicationID { get; set; } // ID заявки
        public string TransferName { get; set; } // Наименование платежа
        public string CompanyName { get; set; } // Название контрагента
        public decimal SumInRubles { get; set; } // Сумма в рублях
        public string StatusName { get; set; } // Наименование статуса
        public string StatusColor { get; set; } // Цвет статуса
        public string DisplayText { get; set; } // Текст для отображения
    }
}
