using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WTSightsEditor
{
    /// <summary>
    /// Main application window for WT Sights Editor
    /// </summary>
    public partial class MainWindow : Window
    {
        // ============================================================================
        // ENUMERATIONS
        // ============================================================================

        /// <summary>
        /// Types of available drawing tools
        /// </summary>
        public enum ToolType
        {
            None,
            Line,
            Circle,
            Rectangle,
            Text,
            Select,
            Move,
            Clone,
            Delete
        }

        // ============================================================================
        // WINAPI CONSTANTS AND METHODS (for screen resolution)
        // ============================================================================

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        private const int DESKTOPVERTRES = 117;  // Vertical desktop resolution
        private const int DESKTOPHORZRES = 118;  // Horizontal desktop resolution

        // ============================================================================
        // PRIVATE FIELDS
        // ============================================================================

        private ToolType _activeTool = ToolType.Select;   // Currently active drawing tool
        private Button? _lastActiveToolButton;            // Last active tool button reference
        private WriteableBitmap? _displayBitmap;          // Bitmap for displaying in canvas
        private const double DisplayScale = 0.3;          // 30% scale for display

        // ============================================================================
        // CONSTRUCTOR AND INITIALIZATION
        // ============================================================================

        /// <summary>
        /// MainWindow constructor - initializes all components
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitializeAllComponents();
        }

        /// <summary>
        /// Initialize all window components in correct order
        /// </summary>
        private void InitializeAllComponents()
        {
            InitializeStatusBar();
            InitializeToolManager();
            InitializeCanvas();
        }

        // ============================================================================
        // STATUS BAR METHODS
        // ============================================================================

        /// <summary>
        /// Initialize the status bar with screen resolution and default values
        /// </summary>
        private void InitializeStatusBar()
        {
            try
            {
                // Get physical screen resolution using WinAPI
                IntPtr hdc = GetDC(IntPtr.Zero);
                int physicalWidth = GetDeviceCaps(hdc, DESKTOPHORZRES);
                int physicalHeight = GetDeviceCaps(hdc, DESKTOPVERTRES);
                ReleaseDC(IntPtr.Zero, hdc);

                ResolutionText.Text = $"{physicalWidth}x{physicalHeight}";
                ZoomText.Text = "30%";
            }
            catch
            {
                // Fallback to WPF system parameters if WinAPI fails
                ResolutionText.Text = $"{SystemParameters.PrimaryScreenWidth}x{SystemParameters.PrimaryScreenHeight}";
                ZoomText.Text = "30%";
            }

            StatusText.Text = "Ready";
        }

        // ============================================================================
        // TOOL MANAGER METHODS
        // ============================================================================

        /// <summary>
        /// Initialize tool manager and set up tool button event handlers
        /// </summary>
        private void InitializeToolManager()
        {
            // Subscribe to all tool button click events
            ToolLine.Click += OnToolButtonClick;
            ToolCircle.Click += OnToolButtonClick;
            ToolRectangle.Click += OnToolButtonClick;
            ToolText.Click += OnToolButtonClick;
            ToolSelect.Click += OnToolButtonClick;
            ToolMove.Click += OnToolButtonClick;
            ToolClone.Click += OnToolButtonClick;
            ToolDelete.Click += OnToolButtonClick;

            // Set default active tool (Select)
            SetActiveTool(ToolSelect, ToolType.Select);
        }

        /// <summary>
        /// Handle tool button click events
        /// </summary>
        private void OnToolButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // If clicking already active tool (except Select), switch to Select tool
                if (_lastActiveToolButton == button && _activeTool != ToolType.Select)
                {
                    SetActiveTool(ToolSelect, ToolType.Select);
                }
                else
                {
                    // Activate the clicked tool
                    ToolType toolType = GetToolTypeFromButton(button);
                    SetActiveTool(button, toolType);
                }
            }
        }

        /// <summary>
        /// Determine tool type from button name
        /// </summary>
        private ToolType GetToolTypeFromButton(Button button)
        {
            return button.Name switch
            {
                "ToolLine" => ToolType.Line,
                "ToolCircle" => ToolType.Circle,
                "ToolRectangle" => ToolType.Rectangle,
                "ToolText" => ToolType.Text,
                "ToolSelect" => ToolType.Select,
                "ToolMove" => ToolType.Move,
                "ToolClone" => ToolType.Clone,
                "ToolDelete" => ToolType.Delete,
                _ => ToolType.None
            };
        }

        /// <summary>
        /// Set active tool and update button styles
        /// </summary>
        private void SetActiveTool(Button button, ToolType toolType)
        {
            // Reset style of previous active button
            if (_lastActiveToolButton != null)
            {
                _lastActiveToolButton.Style = (Style)FindResource("ToolBarButtonStyle");
            }

            // Set new active tool
            _activeTool = toolType;
            _lastActiveToolButton = button;

            // Apply active style to new button
            button.Style = (Style)FindResource("ActiveToolButtonStyle");
        }

        // ============================================================================
        // CANVAS METHODS
        // ============================================================================

        /// <summary>
        /// Initialize canvas with display bitmap
        /// </summary>
        private void InitializeCanvas()
        {
            try
            {
                // Get screen resolution from status bar
                string resolution = ResolutionText.Text;
                if (resolution.Contains("x"))
                {
                    var parts = resolution.Split('x');
                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int screenWidth) &&
                        int.TryParse(parts[1], out int screenHeight))
                    {
                        // Create bitmap with screen resolution scaled to 30%
                        int displayWidth = (int)(screenWidth * DisplayScale);
                        int displayHeight = (int)(screenHeight * DisplayScale);

                        _displayBitmap = new WriteableBitmap(
                            displayWidth,
                            displayHeight,
                            96, 96,
                            PixelFormats.Bgra32,
                            null);

                        // Create Image control to display the bitmap
                        var image = new Image
                        {
                            Source = _displayBitmap,
                            Stretch = Stretch.None
                        };

                        // Clear canvas and add the image
                        MainCanvas.Children.Clear();
                        MainCanvas.Children.Add(image);

                        // Update image position
                        UpdateCanvasPosition();

                        // Fill bitmap with test pattern
                        DrawTestPattern();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error initializing canvas: {ex.Message}";
            }
        }

        /// <summary>
        /// Update canvas position to center in container
        /// </summary>
        private void UpdateCanvasPosition()
        {
            if (MainCanvas.Children.Count > 0 && MainCanvas.Children[0] is Image image && _displayBitmap != null)
            {
                // Center the image in canvas
                double left = (MainCanvas.ActualWidth - _displayBitmap.PixelWidth) / 2;
                double top = (MainCanvas.ActualHeight - _displayBitmap.PixelHeight) / 2;

                Canvas.SetLeft(image, Math.Max(0, left));
                Canvas.SetTop(image, Math.Max(0, top));
            }
        }

        /// <summary>
        /// Draw test pattern on the bitmap
        /// </summary>
        private void DrawTestPattern()
        {
            if (_displayBitmap == null) return;

            try
            {
                int width = _displayBitmap.PixelWidth;
                int height = _displayBitmap.PixelHeight;
                int stride = width * 4; // 4 bytes per pixel for BGRA32

                byte[] pixels = new byte[height * stride];

                // Draw gradient test pattern
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int index = y * stride + x * 4;

                        // Calculate gradient colors
                        byte red = (byte)((double)x / width * 255);
                        byte green = (byte)((double)y / height * 255);
                        byte blue = (byte)(128);
                        byte alpha = 255;

                        pixels[index] = blue;     // B
                        pixels[index + 1] = green; // G
                        pixels[index + 2] = red;   // R
                        pixels[index + 3] = alpha; // A
                    }
                }

                // Write pixels to bitmap
                _displayBitmap.WritePixels(
                    new Int32Rect(0, 0, width, height),
                    pixels,
                    stride,
                    0);

                StatusText.Text = $"Canvas initialized: {width}x{height} at 30% scale";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error drawing pattern: {ex.Message}";
            }
        }

        /// <summary>
        /// Handle mouse down events on canvas
        /// </summary>
        private void PreviewCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(MainCanvas);

            switch (_activeTool)
            {
                case ToolType.Line:
                    StartDrawingLine(position);
                    break;
                case ToolType.Circle:
                    StartDrawingCircle(position);
                    break;
                case ToolType.Rectangle:
                    StartDrawingRectangle(position);
                    break;
                case ToolType.Text:
                    StartAddingText(position);
                    break;
                case ToolType.Select:
                    SelectElement(position);
                    break;
                case ToolType.Move:
                    StartMovingElement(position);
                    break;
                case ToolType.Clone:
                    CloneElement(position);
                    break;
                case ToolType.Delete:
                    DeleteElement(position);
                    break;
            }
        }

        // ============================================================================
        // DRAWING TOOL METHODS (Placeholders for future implementation)
        // ============================================================================

        private void StartDrawingLine(Point position)
        {
            StatusText.Text = $"Drawing line at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement line drawing logic
        }

        private void StartDrawingCircle(Point position)
        {
            StatusText.Text = $"Drawing circle at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement circle drawing logic
        }

        private void StartDrawingRectangle(Point position)
        {
            StatusText.Text = $"Drawing rectangle at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement rectangle drawing logic
        }

        private void StartAddingText(Point position)
        {
            StatusText.Text = $"Adding text at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement text addition logic
        }

        private void SelectElement(Point position)
        {
            StatusText.Text = $"Selecting element at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement element selection logic
        }

        private void StartMovingElement(Point position)
        {
            StatusText.Text = $"Moving element at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement element moving logic
        }

        private void CloneElement(Point position)
        {
            StatusText.Text = $"Cloning element at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement element cloning logic
        }

        private void DeleteElement(Point position)
        {
            StatusText.Text = $"Deleting element at ({position.X:F0}, {position.Y:F0})";
            // TODO: Implement element deletion logic
        }

        // ============================================================================
        // WINDOW EVENT HANDLERS
        // ============================================================================

        /// <summary>
        /// Handle window size changes
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update canvas position when window size changes
            UpdateCanvasPosition();
        }

        /// <summary>
        /// Handle window loaded event
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Additional initialization after window is fully loaded
            StatusText.Text = "Application ready";

            // Force canvas initialization after window is fully loaded
            Dispatcher.BeginInvoke(new Action(() =>
            {
                InitializeCanvas();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        // ============================================================================
        // PRIVATE FIELDS
        // ============================================================================

        private bool _isCustomEditorExpanded = true;      // Custom editor expanded state

        // ============================================================================
        // CUSTOM SIGHTS EDITOR METHODS
        // ============================================================================

        /// <summary>
        /// Toggle custom sights editor between expanded and collapsed states
        /// </summary>
        private void ToggleCustomSightsEditor()
        {
            if (CustomEditorRow.Height.Value > 0) // If expanded, collapse
            {
                // Collapse editor to minimum height (just header visible)
                CustomEditorRow.Height = new GridLength(0, GridUnitType.Pixel);
                StatusText.Text = "Custom Editor collapsed";
            }
            else // If collapsed, expand
            {
                // Expand editor to default height
                CustomEditorRow.Height = new GridLength(150, GridUnitType.Pixel);
                StatusText.Text = "Custom Editor expanded";
            }
        }

        /// <summary>
        /// Handle custom sights editor menu item click
        /// </summary>
        private void CustomSightsEditorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleCustomSightsEditor();
        }

        /// <summary>
        /// Handle click on custom editor header to toggle expansion
        /// </summary>
        private void CustomEditorHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                ToggleCustomSightsEditor();
            }
        }

        /// <summary>
        /// Handle splitter drag completed
        /// </summary>
        private void GridSplitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            // No icon to update since we removed it
        }

        /// <summary>
        /// Handle preview container size changes
        /// </summary>
        private void PreviewContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCanvasPosition();
        }
    }
}