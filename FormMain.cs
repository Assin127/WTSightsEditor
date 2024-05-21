using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace WTSightsEditor
{
    public partial class FormMain : Form
    {

        private Bitmap workSpace;
        private Graphics graphics;

        private float scale = 0.5f; // Текущий масштаб

        // Перемещение изображения
        private int offsetX = 0;
        private int offsetY = 0;

        public FormMain()
        {
            InitializeComponent();
            InitializeWorkSpace();
            pictureBoxWorkSpace.Paint += PictureBoxWorkSpace_Paint; // Отрисовка

            // Изменение размеров
            pictureBoxWorkSpace.Resize += PictureBoxWorkSpace_Resize;

            // Масштабирование
            zoomInToolStripMenuItem.Click += ZoomInToolStripMenuItem_Click;
            zoomOutToolStripMenuItem.Click += ZoomOutToolStripMenuItem_Click;
            zoomToWindowToolStripMenuItem.Click += ZoomToWindowToolStripMenuItem_Click;

            // Перемещение изображения
            hScrollBar.Scroll += HScrollBar_Scroll;
            vScrollBar.Scroll += VScrollBar_Scroll;
            pictureBoxWorkSpace.MouseWheel += PictureBoxWorkSpace_MouseWheel;
            pictureBoxWorkSpace.Resize += PictureBoxWorkSpace_Resize;

            hScrollBar.Minimum = -workSpace.Width;
            hScrollBar.Maximum = workSpace.Width;
            hScrollBar.Value = workSpace.Width / 2;
            hScrollBar.Minimum = 0;
            hScrollBar.SmallChange = 10;
            hScrollBar.LargeChange = 50;

            vScrollBar.Minimum = -workSpace.Height;
            vScrollBar.Maximum = workSpace.Height;
            vScrollBar.Value = workSpace.Height / 2;
            vScrollBar.Minimum = 0;
            vScrollBar.SmallChange = 10;
            vScrollBar.LargeChange = 50;
        }

        /// <summary>
        /// Создаёт графику
        /// </summary>
        private void InitializeWorkSpace()
        {
            workSpace = new Bitmap(1920, 1080);
            graphics = Graphics.FromImage(workSpace);
            graphics.Clear(Color.White);
        }

        /// <summary>
        /// Отрисовка графики
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_Paint(object sender, PaintEventArgs e)
        {
            if (workSpace != null)
            {
                // Вычисляем координаты для центрирования изображения с учетом смещения
                int x = (pictureBoxWorkSpace.ClientSize.Width - (int)(workSpace.Width * scale)) / 2 + offsetX;
                int y = (pictureBoxWorkSpace.ClientSize.Height - (int)(workSpace.Height * scale)) / 2 + offsetY;
                e.Graphics.DrawImage(workSpace, x, y, workSpace.Width * scale, workSpace.Height * scale);
            }
        }

        /// <summary>
        /// Перерисовывает pictureBoxWorkSpace при изменении размера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_Resize(object sender, EventArgs e)
        {
            // Обновляем размеры и положение полос прокрутки
            hScrollBar.Width = pictureBoxWorkSpace.ClientSize.Width - vScrollBar.Width;
            hScrollBar.Top = pictureBoxWorkSpace.ClientSize.Height - hScrollBar.Height;
            hScrollBar.Left = 0;

            vScrollBar.Height = pictureBoxWorkSpace.ClientSize.Height - hScrollBar.Height - menuStrip1.Height;
            vScrollBar.Left = pictureBoxWorkSpace.ClientSize.Width - vScrollBar.Width;
            vScrollBar.Top = menuStrip1.Height;

            // Обновляем изображение
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перерисовывает pictureBoxWorkSpace
        /// </summary>
        private void UpdateWorkSpace()
        {
            pictureBoxWorkSpace.Invalidate();
        }

        /// <summary>
        /// Увеличиваем масштаб на 10%
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomInToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeScale(1.1f);
        }

        /// <summary>
        /// Уменьшаем масштаб на 10%
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ChangeScale(1 / 1.1f);
        }

        /// <summary>
        /// Изменяем масштаб, не выходя за пределы от 1% до 1000%
        /// </summary>
        /// <param name="factor"></param>
        private void ChangeScale(float factor)
        {
            scale = Math.Max(0.01f, Math.Min(10.0f, scale * factor));
            UpdateWorkSpace();
        }

        /// <summary>
        /// Центрирование изображения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomToWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            offsetX = 0;
            offsetY = 0;
            hScrollBar.Value = workSpace.Width / 2 - offsetX;
            vScrollBar.Value = workSpace.Height / 2 - offsetY;
            scale = 0.5f;
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перемещение горизонтально при прокрутке колеса мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetX = -hScrollBar.Value + workSpace.Width / 2;
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перемещение вертикально при прокрутке колеса мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetY = -vScrollBar.Value + workSpace.Height / 2;
            UpdateWorkSpace();
        }

        /// <summary>
        /// Прокрутка колеса мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Shift)
            {
                // Прокрутка по горизонтали
                hScrollBar.Value = Math.Max(hScrollBar.Minimum, Math.Min(hScrollBar.Maximum, hScrollBar.Value - e.Delta));
                offsetX = -hScrollBar.Value + workSpace.Width / 2;
            }
            else
            {
                // Прокрутка по вертикали
                vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum, vScrollBar.Value - e.Delta));
                offsetY = -vScrollBar.Value + workSpace.Height / 2;
            }
            UpdateWorkSpace();
        }
    }
}
