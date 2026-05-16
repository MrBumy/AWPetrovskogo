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
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Page
    {
        public Authorization()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки входа в систему
        /// </summary>
        /// <param name="sender">Источник события</param>
        /// <param name="e">Параметры события</param>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустое поле логина и пароля
            if (string.IsNullOrEmpty(TBLogin.Text) && string.IsNullOrEmpty(PBPassword.Password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на пустое поле логина
            if (string.IsNullOrEmpty(TBLogin.Text))
            {
                MessageBox.Show("Введите логин!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                TBLogin.Focus();
                return;
            }

            // Проверка на пустое поле пароля
            if (string.IsNullOrEmpty(PBPassword.Password))
            {
                MessageBox.Show("Введите пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                PBPassword.Focus();
                return;
            }

            // Поиск пользователя по логину
            var user = ConnectObject.GetConnect().Users.FirstOrDefault(u => u.Login == TBLogin.Text);

            // Проверка на существование пользователя с таким логином
            if (user == null)
            {
                MessageBox.Show("Пользователя с таким логином не существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Проверка на блокировку
            if (user.IsBlocked == true)
            {
                MessageBox.Show("Вы заблокированы! Обратитесь к администратору.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return ;
            }

            // Проверка правильного ввода пароля
            if (user.Login == TBLogin.Text)
            {
                if (user.Password != PBPassword.Password)
                {
                    // Неверный пароль
                    MessageBox.Show("Неправильно введен пароль!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Увеличение счетчика ошибок
                    user.AmountOfMistakes++;
                    ConnectObject.GetConnect().SaveChanges();

                    // Количество оставшихся ошибок
                    MessageBox.Show($"Осталось {3 - user.AmountOfMistakes} попыток входа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Если ошибок >= 3 блокируем пользователя, обнуляем ошибки
                    if (user.AmountOfMistakes >= 3)
                    {
                        user.IsBlocked = true;
                        user.AmountOfMistakes = 0;
                        ConnectObject.GetConnect().SaveChanges();
                    }
                    return;
                }
                else
                {
                    // Если пароль верен, сообщаем об успехе и обнуляем ошибки
                    MessageBox.Show("Вы успешно авторизовались!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    user.AmountOfMistakes = 0;
                    ConnectObject.GetConnect().SaveChanges();

                    // Переключаем страницу в зависимости от роли пользователя
                    switch (user.RoleID)
                    {
                        case 1: // Администратор
                            FrameObject.frameMain.Navigate(new AdministratorPage());
                            break;
                        case 2: // Сотрудник
                            FrameObject.frameMain.Navigate(new EmployeePage());
                            break;
                        case 3: // Руководитель
                            FrameObject.frameMain.Navigate(new ManagerPage());
                            break;
                    }
                }
            }
        }
    }
}
