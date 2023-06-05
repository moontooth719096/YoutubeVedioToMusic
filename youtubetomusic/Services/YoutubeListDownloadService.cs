using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos.Streams;
using youtubetomusic.Interfaces;

namespace youtubetomusic.Services
{
    public class YoutubeListDownloadService : YoutubeDonloadBaseService, IYoutubeListDownloadService
    {
        private YouTubeService _youtubeService;

        public YoutubeListDownloadService()
        {
            _youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "{APIKEY}",
                ApplicationName = "YoutubetoMP3"
            });
        }

        public async Task DownloadAsync(string PlaylistId)
        {
            //取得youtube清單
            List<PlaylistItem> MusicList = await SearchList_Get(PlaylistId);

            if (MusicList == null || MusicList.Count <= 0)
            {
                Console.WriteLine($"清單為空，請確認後再試");
                return;
            }

            Console.WriteLine($"一共有 {MusicList.Count} 個項目，開始下載");

            //組合資料夾名稱
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), DateTime.Now.ToString("yyyyMMddHHmmssffff"));

            // 检查文件夹是否存在
            FileCheck(folderPath);

            //開始下載
            await DownloadProcess(MusicList, folderPath);

            Console.WriteLine($"下載已完成 請至 {folderPath} 目錄下確認 ");
        }

        private async Task<List<PlaylistItem>> SearchList_Get(string PlaylistId)
        {
            List<PlaylistItem> result = null;
            var searchListRequest = _youtubeService.PlaylistItems.List("snippet");
            searchListRequest.PlaylistId = PlaylistId;
            searchListRequest.MaxResults = 200;
            PlaylistItemListResponse? searchListResponse = null;
            var nextPageToken = "";
            while (nextPageToken != null)
            {
                searchListRequest.PageToken = nextPageToken;
                try
                {
                    //取得Youtube清單中的資料
                    searchListResponse = await searchListRequest.ExecuteAsync();
                    if (searchListResponse != null && searchListResponse.Items.Count > 0)
                    {
                        if (result == null)
                            result = new List<PlaylistItem>();
                        result.AddRange(searchListResponse.Items);
                    }
                    nextPageToken = searchListResponse.NextPageToken;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"取得Youtube清單發生錯誤");
                    break;
                }
            }

            return result;
        }

        private async Task<IEnumerable<PlaylistVideo>> SearchListYoutubeClientVer_Get(string PlaylistId)
        {
            var youtube = new YoutubeClient();
            var playlistUrl = "https://youtube.com/playlist?list=" + PlaylistId;

            // Get all playlist videos

            return await youtube.Playlists.GetVideosAsync(playlistUrl);
        }

        /// <summary>
        /// 下載處理
        /// </summary>
        /// <param name="MusicList">取得的youtube清單</param>
        /// <param name="folderPath">要儲存的資料夾目錄位置</param>
        /// <returns></returns>
        private async Task DownloadProcess(List<PlaylistItem> MusicList, string folderPath)
        {
            List<Task> downloadList = new List<Task>();
            List<PlaylistItem> Dolist = MusicList.OrderBy(x => x.Id).ToList();
            int Takecount = 5;
            while (Dolist.Count > 0)
            {
                List<PlaylistItem> nowlist = Dolist.Take(Takecount).ToList();
                foreach (var searchResult in nowlist)
                {
                    string filename = $"{searchResult.Snippet.Title}.mp3";
                    // 保存 MP3 文件
                    var outPath = Path.Combine(folderPath, filename);
                    outPath = outPath.Replace(" ", "");

                    downloadList.Add(Task.Run(async () => await ConvertToMP3(searchResult, outPath, filename)));

                }
                await Task.WhenAll(downloadList);
                Dolist.RemoveRange(0, nowlist.Count);
            }
        }
        /// <summary>
        /// 轉換成MP3檔
        /// </summary>
        /// <param name="item"></param>
        /// <param name="outPath"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        private async Task ConvertToMP3(PlaylistItem item, string outPath, string filename)
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

        private async Task ConvertToMP3(PlaylistVideo item, string outPath, string filename)
        {
            try
            {
                var youtube = new YoutubeClient();
                // 解析影片信息
                var video = await youtube.Videos.Streams.GetManifestAsync(item.Id);

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
