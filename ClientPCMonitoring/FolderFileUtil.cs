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
        
        public static string GetFullFilePath(string filepath)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string dayPath = Path.Combine(filepath, date);
            if (!Directory.Exists(dayPath))
            {
                Directory.CreateDirectory(dayPath);
            }
            string time = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            return Path.Combine(dayPath, time);

        }

    }
}
