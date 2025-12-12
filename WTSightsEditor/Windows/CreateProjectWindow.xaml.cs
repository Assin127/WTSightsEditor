using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using WTSightsEditor.Models;
using WTSightsEditor.Services;
using Ookii.Dialogs.Wpf;

namespace WTSightsEditor.Windows
{
    /// <summary>
    /// Interaction logic for CreateProjectWindow.xaml
    /// </summary>
    public partial class CreateProjectWindow : Window
    {
        private readonly ProjectService _projectService;
        private string _defaultProjectsPath;

        public CreateProjectWindow()
        {
            InitializeComponent();
            _projectService = ProjectService.Instance;
            _defaultProjectsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Projects");

            InitializeWindow();
            LoadRecentProjects();
        }

        private void InitializeWindow()
        {
            // Set default paths
            DataFolderTextBox.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            BallisticFolderTextBox.Text = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ballistic");

            // Setup auto-update checkbox
            AutoUpdateDatamineCheckBox.IsChecked = true;
        }

        private void LoadRecentProjects()
        {
            var recentProjects = _projectService.GetRecentProjects(10);
            RecentProjectsList.ItemsSource = recentProjects;

            if (recentProjects.Count == 0)
            {
                NoRecentProjectsText.Visibility = Visibility.Visible;
            }
            else
            {
                NoRecentProjectsText.Visibility = Visibility.Collapsed;
            }
        }

        private void SwitchToProjectCreationView()
        {
            WelcomePanel.Visibility = Visibility.Collapsed;
            ProjectCreationPanel.Visibility = Visibility.Visible;
            WelcomeContent.Visibility = Visibility.Collapsed;
            ProjectCreationContent.Visibility = Visibility.Visible;
        }

        private void SwitchToWelcomeView()
        {
            WelcomePanel.Visibility = Visibility.Visible;
            ProjectCreationPanel.Visibility = Visibility.Collapsed;
            WelcomeContent.Visibility = Visibility.Visible;
            ProjectCreationContent.Visibility = Visibility.Collapsed;
        }

        private void ValidateProjectFields()
        {
            bool isValid = true;

            // Validate project name
            if (string.IsNullOrWhiteSpace(ProjectNameTextBox.Text))
            {
                ProjectNameError.Text = "Project name is required";
                ProjectNameError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                ProjectNameError.Visibility = Visibility.Collapsed;
            }

            // Validate game folder
            if (string.IsNullOrWhiteSpace(GameFolderTextBox.Text))
            {
                GameFolderError.Text = "Game folder is required";
                GameFolderError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else if (!Directory.Exists(GameFolderTextBox.Text))
            {
                GameFolderError.Text = "Game folder does not exist";
                GameFolderError.Visibility = Visibility.Visible;
                isValid = false;
            }
            else
            {
                GameFolderError.Visibility = Visibility.Collapsed;
            }

            CreateProjectButton.IsEnabled = isValid;
        }

        #region Event Handlers

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToProjectCreationView();
        }

        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Project files (*.json)|*.json",
                InitialDirectory = _defaultProjectsPath,
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var projectName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                if (_projectService.LoadProject(projectName, out var errorMessage))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RecentProject_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock && textBlock.DataContext is string projectName)
            {
                if (_projectService.LoadProject(projectName, out var errorMessage))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchToWelcomeView();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement settings window
            MessageBox.Show("Settings window will be implemented later", "Settings",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProjectNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateProjectFields();
        }

        private void BrowseGameFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select War Thunder game folder",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == true)
            {
                GameFolderTextBox.Text = dialog.SelectedPath;
                ValidateProjectFields();
            }
        }

        private void BrowseDatamineFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select datamine folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == true)
            {
                DatamineFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void BrowseDataFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select data folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == true)
            {
                DataFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void BrowseBallisticFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                Description = "Select ballistic data folder",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == true)
            {
                BallisticFolderTextBox.Text = dialog.SelectedPath;
            }
        }

        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            var settings = new ProjectSettings
            {
                Name = ProjectNameTextBox.Text.Trim(),
                GameFolder = GameFolderTextBox.Text,
                DatamineFolder = DatamineFolderTextBox.Text,
                DataFolder = DataFolderTextBox.Text,
                BallisticFolder = BallisticFolderTextBox.Text,
                AutoUpdateDatamine = AutoUpdateDatamineCheckBox.IsChecked ?? false,
                CreatedDate = DateTime.Now,
                LastModified = DateTime.Now
            };

            if (_projectService.CreateProject(settings, out var errorMessage))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}