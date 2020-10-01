using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Westwind.AspNetCore.Markdown;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using mrkdoc.Models;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Xml;
using System.Text.RegularExpressions;

namespace mrkdoc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IWebHostEnvironment _appEnvironment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment appEnvironment)
        {
            _logger = logger;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            string webRoot = _appEnvironment.WebRootPath;
            string dirName = Path.Combine(webRoot, "files");

            var l = new List<ContentMD>();

            if (Directory.Exists(dirName))
            {
                
                string[] dirs = Directory.GetDirectories(dirName);
                foreach (string d in dirs)
                {
                    string[] files = Directory.GetFiles(Path.Combine(dirName, d), "*.md");
                    foreach (string f in files)
                    {
                        var df = new ContentMD { Path = d, TopicName = d.Split(Path.DirectorySeparatorChar).Last(),  FileName = f };
                        l.Add(df);
                    }
                }

            }
            return View(l);
        }

        public async Task<IActionResult> ViewPage(string dirname, string filename, string searchText)
        {
            if (filename != "")
            {
                var markdown = await System.IO.File.ReadAllTextAsync(filename);
                ViewBag.RenderedMarkdown = Markdown.ParseHtmlString(markdown);
                ViewBag.ShortFileName = filename.Split(Path.DirectorySeparatorChar).Last();
                ViewBag.SearchText = searchText ?? "";
                var md = new ContentMD
                {
                    Path = dirname,
                    TopicName = dirname.Split(Path.DirectorySeparatorChar).Last(),
                    FileName = filename,
                    Content = markdown
                };
                return View(md);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public IActionResult Search()
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public  IActionResult Search(string searchText)
        {
            if (searchText == null || searchText == "") {
                return RedirectToAction("Index");
            }
            string webRoot = _appEnvironment.WebRootPath;
            string dirName = Path.Combine(webRoot, "files");
            FileUtil fu = new FileUtil();
            ViewBag.SearchText = searchText;
            var l = new List<ContentMD>();
            if (Directory.Exists(dirName))
            {
                
                string[] dirs = Directory.GetDirectories(dirName);
                foreach (string d in dirs)
                {
                    string[] files = Directory.GetFiles(Path.Combine(dirName, d), "*.md");
                    foreach (string f in files)
                    {
                        fu.ReadFile(f);
                        l.AddRange(fu.Find(searchText));
                    }
                }

            }
            return View(l);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("Path,FileName,TopicName,Content")] ContentMD cnt)
        {
            if (ModelState.IsValid)
            {
                await System.IO.File.WriteAllTextAsync(cnt.FileName, cnt.Content);
                ViewBag.ShortFileName = cnt.FileName.Split(Path.DirectorySeparatorChar).Last();
                ViewBag.RenderedMarkdown = Markdown.ParseHtmlString(cnt.Content);
                ViewBag.Images = PrepareImgs(cnt.Path);
                return View(cnt);
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Edit(string dirname, string filename)
        {
            if (filename != "")
            {
                // filename must be full qualyty path
                string markdown = await System.IO.File.ReadAllTextAsync(filename);
                ViewBag.RenderedMarkdown = Markdown.ParseHtmlString(markdown);
                ViewBag.ShortFileName = filename.Split(Path.DirectorySeparatorChar).Last();
                ViewBag.Images = PrepareImgs(dirname);
                var md = new ContentMD
                {
                    TopicName = dirname.Split(Path.DirectorySeparatorChar).Last(),
                    Path = dirname,
                    FileName = filename,
                    Content = markdown
                };
                //DoCommit(",,,");
                return View(md);
            }
            else
            {
                return NotFound();
            }
        }


        private IEnumerable<ContentMD> PrepareImgs(string dirname)
        {
            string[] extensions = { ".jpeg", ".jpg", ".png", ".gif" };

            string prefix = _appEnvironment.WebRootPath;
            List<ContentMD> listImages = new List<ContentMD>();
            if (Directory.Exists(dirname))
            {

                IEnumerable<string> files = Directory.EnumerateFiles(dirname, "*.*", SearchOption.AllDirectories);

                foreach (string f in files.Where(s => extensions.Any(ext => ext == Path.GetExtension(s))))
                {
                    var cropName = f.Replace(" ", "%20").Substring(prefix.Length).Replace("\\", "/");
                    var limg = new ContentMD { TopicName = f.Split(Path.DirectorySeparatorChar).Last(), FileName = cropName };
                    listImages.Add(limg);
                }
            }
            return listImages;
          
        }

        public ActionResult AddTopic(string topicName, string tplTopicPath)
        {
            ViewBag.tplTopicPath = tplTopicPath;
            return View();
        }

        private void ReplaceImgPath(string filename, string from, string to)
        {
            string text = System.IO.File.ReadAllText(filename);
            text = text.Replace(from, to);
            System.IO.File.WriteAllText(filename, text);
        }


        [HttpPost]
        public IActionResult AddTopic([Bind("TopicName")] ContentMD cnt, string tplTopicPath)
        {
            if (ModelState.IsValid)
            {
                string filename = "README.md";
                string newpath;
                try
                {
                    newpath = Path.Combine(_appEnvironment.WebRootPath, "files", cnt.TopicName);
                    filename = Path.Combine(newpath, filename);
                    // copy from template
                    if (tplTopicPath != null && tplTopicPath != "")
                    {
                        Directory.Move(tplTopicPath, newpath);

                        ReplaceImgPath(filename, tplTopicPath, ("/files" + "/" + cnt.TopicName).Replace(" ", "%20"));
                    } 
                    else
                    {
                        Directory.CreateDirectory(newpath);
                        System.IO.File.WriteAllText(filename, "# " + cnt.TopicName);
                    }
                }
                catch (System.Exception ex)
                {
                    var nf = new NotFoundObjectResult(ex.Message);
                    return NotFound(nf);
                }
                return RedirectToAction("Edit", new { dirname = newpath, filename });
            }
            else
            {
                return NotFound();
            }
        }

        public IActionResult Delete(string topic)
        {
            var c = new Models.ContentMD
            {
                TopicName = topic
            };
            return View(c);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public  IActionResult DeleteConfirmed(string TopicName)
        {
            try
            {
                string newpath = Path.Combine(_appEnvironment.WebRootPath, "files", TopicName);
                System.IO.Directory.Delete(newpath, true);
            }
            catch (System.Exception ex)
            {
                var nf = new NotFoundObjectResult(ex.Message);
                return NotFound(nf);
            }
            return RedirectToAction("Index");
        }

        public void RunProcess(string codepage, string program, string arg, string mdpath)
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var outputEncoding = Encoding.GetEncoding(codepage);
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = program,
                        Arguments = arg,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = outputEncoding
                    }
                };
                process.Start();
                string result = process.StandardOutput.ReadToEnd();
                string err = process.StandardError.ReadToEnd();

                byte[] bytes = outputEncoding.GetBytes(result);
                result = Encoding.UTF8.GetString(bytes);
                /* remove {width="6.291666666666667in" ... height="5.291666666666667in"} */
                result = Regex.Replace(result, @"({width.+)|(height.+})", "", RegexOptions.None);
                //result = Regex.Replace(result, @"({width.+? in\"})", "", RegexOptions.Singleline);

                System.IO.File.WriteAllText(mdpath, result);
                process.WaitForExit();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

        }


        public string ConvertDocxToMD(string uploadedFile, string topic)
        {
            string filename = "README.md";
            string newpath;
            string mdpath;
            try
            {
                newpath = Path.Combine(_appEnvironment.WebRootPath, "tmp", topic);
                Directory.CreateDirectory(newpath);
                mdpath = Path.Combine(newpath, filename);
                //inputFilename = Path.Combine(_appEnvironment.WebRootPath, "files", topic, "test.docx");
                RunProcess("windows-1251", "pandoc.exe", uploadedFile + " -t markdown " + " --extract-media " + newpath, mdpath);
            }
            catch (System.Exception ex)
            {
                return "error:" + ex.Message;
            }
            return mdpath;
        }

        public IActionResult AddFile(string dirname, string filename, string filetype)
        {
            var fl = new List<ContentMD>();
            ViewBag.DirName = dirname;
            ViewBag.FileName = filename;
            ViewBag.FileType = filetype;
            return View(fl);
        }
        [HttpPost]
        public async Task<IActionResult> AddFile(IList<IFormFile> uploadedFile, string dirname, string filename, string filetype)
        {
            if (uploadedFile != null && uploadedFile.Count > 0)
            {
                foreach (var cfile in uploadedFile)
                {
                    // путь к папке Files
                    
                    string webRoot = _appEnvironment.WebRootPath;
                    string path;
                    if (filetype == "IMAGE")
                    {
                        // сохраняем файл в папку Files в каталоге wwwroot
                        path = Path.Combine(dirname, cfile.FileName);
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await cfile.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                         
                        string tmpTopicName = Guid.NewGuid().ToString();
                        string ext = Path.GetExtension(cfile.FileName);
                        path = Path.Combine(webRoot, "tmp", tmpTopicName + ext);

                        using (var fileStream = new FileStream(path, FileMode.Create))
                        { 
                            await cfile.CopyToAsync(fileStream);
                        }
                        string topicPath = ConvertDocxToMD(path, tmpTopicName);
                        if (topicPath.Contains("error:"))
                        {
                            topicPath = "";
                        }
                        else
                        {
                            topicPath = Path.GetDirectoryName(topicPath);
                        }
                        return RedirectToAction("AddTopic", new { topicName = cfile.FileName, tplTopicPath = topicPath });
                    }
                }
            } else
            {
                return RedirectToAction("AddFile", new { dirname, filename, filetype });
            }
            
            ContentMD file = new ContentMD { FileName = filename, Path = dirname };
            return RedirectToAction("Edit", new { dirname, filename});
        }
    

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

/*
        private void DoCommit(string commitMessage)
        {
            using (var repo = new Repository(@"/home/holmes/projects/mrkdoc/777/mrkdoc"))
            {
                var content = "Hello commit! " + Guid.NewGuid();

                var parents = new Commit[0];
                var treeDefinition = new TreeDefinition();
                if (repo.Head.Tip != null)
                {
                    treeDefinition = TreeDefinition.From(repo.Head.Tip);
                    parents = new[] { repo.Head.Tip };
                }

                var newBlob = repo.ObjectDatabase.CreateBlob(new MemoryStream(Encoding.UTF8.GetBytes(content)));

                treeDefinition.Add("filePath.txt", newBlob, Mode.NonExecutableFile);

                var tree = repo.ObjectDatabase.CreateTree(treeDefinition);
                var committer = new Signature("gitworker", "@noname", DateTime.Now);
                var author = committer;
                var commit = repo.ObjectDatabase.CreateCommit(
                    author,
                    committer,
                    "Commit " + commitMessage,
                    tree, parents, false);

                var master = repo.Branches.FirstOrDefault(b => b.RemoteName == "master");
                if (master == null)
                {
                    master = repo.Branches.Add("master", commit);
                }

                // Update the HEAD reference to point to the latest commit
                repo.Refs.UpdateTarget(repo.Refs.Head, commit.Id);

                repo.Reset(ResetMode.Hard);
            }
        }
*/

            }
}
