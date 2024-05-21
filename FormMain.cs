using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace WTSightsEditor
{
    public partial class FormMain : Form
    {
        static Bitmap bitmap_workSpace = null;
        static Graphics graphics_workSpace = null;

        static Bitmap bitmap_image = null;
        static Graphics graphics_image = null;

        public FormMain()
        {
            InitializeComponent();
            this.MouseWheel += new MouseEventHandler(this_MouseWheel);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Width = WTSights.GetWidth();
            Height = WTSights.GetHeight();

            mainCords[0] = this.Top; mainCords[1] = this.Bottom; mainCords[2] = this.Left; mainCords[3] = this.Right;

            sightSettings.Owner = this;
            sightSettingsToolStripMenuItem.Checked = true;
            sightSettings.Top = this.Bottom - sightSettings.Height - Properties.Settings.Default.InterfaceSpacing; 
            sightSettings.Left = this.Left + Properties.Settings.Default.InterfaceSpacing;
            sightSettings.Show();
            sightSettings.Opacity = Properties.Settings.Default.Opacity;

            toolBar.Owner = this;
            toolbarToolStripMenuItem.Checked = true;
            toolBar.Top = this.Top + menuStrip1.Height + Properties.Settings.Default.InterfaceSpacing + 30; 
            toolBar.Left = this.Left + Properties.Settings.Default.InterfaceSpacing;
            toolBar.Show();
            toolBar.Opacity = Properties.Settings.Default.Opacity;

            layers.Owner = this;
            layersVisibleToolStripMenuItem.Checked = true;
            layers.Top = this.Bottom - layers.Height - Properties.Settings.Default.InterfaceSpacing; 
            layers.Left = this.Right - layers.Width - Properties.Settings.Default.InterfaceSpacing;
            layers.Show();
            layers.Opacity = Properties.Settings.Default.Opacity;

            datamineConversation.Owner = this;

            bitmap_workSpace = new Bitmap(workSpace.Width, workSpace.Height);
            graphics_workSpace = Graphics.FromImage(bitmap_workSpace);
            workSpace.BackgroundImage= bitmap_workSpace;
            workSpace.SizeMode = PictureBoxSizeMode.CenterImage;
            ResizeImage(Convert.ToInt16(WTSights.GetWidth() * ImageScale), Convert.ToInt16(WTSights.GetHeight() * ImageScale));

            bitmap_image = new Bitmap(Convert.ToInt32(WTSights.GetWidth() * ImageScale), Convert.ToInt32(WTSights.GetHeight() * ImageScale));
            graphics_image = Graphics.FromImage(bitmap_image);

            image.Visible = true;
        }

        double ImageScale = 0.8;

        public void ResizeImage(int Width, int Height)
        {
            image.Size = new Size(Width, Height);
            image.Location = new Point(workSpace.Location.X + workSpace.Width / 2 - image.Width / 2, workSpace.Location.Y + workSpace.Height / 2 - image.Height / 2);
        }

        public bool drawCenter = false;
        public bool dravVignette = false;
        public bool substrateChanged = false;
        public string substrateFile = string.Empty;

        public void SubstrateFile()
        {
            if (substrateChanged)
            {
                bitmap_image = new Bitmap(substrateFile);
            }
            else
            {
                bitmap_image = new Bitmap(Convert.ToInt32(WTSights.GetWidth() * ImageScale), Convert.ToInt32(WTSights.GetHeight() * ImageScale));
            }
            graphics_image = Graphics.FromImage(bitmap_image);
            DrawImage();
        }

        public void DrawCenter()
        {
            if (drawCenter)
            {
                Color color = Color.FromArgb(100, Color.Black);
                graphics_image.DrawLine(new Pen(color, 2), 0, bitmap_image.Height / 2, bitmap_image.Width, bitmap_image.Height / 2);
                graphics_image.DrawLine(new Pen(color, 2), bitmap_image.Width / 2, 0, bitmap_image.Width / 2, bitmap_image.Height);
            }
            else
            {
                graphics_image.Clear(Color.White);
                SubstrateFile();
            }
            DrawImage();
        }

        public void DrawImage()
        {
            workSpace.CreateGraphics().Clear(Color.Silver);
            image.BackgroundImage = bitmap_image;
            image.Invalidate();
        }

        int[] mainCords = {0,0,0,0};
        public FormSightSettings sightSettings = new FormSightSettings();
        public FormToolbar toolBar = new FormToolbar();
        public FormLayers layers = new FormLayers();
        public FormDatamineConversion datamineConversation = new FormDatamineConversion();
        private int RefreshPosOfElements ()
        {
            toolBar.Top -= mainCords[0] - this.Top; toolBar.Left -= mainCords[2] - this.Left;
            sightSettings.Top -= mainCords[1] - this.Bottom; sightSettings.Left -= mainCords[2] - this.Left;
            layers.Top -= mainCords[1] - this.Bottom; layers.Left -= mainCords[3] - this.Right;
            mainCords[0] = this.Top;
            mainCords[1] = this.Bottom;
            mainCords[2] = this.Left;
            mainCords[3] = this.Right;
            return 1;
        }

        private void Main_SizeChanged(object sender, EventArgs e)
        {
            RefreshPosOfElements();
        }
        private void Main_LocationChanged(object sender, EventArgs e)
        {
            RefreshPosOfElements();
        }

        private string ImportFolder = null;

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1 = new OpenFileDialog();
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImportFolder = openFileDialog1.FileName;
            }
        }

        private void sightSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(sightSettings.Visible)
            {
                sightSettings.Hide();
            }
            else { 
                sightSettings.Show();
                this.Activate();
            }
        }

        private void toolbarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(toolBar.Visible)
            {
                toolBar.Hide();
            }
            else { 
                toolBar.Show(); 
                this.Activate(); 
            }
        }

        private void layersVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (layers.Visible)
            {
                layers.Hide();
            }
            else
            {
                layers.Show();
                this.Activate();
            }
        }

        private void datamainConversionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            datamineConversation.ShowDialog();
        }

        private void zoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image.Width < Width * 10)
            {
                double zoom = Properties.Settings.Default.Zoom;
                int widthDelta = (int)(image.Width * zoom) - image.Width;
                int heightDelta = (int)(image.Height * zoom) - image.Height;
                image.Width += widthDelta;
                image.Height += heightDelta;
                image.Location = new Point(
                    image.Location.X - Convert.ToInt32((double)(workSpace.Width / 2 - image.Location.X) / (double)image.Width * widthDelta),
                    image.Location.Y - Convert.ToInt32((double)(workSpace.Height / 2 - image.Location.Y) / (double)image.Height * heightDelta)
                    );
            }
        }

        private void zoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (image.Height > Height / 10)
            {
                double zoom = 1 / Properties.Settings.Default.Zoom;
                int widthDelta = (int)(image.Width * zoom) - image.Width;
                int heightDelta = (int)(image.Height * zoom) - image.Height;
                image.Width += widthDelta;
                image.Height += heightDelta;
                image.Location = new Point(
                    image.Location.X - Convert.ToInt32((double)(workSpace.Width / 2 - image.Location.X) / (double)image.Width * widthDelta),
                    image.Location.Y - Convert.ToInt32((double)(workSpace.Height / 2 - image.Location.Y) / (double)image.Height * heightDelta)
                    );
            }
        }

        private void image2_SizeChanged(object sender, EventArgs e)
        {
            DrawImage();
        }

        private void zoomToWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double ImageScale = 0.8;
            ResizeImage(Convert.ToInt16(WTSights.GetWidth() * ImageScale), Convert.ToInt16(WTSights.GetHeight() * ImageScale));
        }

        private Point previousLocation;
        private bool leftButton = false;

        private void image_MouseDown(object sender, MouseEventArgs e)
        {
            leftButton = true;
            previousLocation = new Point(e.X, e.Y);
        }

        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftButton)
            {
                Point p = PointToClient(MousePosition);
                image.Location = new Point(p.X - previousLocation.X, p.Y - previousLocation.Y);
                DrawImage();
            }
        }

        private void image_MouseUp(object sender, MouseEventArgs e)
        {
            leftButton = false;
        }
        void this_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (image.Width < Width * 10)
                {
                    double zoom = Properties.Settings.Default.Zoom;
                    int widthDelta = (int)(image.Width * zoom) - image.Width;
                    int heightDelta = (int)(image.Height * zoom) - image.Height;
                    image.Width += widthDelta;
                    image.Height += heightDelta;
                    image.Location = new Point(
                        image.Location.X - Convert.ToInt32((double)(e.X - image.Location.X) / (double)image.Width * widthDelta),
                        image.Location.Y - Convert.ToInt32((double)(e.Y - image.Location.Y) / (double)image.Height * heightDelta)
                        );
                }
            }
            else
            {
                if (image.Height > Height / 10)
                {
                    double zoom = 1 / Properties.Settings.Default.Zoom;
                    int widthDelta = (int)(image.Width * zoom) - image.Width;
                    int heightDelta = (int)(image.Height * zoom) - image.Height;
                    image.Width += widthDelta;
                    image.Height += heightDelta;
                    image.Location = new Point(
                        image.Location.X - Convert.ToInt32((double)(e.X - image.Location.X) / (double)image.Width * widthDelta),
                        image.Location.Y - Convert.ToInt32((double)(e.Y - image.Location.Y) / (double)image.Height * heightDelta)
                        );
                }
            }
        }
    }
}
