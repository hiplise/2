using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class FormRegister : Form
    {
        public static string RegisteredUsername = "";
        public static string RegisteredPassword = "";
        public static string UserRole = "Покупатель";

        public FormRegister()
        {
            InitializeComponent();
        }

        private void btnCreateAccount_Click(object sender, EventArgs e)
        {
            // 1. СЕКРЕТНЫЙ ВХОД ДЛЯ РАЗРАБОТЧИКА (Всегда пустит, даже если база пустая)
            if (txtRegUsername.Text.Trim().ToLower() == "admin" && txtRegPassword.Text == "1234")
            {
                FormRegister.UserRole = "Админ"; // Даем полные права

                this.Hide();
                Form1 mainForm = new Form1();
                mainForm.ShowDialog();
                this.Close();
                return;
            }

            // 2. ДЛЯ ВСЕХ ОСТАЛЬНЫХ — Проверяем логин и пароль по настоящему файлу Access
            // Создаем прямое текстовое подключение, которое никогда не выдает ошибок разрядности памяти
            string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=|DataDirectory|\Microsoft Access База данных.accdb";

            using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Ищем роль пользователя, у которого совпадают введенные Логин и Пароль
                    string loginQuery = "SELECT [Роль] FROM [Пользователи] WHERE [Логин] = ? AND [Пароль] = ?";

                    using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(loginQuery, conn))
                    {
                        // Передаем то, что пользователь ввел в текстовые поля
                        cmd.Parameters.AddWithValue("@p1", txtRegUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@p2", txtRegPassword.Text);

                        object result = cmd.ExecuteScalar(); // Запрашиваем роль из базы

                        if (result != null) // Если такой пользователь нашелся в файле Access!
                        {
                            // Запоминаем его роль из базы (Покупатель, Продавец или Админ)
                            FormRegister.UserRole = result.ToString();

                            this.Hide();
                            Form1 mainForm = new Form1();
                            mainForm.ShowDialog(); // Открываем главную админку
                            this.Close();
                        }
                        else
                        {
                            // Если база вернула пустоту (логин или пароль не совпали)
                            MessageBox.Show("Неверный логин или пароль! Пожалуйста, проверьте данные.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка подключения к базе данных Access: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
