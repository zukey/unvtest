using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using System.IO;
using System.Threading.Tasks;

using OpenCvSharp;
using Newtonsoft.Json;
using WebApplication2.Models;
using Swashbuckle.Swagger.Annotations;

namespace WebApplication2.Controllers
{
    public class ImageConvertController : ApiController
    {
        [HttpPost]
        [SwaggerOperationFilter(typeof(UploadFileOperationFilter))]
        public async Task<IHttpActionResult> Post()
        {
            if (Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = await Request.Content.ReadAsMultipartAsync();
            var fileContent = provider.Contents.First(x => x.Headers.ContentDisposition.Name == JsonConvert.SerializeObject("buffer"));
            var buffer = await fileContent.ReadAsByteArrayAsync();

            // 入力画像からIplImageを読み込み
            using (var image = Mat.FromImageData(buffer, ImreadModes.Color))
            {
                // グレースケール化・Cannyエッジ検出
                using (var grayImage = new Mat(image.Size(), MatType.CV_8U))
                using (var cannyImage = new Mat(image.Size(), MatType.CV_8U))
                {
                    Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
                    Cv2.Canny(grayImage, cannyImage, 60, 180);

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
    }
}
