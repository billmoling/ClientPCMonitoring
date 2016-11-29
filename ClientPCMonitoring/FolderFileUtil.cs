using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientPCMonitoring
{
    public class FolderFileUtil
    {
        public string _filePath = string.Empty;
        string GetFolderPath()
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string dayPath = Path.Combine(_filePath, date);
            if (!Directory.Exists(dayPath))
            {
                Directory.CreateDirectory(dayPath);
            }
            return dayPath;
        }

        string GetFileName()
        {
            string time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            return Path.Combine(GetFolderPath(), time);

        }

    }
}
