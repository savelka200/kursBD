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

        private async void Form1_Load(object sender, EventArgs e)
        {
            textBox4.Hide();
            label5.Hide();
            dateTimePicker1.Hide();
            comboBox3.Hide();
            button2.Hide();
            // обьявляю строку в которой путь к базе данных, его я взял из обозревателя серверов -> свойства базы -> строка подключения
            string connstr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\savel\source\repos\kursBDf\kursBDf\Database1.mdf;Integrated Security=True";
            sqlConnection = new SqlConnection(connstr); //задал путь подключеня

            await sqlConnection.OpenAsync(); //открываю соединение асинхронно, чтоб ничего не тормозило (наверное)
            tabControl1.TabPages.Remove(tabPage5); // при создании формы скрываю управление и личный кабинет
            tabControl1.TabPages.Remove(tabPage6);
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        { // при смене вкладки все данные обновляются асинхронно
            listBox1.Items.Clear();
            listBox2.Items.Clear(); // прадвариетльно очищаю списки
            listBox3.Items.Clear();
            listBox4.Items.Clear();

            SqlDataReader reader = null; // обьявляю считыватель

            SqlCommand command1 = new SqlCommand("SELECT * FROM [runs]", sqlConnection); // обьявляю комманду

            reader = await command1.ExecuteReaderAsync(); // выполняю комманду и записываю в ридер
            while (reader.Read()) listBox1.Items.Add(reader["id"] + "   " + reader["dist"] + " км      " + reader["date"]); // по циклу записываю по одной записи
            reader.Close(); // закрываю ридер чтоб потом открыть снова, ругается
            // повторяю
            SqlCommand command2 = new SqlCommand("SELECT * FROM [runsinfo]", sqlConnection);

            reader = await command2.ExecuteReaderAsync();
            while (reader.Read()) listBox2.Items.Add(reader["name"] + "     " + reader["dist"] + " км      " + reader["date"] + "       " + reader["result"]);
            reader.Close();

            //повторяю
            SqlCommand command3 = new SqlCommand("SELECT * FROM [blag]", sqlConnection);
            
            reader = await command3.ExecuteReaderAsync();
            while (reader.Read()) listBox3.Items.Add(reader ["id"] +"   " +reader["name"]);
            reader.Close();

            SqlCommand command4 = new SqlCommand("SELECT * FROM [runners]", sqlConnection);
            listBox4.Items.Add("Подтв" + "          " + "ID" + "          " + "Имя"); // заголовки 
            reader = await command4.ExecuteReaderAsync();
            while (reader.Read()) listBox4.Items.Add(reader["verify"] + "          " + reader["id"] + "          " + reader["name"]);
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
    }
}
