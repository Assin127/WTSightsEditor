using System;
using System.Drawing;
using System.Windows.Forms;

namespace WTSightsEditor
{
    public partial class FormMain : Form
    {

        private Bitmap workSpace;
        private Graphics graphics;
        private bool mouseScroll = false;
        private bool middleMouseDown = false;
        private Point lastMousePosition;

        // Масштаб
        private float scale = Properties.Settings.Default.StartScale;

        // Перемещение изображения
        private int offsetX = 0;
        private int offsetY = 0;

        public FormMain()
        {
            InitializeComponent();
            InitializeWorkSpace();

            Width = Common.GetWidth() / 2;
            Height = Common.GetHeight() / 2;

            // Отрисовка
            pictureBoxWorkSpace.Resize += PictureBoxWorkSpace_Resize;
            pictureBoxWorkSpace.Paint += PictureBoxWorkSpace_Paint;

            // Масштабирование
            zoomInToolStripMenuItem.Click += ZoomInToolStripMenuItem_Click;
            zoomOutToolStripMenuItem.Click += ZoomOutToolStripMenuItem_Click;
            zoomToWindowToolStripMenuItem.Click += ZoomToWindowToolStripMenuItem_Click;

            // Перемещение изображения
            hScrollBar.Scroll += HScrollBar_Scroll;
            vScrollBar.Scroll += VScrollBar_Scroll;
            pictureBoxWorkSpace.MouseWheel += PictureBoxWorkSpace_MouseWheel;
            pictureBoxWorkSpace.MouseDown += PictureBoxWorkSpace_MouseDown;
            pictureBoxWorkSpace.MouseMove += PictureBoxWorkSpace_MouseMove;
            pictureBoxWorkSpace.MouseUp += PictureBoxWorkSpace_MouseUp;

            // Настройки scaleBar
            scaleBar.Minimum = -100; // 10% - минимальный масштаб
            scaleBar.Maximum = 100; // 1000% - максимальный масштаб
            scaleBar.Value = (int)(Math.Log10(scale) * 100); // начальное значение
            scaleBar.TickFrequency = 10; // частота отметок
            scaleBar.ValueChanged += ScaleBar_ValueChanged;

            // Настройки scaleLabel
            scaleLabel.Text = $"{Math.Round(scale * 100)}%";
        }

        /// <summary>
        /// Создаёт графику
        /// </summary>
        private void InitializeWorkSpace()
        {
            workSpace = new Bitmap(Common.GetWidth(), Common.GetHeight());
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
            hScrollBar.Top = pictureBoxWorkSpace.ClientSize.Height - hScrollBar.Height - buttonInterface.Height;
            hScrollBar.Left = 0;

            vScrollBar.Height = pictureBoxWorkSpace.ClientSize.Height - hScrollBar.Height - menuStrip1.Height - buttonInterface.Height;
            vScrollBar.Left = pictureBoxWorkSpace.ClientSize.Width - vScrollBar.Width;
            vScrollBar.Top = menuStrip1.Height;

            buttonInterface.Width = pictureBoxWorkSpace.ClientSize.Width;
            buttonInterface.Top = pictureBoxWorkSpace.ClientSize.Height - buttonInterface.Height;
            buttonInterface.Left = 0;
            corner.Top = pictureBoxWorkSpace.ClientSize.Height - corner.Height - buttonInterface.Height;
            corner.Left = pictureBoxWorkSpace.ClientSize.Width - corner.Width;

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
            // Рассчитываем новое значение масштаба
            float newScale = Math.Max(0.1f, Math.Min(10.0f, scale * Properties.Settings.Default.Zoom));
            scaleBar.Value = (int)(Math.Log10(newScale) * 100);
        }

        /// <summary>
        /// Уменьшаем масштаб на 10%
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ZoomOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Рассчитываем новое значение масштаба
            float newScale = Math.Max(0.1f, Math.Min(10.0f, scale / Properties.Settings.Default.Zoom));
            scaleBar.Value = (int)(Math.Log10(newScale) * 100);
        }

        /// <summary>
        /// Изменяем масштаб
        /// </summary>
        /// <param name="factor"></param>
        private void ChangeScale(float newScale, int x, int y)
        {
            if (mouseScroll)
            {
                x = MousePosition.X; y = MousePosition.Y;
            }
            // Определяем положение объекта относительно изображения
            float relativeX = (x - offsetX - pictureBoxWorkSpace.ClientSize.Width / 2f + workSpace.Width * scale / 2f) / scale;
            float relativeY = (y - offsetY - pictureBoxWorkSpace.ClientSize.Height / 2f + workSpace.Height * scale / 2f) / scale;

            // Обновляем масштаб
            scale = newScale;

            // Обновляем смещение, чтобы центрировать приближение
            offsetX = Math.Min((int)(workSpace.Width * scale), Math.Max((int)(-workSpace.Width * scale), x - (int)(relativeX * scale) - pictureBoxWorkSpace.ClientSize.Width / 2 + (int)(workSpace.Width * scale / 2)));
            offsetY = Math.Min((int)(workSpace.Height * scale), Math.Max((int)(-workSpace.Height * scale), y - (int)(relativeY * scale) - pictureBoxWorkSpace.ClientSize.Height / 2 + (int)(workSpace.Height * scale / 2)));

            scaleLabel.Text = $"{Math.Round(newScale * 100)}%";

            UpdateScrollBars();
            UpdateWorkSpace();
        }

        /// <summary>
        /// Изменение масштаба
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScaleBar_ValueChanged(object sender, EventArgs e)
        {
            float newScale = (float)Math.Pow(10, scaleBar.Value / 100f);
            ChangeScale(newScale, (int)(pictureBoxWorkSpace.ClientSize.Width / 2f), (int)(pictureBoxWorkSpace.ClientSize.Height / 2f));
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
            scale = Properties.Settings.Default.StartScale;
            scaleBar.Value = (int)(Math.Log10(scale) * 100);
            scaleLabel.Text = $"{Math.Round(scale * 100)}%";
            UpdateScrollBars();
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перемещение ScrollBar горизонтально
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetX = -hScrollBar.Value;
            UpdateWorkSpace();
        }

        /// <summary>
        /// Перемещение ScrollBar вертикально
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
                float newScale = scale * (e.Delta > 0 ? Properties.Settings.Default.Zoom : 1 / Properties.Settings.Default.Zoom);
                newScale = Math.Max(0.1f, Math.Min(10.0f, newScale));

                mouseScroll = true;
                scaleBar.Value = (int)(Math.Log10(newScale) * 100);
                mouseScroll = false;

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

        /// <summary>
        /// Зажатие кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                middleMouseDown = true;
                lastMousePosition.X = e.X;
                lastMousePosition.Y = e.Y;
                Cursor = Cursors.Hand;
            }
        }

        /// <summary>
        /// Перемещение изображения с помощью средней кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            if (middleMouseDown)
            {
                // Вычисляем смещение
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;

                // Обновляем смещение
                offsetX += deltaX;
                offsetY += deltaY;

                // Сохраняем текущую позицию мыши
                lastMousePosition = e.Location;

                // Обновляем изображение
                UpdateWorkSpace();
            }
        }

        /// <summary>
        /// Отпускание кнопки мыши
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBoxWorkSpace_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                middleMouseDown = false;
                Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Инициализация нового файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to create a new file? Unsaved changes will be lost.", "Confirm New File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                // Перезапуск приложения
                Application.Restart();
                Environment.Exit(0);
            }
        }
    }
}
