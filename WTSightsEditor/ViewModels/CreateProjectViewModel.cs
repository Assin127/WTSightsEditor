using System;
using System.Windows.Input;
using WTSightsEditor.Models;
using WTSightsEditor.Services;

namespace WTSightsEditor.ViewModels
{
    /// <summary>
    /// ViewModel for CreateProjectWindow
    /// </summary>
    public class CreateProjectViewModel : ViewModelBase
    {
        private readonly ProjectService _projectService;
        private readonly LocalizationService _localizationService;

        private ProjectSettings _projectSettings = new();
        private bool _isCreatingNewProject = true;
        private string? _selectedProjectToOpen;

        public ProjectSettings ProjectSettings
        {
            get => _projectSettings;
            set => SetProperty(ref _projectSettings, value);
        }

        public bool IsCreatingNewProject
        {
            get => _isCreatingNewProject;
            set => SetProperty(ref _isCreatingNewProject, value);
        }

        public string? SelectedProjectToOpen
        {
            get => _selectedProjectToOpen;
            set => SetProperty(ref _selectedProjectToOpen, value);
        }

        public ICommand CreateProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand BrowseGameFolderCommand { get; }
        public ICommand BrowseDatamineFolderCommand { get; }
        public ICommand BrowseDataFolderCommand { get; }
        public ICommand BrowseBallisticFolderCommand { get; }
        public ICommand ToggleModeCommand { get; }

        public CreateProjectViewModel()
        {
            _projectService = ProjectService.Instance;
            _localizationService = LocalizationService.Instance;

            CreateProjectCommand = new RelayCommand(CreateProject, CanCreateProject);
            OpenProjectCommand = new RelayCommand(OpenProject, CanOpenProject);
            BrowseGameFolderCommand = new RelayCommand(BrowseGameFolder);
            BrowseDatamineFolderCommand = new RelayCommand(BrowseDatamineFolder);
            BrowseDataFolderCommand = new RelayCommand(BrowseDataFolder);
            BrowseBallisticFolderCommand = new RelayCommand(BrowseBallisticFolder);
            ToggleModeCommand = new RelayCommand(ToggleMode);
        }

        private void CreateProject(object? parameter)
        {
            if (!ProjectSettings.Validate(out var errorMessage))
            {
                // Show error
                return;
            }

            if (_projectService.CreateProject(ProjectSettings, out errorMessage))
            {
                // Close window with success
            }
            else
            {
                // Show error
            }
        }

        private bool CanCreateProject(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(ProjectSettings.Name) &&
                   !string.IsNullOrWhiteSpace(ProjectSettings.GameFolder);
        }

        private void OpenProject(object? parameter)
        {
            if (!string.IsNullOrWhiteSpace(SelectedProjectToOpen))
            {
                if (_projectService.LoadProject(SelectedProjectToOpen, out var errorMessage))
                {
                    // Close window with success
                }
                else
                {
                    // Show error
                }
            }
        }

        private bool CanOpenProject(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(SelectedProjectToOpen);
        }

        private void BrowseGameFolder(object? parameter)
        {
            // TODO: Implement folder browser dialog
        }

        private void BrowseDatamineFolder(object? parameter)
        {
            // TODO: Implement folder browser dialog
        }

        private void BrowseDataFolder(object? parameter)
        {
            // TODO: Implement folder browser dialog
        }

        private void BrowseBallisticFolder(object? parameter)
        {
            // TODO: Implement folder browser dialog
        }

        private void ToggleMode(object? parameter)
        {
            IsCreatingNewProject = !IsCreatingNewProject;
        }
    }
}