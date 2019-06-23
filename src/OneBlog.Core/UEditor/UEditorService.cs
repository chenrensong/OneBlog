using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OneBlog.Editor.Handlers;
using System;
using System.Threading.Tasks;

namespace OneBlog.Editor
{
    public class UEditorService
    {
        public UEditorService(IHostingEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(EditorConfig.WebRootPath))
            {
                EditorConfig.WebRootPath = env.WebRootPath;// env.ContentRootPath;
            }

            if (string.IsNullOrWhiteSpace(EditorConfig.ContentRootPath))
            {
                EditorConfig.ContentRootPath = env.ContentRootPath;
            }
            EditorConfig.EnvName = env.EnvironmentName;
        }

        /// <summary>
        /// 上传并返回结果，已处理跨域Jsonp请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<UEditorResponse> UploadAndGetResponse(HttpContext context)
        {
            var action = context.Request.Query["action"];
            object result;
            if (AppConsts.Action.Config.Equals(action, StringComparison.OrdinalIgnoreCase))
            {
                var configHandle = new ConfigHandler();
                result = configHandle.Process();
            }
            else
            {
                var handle = HandelFactory.GetHandler(action, context);
                result =await handle.Process();
            }
            string resultJson = JsonConvert.SerializeObject(result, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            string contentType = "text/plain";
            string jsonpCallback = context.Request.Query["callback"];
            if (!String.IsNullOrWhiteSpace(jsonpCallback))
            {
                contentType = "application/javascript";
                resultJson = string.Format("{0}({1});", jsonpCallback, resultJson);
                UEditorResponse response = new UEditorResponse(contentType, resultJson);
                return response;
            }
            else
            {
                UEditorResponse response = new UEditorResponse(contentType, resultJson);
                return response;
            }
        }

        /// <summary>
        /// 单纯的上传并返回结果，未处理跨域Jsonp请求
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Upload(HttpContext context)
        {
            var action = context.Request.Query["action"];
            object result;
            if (AppConsts.Action.Config.Equals(action, StringComparison.OrdinalIgnoreCase))
            {
                result = new ConfigHandler();
            }
            else
            {
                var handle = HandelFactory.GetHandler(action, context);
                result = handle.Process();
            }
            return result;
        }
    }
}
