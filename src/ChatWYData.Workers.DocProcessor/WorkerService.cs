using ChatWYData.ApiServices;
using ChatWYData.FileEntities;

namespace ChatWYData.Workers.DocProcessor;

public class WorkerService
{
    private readonly DocumentApiService documentApiService;

    private readonly ILogger<BackgroundService> _logger;

    public DateTime LastRun;
    public Task? runningTask;

    public WorkerService(DocumentApiService documentApiService,
        ILogger<BackgroundService> logger)
    {
        this.documentApiService = documentApiService;
        _logger = logger;
        LastRun = DateTime.UtcNow;
    }

    public bool WorkerStatus()
    {
        _logger.LogInformation("Checking worker status...");
        return runningTask is not null;
    }


    public void Stop()
    {
        _logger.LogInformation("Stopping the worker service...");
        if (runningTask is not null)
        {
            _logger.LogInformation("Disposing the running task...");
            runningTask.Dispose();
            runningTask = null;
        }
        _logger.LogInformation("Worker service stopped.");
    }

    public void Start()
    {
        _logger.LogInformation("Starting the worker service...");
        runningTask = Task.Run(async () =>
        {
            while (true)
            {
                await ProcessDocuments();
                _logger.LogInformation("Waiting for the next iteration...");
                LastRun = DateTime.UtcNow;
                await Task.Delay(TimeSpan.FromSeconds(120));
            }
        });
    }

    public async Task ProcessDocuments()
    {
        try
        {
            _logger.LogInformation("Fetching docs to be processed...");
            var docsToProcess = await documentApiService.GetFileBatchUploadsToProcessAsync();

            foreach (var doc in docsToProcess)
            {
                _logger.LogInformation($"Processing {doc.FileName} - {doc.CreatedDate} ");

                var fur = new FileUploadRequest()
                {
                    FileName = doc.FileName,
                    FileContentType = doc.FileContentType,
                };
                await documentApiService.FileBatchProcessDocAsync(fur);
                _logger.LogInformation($"DONE Processing {doc.FileName} - {doc.CreatedDate} ");
            }

            LastRun = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during processing.");
        }
    }
}