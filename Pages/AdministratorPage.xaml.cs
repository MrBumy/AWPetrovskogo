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
using Microsoft.Win32;
using Newtonsoft.Json;
using AWPetrovskogo.Data;
using System.IO;

namespace AWPetrovskogo.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdministratorPage.xaml
    /// </summary>
    public partial class AdministratorPage : Page
    {
        // Хранит пользователя для редактирования/удаления
        private User tempUser;

        public AdministratorPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки выхода
        /// </summary>
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Возврат обратно на страницу авторизации
            FrameObject.frameMain.GoBack();
        }

        /// <summary>
        /// Срабатывает при изменении видимости страницы
        /// Загружает данные, когда страница становится видимой
        /// </summary>
        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Вызов метода для загрузки данных при показе страницы
            if (Visibility == Visibility.Visible)
            {
                LoadData();
            }
        }

        /// <summary>
        /// Загрузка необходимых для страницы данных
        /// </summary>
        private void LoadData()
        {
            // Загружаем список пользователей
            DGridUsers.ItemsSource = ConnectObject.GetConnect().Users.ToList();

            // Загружаем список ролей в ComboBox добавления
            CBRole.ItemsSource = ConnectObject.GetConnect().Roles.ToList();

            // Загружаем список ролей в ComboBox редактирования
            CBEditRole.ItemsSource = ConnectObject.GetConnect().Roles.ToList();

            // Загружаем пользователей в ComboBox выбора пользователя для редактирования в формате (ID | Фамилия Имя)
            var usersList = ConnectObject.GetConnect().Users.ToList();
            CBUsersForEdit.ItemsSource = usersList.Select(u => new
            {
                u.UserID,
                FullName = string.Format("{0} | {1} {2}", u.UserID, u.LastName, u.FirstName)
            }).ToList();
        }

        /// <summary>
        /// Поиск пользователей по введенному тексту (Фамилия || Имя || Логин || Почта)
        /// </summary>
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Получаем текст поиска в нижнем регистре
            var searchText = TBSearch.Text.ToLower();

            // Фильтр пользователей
            var users = ConnectObject.GetConnect().Users
                .Where(u => u.LastName.ToLower().Contains(searchText) ||
                           u.FirstName.ToLower().Contains(searchText) ||
                           u.Login.ToLower().Contains(searchText) ||
                           u.EMail.ToLower().Contains(searchText))
                .Select(u => new
                {
                    u.UserID,
                    u.LastName,
                    u.FirstName,
                    u.EMail,
                    u.Login,
                    u.Password,
                    u.RoleID,
                    u.IsBlocked
                })
                .ToList();

            // Обновление таблицы отфильтрованными пользователями
            DGridUsers.ItemsSource = users;
        }

        /// <summary>
        /// Обновление списка пользователей и очистка поиска
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс фильтра, обновление таблицы пользователей
            DGridUsers.ItemsSource = ConnectObject.GetConnect().Users.ToList();

            // Обновление ComboBox выбора пользователя
            var usersList = ConnectObject.GetConnect().Users.ToList();
            CBUsersForEdit.ItemsSource = usersList.Select(u => new
            {
                u.UserID,
                FullName = u.UserID.ToString() + " | " + u.LastName + " " + u.FirstName
            }).ToList();

            // Очистка поля поиска
            TBSearch.Clear();
        }

        private void DGridUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        /// <summary>
        /// Добавление нового пользователя
        /// </summary>
        private void AddNewUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения фамилии
            if (string.IsNullOrEmpty(TBLastName.Text))
            {
                MessageBox.Show("Введите фамилию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка заполнения имени
            if (string.IsNullOrEmpty(TBFirstName.Text))
            {
                MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка заполнения логина
            if (string.IsNullOrEmpty(TBLogin.Text))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка заполнения пароля
            if (string.IsNullOrEmpty(PBPassword.Password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка заполнения почты
            if (string.IsNullOrEmpty(TBEmail.Text))
            {
                MessageBox.Show("Введите почту!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка выбора роли
            if (CBRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на существование пользователя с введенным логином
            var existingUser = ConnectObject.GetConnect().Users.Count(u => u.Login == TBLogin.Text);
            if (existingUser >= 1)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на существование пользователя с введенной почтой
            if (!string.IsNullOrEmpty(TBEmail.Text))
            {
                var existingEmail = ConnectObject.GetConnect().Users.Count(u => u.EMail == TBEmail.Text);
                if (existingEmail >= 1)
                {
                    MessageBox.Show("Пользователь с таким Email уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                // Создание нового экземпляра класса User 
                var newUser = new User
                {
                    LastName = TBLastName.Text,
                    FirstName = TBFirstName.Text,
                    Patronymic = string.IsNullOrEmpty(TBPatronymic.Text) ? null : TBPatronymic.Text, // Отчество может быть пустым
                    EMail = TBEmail.Text,
                    Login = TBLogin.Text,
                    Password = PBPassword.Password,
                    RoleID = (int)CBRole.SelectedValue,
                    AmountOfMistakes = 0,
                    IsBlocked = false
                };

                // Добавление экземпляра в базу данных, сохранение
                ConnectObject.GetConnect().Users.Add(newUser);
                ConnectObject.GetConnect().SaveChanges();

                // Уведомление об успехе
                MessageBox.Show($"Пользователь {TBLastName.Text} {TBFirstName.Text} успешно добавлен!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);

                // Очистка формы и повторная загрузка данных
                ClearAddForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Очистка формы добавления пользователя
        /// </summary>
        private void ClearAddForm()
        {
            TBLastName.Clear();
            TBFirstName.Clear();
            TBPatronymic.Clear();
            TBEmail.Clear();
            TBLogin.Clear();
            PBPassword.Clear();
            CBRole.SelectedIndex = 0;
        }

        /// <summary>
        /// Выбор пользователя для редактирования
        /// </summary>
        private void CBUsersForEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBUsersForEdit.SelectedValue == null)
            {
                return;
            }

            // Получение ID пользователя, поиск пользователя
            int userId = (int)CBUsersForEdit.SelectedValue;
            tempUser = ConnectObject.GetConnect().Users.FirstOrDefault(u => u.UserID == userId);

            // Если пользователь найден, заполняются поля редактирования
            if (tempUser != null)
            {
                TBEditLastName.Text = tempUser.LastName;
                TBEditFirstName.Text = tempUser.FirstName;
                TBEditPatronymic.Text = tempUser.Patronymic;
                TBEditEmail.Text = tempUser.EMail;
                TBEditLogin.Text = tempUser.Login;
                CBEditRole.SelectedValue = tempUser.RoleID;
                CHKEditBlocked.IsChecked = tempUser.IsBlocked;
            }
        }

        /// <summary>
        /// Сохранение изменений пользователя
        /// </summary>
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на то, выбран ли пользователь для редактирования
            if (tempUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Обновление данных выбранного пользователя теми, что введены в поля
                tempUser.LastName = TBEditLastName.Text;
                tempUser.FirstName = TBEditFirstName.Text;
                tempUser.Patronymic = string.IsNullOrEmpty(TBEditPatronymic.Text) ? null : TBEditPatronymic.Text;
                tempUser.EMail = string.IsNullOrEmpty(TBEditEmail.Text) ? null : TBEditEmail.Text;
                tempUser.Login = TBEditLogin.Text;
                tempUser.RoleID = (int)CBEditRole.SelectedValue;
                tempUser.IsBlocked = CHKEditBlocked.IsChecked ?? false;

                // Если введен новый пароль, то обновляем его
                if (!string.IsNullOrEmpty(PBEditPassword.Password))
                {
                    tempUser.Password = PBEditPassword.Password;
                }

                // Сохранение изменений
                ConnectObject.GetConnect().SaveChanges();

                MessageBox.Show($"Данные пользователя {tempUser.LastName} {tempUser.FirstName} успешно обновлены!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновление данных на странице
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Удаление пользователя
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на то, был ли выбран пользователь
            if (tempUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {tempUser.LastName} {tempUser.FirstName}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string userName = $"{tempUser.LastName} {tempUser.FirstName}";

                    // Удаление пользователя, сохранение изменений
                    ConnectObject.GetConnect().Users.Remove(tempUser);
                    ConnectObject.GetConnect().SaveChanges();

                    MessageBox.Show($"Пользователь {userName} успешно удален!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Очистка формы редактирования, обновление данных 
                    ClearEditForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Очистка формы редактирования пользователя
        /// </summary>
        private void ClearEditForm()
        {
            TBEditLastName.Clear();
            TBEditFirstName.Clear();
            TBEditPatronymic.Clear();
            TBEditEmail.Clear();
            TBEditLogin.Clear();
            PBEditPassword.Clear();
            CBEditRole.SelectedIndex = 0;
            CHKEditBlocked.IsChecked = false;
            CBUsersForEdit.SelectedIndex = -1;
            tempUser = null;
        }

        /// <summary>
        /// Экспорт списка пользователей в JSON файл
        /// </summary>
        private void ExportUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получение списка пользователей
                var users = ConnectObject.GetConnect().Users
                    .Select(u => new
                    {
                        u.UserID,
                        u.LastName,
                        u.FirstName,
                        u.Patronymic,
                        u.EMail,
                        u.Login,
                        RoleName = u.Role.RoleName, // Имя роли вместо её номера
                        u.AmountOfMistakes,
                        u.IsBlocked
                    })
                    .ToList();

                // Проверка на количнство пользователей для выгрузки
                if (users.Count == 0)
                {
                    MessageBox.Show("Нет пользователей для выгрузки!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Назначение директории для хранения списков
                string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string exportsFolder = System.IO.Path.Combine(projectDirectory, "Exports");

                // Если папки не существует, создается папка
                if (!Directory.Exists(exportsFolder))
                {
                    Directory.CreateDirectory(exportsFolder);
                }

                // Формирование имени файла с указанием текущей даты и расширения
                string fileName = $"Users_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine(exportsFolder, fileName);

                // Подготовка данных для экспорта
                var data = new
                {
                    ExportDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                    TotalUsers = users.Count,
                    Users = users
                };

                // Форматирование данных в JSON
                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

                // Запись файла JSON формата в файл
                File.WriteAllText(filePath, jsonString);

                // Уведомление об успехе
                MessageBox.Show($"Выгрузка выполнена успешно!\n\n" +
                    $"Папка: {exportsFolder}\n" +
                    $"Файл: {fileName}\n" +
                    $"Пользователей: {users.Count}\n\n" +
                    $"Файл сохранен в папке проекта Exports",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выгрузке: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}