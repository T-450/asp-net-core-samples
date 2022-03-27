// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.FileProviders;

namespace FileUpload.Services;

public class FileUploadService
{
    private readonly int _bufferSize = 1024;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<FileUploadService> _logger;
    private byte[] _buffer;
    private int _bytesRead;

    public FileUploadService(IFileProvider fileProvider, ILogger<FileUploadService> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
        _buffer = new byte[_bufferSize];
    }

    public async Task UploadAsync(string filename, string filePath, Stream stream)
    {
        try
        {
            using var destStream = new FileStream(Path.Combine(filePath, filename), FileMode.Append);
            _buffer = new byte[_bufferSize];
            do
            {
                _bytesRead = await stream.ReadAsync(_buffer).ConfigureAwait(false);
                await destStream.WriteAsync(_buffer.AsMemory(0, _bytesRead)).ConfigureAwait(false);
            } while (_bytesRead > 0);
        }
        catch (Exception e)
        {
            _logger.LogError(string.Format("{0}", e.Message), nameof(e));
            throw;
        }
    }

    public async Task<FileStream?> GetAsync(string filename, Stream dest, string? filePath = null)
    {
        try
        {
            _buffer = new byte[_bufferSize];
            var fileInfo = _fileProvider.GetFileInfo(filename);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"{filename} was not found", nameof(filename));
            }

            filePath ??= fileInfo.PhysicalPath;
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            ;
            return fileStream;
        }
        catch (Exception e)
        {
            _logger.LogError(string.Format("{0}", e.Message), nameof(e));
            throw;
        }
    }

    public async Task DeleteAsync(string filename, string? filePath = null)
    {
        try
        {
            _buffer = new byte[_bufferSize];
            var fileInfo = _fileProvider.GetFileInfo(filename);
            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException($"{filename} was not found", nameof(filename));
            }

            filePath ??= fileInfo.PhysicalPath;
            Task.Run(() => File.Delete(filePath)).Wait();
        }
        catch (Exception e)
        {
            _logger.LogError(string.Format("{0}", e.Message), nameof(e));
            throw;
        }
    }
}
