using System;
using System.Collections;
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

namespace mrkdoc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        IWebHostEnvironment _appEnviroment;
        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment appEnviroment)
        {
            _logger = logger;
            _appEnviroment = appEnviroment;
        }

        public IActionResult Index()
        {
            string webRoot = _appEnviroment.WebRootPath;
            string dirName = Path.Combine(webRoot, "files");

            var l = new List<DirectoryFiles>();

            if (Directory.Exists(dirName))
            {
                /*
                Console.WriteLine("Подкаталоги:");
                string[] dirs = Directory.GetDirectories(dirName);
                foreach (string s in dirs)
                {
                    Console.WriteLine(s);
                }
                */
                string[] files = Directory.GetFiles(dirName);
                foreach (string s in files)
                {
                    var df = new DirectoryFiles { FileName = s, FileSize = 0 };
                    l.Add(df);
                }
            }
            return View(l);
        }

        public async Task<IActionResult> ViewPage(string filename)
        {
            if (filename != "")
            {
                var markdown = await System.IO.File.ReadAllTextAsync(filename);
                ViewBag.RenderedMarkdown = Markdown.ParseHtmlString(markdown);
                return View();
            }
            else
            {
                return NotFound();
            }
        }

        public async Task<IActionResult> Edit(string filename)
        {
            if (filename != "")
            {

                var md = new ContentMD
                {
                    FileName = filename,
                    Content = ViewBag.SourceText = await System.IO.File.ReadAllTextAsync(filename)
                };
                return View(md);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("FileName,Content")] ContentMD cnt)
        {
            if (ModelState.IsValid)
            {
                await System.IO.File.WriteAllTextAsync(cnt.FileName, cnt.Content);
                return RedirectToAction("Index");
            }
            else
            {
                return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
