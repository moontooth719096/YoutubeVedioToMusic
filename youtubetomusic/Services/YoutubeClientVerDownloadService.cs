using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos.Streams;
using youtubetomusic.Interfaces;

namespace youtubetomusic.Services
{
    public class YoutubeClientVerDownloadService : YoutubeDonloadBaseService, IYoutubeListDownloadService
    {

        public YoutubeClientVerDownloadService()
        {
        }

        public async Task DownloadAsync(string PlaylistId)
        {
            //取得youtube清單
            IEnumerable<PlaylistVideo> MusicList = await SearchListYoutubeClientVer_Get(PlaylistId);

            if (MusicList == null || MusicList.Count() <= 0)
            {
                Console.WriteLine($"清單為空，請確認後再試");
                return;
            }

            Console.WriteLine($"一共有 {MusicList.Count()} 個項目，開始下載");

            //組合資料夾名稱
            string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), DateTime.Now.ToString("yyyyMMddHHmmssffff"));

            // 检查文件夹是否存在
            FileCheck(folderPath);

            //開始下載
            await DownloadProcess(MusicList, folderPath);

            Console.WriteLine($"下載已完成 請至 {folderPath} 目錄下確認 ");
        }

        private async Task<IEnumerable<PlaylistVideo>> SearchListYoutubeClientVer_Get(string PlaylistId)
        {
            var youtube = new YoutubeClient();
            var playlistUrl ="https://youtube.com/playlist?list="+ PlaylistId;

            // Get all playlist videos
            return  await youtube.Playlists.GetVideosAsync(playlistUrl);
        }

        /// <summary>
        /// 下載處理
        /// </summary>
        /// <param name="MusicList">取得的youtube清單</param>
        /// <param name="folderPath">要儲存的資料夾目錄位置</param>
        /// <returns></returns>
        private async Task DownloadProcess(IEnumerable<PlaylistVideo> MusicList, string folderPath)
        {
            List<Task> downloadList = new List<Task>();
            List<PlaylistVideo> Dolist = MusicList.OrderBy(x => x.Id.Value).ToList();
            int Takecount = 5;
            while (Dolist.Count() > 0)
            {
                IEnumerable<PlaylistVideo> nowlist = Dolist.Take(Takecount);
                foreach (var searchResult in nowlist)
                {
                    string filename = $"{searchResult.Title}.mp3";
                    filename = Regex.Replace(filename, "[\\/:*?\"<>|]", "");
                    // 保存 MP3 文件
                    var outPath = Path.Combine(folderPath, filename);
                    outPath = outPath.Replace(" ", "");

                    downloadList.Add(Task.Run(async () => await ConvertToMP3(searchResult, outPath, filename)));

                }
                await Task.WhenAll(downloadList);
                Dolist.RemoveRange(0, nowlist.Count());
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
            catch(Exception ex)
            {
                Console.WriteLine($"下載 {filename} 發生錯誤");
            }
        }
    }
}
