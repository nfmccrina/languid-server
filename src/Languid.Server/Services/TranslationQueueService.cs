using System.Collections.Concurrent;

namespace Languid.Server.Services
{
    public interface ITranslationQueueService
    {
        void Push(string translation);
        string? Pop();
    }
    public class TranslationQueueService : ITranslationQueueService
    {
        public TranslationQueueService()
        {
            translationQueue = new ConcurrentQueue<string>();
        }

        public string? Pop()
        {
            string? nextTranslation;

            if (!this.translationQueue.TryDequeue(out nextTranslation))
            {
                return null;
            }
            else
            {
                return nextTranslation;
            }
        }

        public void Push(string translation)
        {
            this.translationQueue.Enqueue(translation);
        }

        private ConcurrentQueue<string> translationQueue;
    }
}