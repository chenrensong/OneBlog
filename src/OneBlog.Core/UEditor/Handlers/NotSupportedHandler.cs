using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OneBlog.Editor.Handlers
{
    /// <summary>
    /// NotSupportedHandler 的摘要说明
    /// </summary>
    public class NotSupportedHandler : Handler
    {
        public NotSupportedHandler(HttpContext context)
            : base(context)
        {
        }

        public override async Task<UEditorResult> Process()
        {
            return await Task.FromResult(new UEditorResult
            {
                State = "action 参数为空或者 action 不被支持。"
            });
        }
    }
}