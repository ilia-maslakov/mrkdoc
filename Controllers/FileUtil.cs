using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using mrkdoc.Models;
using Westwind.AspNetCore.Markdown;

namespace mrkdoc.Controllers
{
    public class FileUtil
    {
        private string _path;
        private string[] _lines;
        public FileUtil()
        {
        }
        public async Task<string[]> ReadFileAsync(string filename){
            _path = filename;
            _lines = await File.ReadAllLinesAsync(_path, Encoding.UTF8);
            return _lines;
        }
        public void ReadFile(string filename) 
        {
            _path = filename;
            _lines = File.ReadAllLines(_path, Encoding.UTF8);
        }
        public List<ContentMD> Find(string exp){
            var searchResult = new List<ContentMD>();
            int lineNumber = 0;
            var pathList = _path.Split(Path.DirectorySeparatorChar);
            var filename = pathList.Last();
            int len = pathList.Length;
            string topic = "";
            string path = "/";
            if (len > 1) 
            {
                for (int i = 1; i < len - 2; i++){
                    path += pathList[i] + "/";
                }
                topic = pathList[len - 2];
            }
            path = Path.Combine(path, topic);

            foreach (string l in _lines) {
                lineNumber++;
                
                if (l.ToUpper().IndexOf(exp.ToUpper())>0)
                {
                    var str = Markdown.Parse(GetNearLines(_lines, lineNumber, 2, 5));
                    ContentMD r = new ContentMD{
                        FileName = _path,
                        Path = topic,
                        TopicName = topic + " » " + filename + ":" + lineNumber.ToString(),
                        Content = str
                    };
                    searchResult.Add(r); 
                }
            }
            return searchResult;
        }

        private string GetNearLines(string[] lines, int line, int before, int after)
        {
            StringBuilder sb = new StringBuilder();

            int startLine = line - before;
            if (startLine < 0) {
                startLine = 0;
            }
            int endLine = line + after;
            if (endLine > lines.Count()) {
                endLine = lines.Count();
            }
            
            for (int i = startLine; i < endLine ; i++)
            {
                sb.Append(lines[i] + "\r\n");
            }
            sb.Append("<br /><hr />");
            return sb.ToString();
        }
    }
}