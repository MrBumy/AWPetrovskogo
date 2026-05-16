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
using Application = AWPetrovskogo.Data.Application;

namespace AWPetrovskogo.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeePage.xaml
    /// </summary>
    public partial class EmployeePage : Page
    {
        private int currentUserId;

        public EmployeePage()
        {
            InitializeComponent();
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var currentUser = ConnectObject.GetConnect().Users.FirstOrDefault();
            if (currentUser != null)
            {
                currentUserId = currentUser.UserID;
            }

            LoadData();
        }

        private void LoadData()
        {
            LoadYears();

            LoadMonths();

            CBudgetArticle.ItemsSource = ConnectObject.GetConnect().Articles.ToList();

            CBApplicationCompany.ItemsSource = ConnectObject.GetConnect().Companies.ToList();
            CBApplicationArticle.ItemsSource = ConnectObject.GetConnect().Articles.ToList();

            CBReportType.ItemsSource = ConnectObject.GetConnect().ReportTypes.ToList();

            CBReportFormat.ItemsSource = ConnectObject.GetConnect().ReportFormats.ToList();

            DPFromDate.SelectedDate = DateTime.Now.AddMonths(-1);
            DPToDate.SelectedDate = DateTime.Now;
        }

        private void LoadYears()
        {
            CBudgetYear.ItemsSource = Enumerable.Range(2026, 16).ToList();
            CBudgetYear.SelectedValue = DateTime.Now.Year;
        }

        private void LoadMonths()
        {
            CBudgetMonth.ItemsSource = new[]
            {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };
            CBudgetMonth.SelectedIndex = DateTime.Now.Month - 1;
        }

        private void LoadBudgets()
        {
            if (CBudgetYear.SelectedValue == null || CBudgetMonth.SelectedValue == null) return;

            int year = (int)CBudgetYear.SelectedValue;
            int month = CBudgetMonth.SelectedIndex + 1;

            var budgets = ConnectObject.GetConnect().Budgets
                .Where(b => b.Year == year && b.Month == month)
                .Join(ConnectObject.GetConnect().Articles,
                      b => b.ArticleID,
                      a => a.ArticleID,
                      (b, a) => new
                      {
                          a.ArticleName,
                          b.Limit
                      })
                .ToList();

            DGBudgets.ItemsSource = budgets;
        }

        private void CBudgetYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgets();
        }

        private void CBudgetMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgets();
        }

        private void CBudgetArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBudgetArticle.SelectedValue == null)
            {
                return;
            }

            int articleId = (int)CBudgetArticle.SelectedValue;
            int year = (int)CBudgetYear.SelectedValue;
            int month = CBudgetMonth.SelectedIndex + 1;

            var budget = ConnectObject.GetConnect().Budgets.FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

            if (budget != null)
            {
                TBudgetLimit.Text = budget.Limit.ToString();
            }
            else
            {
                TBudgetLimit.Clear();
            }
        }

        private void SaveBudgetButton_Click(object sender, RoutedEventArgs e)
        {
            if (CBudgetArticle.SelectedValue == null)
            {
                MessageBox.Show("Выберите статью расходов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBudgetLimit.Text))
            {
                MessageBox.Show("Введите лимит!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal limit = Convert.ToDecimal(TBudgetLimit.Text);
            if (limit < 0)
            {
                MessageBox.Show("Лимит не может быть отрицательным!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int articleId = (int)CBudgetArticle.SelectedValue;
            int year = (int)CBudgetYear.SelectedValue;
            int month = CBudgetMonth.SelectedIndex + 1;

            try
            {
                var existingBudget = ConnectObject.GetConnect().Budgets.FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

                if (existingBudget != null)
                {
                    existingBudget.Limit = limit;
                    MessageBox.Show("Лимит успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var newBudget = new Budget
                    {
                        Year = year,
                        Month = month,
                        ArticleID = articleId,
                        Limit = limit
                    };
                    ConnectObject.GetConnect().Budgets.Add(newBudget);
                    MessageBox.Show("Бюджет успешно утвержден!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                ConnectObject.GetConnect().SaveChanges();
                LoadBudgets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CBApplicationArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBApplicationArticle.SelectedValue == null)
            {
                return;
            }

            int articleId = (int)CBApplicationArticle.SelectedValue;
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            var budget = ConnectObject.GetConnect().Budgets
                .FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

            decimal limit = budget?.Limit ?? 0;

            var usedSum = ConnectObject.GetConnect().Applications
                .Where(a => a.ArticleID == articleId &&
                            a.StatusID != 6 &&
                            a.CreatedDate.Year == year &&
                            a.CreatedDate.Month == month)
                .Sum(a => (decimal?)a.SumInRubles) ?? 0;

            decimal remaining = limit - usedSum;
            TBRemaining.Text = remaining.ToString("N2");
        }

        private void CreateApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TBTransferName.Text))
            {
                MessageBox.Show("Введите наименование платежа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CBApplicationCompany.SelectedValue == null)
            {
                MessageBox.Show("Выберите контрагента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBReason.Text))
            {
                MessageBox.Show("Введите основание платежа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CBApplicationArticle.SelectedValue == null)
            {
                MessageBox.Show("Выберите статью расходов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBSum.Text))
            {
                MessageBox.Show("Введите сумму!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal sum = Convert.ToDecimal(TBSum.Text);
            if (sum <= 0)
            {
                MessageBox.Show("Сумма должна быть больше 0!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            decimal remaining = Convert.ToDecimal(TBRemaining.Text);
            if (sum > remaining)
            {
                MessageBox.Show($"Сумма превышает доступный остаток! Доступно: {remaining:N2} руб.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var newApplication = new Application
                {
                    TransferName = TBTransferName.Text,
                    CompanyID = (int)CBApplicationCompany.SelectedValue,
                    Reason = TBReason.Text,
                    ArticleID = (int)CBApplicationArticle.SelectedValue,
                    SumInRubles = sum,
                    StatusID = 1,
                    CreatedDate = DateTime.Now
                };

                ConnectObject.GetConnect().Applications.Add(newApplication);
                ConnectObject.GetConnect().SaveChanges();

                MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                TBTransferName.Clear();
                TBReason.Clear();
                TBSum.Clear();
                CBApplicationCompany.SelectedIndex = -1;
                CBApplicationArticle.SelectedIndex = -1;
                TBRemaining.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (CBReportType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип отчета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (DPFromDate.SelectedDate == null || DPToDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите период!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CBReportFormat.SelectedValue == null)
            {
                MessageBox.Show("Выберите формат отчета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DateTime fromDate = DPFromDate.SelectedDate.Value;
            DateTime toDate = DPToDate.SelectedDate.Value;

            if (fromDate > toDate)
            {
                MessageBox.Show("Дата 'с' не может быть позже даты 'по'!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var newReport = new Report
                {
                    ReportTypeID = (int)CBReportType.SelectedValue,
                    FromDate = fromDate,
                    ToDate = toDate,
                    ReportFormatID = (int)CBReportFormat.SelectedValue,
                    IsCancelledApplications = CHKCancelledApplications.IsChecked ?? false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = currentUserId
                };

                ConnectObject.GetConnect().Reports.Add(newReport);
                ConnectObject.GetConnect().SaveChanges();

                MessageBox.Show($"Отчет успешно сформирован!\n\n" +
                    $"Тип: {CBReportType.Text}\n" +
                    $"Период: {fromDate:dd.MM.yyyy} - {toDate:dd.MM.yyyy}\n" +
                    $"Формат: {CBReportFormat.Text}\n" +
                    $"ID отчета: {newReport.ReportID}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
