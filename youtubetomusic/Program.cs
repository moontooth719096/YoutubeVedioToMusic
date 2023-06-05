

using AngleSharp.Dom;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using NAudio.Lame;
using NAudio.Wave;
using System.Text;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;
using youtubetomusic.Services;

//設定console輸出文字為UTF8
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("YouTube Data API: Search");
Console.WriteLine("========================");
YoutubeListDownloadService service = new YoutubeListDownloadService();
YoutubeClientVerDownloadService service2 = new YoutubeClientVerDownloadService();
while (true)
{
    
    string? PlaylistId = string.Empty;

    while (string.IsNullOrEmpty(PlaylistId))
    {
        Console.Write("請輸入要抓取的youtube清單ID：");
        PlaylistId = Console.ReadLine();
    }

    await service2.DownloadAsync(PlaylistId);
}






