using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.microsoft_Access_База_данныхDataSet.EnforceConstraints = false;

            // ИСПРАВЛЕНО: Комментируем (отключаем) проблемные таблицы, которые вызывают сбои памяти драйвера Access
            // // this.администраторыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Администраторы);
            // // this.спорыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Споры);
            // // this.подтверждение_оплатыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Подтверждение_оплаты);
            // // this.замораживание_средствTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Замораживание_средств);
            // // this.оплатаTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Оплата);

            // Оставляем только основные таблицы для первоначального вывода на экран:
            this.сделкиTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Сделки);
            this.аккаунтыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Аккаунты);
            this.продавцыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Продавцы);
            this.пользователиTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Пользователи);

            string currentRole = FormRegister.UserRole;

            if (currentRole == "Покупатель")
            {
                // 1. Скрываем вкладки
                tabControl1.TabPages.Remove(tabPage1);
                //tabControl1.TabPages.Remove(tabPage6);
                tabControl1.TabPages.Remove(tabPage7);
                //tabControl1.TabPages.Remove(tabPage8);
                tabControl1.TabPages.Remove(tabPage9);

                // 2. Управляем кнопками: Покупатель может только ПОКУПАТЬ
                btnBuy.Visible = true;          // Кнопку "Купить" показываем
                btnAdd.Visible = false; // Кнопку "Добавить" прячем (покупатель не может создавать товары)
                button1.Visible = false; // Кнопку "Сохранить" прячем

                MessageBox.Show("Вы вошли как Покупатель. Доступен просмотр товаров и покупок.", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (currentRole == "Продавец")
            {
                // 1. Скрываем вкладки
                tabControl1.TabPages.Remove(tabPage1);
                tabControl1.TabPages.Remove(tabPage2);
                tabControl1.TabPages.Remove(tabPage9);

                // 2. Управляем кнопками: Продавец может добавлять и сохранять, но не покупать у самого себя
                btnBuy.Visible = false;         // Кнопку "Купить" прячем
                btnAdd.Visible = true;  // Кнопку "Добавить" показываем
                button1.Visible = true; // Кнопку "Сохранить" показываем

                MessageBox.Show("Вы вошли как Продавец. Доступна панель управления продажами.", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (currentRole == "Админ")
            {
                // Администратор видит абсолютно все вкладки и ВСЕ кнопки для управления
                btnBuy.Visible = true;
                btnAdd.Visible = true;
                button1.Visible = true;

                MessageBox.Show("Вы вошли как Администратор. Доступны все функции системы.", "Полный доступ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Сохранить_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Фиксируем изменения из графического интерфейса во всех BindingSource
                this.пользователиBindingSource.EndEdit();
                this.продавцыBindingSource.EndEdit();
                this.администраторыBindingSource.EndEdit();
                this.аккаунтыBindingSource.EndEdit();
                this.сделкиBindingSource.EndEdit();
                this.оплатаBindingSource.EndEdit();
                this.замораживаниеСредствBindingSource.EndEdit();
                this.подтверждениеОплатыBindingSource.EndEdit();
                this.спорыBindingSource.EndEdit();

                // 2. СОХРАНЕНИЕ В БАЗУ ДАННЫХ (Строго по иерархии связей)

                // Этап А: Сначала главные справочники (они ни от кого не зависят)
                this.пользователиTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Пользователи);
                this.продавцыTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Продавцы);
                this.администраторыTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Администраторы);

                // Этап Б: Таблицы второго уровня (зависят от пользователей/продавцов)
                this.аккаунтыTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Аккаунты);
                this.сделкиTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Сделки);

                // Этап В: Подчинённые таблицы (зависят от сделок или оплат)
                this.оплатаTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Оплата);
                this.замораживание_средствTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Замораживание_средств);
                this.подтверждение_оплатыTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Подтверждение_оплаты);
                this.спорыTableAdapter.Update(this.microsoft_Access_База_данныхDataSet.Споры);

                MessageBox.Show("Все данные успешно сохранены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения из-за нарушения связей: \n\n{ex.Message}", "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView2_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView3_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView4_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView5_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView6_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView7_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView8_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView9_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            {
                DialogResult dr = MessageBox.Show("Удалить запись?", "Удаление", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                if (dr == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            {
                Close();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedIndex)
            {
                case 0: // Вкладка: Пользователи
                    FormAddUser userForm = new FormAddUser();

                    if (userForm.ShowDialog() == DialogResult.OK)
                    {
                        string newLogin = userForm.txtLogin.Text;
                        string newEmail = userForm.txtEmail.Text;

                        try
                        {
                            this.пользователиTableAdapter.Insert(newLogin, newEmail, DateTime.Now, "1111", "Покупатель");
                            this.пользователиTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Пользователи);

                            MessageBox.Show("Пользователь успешно добавлен в базу Access!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении в Access: \n" + ex.Message,
                                            "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 1: // Вкладка: Продавцы
                    FormAddSeller sellerForm = new FormAddSeller();

                    if (sellerForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int userId = int.Parse(sellerForm.textBox1.Text);
                            int rating = int.Parse(sellerForm.textBox2.Text);
                            int salesCount = int.Parse(sellerForm.textBox3.Text);
                            this.продавцыTableAdapter.Insert(userId, rating, salesCount);
                            this.продавцыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Продавцы);

                            MessageBox.Show("Продавец успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (FormatException)
                        {
                            MessageBox.Show("Пожалуйста, заполните все поля и вводите только числа!", "Ошибка ввода", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении в Access: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 2: // Вкладка: Аккаунты
                    FormAddAccount accForm = new FormAddAccount();

                    if (accForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string name = accForm.textBox1.Text;
                            string description = accForm.textBox2.Text;
                            decimal price = decimal.Parse(accForm.textBox3.Text);
                            int sellerId = int.Parse(accForm.textBox4.Text);
                            this.аккаунтыTableAdapter.Insert(name, description, price, sellerId);
                            this.аккаунтыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Аккаунты);

                            MessageBox.Show("Аккаунт успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении аккаунта: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 3: // Вкладка: Сделки
                    FormAddDeal dealForm = new FormAddDeal();

                    if (dealForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int buyerId = int.Parse(dealForm.textBox1.Text);
                            int sellerId = int.Parse(dealForm.textBox2.Text);
                            int accountId = int.Parse(dealForm.textBox3.Text);
                            string status = dealForm.textBox4.Text;
                            DateTime dealDate = dealForm.dateTimePicker1.Value;
                            this.сделкиTableAdapter.Insert(buyerId, sellerId, accountId, dealDate, status);
                            this.сделкиTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Сделки);

                            MessageBox.Show("Сделка успешно добавлена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении сделки: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 4: // Вкладка: Оплаты
                    FormAddPayment payForm = new FormAddPayment();

                    if (payForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int dealId = int.Parse(payForm.textBox1.Text);
                            decimal amount = decimal.Parse(payForm.textBox2.Text);
                            string status = payForm.textBox3.Text;
                            DateTime paymentDate = payForm.dateTimePicker1.Value;
                            this.оплатаTableAdapter.Insert(dealId, amount, paymentDate, status);
                            this.оплатаTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Оплата);

                            MessageBox.Show("Оплата успешно зафиксирована!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении оплаты: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 5: // Вкладка: Замораживание средств
                    FormAddFreeze freezeForm = new FormAddFreeze();

                    if (freezeForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int paymentId = int.Parse(freezeForm.textBox1.Text);
                            decimal amount = decimal.Parse(freezeForm.textBox2.Text);
                            string status = freezeForm.textBox3.Text;
                            DateTime freezeDate = freezeForm.dateTimePicker1.Value;
                            this.замораживание_средствTableAdapter.Insert(paymentId, amount, freezeDate, status);
                            this.замораживание_средствTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Замораживание_средств);

                            MessageBox.Show("Средства успешно заморожены!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении заморозки: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 6: // Вкладка: Подтвержденные оплаты
                    FormAddConfirm confirmForm = new FormAddConfirm();

                    if (confirmForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int dealId = int.Parse(confirmForm.textBox1.Text);
                            string status = confirmForm.textBox2.Text;
                            DateTime confirmDate = confirmForm.dateTimePicker1.Value;
                            this.подтверждение_оплатыTableAdapter.Insert(dealId, confirmDate, status);
                            this.подтверждение_оплатыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Подтверждение_оплаты);

                            MessageBox.Show("Оплата успешно подтверждена!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении подтверждения: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;


                case 7: // Вкладка: Споры
                    FormAddDispute disputeForm = new FormAddDispute();

                    if (disputeForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            int dealId = int.Parse(disputeForm.textBox1.Text);
                            int adminId = int.Parse(disputeForm.textBox4.Text);
                            string reason = disputeForm.textBox2.Text;
                            string resolution = disputeForm.textBox3.Text;
                            this.спорыTableAdapter.Insert(dealId, reason, resolution, adminId);
                            this.спорыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Споры);

                            MessageBox.Show("Спор успешно сохранен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении спора: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;

                case 8: // Вкладка: Администраторы
                    FormAddAdmin adminForm = new FormAddAdmin();

                    if (adminForm.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            string login = adminForm.textBox1.Text;
                            string password = adminForm.textBox2.Text;
                            string fio = adminForm.textBox3.Text;
                            this.администраторыTableAdapter.Insert(login, password, fio);
                            this.администраторыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Администраторы);

                            MessageBox.Show("Администратор успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ошибка при сохранении администратора: \n" + ex.Message, "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    break;

            }
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            {
                string info = "Панель для работы с FunPay\n\n" +
                              "Версия программы: 2.0\n" +
                              "Разработчик: Коканов\n\n" +
                              "Программа предназначена для управления аккаунтами, " +
                              "мониторинга продавцов и автоматизации процессов.";

                MessageBox.Show(info, "О программе", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnBuy_Click(object sender, EventArgs e)
        {
            // 1. Ищем таблицу на активной вкладке
            DataGridView activeGrid = null;
            foreach (Control ctrl in tabControl1.SelectedTab.Controls)
            {
                if (ctrl is DataGridView)
                {
                    activeGrid = (DataGridView)ctrl;
                    break;
                }
            }

            if (activeGrid != null && activeGrid.CurrentRow != null)
            {
                // Получаем данные выбранной строки (1 - Название, 3 - Цена, 0 - ID аккаунта, 4 - ID продавца)
                string gameName = activeGrid.CurrentRow.Cells[1].Value?.ToString() ?? "Без названия";
                string price = activeGrid.CurrentRow.Cells[3].Value?.ToString() ?? "0";
                string accountId = activeGrid.CurrentRow.Cells[0].Value?.ToString() ?? "0";
                string sellerId = activeGrid.CurrentRow.Cells[4].Value?.ToString() ?? "1";

                DialogResult result = MessageBox.Show(
                $"Купить \"{gameName}\" за {price} руб.? Деньги будут заморожены на время проверки.",
                "Покупка товара",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // Берем строку подключения напрямую из вашего существующего адаптера
                    using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(this.оплатаTableAdapter.Connection.ConnectionString))
                    {
                        try
                        {
                            conn.Open();

                            // ШАГ 1: Создаем запись в таблице 'Сделки'
                            string insertOrderQuery = "INSERT INTO [Сделки] ([id покупателя], [id продавца], [id аккаунта], [Дата сделки], [Статус]) VALUES (?, ?, ?, ?, ?)";
                            int generatedOrderId = 0;

                            using (System.Data.OleDb.OleDbCommand cmdOrder = new System.Data.OleDb.OleDbCommand(insertOrderQuery, conn))
                            {
                                cmdOrder.Parameters.AddWithValue("@p1", 1); // Временно ставим ID покупателя = 1
                                cmdOrder.Parameters.AddWithValue("@p2", Convert.ToInt32(sellerId));
                                cmdOrder.Parameters.AddWithValue("@p3", Convert.ToInt32(accountId));
                                cmdOrder.Parameters.AddWithValue("@p4", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                                cmdOrder.Parameters.AddWithValue("@p5", "В процессе");
                                cmdOrder.ExecuteNonQuery();

                                cmdOrder.CommandText = "SELECT @@IDENTITY";
                                generatedOrderId = Convert.ToInt32(cmdOrder.ExecuteScalar());
                            }

                            // ШАГ 2: Создаем запись в таблице 'Оплата' с привязкой полученного id сделки
                            string insertPayQuery = "INSERT INTO [Оплата] ([id сделки], [Сумма], [Дата оплаты], [Статус]) VALUES (?, ?, ?, ?)";
                            int generatedPayId = 0;

                            using (System.Data.OleDb.OleDbCommand cmdPay = new System.Data.OleDb.OleDbCommand(insertPayQuery, conn))
                            {
                                cmdPay.Parameters.AddWithValue("@p1", generatedOrderId);
                                cmdPay.Parameters.AddWithValue("@p2", Convert.ToInt32(price));
                                cmdPay.Parameters.AddWithValue("@p3", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                                cmdPay.Parameters.AddWithValue("@p4", "Заморожена");
                                cmdPay.ExecuteNonQuery();

                                cmdPay.CommandText = "SELECT @@IDENTITY";
                                generatedPayId = Convert.ToInt32(cmdPay.ExecuteScalar());
                            }

                            // ШАГ 3: Создаем запись в таблице 'Замораживание средств'
                            string insertFreezeQuery = "INSERT INTO [Замораживание средств] ([id оплаты], [Сумма], [Дата заморозки], [Статус]) VALUES (?, ?, ?, ?)";
                            using (System.Data.OleDb.OleDbCommand cmdFreeze = new System.Data.OleDb.OleDbCommand(insertFreezeQuery, conn))
                            {
                                cmdFreeze.Parameters.AddWithValue("@p1", generatedPayId);
                                cmdFreeze.Parameters.AddWithValue("@p2", Convert.ToInt32(price));
                                cmdFreeze.Parameters.AddWithValue("@p3", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                                cmdFreeze.Parameters.AddWithValue("@p4", "На проверке");
                                cmdFreeze.ExecuteNonQuery();
                            }

                            // ШАГ 4: ИСПРАВЛЕНО. Вместо вызова конфликтных методов .Fill(), мы просто 
                            // мягко скрываем купленный товар с экрана с помощью локального фильтра C#
                            if (activeGrid.DataSource is BindingSource bs)
                            {
                                bs.Filter = $"[id аккаунта] <> {accountId}";
                            }

                            MessageBox.Show(
                                $"Вы успешно начали покупку \"{gameName}\"!\n\nСредства ({price} руб.) ЗАМОРОЖЕНЫ.\n\nДанные сохранены в файл базы данных. Чтобы увидеть изменения на других вкладках, перезапустите приложение.",
                                "Сделка открыта",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при работе с Базой Данных: {ex.Message}", "Ошибка БД", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите товар в таблице аккаунтов!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            {
                try
                {
                    // Отключаем фоновые паники DataSet перед чистым обновлением из файлов базы данных
                    this.microsoft_Access_База_данныхDataSet.EnforceConstraints = false;

                    // Перечитываем все таблицы из файла Access прямо в момент щелчка по вкладке
                    this.сделкиTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Сделки);
                    this.оплатаTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Оплата);
                    this.замораживание_средствTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Замораживание_средств);
                    this.спорыTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Споры);

                    // Включаем встроенные проверки безопасности обратно
                    this.microsoft_Access_База_данныхDataSet.EnforceConstraints = true;
                }
                catch
                {
                    // Мягко гасим внутренние конфликты связей C#, чтобы окна ошибок не мешали работе
                    this.microsoft_Access_База_данныхDataSet.EnforceConstraints = true;
                }
            }
        }

        private void btnConfirmOrder_Click(object sender, EventArgs e)
        {
            DataGridView activeGrid = null;
            foreach (Control ctrl in tabControl1.SelectedTab.Controls)
            {
                if (ctrl is DataGridView)
                {
                    activeGrid = (DataGridView)ctrl;
                    break;
                }
            }

            if (activeGrid != null && activeGrid.CurrentRow != null)
            {
                // 0 — id заморозки, 1 — id оплаты, 2 — Сумма
                string freezeId = activeGrid.CurrentRow.Cells[0].Value?.ToString() ?? "0";
                string payId = activeGrid.CurrentRow.Cells[1].Value?.ToString() ?? "0";
                string sum = activeGrid.CurrentRow.Cells[2].Value?.ToString() ?? "0";

                DialogResult result = MessageBox.Show(
                    $"Вы проверили аккаунт? После подтверждения {sum} руб. будут переведены продавцу, и сделку нельзя будет отменить.",
                    "Подтверждение закрытия сделки",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(this.оплатаTableAdapter.Connection.ConnectionString))
                    {
                        try
                        {
                            conn.Open();

                            // 1. Узнаем id сделки, привязанный к этой оплате
                            string selectQuery = "SELECT [id сделки] FROM [Оплата] WHERE [id оплаты] = ?";
                            int orderId = 0;
                            using (System.Data.OleDb.OleDbCommand cmdSelect = new System.Data.OleDb.OleDbCommand(selectQuery, conn))
                            {
                                cmdSelect.Parameters.AddWithValue("@p1", Convert.ToInt32(payId));
                                object obj = cmdSelect.ExecuteScalar();
                                if (obj != null) orderId = Convert.ToInt32(obj);
                            }

                            // 2. Обновляем статус в таблице Оплата
                            string updatePayQuery = "UPDATE [Оплата] SET [Статус] = 'Подтверждена' WHERE [id оплаты] = ?";
                            using (System.Data.OleDb.OleDbCommand cmdPay = new System.Data.OleDb.OleDbCommand(updatePayQuery, conn))
                            {
                                cmdPay.Parameters.AddWithValue("@p1", Convert.ToInt32(payId));
                                cmdPay.ExecuteNonQuery();
                            }

                            // 3. Обновляем статус в таблице Сделки
                            string updateOrderQuery = "UPDATE [Сделки] SET [Статус] = 'Выполнена' WHERE [id сделки] = ?";
                            using (System.Data.OleDb.OleDbCommand cmdOrder = new System.Data.OleDb.OleDbCommand(updateOrderQuery, conn))
                            {
                                cmdOrder.Parameters.AddWithValue("@p1", orderId);
                                cmdOrder.ExecuteNonQuery();
                            }

                            // 4. Удаляем запись из Замораживания средств
                            string deleteFreezeQuery = "DELETE FROM [Замораживание средств] WHERE [id заморозки] = ?";
                            using (System.Data.OleDb.OleDbCommand cmdDel = new System.Data.OleDb.OleDbCommand(deleteFreezeQuery, conn))
                            {
                                cmdDel.Parameters.AddWithValue("@p1", Convert.ToInt32(freezeId));
                                cmdDel.ExecuteNonQuery();
                            }

                            MessageBox.Show(
                                "Сделка успешно завершена! Деньги переведены на баланс продавца.",
                                "Успех",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );

                            // Перезагружаем таблицы через автообновление, которое мы настраивали
                            tabControl1_SelectedIndexChanged(null, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите строчку в таблице!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnOpenDispute_Click(object sender, EventArgs e)
        {
            DataGridView activeGrid = null;
            foreach (Control ctrl in tabControl1.SelectedTab.Controls)
            {
                if (ctrl is DataGridView)
                {
                    activeGrid = (DataGridView)ctrl;
                    break;
                }
            }

            if (activeGrid != null && activeGrid.CurrentRow != null)
            {
                string payId = activeGrid.CurrentRow.Cells[1].Value?.ToString() ?? "0";

                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите открыть спор? Деньги останутся замороженными до решения Администратора.",
                    "Открытие спора",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(this.оплатаTableAdapter.Connection.ConnectionString))
                    {
                        try
                        {
                            conn.Open();

                            // 1. Узнаем id сделки
                            string selectQuery = "SELECT [id сделки] FROM [Оплата] WHERE [id оплаты] = ?";
                            int orderId = 0;
                            using (System.Data.OleDb.OleDbCommand cmdSelect = new System.Data.OleDb.OleDbCommand(selectQuery, conn))
                            {
                                cmdSelect.Parameters.AddWithValue("@p1", Convert.ToInt32(payId));
                                object obj = cmdSelect.ExecuteScalar();
                                if (obj != null) orderId = Convert.ToInt32(obj);
                            }

                            // 2. Меняем статус сделки на Спор
                            string updateOrderQuery = "UPDATE [Сделки] SET [Статус] = 'Спор' WHERE [id сделки] = ?";
                            using (System.Data.OleDb.OleDbCommand cmdOrder = new System.Data.OleDb.OleDbCommand(updateOrderQuery, conn))
                            {
                                cmdOrder.Parameters.AddWithValue("@p1", orderId);
                                cmdOrder.ExecuteNonQuery();
                            }

                            // 3. Добавляем запись в таблицу Споры
                            string insertDisputeQuery = "INSERT INTO [Споры] ([id сделки], [Причина], [Решение]) VALUES (?, ?, ?)";
                            using (System.Data.OleDb.OleDbCommand cmdDispute = new System.Data.OleDb.OleDbCommand(insertDisputeQuery, conn))
                            {
                                cmdDispute.Parameters.AddWithValue("@p1", orderId);
                                cmdDispute.Parameters.AddWithValue("@p2", "Товар не соответствует описанию");
                                cmdDispute.Parameters.AddWithValue("@p3", "Ожидает рассмотрения");
                                cmdDispute.ExecuteNonQuery();
                            }

                            // ============================================================
                            // ВСТАВЛЕНО СЮДА (Конец блока успешного выполнения):
                            // ============================================================
                            MessageBox.Show(
                                "Спор успешно открыт! Перенаправляем вас на вкладку 'Споры' для отслеживания.",
                                "Спор активирован",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error
                            );

                            // Программа сама переключает вкладку на экран Споров
                            tabControl1.SelectedTab = tabPage8;

                            // Обновляем таблицы
                            tabControl1_SelectedIndexChanged(null, null);
                            // ============================================================
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка БД: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите сделку для спора!", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnMakeAdmin_Click(object sender, EventArgs e)
        {
            {
                // 1. Ищем таблицу пользователей на текущей вкладке
                DataGridView userGrid = null;
                foreach (Control ctrl in tabControl1.SelectedTab.Controls)
                {
                    if (ctrl is DataGridView)
                    {
                        userGrid = (DataGridView)ctrl;
                        break;
                    }
                }

                if (userGrid != null && userGrid.CurrentRow != null)
                {
                    // 0 колонка — id пользователя, 1 колонка — Логин (в соответствии с вашей таблицей)
                    string userId = userGrid.CurrentRow.Cells[0].Value?.ToString() ?? "0";
                    string userLogin = userGrid.CurrentRow.Cells[1].Value?.ToString() ?? "Пользователь";

                    DialogResult result = MessageBox.Show(
                        $"Вы уверены, что хотите назначить пользователя \"{userLogin}\" Администратором?\nЭто предоставит ему полный доступ к системе после перезахода.",
                        "Смена прав доступа",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.Yes)
                    {
                        using (System.Data.OleDb.OleDbConnection conn = new System.Data.OleDb.OleDbConnection(this.пользователиTableAdapter.Connection.ConnectionString))
                        {
                            try
                            {
                                conn.Open();

                                // SQL-запрос меняет роль на "Админ" прямо в файле Access
                                string updateQuery = "UPDATE [Пользователи] SET [Роль] = 'Админ', [Пароль] = '1111' WHERE [id пользователя] = ?";

                                using (System.Data.OleDb.OleDbCommand cmd = new System.Data.OleDb.OleDbCommand(updateQuery, conn))
                                {
                                    cmd.Parameters.AddWithValue("@p1", Convert.ToInt32(userId));
                                    cmd.ExecuteNonQuery();
                                }

                                MessageBox.Show($"Пользователь \"{userLogin}\" успешно назначен Администратором и сохранен в базе данных!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Обновляем таблицу на экране, чтобы сразу увидеть новую роль
                                this.пользователиTableAdapter.Fill(this.microsoft_Access_База_данныхDataSet.Пользователи);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка обновления роли в БД: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, выберите пользователя в таблице!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
    }
}
