using Microsoft.AspNetCore.Mvc;
using OneBlog.Editor;
using System.Threading.Tasks;

namespace OneBlog.Controllers
{
    [Route("ueditor")]
    public class UEditorController : Controller
    {
        private readonly UEditorService _ueditorService;
        public UEditorController(UEditorService ueditorService)
        {
            this._ueditorService = ueditorService;
        }

        [Route("upload")]
        [RequestSizeLimit(2147483648)]
        [HttpGet, HttpPost]
        public async Task<ContentResult> Upload()
        {
            var response = await _ueditorService.UploadAndGetResponse(HttpContext);
            return Content(response.Result, response.ContentType);
        }
    }
}
