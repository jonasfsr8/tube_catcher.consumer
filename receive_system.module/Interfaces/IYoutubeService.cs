namespace receive_system.module.Interfaces
{
    public interface IYoutubeService
    {
        Task<string> DownloadAsync(string url, string format, CancellationToken cancellationToken = default);
    }
}
