using Microsoft.AspNetCore.Mvc;
using UnifiedFileGateway.Contracts;
using UnifiedFileGateway.Service;

namespace UnifiedFileGateway.Host.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;
        private const string StoragePath = @"d:\UnifiedFileGatewayStorage";

        public FilesController(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles([FromQuery] string folder = "CentralStorage")
        {
            try
            {
                var fileNames = await _fileService.GetFiles(folder);

                var files = fileNames.Select(fileName => new
                {
                    name = fileName,
                    size = GetFileSize(fileName),
                    uploadDate = GetFileUploadDate(fileName)
                }).ToArray();

                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private long GetFileSize(string fileName)
        {
            try
            {
                var filePath = Path.Combine(StoragePath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    return new FileInfo(filePath).Length;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private DateTime GetFileUploadDate(string fileName)
        {
            try
            {
                var filePath = Path.Combine(StoragePath, fileName);
                if (System.IO.File.Exists(filePath))
                {
                    return System.IO.File.GetCreationTime(filePath);
                }
                return DateTime.Now;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string folder = "CentralStorage")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file provided" });

                using var stream = file.OpenReadStream();
                var uploadMessage = new FileUploadMessage
                {
                    FileName = file.FileName,
                    FileData = stream
                };

                await _fileService.UploadFile(uploadMessage);

                return Ok(new { message = "File uploaded successfully", fileName = file.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("status/{fileName}")]
        public async Task<IActionResult> GetFileStatus(string fileName)
        {
            try
            {
                var status = await _fileService.GetFileStatus(fileName);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            try
            {
                var stream = await _fileService.DownloadFile(fileName);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { error = "File not found or not clean" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{fileName}")]
        public async Task<IActionResult> DeleteFile(string fileName)
        {
            try
            {
                await _fileService.DeleteFile(fileName);
                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetFileStatistics()
        {
            try
            {
                var statistics = await _fileService.GetFileStatistics();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}