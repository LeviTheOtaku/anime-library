using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anime_Library
{
    public partial class AnimeLibrary : Form
    {
        string selectedAnime = "";
        string selectedEpisode = "0";
        public AnimeLibrary()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            flowLayoutPanel1.Controls.Clear();
            using (WebClient client = new WebClient())
            {
                string htmlCode = client.DownloadString("https://ajax.apimovie.xyz/site/loadAjaxSearch?keyword=" + textBox1.Text);

                string[] words = htmlCode.Split(' ');
                ToolTip current = null;

                foreach (var word in words)
                {
                    if (word.Contains("category"))
                    {
                        try
                        {
                            int pFrom = word.IndexOf("href=\\\"category\\/") + "href=\\\"category\\/".Length;
                            int pTo = word.LastIndexOf("\\\"");

                            string result = word.Substring(pFrom, pTo - pFrom);
                            result = result.Replace("-", " ");
                            result = UpperCaseFirstLetter(result);


                            ToolTip toolTip = new ToolTip();
                            toolTip.InitialDelay = 150;
                            toolTip.ReshowDelay = 500;
                            toolTip.ShowAlways = true;
                            toolTip.Tag = result;
                            current = toolTip;
                        }
                        catch { }
                    }

                    if (word.Contains("cdnimg.xyz"))
                    {
                        try
                        {
                            string animePicture;
                            animePicture = word.Replace("\\", "");
                            int first = animePicture.IndexOf("\"") + 1;
                            int second = animePicture.IndexOf(")") - 6;
                            string result = animePicture.Substring(first, second);

                            PictureBox imageControl = new PictureBox();
                            Size size = new Size(100, 150);
                            imageControl.MinimumSize = size;
                            imageControl.MaximumSize = size;
                            imageControl.SizeMode = PictureBoxSizeMode.StretchImage;
                            imageControl.Load(result);
                            flowLayoutPanel1.Controls.Add(imageControl);
                            current.SetToolTip(imageControl, current.Tag.ToString());
                            string animeName = current.Tag.ToString();

                            imageControl.MouseClick += new MouseEventHandler((s, e1) =>
                            {
                                selectedAnimeLabel.Text = "Anime: " + animeName;
                                selectedAnime = animeName;

                                string animeCode = animeName;
                                animeCode = animeCode.Replace(" ", "-");
                                string htmlCode2 = client.DownloadString("https://www.gogoanime.io/category/" + animeCode);

                                string[] newWords = htmlCode2.Split(' ');

                                Clipboard.SetText(htmlCode2);
                                int times = 0;
                                foreach (var newWord in newWords)
                                {
                                    times++;
                                    if (newWord.Contains("ep_end"))
                                    {
                                        string variable = newWords[times + 1];
                                        int first2 = variable.IndexOf("'") + 1;
                                        int second2 = variable.IndexOf("'", first2) - 1;
                                        string result2 = variable.Substring(first2, second2);

                                        int epCount = int.Parse(result2);

                                        episodeComboBox.Items.Clear();
                                        for (int ep = 1; ep < epCount+1; ep++)
                                        {
                                            episodeComboBox.Items.Add(ep.ToString());
                                            episodeComboBox.SelectedIndex = 0;
                                        }

                                        break;
                                    }
                                }


                            });
                        }
                        catch { }
                    }

                }
            }
        }


        public string UpperCaseFirstLetter(string YourLowerCaseWord)
        {
            if (string.IsNullOrEmpty(YourLowerCaseWord))
                return string.Empty;

            return char.ToUpper(YourLowerCaseWord[0]) + YourLowerCaseWord.Substring(1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                string animeCode = selectedAnime.Replace(" ", "-");
                string newPage = client.DownloadString("https://www.gogoanime.io/" + animeCode + "-episode-" + selectedEpisode);
                var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (Match m in linkParser.Matches(newPage))
                    if (m.Value.Contains("vidstreaming.io"))
                    {
                        string url = m.Value.Replace("download", "streaming.php");
                        int index = url.IndexOf("=&typesub");
                        if (index > 0)
                            url = url.Substring(0, index);

                        System.Diagnostics.Process.Start(url);
                    }

            }
        }

        private void episodeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedEpisode = episodeComboBox.SelectedItem.ToString();
        }
    }
}
