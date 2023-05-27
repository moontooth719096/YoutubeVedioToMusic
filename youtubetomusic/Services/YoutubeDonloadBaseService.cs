using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace youtubetomusic.Services
{
    public class YoutubeDonloadBaseService
    {
        public void FileCheck(string folderPath)
        {
            // 检查文件夹是否存在
            if (!Directory.Exists(folderPath))
                // 如果文件夹不存在，则创建新文件夹
                Directory.CreateDirectory(folderPath);
        }
    }
}
