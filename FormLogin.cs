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
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }

        private void lnkRegister_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            {
                // Создаем окно регистрации
                FormRegister registerForm = new FormRegister();

                // Открываем его поверх окна авторизации
                registerForm.ShowDialog();
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // 1. СЕКРЕТНЫЙ ВХОД ДЛЯ АДМИНА (Всегда пустит без баз данных!)
            if (txtUsername.Text.Trim().ToLower() == "admin" && txtPassword.Text == "1234")
            {
                FormRegister.UserRole = "Админ";

                this.Hide();
                Form1 mainForm = new Form1();
                mainForm.ShowDialog();
                this.Close();
                return;
            }

            // 2. ДЛЯ ВСЕХ ОСТАЛЬНЫХ — Проверяем пользователя через встроенный адаптер проекта
            string connectionString = new Form1().пользователиTableAdapter.Connection.ConnectionString;

            using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(connectionString))
            {
                {
                    try
                    {
                        conn.Open();
                        string loginQuery = "SELECT [Роль] FROM [Пользователи] WHERE [Логин] = ? AND [Пароль] = ?";

                        using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(loginQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@p1", txtUsername.Text.Trim());
                            cmd.Parameters.AddWithValue("@p2", txtPassword.Text);

                            object result = cmd.ExecuteScalar(); // Ищем роль в базе данных

                            if (result != null) // Если нашли пользователя
                            {
                                FormRegister.UserRole = result.ToString(); // Запоминаем роль

                                this.Hide();
                                Form1 mainForm = new Form1();
                                mainForm.ShowDialog();
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль! Пожалуйста, проверьте данные.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка подключения к базе данных: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
