using BankaPIS.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Emit;
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

namespace BankaPIS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static int id;
        Konekcija conn = new Konekcija();
        SqlConnection konekcija = new SqlConnection();

        public MainWindow()
        {
            InitializeComponent();
            konekcija = conn.KreirajKonekciju();
        }

        //----------Prijavljivanje Zaposlenog/Korisnika-------------
        private void Login(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string result;
                string password_txt = password.Password.ToString();
                string username_txt = username.Text;
                if (Zaposleni.IsChecked == true)
                {
                    //SQL za dobijanje sifre korisnika
                    string query_korisnik_password = "select pssword from korisnik where username = @username";
                    SqlCommand cmd_korisnik_password = new SqlCommand(query_korisnik_password, konekcija);
                    cmd_korisnik_password.Parameters.AddWithValue("@username", username_txt);
                    result = Convert.ToString(cmd_korisnik_password.ExecuteScalar());
                    if (result != "")
                    {
                        if (password_txt == result)
                        {
                            //SQL za dobijanje id koriznika(zaposlenog)
                            string query_korisnik_id = "select korisnik_id from korisnik where username = @username";
                            SqlCommand cmd_korisnik_id = new SqlCommand(query_korisnik_id, konekcija);
                            cmd_korisnik_id.Parameters.AddWithValue("@username", username_txt);
                            id = Convert.ToInt32(cmd_korisnik_id.ExecuteScalar());


                            FrmZaposleni window = new FrmZaposleni();
                            window.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Pogresna sifra", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Pogresan username", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                //Prijava klijenta
                else if (Klijent.IsChecked == true)
                {
                    //SQL za dobijanje sifre klijenta
                    string query_klijent_password = "select pssword from Klijent where username = @username";
                    SqlCommand cmd_klijent_password = new SqlCommand(query_klijent_password, konekcija);
                    cmd_klijent_password.Parameters.AddWithValue("@username", username_txt);
                    result = Convert.ToString(cmd_klijent_password.ExecuteScalar());
                    if (result != "")
                    {
                        if (password_txt == result)
                        {
                            //SQL za dobijanje id klijenta
                            string query_klijent_id = "select klijent_id from Klijent where username = @username";
                            SqlCommand cmd_klijent_id = new SqlCommand(query_klijent_id, konekcija);
                            cmd_klijent_id.Parameters.AddWithValue("@username", username_txt);
                            id = Convert.ToInt32(cmd_klijent_id.ExecuteScalar());


                            FrmKlijent window = new FrmKlijent();
                            window.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Pogresna sifra", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Pogresan username", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Neuspesna konekcija " + ex.Message, "Klijent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }
    }
}

