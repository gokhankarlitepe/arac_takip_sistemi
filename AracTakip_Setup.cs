using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace AracTakipSetup
{
    public class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }

    public class SetupForm : Form
    {
        private ProgressBar prgBar;
        private Label lblStatus;
        private TextBox txtTargetDir;
        private CheckBox chkLaunch;
        private CheckBox chkDesktopShortcut;
        private Button btnInstall;
        private string defaultTargetDir;

        public SetupForm()
        {
            // Kullanıcı izinleriyle engelsiz çalışması için LocalAppData en güvenli ve temiz yoldur
            string localApp = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            defaultTargetDir = Path.Combine(localApp, "AracTakipSistemi");

            InitializeUi();
        }

        private void InitializeUi()
        {
            this.Text = "Araç Takip & Filo Yönetim Sistemi - Kurulum Setup";
            this.Size = new Size(580, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            try
            {
                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
                string iconPath = Path.Combine(sourceDir, "app_icon.ico");
                if (File.Exists(iconPath))
                    this.Icon = new Icon(iconPath);
                else
                    this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }

            // HEADER
            Panel pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(15, 23, 42),
                Padding = new Padding(20, 15, 20, 15)
            };

            Label lblHeaderTitle = new Label
            {
                Text = "🚗 ARAÇ TAKİP SİSTEMİ KURULUMU",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 16)
            };

            Label lblHeaderDesc = new Label
            {
                Text = "Yönetici Masası, QR Yazdırma Aracı ve Mobil APK Kurulum Sihirbazı",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(22, 45)
            };

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderDesc);
            this.Controls.Add(pnlHeader);

            // BODY
            Panel pnlBody = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 20, 25, 20)
            };

            Label lblInfo = new Label
            {
                Text = "Bu kurulum sihirbazı, Araç Takip Yönetici Paneli ve araç bileşenlerini bilgisayarınıza yükleyecek ve masaüstünüze tek tıkla açılan kısayol ekleyecektir.\n\nHerhangi bir sunucu (server) veya ek yazılıma ihtiyaç yoktur.",
                Location = new Point(25, 105),
                Size = new Size(515, 60),
                ForeColor = Color.FromArgb(51, 65, 85)
            };
            this.Controls.Add(lblInfo);

            Label lblDir = new Label
            {
                Text = "Kurulumun Yapılacağı Klasör:",
                Location = new Point(25, 175),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(15, 23, 42)
            };
            this.Controls.Add(lblDir);

            txtTargetDir = new TextBox
            {
                Text = defaultTargetDir,
                Location = new Point(25, 198),
                Size = new Size(410, 26),
                Font = new Font("Segoe UI", 9.5f)
            };
            this.Controls.Add(txtTargetDir);

            Button btnBrowse = new Button
            {
                Text = "Gözat...",
                Location = new Point(445, 196),
                Size = new Size(95, 28),
                BackColor = Color.FromArgb(226, 232, 240),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnBrowse.Click += (s, e) =>
            {
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    fbd.SelectedPath = txtTargetDir.Text;
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        txtTargetDir.Text = fbd.SelectedPath;
                    }
                }
            };
            this.Controls.Add(btnBrowse);

            chkDesktopShortcut = new CheckBox
            {
                Text = "Masaüstüne Kısayol Ekle (Araç Takip Yönetici Masası)",
                Checked = true,
                Location = new Point(25, 240),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 41, 59)
            };
            this.Controls.Add(chkDesktopShortcut);

            chkLaunch = new CheckBox
            {
                Text = "Kurulum bittikten sonra uygulamayı hemen başlat",
                Checked = true,
                Location = new Point(25, 268),
                AutoSize = true,
                ForeColor = Color.FromArgb(71, 85, 105)
            };
            this.Controls.Add(chkLaunch);

            prgBar = new ProgressBar
            {
                Location = new Point(25, 310),
                Size = new Size(515, 22),
                Visible = false
            };
            this.Controls.Add(prgBar);

            lblStatus = new Label
            {
                Text = "",
                Location = new Point(25, 338),
                Size = new Size(515, 20),
                ForeColor = Color.FromArgb(5, 150, 105),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            this.Controls.Add(lblStatus);

            // FOOTER BUTTONS
            Panel pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                BackColor = Color.FromArgb(241, 245, 249),
                Padding = new Padding(20, 12, 20, 12)
            };

            btnInstall = new Button
            {
                Text = "🚀 Kurulumu Başlat",
                Size = new Size(160, 38),
                Location = new Point(380, 14),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += BtnInstall_Click;

            Button btnCancel = new Button
            {
                Text = "İptal",
                Size = new Size(90, 38),
                Location = new Point(280, 14),
                BackColor = Color.FromArgb(226, 232, 240),
                ForeColor = Color.FromArgb(51, 65, 85),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => this.Close();

            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(btnInstall);
            this.Controls.Add(pnlFooter);
        }

        private void BtnInstall_Click(object sender, EventArgs e)
        {
            string targetDir = txtTargetDir.Text.Trim();
            if (string.IsNullOrEmpty(targetDir))
            {
                MessageBox.Show("Lütfen geçerli bir kurulum klasörü seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnInstall.Enabled = false;
            prgBar.Visible = true;
            prgBar.Value = 10;
            lblStatus.Text = "Kurulum dizini hazırlanıyor...";
            Application.DoEvents();

            try
            {
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                prgBar.Value = 30;
                lblStatus.Text = "Uygulama dosyaları kopyalanıyor...";
                Application.DoEvents();

                string sourceDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] filesToCopy = new string[]
                {
                    "AracTakipYonetici.exe",
                    "admin.html",
                    "QR_Kod_Yazdir.html",
                    "qrcode.min.js",
                    "index.html",
                    "nizamiye_form.html",
                    "NizamiyeGuvenlik.apk",
                    "app_icon.ico",
                    "app_icon.png"
                };

                foreach (string file in filesToCopy)
                {
                    string srcFile = Path.Combine(sourceDir, file);
                    string destFile = Path.Combine(targetDir, file);

                    if (File.Exists(srcFile))
                    {
                        File.Copy(srcFile, destFile, true);
                    }
                }

                prgBar.Value = 70;
                lblStatus.Text = "Kısayollar oluşturuluyor...";
                Application.DoEvents();

                string exePath = Path.Combine(targetDir, "AracTakipYonetici.exe");
                string iconPath = Path.Combine(targetDir, "app_icon.ico");

                // Masaüstü Kısayolu
                if (chkDesktopShortcut.Checked && File.Exists(exePath))
                {
                    string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    string shortcutPath = Path.Combine(desktopDir, "Araç Takip Yönetici Paneli.lnk");
                    CreateShortcut(shortcutPath, exePath, targetDir, "Araç Takip & Yönetim Masası", iconPath);
                }

                // Başlat Menüsü Kısayolu
                try
                {
                    string startMenu = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
                    string appStartMenuDir = Path.Combine(startMenu, "Araç Takip Sistemi");
                    if (!Directory.Exists(appStartMenuDir)) Directory.CreateDirectory(appStartMenuDir);

                    string smShortcut = Path.Combine(appStartMenuDir, "Araç Takip Yönetici Paneli.lnk");
                    CreateShortcut(smShortcut, exePath, targetDir, "Araç Takip & Yönetim Masası", iconPath);
                }
                catch { }

                prgBar.Value = 100;
                lblStatus.Text = "✅ Kurulum Başarıyla Tamamlandı!";
                btnInstall.Text = "Tamam";
                btnInstall.Enabled = true;
                btnInstall.Click -= BtnInstall_Click;
                btnInstall.Click += (s, ev) => this.Close();

                MessageBox.Show(
                    "Araç Takip Sistemi başarıyla kuruldu!\n\n" +
                    "Masaüstünüze 'Araç Takip Yönetici Paneli' kısayolu eklendi.",
                    "Kurulum Tamamlandı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                if (chkLaunch.Checked && File.Exists(exePath))
                {
                    Process.Start(new ProcessStartInfo(exePath) { WorkingDirectory = targetDir });
                }

                this.Close();
            }
            catch (Exception ex)
            {
                prgBar.Visible = false;
                lblStatus.Text = "Hata oluştu!";
                btnInstall.Enabled = true;
                MessageBox.Show("Kurulum sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string workingDir, string description, string iconPath = null)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;

                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = workingDir;
                shortcut.Description = description;
                if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
                {
                    shortcut.IconLocation = iconPath + ",0";
                }
                else
                {
                    shortcut.IconLocation = targetPath + ",0";
                }
                shortcut.Save();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Kısayol oluşturma hatası: " + ex.Message);
            }
        }
    }
}
