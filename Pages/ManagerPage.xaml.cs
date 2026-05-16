using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AWPetrovskogo.Data;

namespace AWPetrovskogo.Pages
{
    /// <summary>
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        // ID выбранной заявки для утверждения/отклонения
        private int selectedApplicationId;

        public ManagerPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Срабатывает при изменении видимости страницы
        /// Загружает данные, когда страница становится видимой
        /// </summary>
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Загрузка списка заявок
            LoadApplications();

            // Загрузка годов для фильтра
            LoadYears();

            // Загрузка статей расходов
            LoadBudgetArticles();
        }

        /// <summary>
        /// Загрузка списка всех заявок для руководителя
        /// </summary>
        private void LoadApplications()
        {
            // Формирование запроса с JOIN таблиц для получения всех связанных данных
            var query = ConnectObject.GetConnect().Applications
                .Join(ConnectObject.GetConnect().Companies, a => a.CompanyID, c => c.CompanyID, (a, c) => new { a, c })
                .Join(ConnectObject.GetConnect().Articles, ac => ac.a.ArticleID, art => art.ArticleID, (ac, art) => new { ac.a, ac.c, art })
                .Join(ConnectObject.GetConnect().Statuses, aca => aca.a.StatusID, s => s.StatusID, (aca, s) => new { aca.a, aca.c, aca.art, s })
                .Select(x => new
                {
                    x.a.ApplicationID,
                    x.a.TransferName,
                    x.c.CompanyName,
                    x.a.SumInRubles,
                    x.s.StatusName
                })
                .ToList();

            // Преобразование в формат для отображения
            var applications = query.Select(x => new ApplicationItem
            {
                ApplicationID = x.ApplicationID,
                TransferName = x.TransferName,
                CompanyName = x.CompanyName,
                SumInRubles = x.SumInRubles,
                StatusName = x.StatusName,
                StatusColor = GetStatusColor(x.StatusName), // Цвет статуса заявки
                DisplayText = $"{x.TransferName} - {x.CompanyName} - {x.SumInRubles:N2} руб. ({x.StatusName})"
            }).ToList();

            // Отображение списка заявок
            ApplicationsList.ItemsSource = applications;
        }

        /// <summary>
        /// Загрузка заявок с применением фильтра по статусу
        /// </summary>
        private void LoadApplicationsWithFilter()
        {
            // Проверка, выбран ли фильтр
            if (CBStatusFilter.SelectedItem == null)
            {
                return;
            }

            // Получение соответствующего статуса для фильтрации
            string filter = (CBStatusFilter.SelectedItem as ComboBoxItem).Content.ToString();

            // Запрос с JOIN таблиц
            var query = ConnectObject.GetConnect().Applications
                .Join(ConnectObject.GetConnect().Companies, a => a.CompanyID, c => c.CompanyID, (a, c) => new { a, c })
                .Join(ConnectObject.GetConnect().Articles, ac => ac.a.ArticleID, art => art.ArticleID, (ac, art) => new { ac.a, ac.c, art })
                .Join(ConnectObject.GetConnect().Statuses, aca => aca.a.StatusID, s => s.StatusID, (aca, s) => new { aca.a, aca.c, aca.art, s });

            // Если выбран любой фильтр кроме "Все заявки", применяем его
            if (filter != "Все заявки")
            {
                query = query.Where(x => x.s.StatusName == filter);
            }

            // Получение данных 
            var data = query.Select(x => new
            {
                x.a.ApplicationID,
                x.a.TransferName,
                x.c.CompanyName,
                x.a.SumInRubles,
                x.s.StatusName
            })
            .ToList();

            // Преобразование в формат для отображения
            var applications = data.Select(x => new ApplicationItem
            {
                ApplicationID = x.ApplicationID,
                TransferName = x.TransferName,
                CompanyName = x.CompanyName,
                SumInRubles = x.SumInRubles,
                StatusName = x.StatusName,
                StatusColor = GetStatusColor(x.StatusName),
                DisplayText = $"{x.TransferName} - {x.CompanyName} - {x.SumInRubles:N2} руб. ({x.StatusName})"
            }).ToList();

            // Обновление списка
            ApplicationsList.ItemsSource = applications;
        }

        /// <summary>
        /// Возвращает цвет для фона статуса заявки
        /// </summary>
        /// <param name="statusName">Название статуса</param>
        /// <returns>HEX цвет в формате #RRGGBB</returns>
        private string GetStatusColor(string statusName)
        {
            switch (statusName)
            {
                case "На согласовании":
                    return "#2196F3"; // Синий
                case "Согласована": 
                    return "#4CAF50"; // Зеленый
                case "Отклонена":
                    return "#F44336"; // Красный
                default:
                    return "#999"; // Серый
            }
        }

        /// <summary>
        /// Обработчик изменения фильтра статусов
        /// </summary>
        private void CBStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Обновление списка с учетом фильтра
            LoadApplicationsWithFilter();
        }

        private void CBStatusFilter_Loaded(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Обработчик клика по заявке - показывает детальную информацию
        /// </summary>
        private void ApplicationItem_Click(object sender, MouseButtonEventArgs e)
        {
            // Получение выбранной заявки из Tag элемента
            var border = sender as Border;
            var application = border?.Tag as ApplicationItem;

            if (application == null)
            {
                return;
            }

            // Сохранения ID заявки для последующего взаимодействия
            selectedApplicationId = application.ApplicationID;

            // Формировние сообщения с деталями заявки
            string message = $"Заявка #{application.ApplicationID}\n\n" +
                $"Наименование: {application.TransferName}\n" +
                $"Компания: {application.CompanyName}\n" +
                $"Сумма: {application.SumInRubles:N2} руб.\n" +
                $"Статус: {application.StatusName}";

            MessageBox.Show(message, "Детали заявки", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Утверждение заявки
        /// </summary>
        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на выбор заявки
            if (selectedApplicationId == 0)
            {
                MessageBox.Show("Выберите заявку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Смена статуса заявки
            ChangeApplicationStatus(selectedApplicationId, "Согласована");
        }

        /// <summary>
        /// Отклонение заявки
        /// </summary>
        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на выбор заявки
            if (selectedApplicationId == 0)
            {
                MessageBox.Show("Выберите заявку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Смена статуса заявки
            ChangeApplicationStatus(selectedApplicationId, "Отклонена");
        }

        /// <summary>
        /// Изменение статуса заявки
        /// </summary>
        /// <param name="applicationId">ID заявки</param>
        /// <param name="newStatus">Новый статус (Согласована/Отклонена)</param>
        private void ChangeApplicationStatus(int applicationId, string newStatus)
        {
            try
            {
                // Поиск заявки в базе данных
                var application = ConnectObject.GetConnect().Applications.Find(applicationId);
                if (application == null)
                {
                    return;
                }

                // Поиск статуса по названию
                var status = ConnectObject.GetConnect().Statuses.FirstOrDefault(s => s.StatusName == newStatus);
                if (status != null)
                {
                    // Обновление статуса и даты изменения, сохранение изменений
                    application.StatusID = status.StatusID;
                    application.UpdatedDate = DateTime.Now;
                    ConnectObject.GetConnect().SaveChanges();

                    MessageBox.Show($"Заявка #{applicationId} {(newStatus == "Согласована" ? "утверждена" : "отклонена")}!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновление списка заявок
                    LoadApplications();
                    LoadApplicationsWithFilter();
                    selectedApplicationId = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загрузка списка годов для выбора
        /// </summary>
        private void LoadYears()
        {
            // Годы с 2026 по 2041
            CBBudgetYear.ItemsSource = Enumerable.Range(2026, 16).ToList();
            // Текущий год по умолчанию
            CBBudgetYear.SelectedValue = DateTime.Now.Year;
        }

        /// <summary>
        /// Загрузка статей расходов для мониторинга бюджета
        /// </summary>
        private void LoadBudgetArticles()
        {
            CBBudgetArticle.ItemsSource = ConnectObject.GetConnect().Articles.ToList();
        }

        /// <summary>
        /// Обработчик выбора статьи расходов
        /// </summary>
        private void CBBudgetArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgetData();
        }

        /// <summary>
        /// Обработчик выбора года
        /// </summary>
        private void CBBudgetYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgetData();
        }

        /// <summary>
        /// Загрузка и отображение данных по бюджету
        /// </summary>
        private void LoadBudgetData()
        {
            // Проверка на выбор статьи и года
            if (CBBudgetArticle.SelectedValue == null || CBBudgetYear.SelectedValue == null)
            {
                return;
            }

            int articleId = (int)CBBudgetArticle.SelectedValue;
            int year = (int)CBBudgetYear.SelectedValue;

            // Проверка статьи на существование
            var article = ConnectObject.GetConnect().Articles.Find(articleId);
            if (article == null)
            {
                return;
            }

            // Список для хранения данных по месяцам
            var monthlyData = new List<MonthlyBudgetData>();
            decimal totalLimit = 0;
            decimal totalSpent = 0;

            // Массив названий месяцев
            string[] months = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

            // Обработка каждого месяца
            for (int month = 1; month <= 12; month++)
            {
                // Получение бюджета на месяц
                var budget = ConnectObject.GetConnect().Budgets.FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

                decimal limit = budget?.Limit ?? 0; // Лимит на месяц, 0 если отсутствует
                totalLimit += limit;

                // Расчет расходов на месяц
                decimal spent = ConnectObject.GetConnect().Applications
                    .Where(a => a.ArticleID == articleId &&
                               a.CreatedDate.Year == year &&
                               a.CreatedDate.Month == month &&
                               a.StatusID != 3)
                    .Sum(a => (decimal?)a.SumInRubles) ?? 0;
                totalSpent += spent;

                // Расчет остатка и процента использования
                decimal remaining = limit - spent;
                decimal percent = limit > 0 ? (spent / limit) * 100 : 0;

                // Добавление данных за месяц
                monthlyData.Add(new MonthlyBudgetData
                {
                    MonthName = months[month - 1],
                    Limit = limit,
                    Spent = spent,
                    Remaining = remaining,
                    Percent = percent
                });
            }

            // Отображение детализации по месяцам
            DGMonthlyBudget.ItemsSource = monthlyData;

            // Расчет общих показателей
            decimal totalRemaining = totalLimit - totalSpent;
            decimal totalPercent = totalLimit > 0 ? (totalSpent / totalLimit) * 100 : 0;

            // Отображение информации
            TBTotalLimit.Text = $"{totalLimit:N2} руб.";
            TBTotalSpent.Text = $"{totalSpent:N2} руб.";
            TBTotalRemaining.Text = $"{totalRemaining:N2} руб.";
            TBPercentUsed.Text = $"{totalPercent:F1}%";
        }

        /// <summary>
        /// Обработчик кнопки выхода
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            FrameObject.frameMain.GoBack();
        }
    }
}
