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
    public partial class FormToolbar : Form
    {
        public FormToolbar()
        {
            InitializeComponent();
        }

        private void FormToolbar_MouseLeave(object sender, EventArgs e)
        {
            Opacity = Properties.Settings.Default.Opacity;
        }

        private void FormToolbar_MouseEnter(object sender, EventArgs e)
        {
            Opacity = 1;
        }
    }
}
