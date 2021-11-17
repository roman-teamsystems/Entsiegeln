using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Entsiegeln.Models;
using Entsiegeln.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Berlin2bgreen.Controllers
{
    [Route("[controller]")]
    [ApiController]
    // Sollte UploadController heißen
    public class FileController : ControllerBase
    {
        public static IWebHostEnvironment _environment;
        private readonly BlobContainerClient _blobContainerClient;
        private readonly EntsiegelnContext _context;
        private readonly ILogger _logger;

        public FileController(EntsiegelnContext context, IWebHostEnvironment environment, BlobContainerClient blobContainerClient, ILogger<ErrorController> logger)
        {
            _environment = environment;
            _blobContainerClient = blobContainerClient;
            _context = context;
            _logger = logger;
        }

        [EnableCors("Policy1")]
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            long size = file.Length;
            Guid guid;
            guid = Guid.NewGuid();
            Bild bild = new Bild();
            bild.Name = guid;
            if (size > 0)
            {
                var filename = guid.ToString() + ".jpeg";
                await AzureUpload(file.OpenReadStream(), filename);
                return CreatedAtAction("Upload", new { name = guid }, bild); ;
            }
            else
            {
                _logger.LogError("File is empty");
                return BadRequest();
            }
        }

        private async Task AzureUpload(Stream streamData, string filename)
        {
            BlobClient blob = _blobContainerClient.GetBlobClient(filename);
            BlobUploadOptions uploadOptions = new BlobUploadOptions();
            BlobHttpHeaders httpHeaders = new BlobHttpHeaders();
            httpHeaders.ContentType = "image/jpeg";
            uploadOptions.HttpHeaders = httpHeaders;
            await blob.UploadAsync(streamData, uploadOptions);
            return;
        }
    }
}
