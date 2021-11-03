using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace kursBDf
{
    public partial class Form1 : Form
    {
        SqlConnection sqlConnection; // обьявляю соединение, чтобы к нему можно было добраться из любой точки
        public Form1()
        {
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            label1.Text = "ID"; // здесь просто при смене кнопки чтото появляется чтото скрывается
            radioButton3.Visible = true;
            radioButton5.Visible = true;
            label3.Visible = false;
            textBox3.Visible = false;
            button1.Text = radioButton1.Text;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            radioButton4.Checked = true; // здесь просто при смене кнопки чтото появляется чтото скрывается
            label1.Text = "ФИО";
            radioButton3.Visible = false;
            radioButton5.Visible = false;
            label3.Visible = true;
            textBox3.Visible = true;
            button1.Text = radioButton2.Text;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox4.Hide();
            label5.Hide();
            dateTimePicker1.Hide();
            comboBox3.Hide();
            button2.Hide();
            // обьявляю строку в которой путь к базе данных, его я взял из обозревателя серверов -> свойства базы -> строка подключения
            // upd: я немного изменил путь, теперь он относительный благодаря |DataDirectory|, теперь будет работать и на других компах
            string connstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='|DataDirectory|Database1.mdf';Integrated Security=True";
            sqlConnection = new SqlConnection(connstr); //задал путь подключеня

            sqlConnection.Open(); //открываю соединение 
            tabControl1.TabPages.Remove(tabPage5); // при создании формы скрываю управление и личный кабинет
            tabControl1.TabPages.Remove(tabPage6);
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e) // так же эта функция выполняется при нажатии кнопки обновить
        { // при смене вкладки все данные обновляются асинхронно
            listBox1.Items.Clear();
            listBox2.Items.Clear(); // прадвариетльно очищаю списки
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            comboBox1.Items.Clear(); // очищаю комбобокс

            SqlDataReader reader = null; // обьявляю считыватель

            SqlCommand command1 = new SqlCommand("SELECT * FROM [runs]", sqlConnection); // обьявляю комманду

            reader = await command1.ExecuteReaderAsync(); // выполняю комманду и записываю в ридер
            while (reader.Read()) listBox1.Items.Add(reader["id"] + "   " + reader["dist"] + " км      " + reader["date"]); // по циклу записываю по одной записи
            reader.Close(); // закрываю ридер чтоб потом открыть снова, ругается
            // повторяю
            SqlCommand command2 = new SqlCommand("SELECT * FROM [runsinfo]", sqlConnection);

            reader = await command2.ExecuteReaderAsync();
            while (reader.Read()) listBox2.Items.Add(reader["id"] + "     " + reader["name"] + "     " + reader["dist"] + " км      " + reader["date"] + "       " + reader["result"]);
            reader.Close();

            //повторяю
            SqlCommand command3 = new SqlCommand("SELECT * FROM [blag]", sqlConnection);
            
            reader = await command3.ExecuteReaderAsync();
            while (reader.Read()) listBox3.Items.Add(reader ["id"] +"   " +reader["name"]);
            reader.Close();

            SqlCommand command4 = new SqlCommand("SELECT * FROM [runners]", sqlConnection);
            listBox4.Items.Add("Подтв" + "          " + "ID" + "          " + "Имя"); // заголовки 
            reader = await command4.ExecuteReaderAsync();
            while (reader.Read())
            {
                listBox4.Items.Add(reader["verify"] + "          " + reader["id"] + "          " + reader["name"]); // добавляю по записи в листбокс
                comboBox1.Items.Add(reader["id"]); // добавляю в комбобокс айдишники бегунов
            }
            reader.Close();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sqlConnection.Close();// при закрытии программы закрываю соединение
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked) // выполнение входа
            {
                if (radioButton3.Checked) // вход координатора
                {

                }
                else if (radioButton4.Checked) // вход бегуна
                {

                }
                else if (radioButton5.Checked) // вход администратора
                {
                    SqlDataReader reader = null; // обьявляю считыватель
                    SqlCommand command = new SqlCommand("SELECT * FROM Admins WHERE id = @id", sqlConnection); 
                    // комманда выбирает строку где айди равен ввденённому
                    command.Parameters.AddWithValue("id", textBox1.Text); // показываю команде что такое айди

                    reader = await command.ExecuteReaderAsync();
                    reader.Read(); // выполняю чтение
                    try
                    {
                        string pass = Convert.ToString(reader["password"]); // записываю в переменную пароль
                        reader.Close(); // закрываю чтение
                        if (pass == textBox2.Text) // если пароль подходит
                        {
                            tabControl1.TabPages.Remove(tabPage1); // скрываю вход
                            tabControl1.TabPages.Add(tabPage5); // открываю управление
                            MessageBox.Show("Добро пожаловать, администратор!", "Вход", MessageBoxButtons.OK);
                        }
                        else MessageBox.Show("Неверный ID или пароль", "Ошибка", MessageBoxButtons.OK); // иначе ошибка
                    }
                    catch // если айди неправильный то показываю вот это
                    {
                        reader.Close();
                        MessageBox.Show("Неверный ID или пароль", "Ошибка", MessageBoxButtons.OK);
                    }
                    
                    
                }
            }
            else if (radioButton2.Checked) // регистрация бегуна
            {
                if (!string.IsNullOrEmpty(textBox1.Text)&& !string.IsNullOrWhiteSpace(textBox1.Text) && // проверяю пусты ли поля и совпадают ли поля с паролями
                    !string.IsNullOrEmpty(textBox2.Text) && !string.IsNullOrWhiteSpace(textBox2.Text) &&
                    textBox2.Text == textBox3.Text)
                {
                    SqlCommand command = new SqlCommand("INSERT INTO [runners] (name, password) VALUES (@name, @password)", sqlConnection); // назначаю комманду
                    command.Parameters.AddWithValue("name", textBox1.Text); // указываю что есть нейм
                    command.Parameters.AddWithValue("password", textBox2.Text); // указываю что есьт пароль
                    await command.ExecuteNonQueryAsync(); // выполняю комманду
                    MessageBox.Show("Регистрация успешна!", "Регистрация", MessageBoxButtons.OK);
                }
                else MessageBox.Show("Поля должны быть заполнены, подтверждение пароля должно совпадать", "Ошибка", MessageBoxButtons.OK); // если чтото не так то выскакивет это

            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox2.SelectedIndex == 0)
            {
                textBox4.Hide(); // скрываю и показываю нужные вещи для добавления зебега
                label5.Hide();
                dateTimePicker1.Show();
                comboBox3.Show();
                button2.Show();
            }
            else if (comboBox2.SelectedIndex == 1)
            {
                textBox4.Show(); // скрываю и показываю нужные вещи для добавления организации
                label5.Show();
                dateTimePicker1.Hide();
                comboBox3.Hide();
                button2.Show();
            }
        }

        private async void button4_Click(object sender, EventArgs e) // подтверждение бегуна
        {
            if(!string.IsNullOrEmpty(comboBox1.Text) && !string.IsNullOrWhiteSpace(comboBox1.Text)) // проверяю на пустоту
            {
                SqlCommand command = new SqlCommand("UPDATE [runners] SET [verify]='true' WHERE [id] = @id", sqlConnection); // поставить тру в выбранном айди
                command.Parameters.AddWithValue("id", comboBox1.Text); // показываю что есть айди
                await command.ExecuteNonQueryAsync(); // выполняю
            }
            else
            {
                MessageBox.Show("Выберите ID", "Ошибка", MessageBoxButtons.OK); // ошибка
            }
        }

        private async void button3_Click(object sender, EventArgs e) // отказ подтверждения бегуна
        {
            if (!string.IsNullOrEmpty(comboBox1.Text) && !string.IsNullOrWhiteSpace(comboBox1.Text))// проверяю на пустоту
            {
                SqlCommand command = new SqlCommand("UPDATE [runners] SET [verify]='false' WHERE [id] = @id", sqlConnection); // поставить фолз в выбранном айди
                command.Parameters.AddWithValue("id", comboBox1.Text);// показываю что есть айди
                await command.ExecuteNonQueryAsync();// выполняю
            }
            else
            {
                MessageBox.Show("Выберите ID", "Ошибка", MessageBoxButtons.OK); // ошибка
            }
        }

        private async void button2_Click(object sender, EventArgs e)// добавление записи
        {
           switch (comboBox2.SelectedIndex) // оптимизация зазааз
            {
                case 0: // добавление забега
                    if (!string.IsNullOrEmpty(comboBox3.Text) && !string.IsNullOrWhiteSpace(comboBox3.Text)) // проверяю дистанцию на пустоту
                    {
                        SqlCommand command = new SqlCommand("INSERT INTO [runs] (dist, date) VALUES (@dist, @date)", sqlConnection); // добавить в таблицу ранс дистанцию и дату
                        command.Parameters.AddWithValue("dist", comboBox3.Text); //  показываю что такое дистания
                        command.Parameters.AddWithValue("date", dateTimePicker1.Value); // что такое дата
                        await command.ExecuteNonQueryAsync(); // работаем
                    }
                    else
                    {
                        MessageBox.Show("Выберите дистанцию", "Ошибка", MessageBoxButtons.OK); // ошибка
                    }
                    break; // брейк
                case 1: // добвление организации
                    if (!string.IsNullOrEmpty(textBox4.Text) && !string.IsNullOrWhiteSpace(textBox4.Text)) // проверяю на пустоту
                    {
                        SqlCommand command1 = new SqlCommand("INSERT INTO [blag] (name) VALUES (@name)", sqlConnection); // добавить в благ навание
                        command1.Parameters.AddWithValue("name", textBox4.Text); // показываю что такое название
                        await command1.ExecuteNonQueryAsync(); // выполняю
                    }
                    else
                    {
                        MessageBox.Show("Введите название", "Ошибка", MessageBoxButtons.OK); // ошибка
                    }
                    break; // данс
            }
        }

        private async void button5_Click(object sender, EventArgs e) // удаление записей
        {
            if (!string.IsNullOrEmpty(textBox5.Text) && !string.IsNullOrWhiteSpace(textBox5.Text)) // проверяю на пустоту
            {
                switch (comboBox4.SelectedIndex) // بهینهسازی
                {
                    case 0:
                        SqlCommand command1 = new SqlCommand("DELETE FROM [runs] WHERE [id] = @id", sqlConnection); // удалить из таблицы запись с указанными айди
                        command1.Parameters.AddWithValue("id", textBox5.Text);// показываю что такое айди
                        await command1.ExecuteNonQueryAsync(); // выполняю
                        break;
                    case 1:
                        SqlCommand command2 = new SqlCommand("DELETE FROM [runsinfo] WHERE [id] = @id", sqlConnection);// удалить из таблицы запись с указанными айди
                        command2.Parameters.AddWithValue("id", textBox5.Text);// показываю что такое айди
                        await command2.ExecuteNonQueryAsync();// выполняю
                        break;
                    case 2:
                        SqlCommand command3 = new SqlCommand("DELETE FROM [blag] WHERE [id] = @id", sqlConnection);// удалить из таблицы запись с указанными айди
                        command3.Parameters.AddWithValue("id", textBox5.Text);// показываю что такое айди
                        await command3.ExecuteNonQueryAsync();// выполняю
                        break;
                    case 3:
                        SqlCommand command4 = new SqlCommand("DELETE FROM [runners] WHERE [id] = @id", sqlConnection);// удалить из таблицы запись с указанными айди
                        command4.Parameters.AddWithValue("id", textBox5.Text);// показываю что такое айди
                        await command4.ExecuteNonQueryAsync();// выполняю
                        break;
                        // я вообще хотел по другому, чтобы в кейсах было только название таблицы а сама коммада после кейса выполнялась но чет ругается, пример ниже
                        /* string str = null;
                         * case 0:
                         * str = "runs"; break;
                         * и в комманд вводишь не ["runs"] а [@str] но он чета ругается поэтому вот так громозко но работает
                         */
                }

            }
            else
            {
                MessageBox.Show("Введите ID", "Ошибка", MessageBoxButtons.OK); // ошибка
            }
        }
    }
}
