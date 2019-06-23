using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OneBlog.Editor.Handlers
{
    public class HandelFactory
    {
        public static Handler GetHandler(string action, HttpContext context)
        {
            switch (action)
            {
                case AppConsts.Action.UploadImage:
                    return new UploadHandler(context, new UploadConfig
                    {
                        AllowExtensions = EditorConfig.GetStringList("imageAllowFiles"),
                        UrlPrefix= EditorConfig.GetString("imageUrlPrefix"),
                        PathFormat = EditorConfig.GetString("imagePathFormat"),
                        SizeLimit = EditorConfig.GetInt("imageMaxSize"),
                        UploadFieldName = EditorConfig.GetString("imageFieldName")
                    });
                case AppConsts.Action.UploadScrawl:
                    return new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = new string[] { ".png" },
                        UrlPrefix = EditorConfig.GetString("scrawlUrlPrefix"),
                        PathFormat = EditorConfig.GetString("scrawlPathFormat"),
                        SizeLimit = EditorConfig.GetInt("scrawlMaxSize"),
                        UploadFieldName = EditorConfig.GetString("scrawlFieldName"),
                        Base64 = true,
                        Base64Filename = "scrawl.png"
                    });
                case AppConsts.Action.UploadVideo:
                    return new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = EditorConfig.GetStringList("videoAllowFiles"),
                        UrlPrefix = EditorConfig.GetString("videoUrlPrefix"),
                        PathFormat = EditorConfig.GetString("videoPathFormat"),
                        SizeLimit = EditorConfig.GetInt("videoMaxSize"),
                        UploadFieldName = EditorConfig.GetString("videoFieldName")
                    });
                case AppConsts.Action.UploadFile:
                    return new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = EditorConfig.GetStringList("fileAllowFiles"),
                        UrlPrefix = EditorConfig.GetString("fileUrlPrefix"),
                        PathFormat = EditorConfig.GetString("filePathFormat"),
                        SizeLimit = EditorConfig.GetInt("fileMaxSize"),
                        UploadFieldName = EditorConfig.GetString("fileFieldName")
                    });

                case AppConsts.Action.ListImage:
                    return new ListFileManager(context, EditorConfig.GetString("imageManagerListPath"), EditorConfig.GetStringList("imageManagerAllowFiles"));
                case AppConsts.Action.ListFile:
                    return new ListFileManager(context, EditorConfig.GetString("fileManagerListPath"), EditorConfig.GetStringList("fileManagerAllowFiles"));
                case AppConsts.Action.CatchImage:
                    return new CrawlerHandler(context);
                default:
                    return new NotSupportedHandler(context);
            }
        }
    }
}
