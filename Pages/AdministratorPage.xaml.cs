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
        private User tempUser;

        public AdministratorPage()
        {
            InitializeComponent();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                FrameObject.frameMain.GoBack();
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                LoadData();
            }
        }

        private void LoadData()
        {
            DGridUsers.ItemsSource = ConnectObject.GetConnect().Users.ToList();
            CBRole.ItemsSource = ConnectObject.GetConnect().Roles.ToList();
            CBEditRole.ItemsSource = ConnectObject.GetConnect().Roles.ToList();

            var usersList = ConnectObject.GetConnect().Users.ToList();

            CBUsersForEdit.ItemsSource = usersList.Select(u => new
            {
                u.UserID,
                FullName = string.Format("{0} | {1} {2}", u.UserID, u.LastName, u.FirstName)
            }).ToList();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            var searchText = TBSearch.Text.ToLower();

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

            DGridUsers.ItemsSource = users;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            DGridUsers.ItemsSource = ConnectObject.GetConnect().Users.ToList();

            var usersList = ConnectObject.GetConnect().Users.ToList();
            CBUsersForEdit.ItemsSource = usersList.Select(u => new
            {
                u.UserID,
                FullName = u.UserID.ToString() + " | " + u.LastName + " " + u.FirstName
            }).ToList();

            TBSearch.Clear();
        }

        private void DGridUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void AddNewUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TBLastName.Text))
            {
                MessageBox.Show("Введите фамилию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBFirstName.Text))
            {
                MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(TBLogin.Text))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(PBPassword.Password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingUser = ConnectObject.GetConnect().Users.Count(u => u.Login == TBLogin.Text);
            if (existingUser >= 1)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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
                var newUser = new User
                {
                    LastName = TBLastName.Text,
                    FirstName = TBFirstName.Text,
                    Patronymic = string.IsNullOrEmpty(TBPatronymic.Text) ? null : TBPatronymic.Text,
                    EMail = string.IsNullOrEmpty(TBEmail.Text) ? null : TBEmail.Text,
                    Login = TBLogin.Text,
                    Password = PBPassword.Password,
                    RoleID = CBRole.SelectedValue != null ? (int)CBRole.SelectedValue : 2,
                    AmountOfMistakes = 0,
                    IsBlocked = false
                };

                ConnectObject.GetConnect().Users.Add(newUser);
                ConnectObject.GetConnect().SaveChanges();

                MessageBox.Show($"Пользователь {TBLastName.Text} {TBFirstName.Text} успешно добавлен!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);

                ClearAddForm();
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

        private void CBUsersForEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CBUsersForEdit.SelectedValue == null) return;

            int userId = (int)CBUsersForEdit.SelectedValue;
            tempUser = ConnectObject.GetConnect().Users.FirstOrDefault(u => u.UserID == userId);

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

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                tempUser.LastName = TBEditLastName.Text;
                tempUser.FirstName = TBEditFirstName.Text;
                tempUser.Patronymic = string.IsNullOrEmpty(TBEditPatronymic.Text) ? null : TBEditPatronymic.Text;
                tempUser.EMail = string.IsNullOrEmpty(TBEditEmail.Text) ? null : TBEditEmail.Text;
                tempUser.Login = TBEditLogin.Text;
                tempUser.RoleID = (int)CBEditRole.SelectedValue;
                tempUser.IsBlocked = CHKEditBlocked.IsChecked ?? false;

                if (!string.IsNullOrEmpty(PBEditPassword.Password))
                {
                    tempUser.Password = PBEditPassword.Password;
                }

                ConnectObject.GetConnect().SaveChanges();

                MessageBox.Show($"Данные пользователя {tempUser.LastName} {tempUser.FirstName} успешно обновлены!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя {tempUser.LastName} {tempUser.FirstName}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string userName = $"{tempUser.LastName} {tempUser.FirstName}";

                    ConnectObject.GetConnect().Users.Remove(tempUser);
                    ConnectObject.GetConnect().SaveChanges();

                    MessageBox.Show($"Пользователь {userName} успешно удален!", "Успех!", MessageBoxButton.OK, MessageBoxImage.Information);

                    ClearEditForm();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

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

        private void ExportUsersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var users = ConnectObject.GetConnect().Users
                    .Select(u => new
                    {
                        u.UserID,
                        u.LastName,
                        u.FirstName,
                        u.Patronymic,
                        u.EMail,
                        u.Login,
                        RoleName = u.Role.RoleName,
                        u.AmountOfMistakes,
                        u.IsBlocked
                    })
                    .ToList();

                if (users.Count == 0)
                {
                    MessageBox.Show("Нет пользователей для выгрузки!", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string exportsFolder = System.IO.Path.Combine(projectDirectory, "Exports");

                if (!Directory.Exists(exportsFolder))
                {
                    Directory.CreateDirectory(exportsFolder);
                }

                string fileName = $"Users_Export_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                string filePath = System.IO.Path.Combine(exportsFolder, fileName);

                var data = new
                {
                    ExportDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
                    TotalUsers = users.Count,
                    Users = users
                };

                string jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);

                File.WriteAllText(filePath, jsonString);

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