using System;
using System.Data.SqlClient;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

class Datadownload
{
    static async Task Main()
    {
        using (HttpClient client = new HttpClient())
        {
            string url = "https://www.nbp.pl/kursy/xml/LastA.xml"; // Podanie dokładny URL do pliku XML

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    using (Stream xmlStream = await response.Content.ReadAsStreamAsync())
                    {
                        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                        XDocument xmlDoc = XDocument.Load(xmlStream, LoadOptions.PreserveWhitespace);

                        // Połączenie z bazą danych
                        string connectionString = "Server= DESKTOP-M7U9UVV;Database=CurrencyConverter;Integrated Security=True;";

                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            connection.Open();

                            // Wyodrębnianie danych z pliku XML
                            var currencyElements = xmlDoc.Descendants("pozycja");
                            foreach (var currencyElement in currencyElements)
                            {
                                string currencyCode = currencyElement.Element("kod_waluty").Value;
                                string currencyName = currencyElement.Element("nazwa_waluty").Value;
                                decimal exchangeRate = Convert.ToDecimal(currencyElement.Element("kurs_sredni").Value);

                                if (currencyCode != "PLN" && currencyName != "złoty polski")
                                {
                                    // Sprawdź, czy rekord już istnieje w tabeli
                                    string checkIfExistsQuery = "SELECT COUNT(*) FROM CurrencyRates WHERE CurrencyFrom = @CurrencyFrom AND CurrencyTo = 'PLN'";
                                    using (SqlCommand checkCmd = new SqlCommand(checkIfExistsQuery, connection))
                                    {
                                        checkCmd.Parameters.AddWithValue("@CurrencyFrom", currencyCode);
                                        int existingRecords = (int)checkCmd.ExecuteScalar();

                                        if (existingRecords > 0)
                                        {
                                            // Rekord już istnieje, zaktualizuj wartość Rate
                                            string updateQuery = "UPDATE CurrencyRates SET Rate = @Rate WHERE CurrencyFrom = @CurrencyFrom AND CurrencyTo = 'PLN'";
                                            using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                                            {
                                                updateCmd.Parameters.AddWithValue("@CurrencyFrom", currencyCode);
                                                updateCmd.Parameters.AddWithValue("@Rate", exchangeRate);
                                                int rowsAffected = updateCmd.ExecuteNonQuery();
                                                Console.WriteLine($"Zaktualizowano {rowsAffected} rekordów w tabeli CurrencyRates ({currencyCode} to PLN).");
                                            }
                                        }
                                        else
                                        {
                                            // Rekord nie istnieje, wstaw nowy rekord do tabeli
                                            string insertQuery = "INSERT INTO CurrencyRates (CurrencyFrom, CurrencyTo, Rate) VALUES (@CurrencyFrom, 'PLN', @Rate)";
                                            using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                                            {
                                                insertCmd.Parameters.AddWithValue("@CurrencyFrom", currencyCode);
                                                insertCmd.Parameters.AddWithValue("@Rate", exchangeRate);
                                                int rowsAffected = insertCmd.ExecuteNonQuery();
                                                Console.WriteLine($"Dodano {rowsAffected} rekordów do tabeli CurrencyRates ({currencyCode} to PLN).");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Błąd HTTP: {response.StatusCode}");
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Błąd HTTP: {e.Message}");
            }
        }

        Console.WriteLine("Naciśnij dowolny klawisz, aby zakończyć...");
        Console.ReadKey();
    }
}
