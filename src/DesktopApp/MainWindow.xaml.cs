using EtNotif.Libs.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Windows;

namespace EtNotif.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly AppDbContext _db;
        public MainWindow()
        {
            InitializeComponent();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite("Data Source=etnotif.db")
                .Options;
            _db = new AppDbContext(options);
            _db.Database.Migrate(); // İlk çalıştırmada otomatik migration/DB yarat (basit)
            LoadGrid();
        }

        private void LoadGrid()
        {
            Grid.ItemsSource = _db.Taxpayers.OrderBy(t => t.DisplayName).ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var vkn = VknBox.Text?.Trim();
            var name = NameBox.Text?.Trim();
            var pwd = PasswordBox.Password;
            if (string.IsNullOrEmpty(vkn) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pwd))
            {
                MessageBox.Show("VKN/Ad/Şifre girin.");
                return;
            }

            var enc = EtNotif.Libs.Security.CryptoHelper.ProtectToBase64(pwd);
            var t = new Taxpayer { Vkn = vkn, DisplayName = name, EncryptedPassword = enc, Enabled = true };
            try
            {
                _db.Taxpayers.Add(t);
                _db.SaveChanges();
                LoadGrid();
                StatusText.Text = "Mükellef eklendi.";
                VknBox.Text = NameBox.Text = "";
                PasswordBox.Password = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }
    }
}
