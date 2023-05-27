using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace youtubetomusic.Services
{
    public class YoutubeListDownloadService: YoutubeDonloadBaseService
    {
        private YouTubeService _youtubeService;

        public YoutubeListDownloadService()
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "{yourAPIKey}",
                ApplicationName = "YoutubetoMP3"
            });
        }
        public async Task DownloadAsync(string PlaylistId)
        {
            var searchListRequest = _youtubeService.PlaylistItems.List("snippet");
            searchListRequest.PlaylistId = PlaylistId;;
            searchListRequest.MaxResults = 50;
            PlaylistItemListResponse? searchListResponse = null;
            try
            {
                //取得Youtube清單中的資料
                searchListResponse = await searchListRequest.ExecuteAsync();
            }
            catch
            {
                Console.WriteLine($"取得Youtube清單發生錯誤");
                return;
            }

            Console.WriteLine($"一共有 {searchListResponse.Items.Count} 個項目，開始下載");

            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), DateTime.Now.ToString("yyyyMMddHHmmssffff"));

            // 检查文件夹是否存在
            FileCheck(folderPath);

            List<Task> downloadList = new List<Task>();
            foreach (var searchResult in searchListResponse.Items)
            {
                string filename = $"{searchResult.Snippet.Title}.mp3";
                // 保存 MP3 文件
                var outPath = Path.Combine(folderPath, filename);
                outPath = outPath.Replace(" ", "");

                downloadList.Add(Task.Run(async()=>await ConvertToMP3(searchResult, outPath, filename)));
                
            }
            await Task.WhenAll(downloadList);
             
            Console.WriteLine($"下載已完成 請至 {folderPath} 目錄下確認 ");
        }

        private async Task ConvertToMP3(PlaylistItem item,string outPath,string filename)
        {
            try
            {
                var youtube = new YoutubeClient();
                // 解析影片信息
                var video = await youtube.Videos.Streams.GetManifestAsync(item.Snippet.ResourceId.VideoId);

                // 擷取聲音
                var audioStreamInfo = video.GetAudioOnlyStreams().GetWithHighestBitrate();
                var audioStream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);

                //下載
                await youtube.Videos.Streams.DownloadAsync(audioStreamInfo, outPath);
                Console.WriteLine($"已保存 MP3 文件至 {outPath}。");
            }
            catch
            {
                Console.WriteLine($"下載 {filename} 發生錯誤");
            }
        }
    }
}
