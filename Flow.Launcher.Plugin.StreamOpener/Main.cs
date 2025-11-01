using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System;

namespace Flow.Launcher.Plugin.StreamOpener
{
    public class StreamOpener : IPlugin
    {
        private readonly string pluginDataFolder = Environment.GetEnvironmentVariable("appdata") + "\\FlowLauncher\\Plugins\\Stream opener";
        private const    string DATA_FILE_NAME   = "settings.txt";

        private PluginInitContext _context;

        private bool instantOpen = true;
        private string vlcPath = "";

        public void Init(PluginInitContext context)
        {
            _context = context;

            CheckIfDataFolderExists();
            CheckIfDataFileExists();
            ReadSettingsDataFile();
        }

        public List<Result> Query(Query query)
        {
            ReadSettingsDataFile();

            List<Result> results = new List<Result>();

            if (IsStringNullOrEmpty(vlcPath) == false)
            {
                if (IsValidUrl(query.Search) == true)
                {
                    if (instantOpen == true)
                    {
                        OpenStream(query.Search);
                    }
                    else
                    {
                        results.Add(new Result()
                        {
                            Title = "Open stream",
                            IcoPath = @"img\empty.png",
                            SubTitle = query.Search,
                            Action = _ =>
                            {
                                OpenStream(query.Search);
                                return true;
                            }
                        });
                    }
                }
                else
                {
                    results.Add(new Result()
                    {
                        Title = "Enter a vlaid URL",
                        IcoPath = @"img\empty.png",
                        Score = 10,
                    });
                }
            }
            else
            {
                results.Add(new Result()
                {
                    Title = "Path to a media player executable is empty",
                    SubTitle = "Set a full path to a player executable in the settings.txt file",
                    IcoPath = @"img\empty.png",
                    Score = 10,
                });
                results.Add(new Result()
                {
                    Title = "Click here to edit the settings file",
                    IcoPath = @"img\empty.png",
                    Score = 1,
                    Action = _ =>
                    {
                        StartProcess("notepad.exe", $"{pluginDataFolder}\\{DATA_FILE_NAME}");
                        return true;
                    }
                });
            }

            return results;
        }

        #region Validate string 
        private bool IsStringNullOrEmpty(string _string)
        {
            return (_string == null || _string.Length == 0 || _string == "");
        }

        private bool IsValidUrl(string _string)
        {
            if (IsStringNullOrEmpty(_string) == true)
            {
                return false;
            }

            if (_string.Contains("http") && _string.Contains("://"))
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Open URL
        private void OpenStream(string _streamUrl)
        {
            try
            {
                StartProcess(vlcPath, $"\"{_streamUrl}\"");
            }
            catch (Exception ex)
            {
                throw new Exception($"\n--- INVALID EXECUTABLE PATH: \"{vlcPath}\" ---\n", ex);
            }
        }

        private void StartProcess(string _fullPath, string _arguments)
        {
            Console.WriteLine(_fullPath + " | " + _arguments);

            using Process process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _fullPath,
                    Arguments = _arguments,
                    UseShellExecute = false,
                }
            };

            process.Start();
        }
        #endregion


        #region File stuff
        private void CheckIfDataFolderExists ()
        {
            if (!Directory.Exists(pluginDataFolder))
            {
                Directory.CreateDirectory(pluginDataFolder);
            }
        }
        private void CheckIfDataFileExists()
        {
            if (!File.Exists(pluginDataFolder + "\\" + DATA_FILE_NAME))
            {
                CreateSettingsDataFile();
            }
        }

        private void ReadSettingsDataFile()
        {
            if (File.Exists(pluginDataFolder + "\\" + DATA_FILE_NAME) == false)
            {
                return;
            }

            var fileContents = File.ReadLines(pluginDataFolder + "\\" + DATA_FILE_NAME);

            string[] propertyNameAndValue = { };
            foreach (var line in fileContents)
            {
                propertyNameAndValue = line.Split('=');

                switch(propertyNameAndValue[0])
                {
                    case "mediaPlayerPath":
                        vlcPath = propertyNameAndValue[1];
                        break;

                    case "instantOpen":
                        switch (propertyNameAndValue[1].ToLower())
                        {
                            case "true":
                                instantOpen = true;
                                break;

                            case "false":
                                instantOpen = false;
                                break;
                        }
                        break;
                }
            }
        }

        private void CreateSettingsDataFile()
        {
            List<string> fileLines = new();

            fileLines.Add($"mediaPlayerPath={vlcPath}");
            fileLines.Add($"instantOpen={instantOpen}");

            File.WriteAllLines(pluginDataFolder + "\\" + DATA_FILE_NAME, fileLines);
        }
        #endregion
    }
}