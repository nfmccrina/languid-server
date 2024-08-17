using System.Net.WebSockets;
using System.Text;
using Azure.Messaging.ServiceBus;

namespace Languid.Server.Services
{
    class TranslationProcessingState
    {
        public Task? ProcessingTask { get; set; }
    }
    public class BackgroundSocketProcessor : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<BackgroundSocketProcessor> logger;
        private readonly ITranslationQueueService translationQueueService;
        private Timer? timer;
        private readonly ISocketManager socketManager;

        public BackgroundSocketProcessor(
            IConfiguration configuration,
            ILogger<BackgroundSocketProcessor> logger,
            ITranslationQueueService translationQueueService,
            ISocketManager socketManager)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.translationQueueService = translationQueueService;
            this.socketManager = socketManager;
            timer = null;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            socketManager.Stop();
            timer?.Change(Timeout.Infinite, 0);

            return base.StopAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            socketManager.StoppingToken = stoppingToken;
            timer = new Timer(ProcessTranslations, new TranslationProcessingState()
            {
                ProcessingTask = null
            }, TimeSpan.Zero, TimeSpan.FromSeconds(2));

            return Task.CompletedTask;
        }

        private void ProcessTranslations(object? state)
        {
            var processingState = (TranslationProcessingState)(state ?? new TranslationProcessingState() { ProcessingTask = null });

            if (processingState.ProcessingTask != null && !processingState.ProcessingTask.IsCompleted)
            {
                return;
            }
            
            processingState.ProcessingTask = Task.Run(async () => {
                var nextTranslation = this.translationQueueService.Pop();

                if (!string.IsNullOrEmpty(nextTranslation))
                {
                    await socketManager.PushTranslationAsync(nextTranslation);
                }
            });
        }
    }
}