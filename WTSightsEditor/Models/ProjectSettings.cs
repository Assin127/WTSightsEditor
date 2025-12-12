using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WTSightsEditor.Models
{
    /// <summary>
    /// Project settings model
    /// </summary>
    public class ProjectSettings
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("game_folder")]
        public string GameFolder { get; set; } = string.Empty;

        [JsonProperty("datamine_folder")]
        public string DatamineFolder { get; set; } = string.Empty;

        [JsonProperty("data_folder")]
        public string DataFolder { get; set; } = string.Empty;

        [JsonProperty("ballistic_folder")]
        public string BallisticFolder { get; set; } = string.Empty;

        [JsonProperty("created_date")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }

        [JsonProperty("recent_files")]
        public List<string> RecentFiles { get; set; } = new List<string>();

        [JsonProperty("datamine_url")]
        public string? DatamineUrl { get; set; }

        [JsonProperty("auto_update_datamine")]
        public bool AutoUpdateDatamine { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; } = "en";

        /// <summary>
        /// Validates project settings
        /// </summary>
        public bool Validate(out string? errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                errorMessage = "Project name is required";
                return false;
            }

            if (string.IsNullOrWhiteSpace(GameFolder))
            {
                errorMessage = "Game folder is required";
                return false;
            }

            // Check if game folder exists and contains War Thunder executable
            if (!Directory.Exists(GameFolder))
            {
                errorMessage = "Game folder does not exist";
                return false;
            }

            // Check for War Thunder executable
            var exeFiles = Directory.GetFiles(GameFolder, "*.exe", SearchOption.TopDirectoryOnly);
            bool hasWarThunderExe = false;
            foreach (var exe in exeFiles)
            {
                var fileName = Path.GetFileName(exe).ToLower();
                if (fileName.Contains("war") && fileName.Contains("thunder") ||
                    fileName.Contains("aces") ||
                    fileName == "win64_aviaclient.exe" ||
                    fileName == "win32_aviaclient.exe")
                {
                    hasWarThunderExe = true;
                    break;
                }
            }

            if (!hasWarThunderExe)
            {
                errorMessage = "War Thunder executable not found in selected folder";
                return false;
            }

            // Create optional folders if they don't exist
            if (!string.IsNullOrWhiteSpace(DataFolder) && !Directory.Exists(DataFolder))
            {
                try
                {
                    Directory.CreateDirectory(DataFolder);
                }
                catch
                {
                    errorMessage = "Could not create Data folder";
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(BallisticFolder) && !Directory.Exists(BallisticFolder))
            {
                try
                {
                    Directory.CreateDirectory(BallisticFolder);
                }
                catch
                {
                    errorMessage = "Could not create Ballistic folder";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        /// <summary>
        /// Adds a file to recent files list
        /// </summary>
        public void AddRecentFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            // Remove if already exists
            RecentFiles.Remove(filePath);

            // Add to beginning
            RecentFiles.Insert(0, filePath);

            // Keep only last 32 files
            if (RecentFiles.Count > 32)
            {
                RecentFiles.RemoveRange(32, RecentFiles.Count - 32);
            }

            LastModified = DateTime.Now;
        }

        /// <summary>
        /// Clears recent files list
        /// </summary>
        public void ClearRecentFiles()
        {
            RecentFiles.Clear();
            LastModified = DateTime.Now;
        }
    }
}