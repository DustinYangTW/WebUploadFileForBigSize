using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UploadTest.Models;

namespace UploadTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Files");

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // 處理檔案上傳，並存到temp目錄
            if (file != null && file.Length > 0)
            {
                var tempPath = Path.Combine(uploadPath, "temp");

                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                var filePath = Path.Combine(tempPath, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok(new { message = "上傳成功" });
            }

            return BadRequest(new { message = "失敗" });
        }

        [HttpPost]
        public IActionResult MergeFiles(string fileName)
        {
            // 合併文件塊
            var tempPath = Path.Combine(uploadPath, "temp");

            string FindfileName = fileName.Split('.')[0];
            var outputPath = Path.Combine(uploadPath, "complete", fileName);
            var tempFiles = Directory.GetFiles(tempPath).Where(f => f.StartsWith(Path.Combine(tempPath, FindfileName)));

            using (var output = new FileStream(outputPath, FileMode.Create))
            {
                foreach (var tempFile in tempFiles)
                {
                    using (var input = new FileStream(tempFile, FileMode.Open))
                    {
                        input.CopyTo(output);
                    }
                    System.IO.File.Delete(tempFile); // 删除临时文件
                }
            }

            return Ok(new { message = "檔案合併成功" });
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