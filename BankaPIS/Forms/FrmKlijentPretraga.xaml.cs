using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
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
using System.Windows.Shapes;
using System.Collections;
using System.Collections.ObjectModel;


namespace BankaPIS.Forms
{
    /// <summary>
    /// Interaction logic for FrmKlijentPretraga.xaml
    /// </summary>
    public partial class FrmKlijentPretraga : Window
    {
        private Konekcija conn = new Konekcija();
        private SqlConnection konekcija = new SqlConnection();
        private int id_klijenta = Convert.ToInt32(FrmZaposleni.id_pretraga);
        private double stanje;
        private int brojac=0;
        public FrmKlijentPretraga()
        {
            InitializeComponent();
            konekcija = conn.KreirajKonekciju();
            Ucitaj_podatke();
            Ucitaj_stanje();
            Set_disable();
        }

        //----------Ucitavanje podataka klijenta-------------
        private void Ucitaj_podatke()
        {
            try
            {
                konekcija.Open();
                string query_klijent = "select * from klijent where klijent_id=@id";
                SqlCommand cmd_Klijent = new SqlCommand(query_klijent, konekcija);
                cmd_Klijent.Parameters.AddWithValue("@id", id_klijenta);
                SqlDataReader reader = cmd_Klijent.ExecuteReader();
                while (reader.Read())
                {
                    ime_txt.Text = reader["ime"].ToString();
                    prezime_txt.Text = reader["prezime"].ToString();
                    jmbg_txt.Text = reader["jmbg"].ToString();
                    email_txt.Text = reader["email"].ToString();
                    brojTelefona_txt.Text = reader["broj_telefona"].ToString();
                    adresa_txt.Text = reader["adresa"].ToString();
                    grad_txt.Text = reader["grad"].ToString();
                }
                reader.Close();
                string query_racun = "select broj_racuna from Racun where klijent_id=@id";
                SqlCommand cmd_brojRacuna = new SqlCommand(query_racun, konekcija);
                cmd_brojRacuna.Parameters.AddWithValue("@id", id_klijenta);
                SqlDataReader reader2 = cmd_brojRacuna.ExecuteReader();
                while (reader2.Read())
                {
                    brojRacuna_txt.Text = reader2["broj_racuna"].ToString();
                }
            }
            catch
            {
                MessageBox.Show("Neuspesna konekcija", "Podaci", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Ucitavanje stanja sa racuna-------------
        public void Ucitaj_stanje()
        {
            try
            {
                konekcija.Open();
                string query_racun = "select kolicina_novca from racun where klijent_id=@id";
                SqlCommand cmd_stanje = new SqlCommand(query_racun, konekcija);
                cmd_stanje.Parameters.AddWithValue("@id", id_klijenta);
                SqlDataReader reader = cmd_stanje.ExecuteReader();
                while (reader.Read())
                {
                    stanje = Convert.ToDouble(reader["kolicina_novca"]);
                    label_stanje.Content = stanje;
                }
            }
            catch
            {
                MessageBox.Show("Neuspesna konekcija", "Stanje", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Uplacivanje novca-------------
        private void Uplati(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                if (suma_txt.Text.Length > 0)
                {
                    double suma = Convert.ToDouble(suma_txt.Text);
                    double novo_stanje = suma + stanje;
                    string query_novo_stanje = "UPDATE racun SET kolicina_novca = @stanje WHERE klijent_id = @id";
                    SqlCommand cmd_novo_stanje = new SqlCommand(query_novo_stanje, konekcija);
                    cmd_novo_stanje.Parameters.AddWithValue("@id", id_klijenta);
                    cmd_novo_stanje.Parameters.AddWithValue("@stanje", novo_stanje);
                    int rows_Affecred = cmd_novo_stanje.ExecuteNonQuery();
                    if (rows_Affecred == 0)
                    {
                        MessageBox.Show("Neuspesna konekcija", "Novo stanje", MessageBoxButton.OK, MessageBoxImage.Error);
                        suma_txt.Text = "";
                    }
                    else
                    {
                        Upisi_transakciju_uplati(suma);
                        stanje = novo_stanje;
                        label_stanje.Content = stanje;
                        suma_txt.Text = "";
                        MessageBox.Show("Uspesna uplata", "Uplata", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    suma_txt.Text = "";
                    MessageBox.Show("Unseite sumu", "Uplata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch
            {
                suma_txt.Text = "";
                MessageBox.Show("Neuspesna konekcija", "Uplata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Isplacivanje novca-------------
        private void Isplati(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                double suma = (suma_txt.Text.Length > 0) ? Convert.ToDouble(suma_txt.Text) : 0;
                if (suma <= stanje && suma != 0)
                {
                    double novo_stanje = stanje - suma;
                    string query_novo_stanje = "UPDATE racun SET kolicina_novca = @stanje WHERE klijent_id = @id";
                    SqlCommand cmd_novo_stanje = new SqlCommand(query_novo_stanje, konekcija);
                    cmd_novo_stanje.Parameters.AddWithValue("@id", id_klijenta);
                    cmd_novo_stanje.Parameters.AddWithValue("@stanje", novo_stanje * (-1));
                    int rows_Affecred = cmd_novo_stanje.ExecuteNonQuery();
                    if (rows_Affecred == 0)
                    {
                        MessageBox.Show("Neuspesna konekcija", "Novo stanje", MessageBoxButton.OK, MessageBoxImage.Error);
                        suma_txt.Text = "";
                    }
                    else
                    {
                        stanje = novo_stanje;
                        label_stanje.Content = stanje;
                        Upisi_transakciju_isplati(suma * (-1));
                        suma_txt.Text = "";
                        MessageBox.Show("Uspesna isplata", "Isplata", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    suma_txt.Text = "";
                    MessageBox.Show("Nemate dovoljno novca na racunu", "Isplata", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch
            {
                suma_txt.Text = "";
                MessageBox.Show("Neuspesna konekcija", "Uplata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Upisivanje transakcije uplati-------------
        private void Upisi_transakciju_uplati(double suma)
        {
            try
            {
                //SQL za dobijanje racun_id
                string query_select_racun_id = "select racun_id from racun where klijent_id=@klijent_id";
                SqlCommand cmd_select_racun_id = new SqlCommand(query_select_racun_id, konekcija);
                cmd_select_racun_id.Parameters.AddWithValue("@klijent_id", id_klijenta);
                int racun_id = Convert.ToInt32(cmd_select_racun_id.ExecuteScalar());
                cmd_select_racun_id.Dispose();

                // SQL za upisivanje transakcije u tabele transakcija i uplacivanje
                string query_insert_transakcija = "INSERT INTO [dbo].[Transakcija] ([klijent_id], [kolicina_novca], [created_at], [vrsta_transakcije]) VALUES (@klijent_id, @suma, GETDATE(), 'Uplata'); " +
                            "DECLARE @transakcija_id INT; SET @transakcija_id = SCOPE_IDENTITY(); " +
                            "INSERT INTO [dbo].[Uplacivanje] ([racun_id], [transakcija_id]) VALUES (@racun_id, @transakcija_id)";
                SqlCommand cmd_insert_transakcija = new SqlCommand(query_insert_transakcija, konekcija);
                cmd_insert_transakcija.Parameters.AddWithValue("@klijent_id", id_klijenta);
                cmd_insert_transakcija.Parameters.AddWithValue("@suma", suma);
                cmd_insert_transakcija.Parameters.AddWithValue("@racun_id", racun_id);
                cmd_insert_transakcija.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom upisa transakcije uplate: " + ex.Message, "Upis transakcije", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Upisivanje transakcije isplati-------------
        private void Upisi_transakciju_isplati(double suma)
        {
            try
            {
                //SQL za dobijanje racun_id
                string query_select_racun_id = "select racun_id from racun where klijent_id=@klijent_id";
                SqlCommand cmd_select_racun_id = new SqlCommand(query_select_racun_id, konekcija);
                cmd_select_racun_id.Parameters.AddWithValue("@klijent_id", id_klijenta);
                int racun_id = Convert.ToInt32(cmd_select_racun_id.ExecuteScalar());
                cmd_select_racun_id.Dispose();

                // SQL za upisivanje transakcije u tabele transakcija i Podizanje_novca
                string query_insert_transakcija = "INSERT INTO [dbo].[Transakcija] ([klijent_id], [kolicina_novca], [created_at], [vrsta_transakcije]) VALUES (@klijent_id, @suma, GETDATE(), 'Isplata'); " +
                            "DECLARE @transakcija_id INT; SET @transakcija_id = SCOPE_IDENTITY(); " +
                            "INSERT INTO[dbo].[Podizanje_novca] ([racun_id], [transakcija_id]) VALUES(@racun_id, @transakcija_id)";
                SqlCommand cmd_insert_transakcija = new SqlCommand(query_insert_transakcija, konekcija);
                cmd_insert_transakcija.Parameters.AddWithValue("@klijent_id", id_klijenta);
                cmd_insert_transakcija.Parameters.AddWithValue("@suma", suma);
                cmd_insert_transakcija.Parameters.AddWithValue("@racun_id", racun_id);
                cmd_insert_transakcija.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom upisa transakcije isplate: " + ex.Message, "Upis transakcije", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        private void Prikazi_uplate(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string qurey_prikaz_uplate = "select vrsta_transakcije as Opis, kolicina_novca as Suma, created_at as 'Datum i vreme' from Transakcija " +
                    "where vrsta_transakcije = 'Uplata' or vrsta_transakcije = 'Isplata' and klijent_id =" + id_klijenta;
                SqlDataAdapter adapter_prikaz_uplata = new SqlDataAdapter(qurey_prikaz_uplate, konekcija);
                DataTable dt = new DataTable();
                adapter_prikaz_uplata.Fill(dt);
                if (dataGridPodaci != null)
                {
                    dataGridPodaci.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prikaz uplata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }
        private void Prikai_transfer(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string qurey_prikaz_transfere = "select vrsta_transakcije as Vrsta, kolicina_novca as Suma, broj_racuna_primaoc as primaoc, broj_racuna_posaljilac as Podaljilac, opis as 'Opis transkacije', created_at as Datum " +
                                                "from Transakcija " +
                                                "inner join " +
                                                "Transfer " +
                                                "on Transakcija.transakcija_id = Transfer.transakcija_id where klijent_id = " + id_klijenta;
                SqlDataAdapter adapter_prikaz_transfere = new SqlDataAdapter(qurey_prikaz_transfere, konekcija);
                DataTable dt = new DataTable();
                adapter_prikaz_transfere.Fill(dt);
                if (dataGridPodaci != null)
                {
                    dataGridPodaci.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prikaz transfera", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        private void Set_disable()
        {
            brojTelefona_txt.IsEnabled = false;
            ime_txt.IsEnabled = false;
            prezime_txt.IsEnabled = false;
            adresa_txt.IsEnabled = false;
            grad_txt.IsEnabled = false;
            jmbg_txt.IsEnabled = false;
            brojRacuna_txt.IsEnabled = false;
            email_txt.IsEnabled = false;
            btn_izmeni.Content = "Izmeni podatke";
        }

        private void Set_enable()
        {
            brojTelefona_txt.IsEnabled = true;
            ime_txt.IsEnabled = true;
            prezime_txt.IsEnabled = true;
            adresa_txt.IsEnabled = true;
            grad_txt.IsEnabled = true;
            jmbg_txt.IsEnabled = true;
            email_txt.IsEnabled = true;
            btn_izmeni.Content = "Sacuvaj izmene";
        }

        private void Sacuvaj_promene(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                if (brojac == 0)
                {
                    brojac = 1;
                    Set_enable();
                }
                else
                {
                    brojac = 0;
                    Set_disable();
                    string query_update_klijent = "update klijent set ime = @ime , prezime = @prezime, jmbg = @jmbg, adresa = @adresa, email = @email, grad = @grad, broj_telefona = @broj_telefona where klijent_id = @id_klijenta";
                    SqlCommand cmd_update_klijent = new SqlCommand(query_update_klijent, konekcija);

                    string ime = ime_txt.Text;
                    string prezime = prezime_txt.Text;
                    string jmbg = jmbg_txt.Text;
                    string adresa = adresa_txt.Text;
                    string grad = grad_txt.Text;
                    string broj_telefona = brojTelefona_txt.Text;
                    string email = email_txt.Text;

                    cmd_update_klijent.Parameters.AddWithValue("@ime", ime);
                    cmd_update_klijent.Parameters.AddWithValue("@prezime", prezime);
                    cmd_update_klijent.Parameters.AddWithValue("@jmbg", jmbg);
                    cmd_update_klijent.Parameters.AddWithValue("@adresa", adresa);
                    cmd_update_klijent.Parameters.AddWithValue("@email", email);
                    cmd_update_klijent.Parameters.AddWithValue("@grad", grad);
                    cmd_update_klijent.Parameters.AddWithValue("@broj_telefona", broj_telefona);
                    cmd_update_klijent.Parameters.AddWithValue("@id_klijenta", id_klijenta);

                    int broj_izmena = cmd_update_klijent.ExecuteNonQuery();

                    if (broj_izmena > 0)
                    {
                        MessageBox.Show("Uspesna izemna", "Izmena", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Izmena", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }
    }
}
