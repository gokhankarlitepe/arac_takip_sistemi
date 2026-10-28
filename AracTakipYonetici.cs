using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AracTakipYonetici
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "AracTakipYonetici_SingleInstance_Mutex_v2", out createdNew))
            {
                if (!createdNew)
                {
                    OpenUrlOrFile("admin.html");
                    MessageBox.Show("Araç Takip Yönetici Masası zaten çalışmaktadır.\nYönetici paneli tarayıcınızda açıldı.",
                        "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DashboardForm());
            }
        }

        public static void OpenUrlOrFile(string pathOrUrl)
        {
            try
            {
                string full = pathOrUrl;
                if (!pathOrUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !pathOrUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pathOrUrl);
                    if (File.Exists(localPath))
                    {
                        full = localPath;
                    }
                    else
                    {
                        full = "https://gokhankarlitepe.github.io/arac_takip_sistemi/" + pathOrUrl;
                    }
                }
                Process.Start(new ProcessStartInfo(full) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Açılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class DashboardForm : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private string appDir;

        public DashboardForm()
        {
            appDir = AppDomain.CurrentDomain.BaseDirectory;
            InitializeUi();
        }

        private void InitializeUi()
        {
            this.Text = "İşletme Araç Takip & Yönetim Masası";
            this.Size = new Size(680, 560);
            this.MinimumSize = new Size(640, 520);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(15, 23, 42); // slate-900
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.Icon = SystemIcons.Application;

            // HEADER PANEL
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(30, 41, 59), // slate-800
                Padding = new Padding(20, 15, 20, 15)
            };

            Label lblTitle = new Label
            {
                Text = "🚗 ARAÇ TAKİP VE FİLO YÖNETİM SİSTEMİ",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 250, 252),
                AutoSize = true,
                Location = new Point(20, 14)
            };

            Label lblSub = new Label
            {
                Text = "Yönetici Masası • Hızlı Erişim ve Raporlama Merkezi",
                Font = new Font("Segoe UI", 9.0f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(22, 44)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSub);
            this.Controls.Add(pnlHeader);

            // MAIN CONTENT PANEL
            TableLayoutPanel pnlGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(20),
                BackColor = Color.FromArgb(15, 23, 42)
            };
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            pnlGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            pnlGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            // KART 1: YÖNETİCİ PANELİ (En Önemli)
            Panel cardAdmin = CreateCard(
                "📊 YÖNETİM & DENETİM PANELİ",
                "Güvenlikten gelen WhatsApp raporlarını içe aktarabilir, araç hareketlerini inceleyebilir ve Excel (CSV) olarak döküm alabilirsiniz.",
                "Yönetici Panelini Aç",
                Color.FromArgb(37, 99, 235), // Blue
                (s, e) => Program.OpenUrlOrFile("admin.html")
            );
            pnlGrid.Controls.Add(cardAdmin, 0, 0);

            // KART 2: QR KOD YAZDIRMA
            Panel cardQr = CreateCard(
                "🖨️ ARAÇ QR KODLARI (A4 YAZDIR)",
                "Araç torpido veya güneşliklerine yapıştırılacak karekod etiketlerini A4 sayfa formatında üretir ve çıktısını alır.",
                "QR Yazdırma Sayfasını Aç",
                Color.FromArgb(5, 150, 105), // Emerald
                (s, e) => Program.OpenUrlOrFile("QR_Kod_Yazdir.html")
            );
            pnlGrid.Controls.Add(cardQr, 1, 0);

            // KART 3: ŞOFÖR FORMU
            Panel cardDriver = CreateCard(
                "📱 ŞOFÖR MOBİL ÇIKIŞ FORMU",
                "Şoförün araçtaki QR kodu okuttuğunda telefonunda açılan otomatik giriş/çıkış bildirim formunu bilgisayardan test edin.",
                "Şoför Formunu Aç",
                Color.FromArgb(14, 165, 233), // Sky
                (s, e) => Program.OpenUrlOrFile("index.html")
            );
            pnlGrid.Controls.Add(cardDriver, 0, 1);

            // KART 4: GÜVENLİK APK DOSYASI
            Panel cardApk = CreateCard(
                "📲 GÜVENLİK TELEFONU APK",
                "Nizamiyedeki güvenlik görevlisinin telefonuna kurulacak Android uygulamasını (.apk) klasörde bulun veya telefona aktarın.",
                "APK Dosyasını Klasörde Göster",
                Color.FromArgb(217, 119, 6), // Amber
                (s, e) =>
                {
                    string apkPath = Path.Combine(appDir, "NizamiyeGuvenlik.apk");
                    if (File.Exists(apkPath))
                    {
                        Process.Start("explorer.exe", "/select,\"" + apkPath + "\"");
                    }
                    else
                    {
                        Program.OpenUrlOrFile("https://gokhankarlitepe.github.io/arac_takip_sistemi/NizamiyeGuvenlik.apk");
                    }
                }
            );
            pnlGrid.Controls.Add(cardApk, 1, 1);

            this.Controls.Add(pnlGrid);

            // FOOTER BAR
            Panel pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 8, 20, 8)
            };

            Label lblCloudUrl = new Label
            {
                Text = "🌐 Bulut Yayını: https://gokhankarlitepe.github.io/arac_takip_sistemi/",
                ForeColor = Color.FromArgb(56, 189, 248),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Underline),
                Cursor = Cursors.Hand,
                AutoSize = true,
                Location = new Point(20, 12)
            };
            lblCloudUrl.Click += (s, e) => Program.OpenUrlOrFile("https://gokhankarlitepe.github.io/arac_takip_sistemi/");

            Button btnOpenFolder = new Button
            {
                Text = "📁 Dosya Klasörü",
                ForeColor = Color.FromArgb(226, 232, 240),
                BackColor = Color.FromArgb(51, 65, 85),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                Size = new Size(130, 28),
                Location = new Point(510, 8),
                Anchor = AnchorStyles.Right | AnchorStyles.Top,
                Cursor = Cursors.Hand
            };
            btnOpenFolder.FlatAppearance.BorderSize = 0;
            btnOpenFolder.Click += (s, e) => Process.Start("explorer.exe", appDir);

            pnlFooter.Controls.Add(lblCloudUrl);
            pnlFooter.Controls.Add(btnOpenFolder);
            this.Controls.Add(pnlFooter);

            // SYSTEM TRAY ICON
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("📊 Yönetici Panelini Aç", null, (s, e) => Program.OpenUrlOrFile("admin.html"));
            trayMenu.Items.Add("🖨️ QR Kodları Yazdır", null, (s, e) => Program.OpenUrlOrFile("QR_Kod_Yazdir.html"));
            trayMenu.Items.Add("📱 Şoför Formu", null, (s, e) => Program.OpenUrlOrFile("index.html"));
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Pencereyi Göster", null, (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; this.BringToFront(); });
            trayMenu.Items.Add("Çıkış", null, (s, e) => { trayIcon.Visible = false; Application.Exit(); });

            trayIcon = new NotifyIcon
            {
                Text = "Araç Takip Yönetici Masası",
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; this.BringToFront(); };
        }

        private Panel CreateCard(string title, string desc, string buttonText, Color btnColor, EventHandler onClick)
        {
            Panel card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59),
                Margin = new Padding(8),
                Padding = new Padding(16)
            };

            Label lblCardTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Top,
                Height = 28
            };

            Label lblCardDesc = new Label
            {
                Text = desc,
                Font = new Font("Segoe UI", 8.8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 4, 0, 8)
            };

            Button btnCard = new Button
            {
                Text = buttonText,
                Dock = DockStyle.Bottom,
                Height = 38,
                BackColor = btnColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCard.FlatAppearance.BorderSize = 0;
            btnCard.Click += onClick;

            card.Controls.Add(lblCardDesc);
            card.Controls.Add(lblCardTitle);
            card.Controls.Add(btnCard);

            return card;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Çarpıya basınca arka planda tray'e küçült
                e.Cancel = true;
                this.Hide();
                trayIcon.ShowBalloonTip(2000, "Araç Takip Masası", "Uygulama arka planda simge durumunda çalışmaya devam ediyor.", ToolTipIcon.Info);
            }
            else
            {
                trayIcon.Visible = false;
                base.OnFormClosing(e);
            }
        }
    }
}
