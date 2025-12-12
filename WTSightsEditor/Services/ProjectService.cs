using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WTSightsEditor.Models;

namespace WTSightsEditor.Services
{
    /// <summary>
    /// Service for managing project files
    /// </summary>
    public class ProjectService
    {
        private static ProjectService? _instance;
        public static ProjectService Instance => _instance ??= new ProjectService();

        private readonly string _projectsFolder;
        private ProjectSettings? _currentProject;

        public ProjectSettings? CurrentProject => _currentProject;

        public event Action<ProjectSettings>? ProjectLoaded;
        public event Action? ProjectClosed;

        private ProjectService()
        {
            // Create projects folder in application directory
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            _projectsFolder = Path.Combine(appPath, "Projects");

            if (!Directory.Exists(_projectsFolder))
            {
                Directory.CreateDirectory(_projectsFolder);
            }
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        public bool CreateProject(ProjectSettings settings, out string? errorMessage)
        {
            try
            {
                // Validate settings
                if (!settings.Validate(out errorMessage))
                    return false;

                // Set creation date
                settings.CreatedDate = DateTime.Now;
                settings.LastModified = DateTime.Now;

                // Save project file
                var projectPath = GetProjectPath(settings.Name);
                var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                File.WriteAllText(projectPath, json);

                _currentProject = settings;
                ProjectLoaded?.Invoke(settings);

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error creating project: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Loads an existing project
        /// </summary>
        public bool LoadProject(string projectName, out string? errorMessage)
        {
            try
            {
                var projectPath = GetProjectPath(projectName);

                if (!File.Exists(projectPath))
                {
                    errorMessage = "Project file not found";
                    return false;
                }

                var json = File.ReadAllText(projectPath);
                var settings = JsonConvert.DeserializeObject<ProjectSettings>(json);

                if (settings == null)
                {
                    errorMessage = "Failed to parse project file";
                    return false;
                }

                // Validate loaded settings
                if (!settings.Validate(out errorMessage))
                    return false;

                _currentProject = settings;
                ProjectLoaded?.Invoke(settings);

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error loading project: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Saves current project
        /// </summary>
        public bool SaveCurrentProject(out string? errorMessage)
        {
            if (_currentProject == null)
            {
                errorMessage = "No project is currently loaded";
                return false;
            }

            try
            {
                _currentProject.LastModified = DateTime.Now;
                var projectPath = GetProjectPath(_currentProject.Name);
                var json = JsonConvert.SerializeObject(_currentProject, Formatting.Indented);
                File.WriteAllText(projectPath, json);

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error saving project: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Closes current project
        /// </summary>
        public void CloseCurrentProject()
        {
            _currentProject = null;
            ProjectClosed?.Invoke();
        }

        /// <summary>
        /// Gets list of all saved projects
        /// </summary>
        public List<string> GetProjectList()
        {
            try
            {
                if (!Directory.Exists(_projectsFolder))
                    return new List<string>();

                var files = Directory.GetFiles(_projectsFolder, "*.json");
                var result = new List<string>();

                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        result.Add(fileName);
                    }
                }

                return result;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Gets recent projects (by last modified date)
        /// </summary>
        public List<string> GetRecentProjects(int maxCount = 10)
        {
            try
            {
                if (!Directory.Exists(_projectsFolder))
                    return new List<string>();

                var files = Directory.GetFiles(_projectsFolder, "*.json");
                var projects = new List<(string Name, DateTime LastModified)>();

                foreach (var file in files)
                {
                    try
                    {
                        var json = File.ReadAllText(file);
                        var settings = JsonConvert.DeserializeObject<ProjectSettings>(json);
                        var fileName = Path.GetFileNameWithoutExtension(file);

                        if (settings != null && !string.IsNullOrEmpty(fileName))
                        {
                            projects.Add((fileName, settings.LastModified));
                        }
                    }
                    catch
                    {
                        // Skip corrupted files
                    }
                }

                return projects
                    .OrderByDescending(p => p.LastModified)
                    .Take(maxCount)
                    .Select(p => p.Name)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToList()!;
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Deletes a project
        /// </summary>
        public bool DeleteProject(string projectName, out string? errorMessage)
        {
            try
            {
                var projectPath = GetProjectPath(projectName);

                if (!File.Exists(projectPath))
                {
                    errorMessage = "Project file not found";
                    return false;
                }

                File.Delete(projectPath);

                // If deleting current project, close it
                if (_currentProject?.Name == projectName)
                {
                    CloseCurrentProject();
                }

                errorMessage = null;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = $"Error deleting project: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets full path for a project file
        /// </summary>
        private string GetProjectPath(string projectName)
        {
            var safeName = string.Join("_", projectName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_projectsFolder, $"{safeName}.json");
        }

        /// <summary>
        /// Updates datamine URL in current project
        /// </summary>
        public void UpdateDatamineUrl(string? url)
        {
            if (_currentProject != null)
            {
                _currentProject.DatamineUrl = url;
                SaveCurrentProject(out _);
            }
        }

        /// <summary>
        /// Updates language setting in current project
        /// </summary>
        public void UpdateLanguage(string languageCode)
        {
            if (_currentProject != null)
            {
                _currentProject.Language = languageCode;
                SaveCurrentProject(out _);
            }
        }
    }
}