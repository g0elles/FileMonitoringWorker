using GTFileMonitorService.Configuration;
using Microsoft.Extensions.Options;

namespace GTFileMonitorService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly MonitoredPath _options;
        private readonly TimeSettings _timeSettings;


        public Worker(ILogger<Worker> logger, IOptionsMonitor<MonitoredPath> optionsMonitor, 
            IOptionsMonitor<TimeSettings> timeSettingsMonitor)
        {
            _logger = logger;
            _options = optionsMonitor.CurrentValue;
            _timeSettings = timeSettingsMonitor.CurrentValue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {                
                await Task.Delay(_timeSettings.DelayTime, stoppingToken);

                try
                {
                    var fileList = Directory.GetFiles(_options.FileRoute);
                    if (fileList.Length > 0)
                        foreach (var file in fileList) DeleteFile(file);
                }
                catch (Exception ex)
                {

                    _logger.LogError($"Error could not load files due to: {ex}", DateTimeOffset.Now);
                }




            }
        }

        public void DeleteFile(string FileName)
        {
            var Name = Path.GetFileName(FileName);
            try
            {
                FileInfo fileInfo = new FileInfo(FileName);

                using (FileStream stream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }                
                var lastAccesedTime = DateTime.Now - fileInfo.LastAccessTime;

                if (lastAccesedTime.Hours >= _timeSettings.MaxCreationTime)
                {
                    try
                    {
                        File.Delete(FileName);
                        _logger.LogInformation($"The file {Name} was deleted", DateTimeOffset.Now);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"The file {Name} wasn't deleted due to: {ex}", DateTimeOffset.Now);
                    }

                }
               
            }
            catch (Exception e)
            {
                _logger.LogWarning($"The file {Name} is locked due to {e}", DateTimeOffset.Now);
            }


        }

    }
}