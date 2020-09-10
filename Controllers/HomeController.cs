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

        public async Task<IActionResult> Edit(string dirname, string filename)
        {
            if (filename != "")
            {
                // filename must be full qualyty path
                String markdown = await System.IO.File.ReadAllTextAsync(filename);
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

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("Path,FileName,Content")] ContentMD cnt)
        {
            if (ModelState.IsValid)
            {
                await System.IO.File.WriteAllTextAsync(cnt.FileName, cnt.Content);
                ViewBag.RenderedMarkdown = Markdown.ParseHtmlString(cnt.Content);
                ViewBag.Images = PrepareImgs(cnt.Path);
                return View(cnt);
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
                    var cropName = f.Replace(" ", "%20").Substring(prefix.Length);
                    var limg = new ContentMD { TopicName = f.Split(Path.DirectorySeparatorChar).Last(), FileName = cropName };
                    listImages.Add(limg);
                }
            }
            return listImages;
          
        }

        public async Task<IActionResult> AddTopic()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddTopic([Bind("TopicName")] ContentMD cnt)
        {
            if (ModelState.IsValid)
            {
                string filename = "README.md";
                string newpath = Path.Combine(_appEnvironment.WebRootPath, "files", cnt.TopicName);
                try
                {
                    System.IO.Directory.CreateDirectory(newpath);
                    filename = Path.Combine(newpath, filename);
                    System.IO.File.WriteAllText(filename, "# " + cnt.TopicName);
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




/*
        private async IAsyncEnumerable<ContentMD> PrepareImgsAsync(string dirname)
        {
            List<ContentMD> listImages = new List<ContentMD>();
            if (Directory.Exists(dirname))
            {

                IEnumerable<String> files = Directory.EnumerateFiles(dirname, "*.*", SearchOption.AllDirectories)
                                        .Where(s => s.EndsWith(".png") || s.EndsWith("*.jp?g") || s.EndsWith("*.gif"));

                await foreach (string f in files)
                {
                    var limg = new ContentMD { TopicName = f.Split(Path.DirectorySeparatorChar).Last(), FileName = f };
                    yield return limg;
                }
            }
        }

*/

        public async Task<IActionResult> AddFile(string dirname, string filename)
        {
            var fl = new List<ContentMD>();
            ViewBag.DirName = dirname;
            ViewBag.FileName = filename;
            return View(fl);
        }
        [HttpPost]
        public async Task<IActionResult> AddFile(IList<IFormFile> uploadedFile, string dirname, string filename)
        {
            if (uploadedFile != null)
            {
                foreach (var cfile in uploadedFile)
                {
                    // путь к папке Files
                    string webRoot = _appEnvironment.WebRootPath;
                    string path = Path.Combine(dirname, cfile.FileName);

                    // сохраняем файл в папку Files в каталоге wwwroot
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await cfile.CopyToAsync(fileStream);
                    }
                }
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
