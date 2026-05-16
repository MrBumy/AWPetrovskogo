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
using System.IO;
using Microsoft.Win32;
using AWPetrovskogo.Data;
using Application = AWPetrovskogo.Data.Application;

namespace AWPetrovskogo.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeePage.xaml
    /// </summary>
    public partial class EmployeePage : Page
    {
        // ID текущего пользователя
        private int currentUserId;

        public EmployeePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Срабатывает при изменении видимости страницы
        /// Загружает данные, когда страница становится видимой
        /// </summary>
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Получение текущего пользователя
            var currentUser = ConnectObject.GetConnect().Users.FirstOrDefault();
            if (currentUser != null)
            {
                currentUserId = currentUser.UserID;
            }

            // Загрузка данных
            LoadData();
        }

        /// <summary>
        /// Загрузка начальных данных для страницы
        /// </summary>
        private void LoadData()
        {
            // Загрузка годов и месяцев для фильтров
            LoadYears();
            LoadMonths();

            // Загрузка статей расходов для бюджета
            CBudgetArticle.ItemsSource = ConnectObject.GetConnect().Articles.ToList();

            // Загрузка данных для формы создания заявки
            CBApplicationCompany.ItemsSource = ConnectObject.GetConnect().Companies.ToList();
            CBApplicationArticle.ItemsSource = ConnectObject.GetConnect().Articles.ToList();

            // Загрузка данных для формы отчета
            CBReportType.ItemsSource = ConnectObject.GetConnect().ReportTypes.ToList();
            CBReportFormat.ItemsSource = ConnectObject.GetConnect().ReportFormats.ToList();

            // Установка изначальной даты
            DPFromDate.SelectedDate = DateTime.Now.AddMonths(-1);
            DPToDate.SelectedDate = DateTime.Now;
        }

        /// <summary>
        /// Загрузка списка годов для выбора
        /// </summary>
        private void LoadYears()
        {
            // Годы с 2026 по 2041
            CBudgetYear.ItemsSource = Enumerable.Range(2026, 16).ToList();
            CBudgetYear.SelectedValue = DateTime.Now.Year; // Текущий год по умолчанию
        }

        /// <summary>
        /// Загрузка списка месяцев для выбора
        /// </summary>
        private void LoadMonths()
        {
            // Массив месяцев
            CBudgetMonth.ItemsSource = new[]
            {
                "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь"
            };
            
            // Текущий месяц по умолчанию
            CBudgetMonth.SelectedIndex = DateTime.Now.Month - 1;
        }

        /// <summary>
        /// Загрузка лимитов бюджета для выбранного года и месяца
        /// </summary>
        private void LoadBudgets()
        {
            if (CBudgetYear.SelectedValue == null || CBudgetMonth.SelectedValue == null) return;

            int year = (int)CBudgetYear.SelectedValue;
            int month = CBudgetMonth.SelectedIndex + 1;

            // Бюджеты за выбранный период
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

        // Обработчик смены года
        private void CBudgetYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgets();
        }

        // Обработчик смены месяца
        private void CBudgetMonth_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadBudgets();
        }

        /// <summary>
        /// Обработчик выбора статьи расходов для бюджета
        /// Подгружает текущий лимит для редактирования
        /// </summary>
        private void CBudgetArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBudgetArticle.SelectedValue == null)
            {
                return;
            }

            int articleId = (int)CBudgetArticle.SelectedValue;
            int year = (int)CBudgetYear.SelectedValue;
            int month = CBudgetMonth.SelectedIndex + 1;

            // Поиск существующего бюджета
            var budget = ConnectObject.GetConnect().Budgets.FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

            if (budget != null)
            {
                // Если найден, заполняется поле лимита
                TBudgetLimit.Text = budget.Limit.ToString();
            }
            else
            {
                // Иначе очищается
                TBudgetLimit.Clear();
            }
        }

        /// <summary>
        /// Сохранение/обновление лимита бюджета
        /// </summary>
        private void SaveBudgetButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на выбор статьи расходов
            if (CBudgetArticle.SelectedValue == null)
            {
                MessageBox.Show("Выберите статью расходов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на указание лимита
            if (string.IsNullOrEmpty(TBudgetLimit.Text))
            {
                MessageBox.Show("Введите лимит!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Логическая проверка лимита на отрицательные значения
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
                // Проверка, существует ли уже бюджет на этот месяц/статью
                var existingBudget = ConnectObject.GetConnect().Budgets.FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

                // Если бюджет уже существует, обновляем значение
                if (existingBudget != null)
                {
                    existingBudget.Limit = limit;
                    MessageBox.Show("Лимит успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Иначе создаем новый экзлемпляр Budget
                    var newBudget = new Budget
                    {
                        Year = year,
                        Month = month,
                        ArticleID = articleId,
                        Limit = limit
                    };

                    // Добавляем экземпляр в базу данных
                    ConnectObject.GetConnect().Budgets.Add(newBudget);
                    MessageBox.Show("Бюджет успешно утвержден!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Сохраняем изменения и обновляем список бюджетов
                ConnectObject.GetConnect().SaveChanges();
                LoadBudgets();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обработчик выбора статьи расходов в заявке
        /// Рассчитывает доступный остаток по бюджету
        /// </summary>
        private void CBApplicationArticle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBApplicationArticle.SelectedValue == null)
            {
                return;
            }

            int articleId = (int)CBApplicationArticle.SelectedValue;
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;

            // Получение лимита по статье на текущий месяц
            var budget = ConnectObject.GetConnect().Budgets.FirstOrDefault(b => b.ArticleID == articleId && b.Year == year && b.Month == month);

            // Если бюджета нету, лимит 0
            decimal limit = budget?.Limit ?? 0;

            // Сумма потраченных средств по статье расходов
            var usedSum = ConnectObject.GetConnect().Applications
                .Where(a => a.ArticleID == articleId &&
                            a.StatusID != 3 &&
                            a.CreatedDate.Year == year &&
                            a.CreatedDate.Month == month)
                .Sum(a => (decimal?)a.SumInRubles) ?? 0;

            // Рассчет остатка по статье, вычитаем из лимита использованную сумму
            decimal remaining = limit - usedSum;
            TBRemaining.Text = remaining.ToString("N2");
        }

        /// <summary>
        /// Создание новой заявки
        /// </summary>
        private void CreateApplicationButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения поля с названием платежа
            if (string.IsNullOrEmpty(TBTransferName.Text))
            {
                MessageBox.Show("Введите наименование платежа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на выбор контрагента
            if (CBApplicationCompany.SelectedValue == null)
            {
                MessageBox.Show("Выберите контрагента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на заполнение основания платежа
            if (string.IsNullOrEmpty(TBReason.Text))
            {
                MessageBox.Show("Введите основание платежа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на выбор статьи расходов
            if (CBApplicationArticle.SelectedValue == null)
            {
                MessageBox.Show("Выберите статью расходов!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на заполнение поля с суммой
            if (string.IsNullOrEmpty(TBSum.Text))
            {
                MessageBox.Show("Введите сумму!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Логическая проверка на сумму, трата не может быть меньше нуля
            decimal sum = Convert.ToDecimal(TBSum.Text);
            if (sum <= 0)
            {
                MessageBox.Show("Сумма должна быть больше 0!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на превышение остатка
            decimal remaining = Convert.ToDecimal(TBRemaining.Text);
            if (sum > remaining)
            {
                MessageBox.Show($"Сумма превышает доступный остаток! Доступно: {remaining} руб.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Создание экземпляра заявки (Application)
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

                // Добавление экземпляра в базу данных и сохранение изменений
                ConnectObject.GetConnect().Applications.Add(newApplication);
                ConnectObject.GetConnect().SaveChanges();

                MessageBox.Show("Заявка успешно создана!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка полей
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

        /// <summary>
        /// Генерация отчета в соответствующем формате
        /// </summary>
        private void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка на выбор типа отчета
                if (CBReportType.SelectedValue == null)
                {
                    MessageBox.Show("Выберите тип отчета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка на выбор формата отчета
                if (CBReportFormat.SelectedValue == null)
                {
                    MessageBox.Show("Выберите формат отчета!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Получение состояния чекбокса про отклоненные заявки
                bool includeCancelled = CHKCancelledApplications.IsChecked ?? false;

                // Создание папки для отчетов
                string reportsFolder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports");
                if (!Directory.Exists(reportsFolder))
                {
                    Directory.CreateDirectory(reportsFolder);
                }

                // Создание имени отчета
                string fileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                string filePath = System.IO.Path.Combine(reportsFolder, fileName);

                using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    if ((int)CBReportType.SelectedValue == 1) // Отчет по заявкам
                    {
                        // Заголовки столбцов
                        writer.WriteLine("ID;Наименование платежа;Компания;Статья;Сумма;Статус;Дата");

                        // Получение заявок, соответствующих выбранным фильтрам
                        var appsQuery = ConnectObject.GetConnect().Applications
                            .Where(a => a.CreatedDate >= DPFromDate.SelectedDate.Value && // Дата начала
                                       a.CreatedDate <= DPToDate.SelectedDate.Value); // Дата конца

                        // Если не нужно учитывать отклоненные заявки, фильтруем список
                        if (!includeCancelled)
                        {
                            appsQuery = appsQuery.Where(a => a.StatusID != 3);
                        }

                        var apps = appsQuery.ToList();

                        // Запись каждой заявки в файл
                        foreach (var app in apps)
                        {
                            var company = ConnectObject.GetConnect().Companies.Find(app.CompanyID);
                            var article = ConnectObject.GetConnect().Articles.Find(app.ArticleID);
                            var status = ConnectObject.GetConnect().Statuses.Find(app.StatusID);

                            // Формирование строки
                            writer.WriteLine($"{app.ApplicationID};{app.TransferName};{company?.CompanyName};{article?.ArticleName};{app.SumInRubles};{status?.StatusName};{app.CreatedDate:dd.MM.yyyy}");
                        }
                    }
                    else // Отчет по бюджету
                    {
                        // Заголовки столбцов
                        writer.WriteLine("Год;Месяц;Статья;Лимит;Расход;Остаток");

                        // Получение всех бюджетов
                        var budgets = ConnectObject.GetConnect().Budgets.ToList();

                        // Запись каждого бюджета в файл
                        foreach (var budget in budgets)
                        {
                            var article = ConnectObject.GetConnect().Articles.Find(budget.ArticleID);

                            // Счет суммы заявок
                            var spentQuery = ConnectObject.GetConnect().Applications
                                .Where(a => a.ArticleID == budget.ArticleID && // Тип статьи
                                           a.CreatedDate.Year == budget.Year && // Год
                                           a.CreatedDate.Month == budget.Month); // Месяц

                            // Если не нужно учитывать отклоненные, фильтруем
                            if (!includeCancelled)
                            {
                                spentQuery = spentQuery.Where(a => a.StatusID != 3);
                            }

                            // Суммирование всех значений
                            decimal spent = spentQuery.Sum(a => (decimal?)a.SumInRubles) ?? 0;

                            // Запись строки в файл
                            writer.WriteLine($"{budget.Year};{budget.Month};{article?.ArticleName};{budget.Limit};{spent};{budget.Limit - spent}");
                        }
                    }
                }

                MessageBox.Show($"Отчет сохранен: {filePath}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
