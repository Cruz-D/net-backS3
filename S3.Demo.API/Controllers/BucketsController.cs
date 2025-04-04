using Amazon.S3;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace S3.Demo.API.Controllers
{
    [Route("api/buckets")]
    [ApiController]
    public class BucketsController : ControllerBase
    {
        private readonly IAmazonS3? _s3Client;

        public BucketsController(IAmazonS3? s3Client)
        {
            _s3Client = s3Client;
        }

        //Gestion de los Buckets de S3

        //Crear un bucket
        [HttpPost("create")]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            try
            {
                var bucketsExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

                if (bucketsExists)
                {
                    return BadRequest($"El bucket {bucketName} ya existe.");
                }
                else
                {
                    var bucketResponse = await _s3Client.PutBucketAsync(bucketName);
                    return Ok($"Bucket {bucketName} creado correctamente.");
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        //Listar los buckets
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllBucketsAsync()
        {
            try
            {
                var buckets = await _s3Client!.ListBucketsAsync();
                var bucketList = buckets.Buckets.Select(b => new
                {
                    Name = b.BucketName,
                }).ToList();

                return Ok(bucketList);
            }
            catch (Exception)
            {
                return BadRequest("Error al listar los buckets.");
                throw;
            }
        }

        //Eliminar un bucket
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBucketAsync(string bucketName)
        {
            try
            {
                var bucketsExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
                if (!bucketsExists)
                {
                    return BadRequest($"El bucket {bucketName} no existe.");
                }
                else
                {
                    var deleteResponse = await _s3Client.DeleteBucketAsync(bucketName);
                    return Ok($"Bucket {bucketName} eliminado correctamente.");
                }
            }
            catch (Exception)
            {
                return BadRequest("Error al eliminar el bucket.");
                throw;
            }
        }


    }
}
