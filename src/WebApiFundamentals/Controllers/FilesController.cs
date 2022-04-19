using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace WebApiFundamentals.Controllers;

using static ArgumentNullException;

[Route("api/[controller]")]
[Authorize]
[ApiController]
public class FilesController : ControllerBase
{
    private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider;

    public FilesController(FileExtensionContentTypeProvider provider)
    {
        ThrowIfNull(provider, nameof(provider));
        _fileExtensionContentTypeProvider = provider;
    }

    [HttpGet("{fileId}")]
    public ActionResult GetFile(string fileId)
    {
        // look up the actual file, depending on the fileId...
        var pathToFile = "some-file.pdf";

        // check whether the file exists
        if (!System.IO.File.Exists(pathToFile))
        {
            return NotFound();
        }

        var hasContentType = _fileExtensionContentTypeProvider
            .TryGetContentType(pathToFile, out var contentType);
        if (!hasContentType)
        {
            contentType = "application/octet-stream";
        }

        var bytes = System.IO.File.ReadAllBytes(pathToFile);
        return File(bytes, contentType!, Path.GetFileName(pathToFile));
    }
}
