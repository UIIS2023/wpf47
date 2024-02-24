using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BankaPIS.Forms
{
    /// <summary>
    /// Interaction logic for FrmZaposleni.xaml
    /// </summary>
    public partial class FrmZaposleni : Window
    {
        Konekcija conn = new Konekcija();
        SqlConnection konekcija = new SqlConnection();

        public static string id_pretraga;

        public FrmZaposleni()
        {
            InitializeComponent();
            konekcija = conn.KreirajKonekciju();
            int id = MainWindow.id;
            try
            {
                konekcija.Open();

                //SQL za ucitavanje podataka klijenta
                string query_podaci_zaposlenog = "select ime, prezime, jmbg, email, broj_telefona, adresa from Korisnik where korisnik_id = @id";
                SqlCommand cmd_podaci_zaposlenog = new SqlCommand(query_podaci_zaposlenog, konekcija);
                cmd_podaci_zaposlenog.Parameters.AddWithValue("@id", id);
                SqlDataReader reader = cmd_podaci_zaposlenog.ExecuteReader();
                while (reader.Read())
                {
                    label_ime.Content = reader["ime"].ToString();
                    label_prezime.Content = reader["prezime"].ToString();
                    label_jmbg.Content = reader["jmbg"].ToString();
                    label_email.Content = reader["email"].ToString();
                    label_brojTelefona.Content = reader["broj_telefona"].ToString();
                    label_adresa.Content = reader["adresa"].ToString();
                }
                reader.Close();
                cmd_podaci_zaposlenog.Dispose();
            }
            catch
            {
                MessageBox.Show("Neuspesna konekcija!!!", "Greska2", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Pretrazivanje klijenata-------------
        private void Pretrazi(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string ime = txt_srcIme.Text;
                string prezime = txt_srcPrezime.Text;
                string jmbg = txt_srcJmbg.Text;

                //SQL za pretragu klijenta po imenu preyimenu i jmbg
                string query_pretraga = "select klijent_id from klijent where ime = @ime and prezime = @prezime and jmbg = @jmbg";
                SqlCommand cmd_pretraga = new SqlCommand(query_pretraga, konekcija);
                cmd_pretraga.Parameters.AddWithValue("@ime", ime);
                cmd_pretraga.Parameters.AddWithValue("@prezime", prezime);
                cmd_pretraga.Parameters.AddWithValue("@jmbg", jmbg);
                id_pretraga = Convert.ToString(cmd_pretraga.ExecuteScalar());
                if (id_pretraga != "")
                {
                    FrmKlijentPretraga window = new FrmKlijentPretraga();
                    window.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Neuspesna pretraga", "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                MessageBox.Show(ex.Message, "Pretraga", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        private void Dodaj_klijenta(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string ime = txt_dodavanje_ime.Text;
                string prezime = txt_dodavanje_prezime.Text;
                string jmbg = txt_dodavanje_jmbg.Text;
                string adresa = txt_dodavanje_adresa.Text;
                string grad = txt_dodavanje_grad.Text;
                string broj_telefona = txt_dodavanje_broj_telefona.Text;
                string email = txt_dodavanje_email.Text;
                string username = txt_dodavanje_username.Text;
                string password = txt_dodavanje_password.Text; ;

                string query_jmbg = "select jmbg from klijent";
                SqlCommand cmd_jmbg = new SqlCommand(query_jmbg, konekcija);
                ArrayList jmbg_list = new ArrayList();
                SqlDataReader jmbg_reader = cmd_jmbg.ExecuteReader();
                while(jmbg_reader.Read())
                {
                    jmbg_list.Add(jmbg_reader["jmbg"].ToString());
                }
                jmbg_reader.Close();
                if (!jmbg_list.Contains(jmbg))
                {
                    try
                    {
                        string query_dodaj_klijenta = "insert into Klijent (ime, prezime, jmbg, adresa, grad, broj_telefona, email, username, pssword) " +
                        "values(@ime, @prezime, @jmbg, @adresa, @grad, @broj_telefona, @email, @username, @password)";
                        SqlCommand cmd_dodaj_klijenta = new SqlCommand(query_dodaj_klijenta, konekcija);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@username", username);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@password", password);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@ime", ime);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@prezime", prezime);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@jmbg", jmbg);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@adresa", adresa);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@grad", grad);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@broj_telefona", broj_telefona);
                        cmd_dodaj_klijenta.Parameters.AddWithValue("@email", email);
                        cmd_dodaj_klijenta.ExecuteNonQuery();
                        Kreiranje_racuna();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Dodavanje SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        MessageBox.Show("Uspesno dadat klijent", "Dodavanje", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("JMBG vec unet", "Dodavanje klijenta", MessageBoxButton.OK, MessageBoxImage.Error);
                    txt_dodavanje_ime.Text = "";
                    txt_dodavanje_prezime.Text = "";
                    txt_dodavanje_jmbg.Text = "";
                    txt_dodavanje_adresa.Text = "";
                    txt_dodavanje_grad.Text = "";
                    txt_dodavanje_broj_telefona.Text = "";
                    txt_dodavanje_email.Text = "";
                    txt_dodavanje_username.Text = "";
                    txt_dodavanje_password.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dodavanje", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                txt_dodavanje_ime.Text = "";
                txt_dodavanje_prezime.Text = "";
                txt_dodavanje_jmbg.Text = "";
                txt_dodavanje_adresa.Text = "";
                txt_dodavanje_grad.Text = "";
                txt_dodavanje_broj_telefona.Text = "";
                txt_dodavanje_email.Text = "";
                txt_dodavanje_username.Text = "";
                txt_dodavanje_password.Text = "";
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        private void Kreiranje_racuna()
        {
            try
            {
                Random rnd = new Random();
                int novi_racun = rnd.Next(1000);
                
                string query_broj_racuna = "select broj_racuna from racun";
                SqlCommand cmd_broj_racuna = new SqlCommand(query_broj_racuna, konekcija);
                ArrayList broj_racuna_list = new ArrayList();
                SqlDataReader reader_broj_racuna = cmd_broj_racuna.ExecuteReader();
                while (reader_broj_racuna.Read())
                {
                    broj_racuna_list.Add(reader_broj_racuna["broj_racuna"].ToString());
                }
                while (broj_racuna_list.Contains(novi_racun))
                    novi_racun = rnd.Next(1000);
                reader_broj_racuna.Close();

                string query_id_novi_klijent = "SELECT TOP 1 klijent_id FROM klijent ORDER BY klijent_id DESC";
                SqlCommand cmd_novi_klijent = new SqlCommand(query_id_novi_klijent, konekcija);
                int id_novog_klijenta = Convert.ToInt32(cmd_novi_klijent.ExecuteScalar());
                

                string query_dodaj_racun = "insert into racun (broj_racuna, kolicina_novca, klijent_id) values(@broj_racuna, 0, @klijent_id)";
                SqlCommand cmd_dodaj_racun = new SqlCommand(query_dodaj_racun, konekcija);
                cmd_dodaj_racun.Parameters.AddWithValue("@broj_racuna", novi_racun);
                cmd_dodaj_racun.Parameters.AddWithValue("@klijent_id", id_novog_klijenta);
                cmd_dodaj_racun.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Kreiranje Racun", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}