using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

namespace BankaPIS.Forms
{
    /// <summary>
    /// Interaction logic for FrmKlijent.xaml
    /// </summary>
    public partial class FrmKlijent : Window
    {
        private Konekcija conn = new Konekcija();
        private SqlConnection konekcija = new SqlConnection();
        private double stanje;
        int klijent_id = MainWindow.id;
        private int id_primaoca;
        public FrmKlijent()
        {
            InitializeComponent();
            konekcija = conn.KreirajKonekciju();
            klijent_id = MainWindow.id;//Uzimanje id klijenta koji se prijavio

            //----------Ucitavanje podataka klijenta-------------
            try
            {
                konekcija.Open();

                //SQL za ucitavanje podataka klijenta iz tabele klijent
                string query_podaci_klijenta = "select ime, prezime, email, jmbg, broj_telefona, adresa from Klijent where klijent_id = @id";
                SqlCommand cmd_podaci_klijenta = new SqlCommand(query_podaci_klijenta, konekcija);
                cmd_podaci_klijenta.Parameters.AddWithValue("@id", klijent_id);
                SqlDataReader reader = cmd_podaci_klijenta.ExecuteReader();
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
                cmd_podaci_klijenta.Dispose();

                //SQL za ucitavanje broja racuna iz tabele racun
                string query_racun = "select broj_racuna from Racun where klijent_id = @id";
                SqlCommand cmd_racun = new SqlCommand(query_racun, konekcija);
                cmd_racun.Parameters.AddWithValue("@id", klijent_id);
                int broj_racuna = Convert.ToInt32(cmd_racun.ExecuteScalar());
                cmd_racun.Dispose();
                label_racun.Content = Convert.ToString(broj_racuna);
                Ucitaj_stanje();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Neuspesno ucitavanje podataka: " + ex.Message, "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (konekcija != null)
                    konekcija.Close();
            }
        }

