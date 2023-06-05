namespace youtubetomusic.Interfaces
{
    public interface IYoutubeListDownloadService
    {
        Task DownloadAsync(string PlaylistId);
    }
}