using Microsoft.AspNetCore.Mvc;
using PuppeteerSharp;
using Scrappings.Models;
using System.Diagnostics;

namespace Scrappings.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            string fullUrl = "https://www.myjoyonline.com";

            List<string> programmerLinks = new List<string>();
            List<MyResponse >response = new List<MyResponse>();
                 
            var options = new LaunchOptions()
            {
                Headless = true,
                ExecutablePath = "C:\\Program Files (x86)\\Google\\Chrome\\Application\\chrome.exe",

            };

            var browser = await Puppeteer.LaunchAsync(options);
            var page = await browser.NewPageAsync();
            await page.GoToAsync(fullUrl, WaitUntilNavigation.DOMContentLoaded);
           // await page.Client.SendAsync("Page.stopLoading");
            
       

            var links = @"Array.from(document.querySelectorAll('ul.home-latest-list li a')).map(a => a.href);"; 
            var urls = await page.EvaluateExpressionAsync<string[]>(links);
            

            foreach (string url in urls)
            {
                try
                {
                    await Task.Delay(100);
                    var newspage = await page.GoToAsync(url,WaitUntilNavigation.DOMContentLoaded);
                    var timeexp = @"document.querySelector('div.article-meta div').innerText;";
                    var time = await page.EvaluateExpressionAsync<DateTime>(timeexp);
                    if (DateTime.UtcNow > time.ToUniversalTime().AddMinutes(120))
                    {
                        continue;
                    }

                    var scrappedjs = @"[].map.call(document.querySelectorAll('#article-text p'), function(el) { return el.innerHTML;}).join();";
                    var scarppeddata = await page.EvaluateExpressionAsync<string>(scrappedjs);
                    var titlejs = @"document.querySelector('div.article-title a h1').innerText;";
                    var titledata = await page.EvaluateExpressionAsync<string>(titlejs);
                    var imagejs = @"document.querySelector('img.article-thumb').getAttribute('src')";
                    var img  = await page.EvaluateExpressionAsync<string>(imagejs);
                    response.Add(new MyResponse { Article = scarppeddata, Title = titledata ,Image= img});

                }
                catch (Exception e)
                {

                    return Json(e.Message + " " + e.InnerException);
                }
            }

         //   programmerLinks = programmerLinks.Distinct().ToList();
           // WriteToCsv(programmerLinks);

            return Json(response);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}