using System.Net;
using FileUpload.Filters;
using FileUpload.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace FileUpload.Controllers;

[ApiController]
[Route("[controller]")]
public class UploadController : ControllerBase
{
    private readonly long _fileSizeLimit;
    private readonly FileUploadService _fileUploadService;
    private readonly ILogger<UploadController> _logger;
    private readonly int _maxAllowedFileSize;
    private readonly string[] _permittedExtensions = {".jpg", ".png", "jpeg", ".mkv"};
    private readonly string _targetFilePath;

    public UploadController(ILogger<UploadController> logger, IConfiguration config,
        FileUploadService fileUploadService)
    {
        _logger = logger;
        _fileUploadService = fileUploadService;
        _targetFilePath = Path.Combine(Directory.GetCurrentDirectory(), config["StoredFilesPath"]);
        _fileSizeLimit = long.MaxValue;
        _maxAllowedFileSize = config.GetValue<int>("MaxFileSize");
    }

    /// <summary>
    ///     Get a file from fileSystem
    /// </summary>
    /// <returns></returns>
    [HttpGet("{filename}")]
    public async Task<IActionResult> Download(string filename)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = PullErrors(ModelState);
                BadRequest(errors);
            }

            var fileStream = await _fileUploadService.GetAsync(filename, null).ConfigureAwait(false);
            if (fileStream == null)
            {
                return BadRequest();
            }

            return File(fileStream, "application/octet-stream");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, nameof(e));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "The house is burning down!");
        }
    }


    /// <summary>
    ///     Action to upload large file
    /// </summary>
    /// <remarks>
    ///     Request to this action will not trigger any model binding or model validation,
    ///     because this is a no-argument action
    /// </remarks>
    /// <returns></returns>
    // Set the limit to 1.536 GB
    [HttpPost]
    [ServiceFilter(typeof(ValidateMimeMultipartContentFilterService))]
    [DisableFormValueModelBinding]
    public async Task<IActionResult> Upload()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = PullErrors(ModelState);
                return BadRequest(errors);
            }

            var mediaTypeheader = MediaTypeHeaderValue.Parse(Request.ContentType);
            var boundary = HeaderUtilities.RemoveQuotes(mediaTypeheader.Boundary).Value;
            //
            var reader = new MultipartReader(boundary, Request.Body);
            var section = await reader.ReadNextSectionAsync().ConfigureAwait(false);
            //
            _ = ContentDispositionHeaderValue.TryParse(section!.ContentDisposition, out var contentDisposition);
            if (contentDisposition == null)
            {
                return BadRequest();
            }

            var trustedFileNameForDisplay = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(trustedFileNameForDisplay).ToLowerInvariant()}";
            await _fileUploadService.UploadAsync(fileName, _targetFilePath, section.Body).ConfigureAwait(false);
            return Ok("File Upload: success");
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, nameof(e));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "The house is burning down!");
        }
    }

    /// <summary>
    ///     Delete a file from fileSystem
    /// </summary>
    /// <returns></returns>
    [HttpDelete("{filename}")]
    public async Task<IActionResult> Delete(string filename)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = PullErrors(ModelState);
                return BadRequest(errors);
            }

            await _fileUploadService.DeleteAsync(filename).ConfigureAwait(false);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, nameof(e));
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                "The house is burning down!");
        }
    }

    private static IEnumerable<string> PullErrors(ModelStateDictionary modelStateDictionary)
    {
        return modelStateDictionary.Values
            .SelectMany(v =>
                v.Errors.Select(e => e.ErrorMessage));
    }
}
