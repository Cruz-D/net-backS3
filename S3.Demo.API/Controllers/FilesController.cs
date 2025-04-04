using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Amazon.S3.Model;
using Amazon.S3.Util;
using S3.Demo.API.Models;

namespace S3.Demo.API.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3? _s3Client;

        public FilesController(IAmazonS3? s3Client)
        {
            _s3Client = s3Client;
        }

        //Subir un archivo
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFileAsync(IFormFile file, string bucketName, string? prefix)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists)
            {
                return BadRequest($"El bucket {bucketName} no existe.");
            }
            else
            {
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                    InputStream = file.OpenReadStream()

                };
                // añadir metadata al archivo
                request.Metadata.Add("Content-Type", file.ContentType);

                await _s3Client!.PutObjectAsync(request);
                return Ok($"Archivo {file.FileName} subido correctamente al bucket {bucketName}.");
            }
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllFileAsync(string bucketName, string? prefix)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists)
            {
                return BadRequest("El bucket no existe.");

            }
            else
            {
                var request = new ListObjectsV2Request()
                {
                    BucketName = bucketName,
                    Prefix = prefix
                };

                var result = await _s3Client!.ListObjectsV2Async(request);

                var s3Objects = result.S3Objects.Select(s =>
                {
                    var urlRequest = new GetPreSignedUrlRequest()
                    {
                        BucketName = bucketName,
                        Key = s.Key,
                        Expires = DateTime.UtcNow.AddMinutes(1)
                    };

                    return new S3ObjectDto()
                    {
                        name = s.Key,
                        presignedUrl = _s3Client.GetPreSignedURL(urlRequest)
                    };
                });
                return Ok(result);
            }
        }

        [HttpGet("get-by-key")]
        public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists)
            {
                return BadRequest("El bucket no existe.");

            }
            else
            {
                var s3Object = await _s3Client!.GetObjectAsync(bucketName, key);
                return File(s3Object.ResponseStream, s3Object.Headers.ContentType, s3Object.Key);
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteFileAsync(string bucketName, string key)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists)
            {
                return BadRequest("El bucket no existe.");
            }
            else
            {
                var deleteRequest = new DeleteObjectRequest()
                {
                    BucketName = bucketName,
                    Key = key
                };
                await _s3Client!.DeleteObjectAsync(deleteRequest);

                return Ok($"Archivo {key} eliminado correctamente del bucket {bucketName}.");
            }
        }
    }
}
