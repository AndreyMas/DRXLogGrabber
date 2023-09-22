using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO.Compression;

namespace DrxLogGrabber
{
    internal class Program
    {
        static Config _config;

        static void Main(string[] args)
        {
            Init();

            var validate = new List<string>();
            if (_config.LogPath.Count == 0)
                validate.Add("Конфиг не содержит путей к логам");
            if (_config.Services.Count == 0)
                validate.Add("Конфиг не содержит имен сервисов");
            if (!Directory.Exists(_config.OutputPath))
                validate.Add("Папка выгрузки недоступна: " + _config.OutputPath);
            foreach (var logPath in _config.LogPath)
                if (!Directory.Exists(logPath))
                    validate.Add("Папка с логами не доступна: " + logPath);
            if (string.IsNullOrEmpty(_config.Year))
                validate.Add("Папка выгрузки недоступна: " + _config.OutputPath);

            if (validate.Count > 0)
            {
                Console.WriteLine(string.Join(Environment.NewLine, validate));
                return;
            }

            Console.WriteLine("Введите число и месяц слитно:");
            var enter = Console.ReadLine();
            var day = enter.Substring(0, 2);
            var month = enter.Substring(2, 2);
            var year = _config.Year;
            var date = $"{year}-{month}-{day}";

            var logFiles = new List<string>();
            try
            {
                foreach (var logPath in _config.LogPath)
                {
                    foreach (var service in _config.Services)
                    {
                        var serviceDirectories = Directory.GetDirectories(logPath, "*" + service + "*").ToList();
                        var isDirectoryLog = serviceDirectories.Count > 0;

                        Console.WriteLine($"{logPath} : {service} : " + (isDirectoryLog ? "directory" : "root"));
                        if (isDirectoryLog)
                        {
                            foreach (var serviceDirectory in serviceDirectories)
                            {
                                Console.WriteLine("Folder: " + serviceDirectory);
                                var fileMask = $"*{date}*.log";
                                var files = Directory.GetFiles(serviceDirectory, fileMask).ToList();
                                if (files.Count > 0)
                                    logFiles.AddRange(files);
                            }
                        }
                        else
                        {
                            var fileMask = $"*{service}*{date}*.log";
                            var files = Directory.GetFiles(logPath, fileMask).ToList();
                            if (files.Count > 0)
                                logFiles.AddRange(files);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            var fileName = Path.Combine(_config.OutputPath, $"logs_{enter}.zip");
            try
            {
                CreateZip(fileName, logFiles.Distinct().ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine($"Done: {fileName}. Press Enter");
            Console.ReadLine();
        }

        static void Init()
        {
            var configName = "config.drxg";
            if (!File.Exists(configName))
            {
                _config = new Config();
                _config.LogPath = new List<string>();
                _config.LogPath.Add("");
                _config.OutputPath = "";
                _config.FtpAddress = "127.0.0.1";
                _config.FtpLogin = "login";
                _config.FtpPass = "password";
                _config.Services = new List<string>();
                _config.Services.Add("GenericService");
                _config.Services.Add("IntegrationService");
                _config.Services.Add("WebServer");
                _config.Services.Add("WorkflowProcessService");
                _config.Services.Add("WorkflowBlockService");
                _config.Services.Add("Worker");
                _config.Year = "2023";

                var json = JsonSerializer.Serialize<Config>(_config);
                File.WriteAllText(configName, json);
            }

            var configJson = File.ReadAllText(configName);
            _config = JsonSerializer.Deserialize<Config>(configJson);
        }

        static void CreateZip(string fileName, List<string> files)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);

            using (var zip = ZipFile.Open(fileName, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
                    Console.WriteLine("Add to zip : " + file);
                }
            }
        }
    }
}