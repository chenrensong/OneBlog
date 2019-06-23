
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OneBlog.Editor;
using OneBlog.Uploader;

namespace Microsoft.AspNetCore.Builder
{
    public static class UEditorExtensions
    {
        /// <summary>
        /// 添加UEditor后端服务
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="configFileRelativePath">配置文件相对路径</param>
        /// <param name="isCacheConfig">是否缓存配置文件</param>
        /// <param name="basePath">配置文件、文件存储路径等各种路径的根目录，默认为Web项目的根目录</param>
        public static void AddUEditorServices(this IServiceCollection services,
            UploadProvider uploadProvider = UploadProvider.Physical,
            string configFileRelativePath = "ueditor.json",
            bool isCacheConfig = true,
            string contentRootPath = "",
            string webRootPath = "")
        {

            EditorConfig.ConfigFile = configFileRelativePath;
            EditorConfig.NoCache = isCacheConfig;
            EditorConfig.ContentRootPath = contentRootPath;
            EditorConfig.WebRootPath = webRootPath;
            services.TryAddSingleton<UEditorService>();
            switch (uploadProvider)
            {
                case UploadProvider.Physical:
                    services.AddScoped<IFileUploder, PhysicalFileUploder>();
                    break;
                case UploadProvider.Bos:
                    services.AddScoped<IFileUploder, BosUploader>();
                    break;
                case UploadProvider.Qiniu:
                    services.AddScoped<IFileUploder, QiniuUploader>();
                    break;
                default:
                    break;
            }
        }



    }

    public enum UploadProvider
    {
        Physical,
        Bos,
        Qiniu
    }
}
