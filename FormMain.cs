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

        // Текущий масштаб
        private float scale = 0.5f;

        // Перемещение изображения
        private int offsetX = 0;
        private int offsetY = 0;

        public FormMain()
        {
            InitializeComponent();
            InitializeWorkSpace();

            // Отрисовка
            pictureBoxWorkSpace.Paint += PictureBoxWorkSpace_Paint;

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
                int x = (pictureBoxWorkSpace.ClientSize.Width - (int)(workSpace.Width * scale)) / 2 + offsetX;
                int y = (pictureBoxWorkSpace.ClientSize.Height - (int)(workSpace.Height * scale)) / 2 + offsetY;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
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

            // Обновляем полосы прокрутки
            UpdateScrollBars();

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
            scale = 0.5f;
            UpdateScrollBars();
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перемещение горизонтально при прокрутке колеса мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetX = -hScrollBar.Value;
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перемещение вертикально при прокрутке колеса мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetY = -vScrollBar.Value;
            UpdateWorkSpace();
        }

        /// <summary>
        /// Прокрутка колеса мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_MouseWheel(object sender, MouseEventArgs e)
        {
            if (ModifierKeys == Keys.Control)
            {
                // Рассчитываем новое значение масштаба
                float newScale = scale * (e.Delta > 0 ? 1.1f : 0.9f);
                newScale = Math.Max(0.01f, Math.Min(10.0f, newScale));

                // Определяем положение курсора относительно изображения
                float relativeX = (e.X - offsetX - pictureBoxWorkSpace.ClientSize.Width / 2f + workSpace.Width * scale / 2f) / scale;
                float relativeY = (e.Y - offsetY - pictureBoxWorkSpace.ClientSize.Height / 2f + workSpace.Height * scale / 2f) / scale;

                // Обновляем смещение, чтобы центрировать приближение на курсоре
                offsetX = Math.Min((int)(workSpace.Width * scale), Math.Max((int)(-workSpace.Width * scale), e.X - (int)(relativeX * newScale) - pictureBoxWorkSpace.ClientSize.Width / 2 + (int)(workSpace.Width * newScale / 2)));
                offsetY = Math.Min((int)(workSpace.Height * scale), Math.Max((int)(-workSpace.Height * scale), e.Y - (int)(relativeY * newScale) - pictureBoxWorkSpace.ClientSize.Height / 2 + (int)(workSpace.Height * newScale / 2)));

                // Обновляем масштаб
                scale = newScale;

                // Обновляем полосы прокрутки
                UpdateScrollBars();

                // Обновляем изображение
                UpdateWorkSpace();
            }
            else if (ModifierKeys == Keys.Shift)
            {
                // Прокрутка по горизонтали
                hScrollBar.Value = Math.Max(hScrollBar.Minimum, Math.Min(hScrollBar.Maximum, hScrollBar.Value - e.Delta));
                offsetX = -hScrollBar.Value;
            }
            else
            {
                // Прокрутка по вертикали
                vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum, vScrollBar.Value - e.Delta));
                offsetY = -vScrollBar.Value;
            }
            UpdateWorkSpace();
        }

        /// <summary>
        /// Обновление скроллбаров в зависимости от масштаба изображения
        /// </summary>
        private void UpdateScrollBars()
        {
            hScrollBar.Minimum = Math.Min(0, (int)(-workSpace.Width * scale));
            hScrollBar.Maximum = Math.Max(0, (int)(workSpace.Width * scale));
            hScrollBar.LargeChange = (hScrollBar.Maximum - hScrollBar.Minimum) / 10;

            vScrollBar.Minimum = Math.Min(0, (int)(-workSpace.Height * scale));
            vScrollBar.Maximum = Math.Max(0, (int)(workSpace.Height * scale));
            vScrollBar.LargeChange = (vScrollBar.Maximum - vScrollBar.Minimum) / 10;

            hScrollBar.Value = Math.Max(hScrollBar.Minimum, Math.Min(hScrollBar.Maximum, -offsetX));
            vScrollBar.Value = Math.Max(vScrollBar.Minimum, Math.Min(vScrollBar.Maximum, -offsetY));
        }
    }
}
