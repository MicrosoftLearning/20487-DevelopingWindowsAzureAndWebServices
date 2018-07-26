using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace BlobStorage.Web.Controllers
{
    public class BlobsController : ApiController
    {
        public HttpResponseMessage Get(string blobName)
        {
            CloudBlobContainer container = ContainerHelper.GetContainer();
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(blobName);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(blockBlob.OpenRead());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.Add("Content-Disposition", "attachment; filename=" + blobName);
            return result;
        }      
    }
}
