using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using OpenCvSharp;
using Newtonsoft.Json;
using WebApplication3.Models;
using Swashbuckle.Swagger.Annotations;

namespace WebApplication3.Controllers
{
    public class ImageConvert2Controller : ApiController
    {
        [HttpPost]
        [SwaggerOperationFilter(typeof(UploadFileOperationFilter))]
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                if (Request.Content.IsMimeMultipartContent() == false)
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var provider = await Request.Content.ReadAsMultipartAsync();
                var fileContent = provider.Contents.First(x => x.Headers.ContentDisposition.Name == JsonConvert.SerializeObject("buffer"));
                var buffer = await fileContent.ReadAsByteArrayAsync();


                // 入力画像からIplImageを読み込み
                using (var image = IplImage.FromImageData(buffer, LoadMode.Color))
                {
                    // グレースケール化・Cannyエッジ検出
                    using (var grayImage = new IplImage(image.Size, BitDepth.U8, 1))
                    using (var cannyImage = new IplImage(image.Size, BitDepth.U8, 1))
                    {
                        Cv.CvtColor(image, grayImage, ColorConversion.BgrToGray);
                        Cv.Canny(grayImage, cannyImage, 60, 180);

                        // Canny画像をPNGエンコードでバイト配列に変換し、さらにBase64エンコードする
                        byte[] cannyBytes = cannyImage.ToBytes(".png");
                        string base64 = Convert.ToBase64String(cannyBytes);

                        using (var fs = File.Create(Path.Combine(Path.GetTempPath(), "temp.png")))
                        {
                            fs.Write(cannyBytes, 0, cannyBytes.Length);
                        }

                        return Ok();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("例外");
                Debug.Print(ex.ToString());
                throw;
            }
        }
    }
}
