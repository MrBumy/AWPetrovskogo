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
        private int selectedApplicationId;

        public ManagerPage()
        {
            InitializeComponent();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoadApplications();
            LoadYears();
            LoadBudgetArticles();
        }

        private void LoadApplications()
        {
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

            var applications = query.Select(x => new ApplicationItem
            {
                ApplicationID = x.ApplicationID,
                TransferName = x.TransferName,
                CompanyName = x.CompanyName,
                SumInRubles = x.SumInRubles,
                StatusName = x.StatusName,
                StatusColor = GetStatusColor(x.StatusName),
                DisplayText = $"{x.TransferName} - {x.CompanyName} - {x.SumInRubles:N2} руб. ({x.StatusName})"
            }).ToList();

            ApplicationsList.ItemsSource = applications;
        }

        private void LoadApplicationsWithFilter()
        {
            if (CBStatusFilter.SelectedItem == null)
            {
                return;
            }

            string filter = (CBStatusFilter.SelectedItem as ComboBoxItem).Content.ToString();

            var query = ConnectObject.GetConnect().Applications
                .Join(ConnectObject.GetConnect().Companies, a => a.CompanyID, c => c.CompanyID, (a, c) => new { a, c })
                .Join(ConnectObject.GetConnect().Articles, ac => ac.a.ArticleID, art => art.ArticleID, (ac, art) => new { ac.a, ac.c, art })
                .Join(ConnectObject.GetConnect().Statuses, aca => aca.a.StatusID, s => s.StatusID, (aca, s) => new { aca.a, aca.c, aca.art, s });

            if (filter != "Все заявки")
            {
                query = query.Where(x => x.s.StatusName == filter);
            }

            var data = query.Select(x => new
            {
                x.a.ApplicationID,
                x.a.TransferName,
                x.c.CompanyName,
                x.a.SumInRubles,
                x.s.StatusName
            })
            .ToList();

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

            ApplicationsList.ItemsSource = applications;
        }

        private string GetStatusColor(string statusName)
        {
            switch (statusName)
            {
                case "На согласовании":
                    return "#2196F3";
                case "Согласована":
                    return "#4CAF50";
                case "Отклонена":
                    return "#F44336";
                default:
                    return "#999";
            }
        }

        private void CBStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadApplicationsWithFilter();
        }

        private void CBStatusFilter_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void ApplicationItem_Click(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;
            var application = border?.Tag as ApplicationItem;

            if (application == null)
            {
                return;
            }

            selectedApplicationId = application.ApplicationID;

            string message = $"Заявка #{application.ApplicationID}\n\n" +
                $"Наименование: {application.TransferName}\n" +
                $"Компания: {application.CompanyName}\n" +
                $"Сумма: {application.SumInRubles:N2} руб.\n" +
                $"Статус: {application.StatusName}";

            MessageBox.Show(message, "Детали заявки", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicationId == 0)
            {
                MessageBox.Show("Выберите заявку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChangeApplicationStatus(selectedApplicationId, "Согласована");
        }

        private void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedApplicationId == 0)
            {
                MessageBox.Show("Выберите заявку!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChangeApplicationStatus(selectedApplicationId, "Отклонена");
        }

        private void ChangeApplicationStatus(int applicationId, string newStatus)
        {
            try
            {
                var application = ConnectObject.GetConnect().Applications.Find(applicationId);
                if (application == null) return;

                var status = ConnectObject.GetConnect().Statuses.FirstOrDefault(s => s.StatusName == newStatus);
                if (status != null)
                {
                    application.StatusID = status.StatusID;
                    application.UpdatedDate = DateTime.Now;
                    ConnectObject.GetConnect().SaveChanges();

                    MessageBox.Show($"Заявка #{applicationId} {(newStatus == "Согласована" ? "утверждена" : "отклонена")}!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

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

        private void LoadYears()
        {
            var years = new List<int>();
            for (int y = 2020; y <= 2035; y++)
            {
                years.Add(y);
            }
            CBBudgetYear.ItemsSource = years;
            CBBudgetYear.SelectedValue = DateTime.Now.Year;
        }

        private void LoadBudgetArticles()
        {
            CBBudgetArticle.ItemsSource = ConnectObject.GetConnect().Articles.ToList();
        }

        private void CBBudgetArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgetData();
        }

        private void CBBudgetYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgetData();
        }

        private void LoadBudgetData()
        {
            if (CBBudgetArticle.SelectedValue == null || CBBudgetYear.SelectedValue == null) return;

            int articleId = (int)CBBudgetArticle.SelectedValue;
            int year = (int)CBBudgetYear.SelectedValue;

            var article = ConnectObject.GetConnect().Articles.Find(articleId);
            if (article == null) return;

            var monthlyData = new List<MonthlyBudgetData>();
            decimal totalLimit = 0;
            decimal totalSpent = 0;

            string[] months = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

            for (int month = 1; month <= 12; month++)
            {
                var budget = ConnectObject.GetConnect().Budgets
                    .FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

                decimal limit = budget?.Limit ?? 0;
                totalLimit += limit;

                decimal spent = ConnectObject.GetConnect().Applications
                    .Where(a => a.ArticleID == articleId &&
                               a.CreatedDate.Year == year &&
                               a.CreatedDate.Month == month &&
                               a.StatusID != 3)
                    .Sum(a => (decimal?)a.SumInRubles) ?? 0;
                totalSpent += spent;

                decimal remaining = limit - spent;
                decimal percent = limit > 0 ? (spent / limit) * 100 : 0;

                monthlyData.Add(new MonthlyBudgetData
                {
                    MonthName = months[month - 1],
                    Limit = limit,
                    Spent = spent,
                    Remaining = remaining,
                    Percent = percent
                });
            }

            DGMonthlyBudget.ItemsSource = monthlyData;

            decimal totalRemaining = totalLimit - totalSpent;
            decimal totalPercent = totalLimit > 0 ? (totalSpent / totalLimit) * 100 : 0;

            TBTotalLimit.Text = $"{totalLimit:N2} руб.";
            TBTotalSpent.Text = $"{totalSpent:N2} руб.";
            TBTotalRemaining.Text = $"{totalRemaining:N2} руб.";
            TBPercentUsed.Text = $"{totalPercent:F1}%";

            if (totalPercent > 80)
                TBPercentUsed.Foreground = new SolidColorBrush(Colors.Red);
            else if (totalPercent > 50)
                TBPercentUsed.Foreground = new SolidColorBrush(Colors.Orange);
            else
                TBPercentUsed.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                FrameObject.frameMain.GoBack();
            }
        }
    }
}
