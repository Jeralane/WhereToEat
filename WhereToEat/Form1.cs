using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WhereToEat
{
    public partial class Form1 : Form
    {
        private int _counter;

        public Form1()
        {
            InitializeComponent();

            InitializeComboItems();
            KeyDown += (sender, e) =>
            {
                if (e.Control && e.Shift && e.KeyCode == Keys.X)
                {
                    if (MessageBox.Show(@"Gusto mo bang balikan ang mga nakainan mo na?", @"Teka!", MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Reset();
                    }
                }
            };

            btnSelect.Click += (s, e) =>
            {
                if (!ValidateFields())
                {
                    return;
                }
                Animate();
                btnSelect.Enabled = false;
            };

            timer1.Tick += (sender, args) =>
            {
                _counter++;
                var file = RandomPick(AllImage());
                pictureBox1.Image = new Bitmap(file);
                if (_counter >= 30)
                {
                    timer1.Stop();
                    btnSelect.Enabled = true;
                    HungerPick();
                }
            };
        }

        private void HungerPick()
        {
            var rootFolder = AppDomain.CurrentDomain.BaseDirectory;
            var type = cboType.SelectedItem;

            if (type == null)
            {
                return;
            }

            var imagesFolder = Path.Combine(rootFolder, type.ToString());

            if (!Directory.Exists(imagesFolder)) return;

            var filesFromImagesFolder = Directory.GetFiles(imagesFolder, "*.*", SearchOption.AllDirectories);
            if (!filesFromImagesFolder.Any()) return;

            var imageFiles = filesFromImagesFolder.Where(file => Regex.IsMatch(file, @".jpg|.png|.gif$")).ToList();
            if (!imageFiles.Any()) return;

            var previousFolder = Path.Combine(rootFolder, "Previous");
            var previousCards = Directory.GetFiles(previousFolder, "*.*", SearchOption.AllDirectories);

            if (previousCards.Count() == imageFiles.Count())
            {
                Reset();
                previousCards = new string[] {};
            }

            var fileNames = from x in previousCards
                select Path.GetFileName(x);

            var availableCards = (from imageFile in imageFiles
                let imageName = Path.GetFileName(imageFile)
                where !fileNames.Contains(imageName)
                select imageFile).ToList();

            var shuffledcards = availableCards.OrderBy(a => Guid.NewGuid()).ToList();

            if (!shuffledcards.Any())
            {
                Reset();
                HungerPick();
                return;
            }

            var rnd = new Random();
            var index = rnd.Next(1, shuffledcards.Count);
            var selectedImage = shuffledcards[index - 1];
            if (selectedImage == null) return;

            var duplicate = Path.Combine(previousFolder, Path.GetFileName(selectedImage));
            File.Copy(selectedImage, duplicate);
            pictureBox1.Image = new Bitmap(selectedImage);

            GreetingsFor(Path.GetFileNameWithoutExtension(selectedImage));
        }

        private void Reset()
        {
            var rootFolder = AppDomain.CurrentDomain.BaseDirectory;
            var previousFolder = Path.Combine(rootFolder, "Previous");
            var di = new DirectoryInfo(previousFolder);

            foreach (var file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (var dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void InitializeComboItems()
        {
            cboType.Items.Clear();
            foreach (var type in RestaurantTypes())
            {
                    cboType.Items.Add(type);
            }
        }

        private List<string> RestaurantTypes()
        {
            var rootFolder = AppDomain.CurrentDomain.BaseDirectory;
            var folders = Directory.GetDirectories(rootFolder);
            return folders.Select(Path.GetFileName).Where(x => x != "Previous").ToList();
        } 

        private void Animate()
        {
            _counter = 0;
            btnSelect.Enabled = false;
            timer1.Start();
        }

        private List<string> AllImage()
        {
            var imageFiles = new List<string>();
            var rootFolder = AppDomain.CurrentDomain.BaseDirectory;
            var types = RestaurantTypes();
            foreach (var type in types)
            {
                var imagesFolder = Path.Combine(rootFolder, type);

                if (!Directory.Exists(imagesFolder)) continue;

                var filesFromImagesFolder = Directory.GetFiles(imagesFolder, "*.*", SearchOption.AllDirectories);
                if (!filesFromImagesFolder.Any())
                    continue;
                imageFiles.AddRange(
                    filesFromImagesFolder.Where(file => Regex.IsMatch(file, @".jpg|.png|.gif$")).ToList());
            }
            return imageFiles;
        }

        private string RandomPick(List<string> shuffledcards)
        {
            var rnd = new Random();
            var index = rnd.Next(1, shuffledcards.Count);
            return shuffledcards[index - 1];
        }

        private bool ValidateFields()
        {
            var type = cboType.SelectedItem;
            if (type == null)
            {
                MessageBox.Show(@"Mamili ka muna kung ano ang tipo ng kainan na gusto mong puntahan! Excited?", @"Teka!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            return true;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            label2.Text = LocalDateTime(DateTime.Now);
        }

        private List<string> Expressions()
        {
            return new List<string>
            {
                "matutuwa",
                "masisiyahan",
                "maiiyak",
                "malulungkot",
                "maiinis",
                "mababaliw",
                "mabubusog",
                "lalong magugutom",
                "matatawa",
                "mababaliw",
                "masasarapan",
                "maaaliw",
                "malilibang",
                "makakatagpo ng swerte",
                "maglalabas ng sikreto",
                "maglalabas ng sama ng loob",
                "manlilibre",
                "mag-eextra rice",
                "makakahanap ng pag-ibig",
            };
        }

        private void GreetingsFor(string restaurant)
        {
            var message = string.Format("Ikaw ay {0} sa iyong pagkain sa {1}!", RandomPick(Expressions()), restaurant);
            lblFortuneMessage.Text = message;
        }

        private string LocalDateTime(DateTime dateTime)
        {
            var x = string.Format("{0:F}", dateTime);

            var buwan = MonthLookup()[dateTime.Month];
            var araw = DayLookup()[dateTime.DayOfWeek];

            var time = dateTime.ToString("hh:mm:ss tt").Replace("AM", "NU").Replace("PM", "NH");

            return string.Format("{0}, {1} {2}, {3} {4}", araw, buwan, dateTime.Day, dateTime.Year, time);
        }

        private Dictionary<int, string> MonthLookup()
        {
            var lookup = new Dictionary<int, string>
            {
                {1, "Enero"},
                {2, "Pebrero"},
                {3, "Marso"},
                {4, "Abril"},
                {5, "Mayo"},
                {6, "Hunyo"},
                {7, "Hulyo"},
                {8, "Agosto"},
                {9, "Setyembre"},
                {10, "Oktubre"},
                {11, "Nobyembre"},
                {12, "Disyembre"}
            };

            return lookup;
        }

        private Dictionary<DayOfWeek, string> DayLookup()
        {
            var lookup = new Dictionary<DayOfWeek, string>();
            lookup.Add(DayOfWeek.Monday, "Lunes");
            lookup.Add(DayOfWeek.Tuesday, "Martes");
            lookup.Add(DayOfWeek.Wednesday, "Miyerkules");
            lookup.Add(DayOfWeek.Thursday, "Huwebes");
            lookup.Add(DayOfWeek.Friday, "Biyernes");
            lookup.Add(DayOfWeek.Saturday, "Sabado");
            lookup.Add(DayOfWeek.Sunday, "Linggo");
            return lookup;
        }
    }
}