        //----------Ucitavanje stanja sa racuna-------------
        private void Ucitaj_stanje()
        {
            try
            {
                //SQL za ucitavanje kolicine_novca iz tabele racun
                string query_kolicina_novca = "select kolicina_novca from Racun where klijent_id = @id";
                SqlCommand cmd_kolicina_novca = new SqlCommand(query_kolicina_novca, konekcija);
                cmd_kolicina_novca.Parameters.AddWithValue("@id", klijent_id);
                stanje = Convert.ToDouble(cmd_kolicina_novca.ExecuteScalar());
                cmd_kolicina_novca.Dispose();
                label_stanje.Content = Convert.ToString(stanje) + " RSD";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Neuspesno ucitavanje stanja: " + ex.Message, "Greska", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Isvrsi_transfer(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string broj_racuna = txt_racun.Text;
                double suma = (txt_suma_transfer.Text.Length > 0) ? Convert.ToDouble(txt_suma_transfer.Text) : 0;
                string opis = txt_opis.Text;

                string query_broj_racuna = "select broj_racuna from racun";
                SqlCommand cmd_broj_racuna = new SqlCommand(query_broj_racuna, konekcija);
                ArrayList broj_racuna_list = new ArrayList();
                SqlDataReader reader_broj_racuna = cmd_broj_racuna.ExecuteReader();
                while (reader_broj_racuna.Read())
                    broj_racuna_list.Add(reader_broj_racuna["broj_racuna"].ToString());
                reader_broj_racuna.Close();
                if (broj_racuna_list.Contains(broj_racuna))
                {
                    if(stanje >= suma && suma != 0)
                    {
                        string query_broj_racuna_prosaljioca = "select broj_racuna from racun where klijent_id = @id_posaljioca";
                        SqlCommand cmd_broj_racuna_posaljioca = new SqlCommand(query_broj_racuna_prosaljioca, konekcija);
                        cmd_broj_racuna_posaljioca.Parameters.AddWithValue("@id_posaljioca", klijent_id);
                        int broj_racuna_posaljioca = Convert.ToInt32(cmd_broj_racuna_posaljioca.ExecuteScalar());

                        string query_id_primaoca = "select klijent_id from racun where broj_racuna = @broj_racuna";
                        SqlCommand cmd_id_primaoca = new SqlCommand(query_id_primaoca, konekcija);
                        cmd_id_primaoca.Parameters.AddWithValue("@broj_racuna", broj_racuna);
                        id_primaoca = Convert.ToInt32(cmd_id_primaoca.ExecuteScalar());

                        try
                        {
                            string query_insert_transakcija_posaljilac = "INSERT INTO [dbo].[Transakcija] ([klijent_id], [kolicina_novca], [created_at], [vrsta_transakcije]) " +
                                "VALUES (@klijent_id, @suma, GETDATE(), 'Transfer isplata'); " +
                                "DECLARE @transakcija_id INT; SET @transakcija_id = SCOPE_IDENTITY(); " +
                                "INSERT INTO [dbo].[Transfer] (posaljilac_id, primalac_id, broj_racuna_primaoc, broj_racuna_posaljilac, opis, [transakcija_id]) " +
                                "VALUES (@posaljilac_id, @primalac_id, @broj_racuna_primaoc, @broj_racuna_posaljilac, @opis, @transakcija_id)";

                            SqlCommand cmd_insert_transakcija_posaljilac = new SqlCommand(query_insert_transakcija_posaljilac, konekcija);
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@klijent_id", klijent_id);
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@suma", suma * (-1));
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@posaljilac_id", klijent_id);
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@primalac_id", id_primaoca);
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@broj_racuna_primaoc", broj_racuna);
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@broj_racuna_posaljilac", broj_racuna_posaljioca);
                            cmd_insert_transakcija_posaljilac.Parameters.AddWithValue("@opis", opis);
                            cmd_insert_transakcija_posaljilac.ExecuteNonQuery();

                            string query_insert_transakcija_primalac = "INSERT INTO [dbo].[Transakcija] ([klijent_id], [kolicina_novca], [created_at], [vrsta_transakcije]) " +
                                "VALUES (@klijent_id, @suma, GETDATE(), 'Transfer uplata'); " +
                                "DECLARE @transakcija_id INT; SET @transakcija_id = SCOPE_IDENTITY(); " +
                                "INSERT INTO [dbo].[Transfer] (posaljilac_id, primalac_id, broj_racuna_primaoc, broj_racuna_posaljilac, opis, [transakcija_id]) " +
                                "VALUES (@posaljilac_id, @primalac_id, @broj_racuna_primaoc, @broj_racuna_posaljilac, @opis, @transakcija_id)";

                            SqlCommand cmd_insert_transakcija_primalac = new SqlCommand(query_insert_transakcija_primalac, konekcija);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@klijent_id", id_primaoca);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@suma", suma);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@posaljilac_id", klijent_id);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@primalac_id", id_primaoca);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@broj_racuna_primaoc", broj_racuna);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@broj_racuna_posaljilac", broj_racuna_posaljioca);
                            cmd_insert_transakcija_primalac.Parameters.AddWithValue("@opis", opis);
                            cmd_insert_transakcija_primalac.ExecuteNonQuery();
                            stanje -= suma;

                            string query_novo_stanje = "UPDATE racun SET kolicina_novca = @stanje WHERE klijent_id = @id";
                            SqlCommand cmd_novo_stanje = new SqlCommand(query_novo_stanje, konekcija);
                            cmd_novo_stanje.Parameters.AddWithValue("@id", klijent_id);
                            cmd_novo_stanje.Parameters.AddWithValue("@stanje", stanje);
                            int rows_Affecred = cmd_novo_stanje.ExecuteNonQuery();
                            Novo_stanje_primaoca(suma);
                            label_stanje.Content = stanje;
                            if(rows_Affecred == 0)
                            {
                                MessageBox.Show("Neuspesna konekcija", "Novo stanje posaljilac", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            else
                            {
                                MessageBox.Show("Uspesna transakcija", "Prenos", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Upis prenosa", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nedovoljno sredstava na racunu", "Prenos", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Broj racuna ne postoji", "Prenos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Prenos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if(konekcija != null)
                    konekcija.Close();
            }
        }

        private void Novo_stanje_primaoca(double suma)
        {
            string query_racun = "select kolicina_novca from racun where klijent_id=@id";
            SqlCommand cmd_stanje = new SqlCommand(query_racun, konekcija);
            cmd_stanje.Parameters.AddWithValue("@id", id_primaoca);
            double novo_stanje = Convert.ToDouble(cmd_stanje.ExecuteScalar()) + suma;

            string query_novo_stanje = "UPDATE racun SET kolicina_novca = @stanje WHERE klijent_id = @id";
            SqlCommand cmd_novo_stanje = new SqlCommand(query_novo_stanje, konekcija);
            cmd_novo_stanje.Parameters.AddWithValue("@id", id_primaoca);
            cmd_novo_stanje.Parameters.AddWithValue("@stanje", novo_stanje);
            int rows_Affecred = cmd_novo_stanje.ExecuteNonQuery();
            if (rows_Affecred == 0)
            {
                MessageBox.Show("Neuspesna konekcija", "Novo stanje primaoca", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Prikazi_uplate_isplate(object sender, RoutedEventArgs e)
        {
            //select * from Transakcija where vrsta_transakcije='Uplata' and klijent_id=1
            try
            {
                konekcija.Open();
                string qurey_prikaz_uplate = "select vrsta_transakcije as Opis, kolicina_novca as Suma, created_at as 'Datum i vreme' from Transakcija where vrsta_transakcije = 'Uplata' or vrsta_transakcije = 'Isplata' and klijent_id =" + klijent_id;
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
        private void Prikazi_transfer(object sender, RoutedEventArgs e)
        {
            try
            {
                konekcija.Open();
                string qurey_prikaz_transfere = "select vrsta_transakcije as Vrsta, kolicina_novca as Suma, broj_racuna_primaoc as primaoc, broj_racuna_posaljilac as Podaljilac, opis as 'Opis transkacije', created_at as Datum " +
                                                "from Transakcija " +
                                                "inner join " +
                                                "Transfer " +
                                                "on Transakcija.transakcija_id = Transfer.transakcija_id where klijent_id = " + klijent_id;
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
    }
}