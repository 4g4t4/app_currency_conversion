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

namespace Interface
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out int value) || value <= 0)
            {
                // Jeśli wprowadzona wartość nie jest liczbą dodatnią większą od zera, zresetuj TextBox1
                textBox1.Text = "";
            }
        }

        private void textBox2_TextChanged_1(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox2.Text, out int value) || value <= 0)
            {
                // Jeśli wprowadzona wartość nie jest liczbą dodatnią większą od zera, zresetuj TextBox1
                textBox2.Text = "";
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedCurrency = comboBox1.SelectedItem.ToString();

            // Wyodrębnij kod waluty (pierwsza część) z pełnej nazwy wybranej waluty
            string[] currencyParts = selectedCurrency.Split(' ');
            string currencyCode = currencyParts[0];

            // Połączenie z bazą danych
            string connectionString = "Server=DESKTOP-M7U9UVV;Database=CurrencyConverter;Integrated Security=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string checkCurrencyQuery = "SELECT Rate FROM CurrencyRates WHERE CurrencyFrom = @CurrencyFrom AND CurrencyTo = 'PLN'";
                    using (SqlCommand checkCmd = new SqlCommand(checkCurrencyQuery, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@CurrencyFrom", currencyCode);

                        SqlDataReader reader = checkCmd.ExecuteReader();
                        if (reader.Read())
                        {
                            decimal exchangeRate = Convert.ToDecimal(reader["Rate"]);

                            if (decimal.TryParse(textBox1.Text, out decimal amount))
                            {
                                decimal convertedAmount = amount * exchangeRate;
                                textBox2.Text = convertedAmount.ToString("N2");
                            }
                            else
                            {
                                MessageBox.Show("Wprowadź prawidłową liczbę w TextBox1.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Nie znaleziono kursu dla wybranej waluty.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Wystąpił błąd: {ex.Message}");
                }
            }
        }
    }
} 