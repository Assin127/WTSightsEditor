using System;
using System.IO;
using System.Windows.Forms;


namespace WTSightsEditor
{
    public partial class FormSightSettings : Form
    {
        public FormSightSettings()
        {
            InitializeComponent();
            comboBox2.Text = Convert.ToString(WTSights.GetWidth()) + "x" + Convert.ToString(WTSights.GetHeight());
            foreach (string file in subtratesList)
            {
                comboBox3.Items.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        public string[] subtratesList = Directory.GetFiles("Substrates", "*.jpg");

        private void FormSightSettings_MouseLeave(object sender, EventArgs e)
        {
            Opacity = Properties.Settings.Default.Opacity;
        }

        private void FormSightSettings_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            FormMain main = this.Owner as FormMain;
            if (main != null)
            {
                if (checkBox3.Checked)
                {
                    main.drawCenter = true;
                    main.DrawCenter();
                }
                else
                {
                    main.drawCenter = false;
                    main.DrawCenter();
                }
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            FormMain main = this.Owner as FormMain;
            if (comboBox3 != null)
            {
                main.substrateChanged = true;
                main.substrateFile = subtratesList[comboBox3.SelectedIndex];
                main.SubstrateFile();
            }
            else
            {
                main.substrateChanged = false;
                main.SubstrateFile();
            }
        }
    }
}
