using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WTSightsEditor
{
    public partial class FormLayers : Form
    {
        public FormLayers()
        {
            InitializeComponent();
        }

        private void FormLayers_MouseLeave(object sender, EventArgs e)
        {
            Opacity = Properties.Settings.Default.Opacity;
        }

        private void FormLayers_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1;
        }
    }
}
