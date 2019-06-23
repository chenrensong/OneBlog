using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OneBlog.Editor.Handlers
{
    public abstract class Handler
    {
        public Handler(HttpContext context)
        {
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
        }

        public abstract Task<UEditorResult> Process();

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
    }
}