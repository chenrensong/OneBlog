using BaiduBce;
using BaiduBce.Auth;
using BaiduBce.Services.Bos;
using OneBlog.Editor;
using OneBlog.Editor.Handlers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Uploader
{
    public class BosUploader : IFileUploder
    {

        public Task<UploadResult> InvokeAsync(string uploadFileName, string savePath, byte[] uploadFileBytes)
        {
            var bosAccessKey = EditorConfig.GetString("BosAccessKey");
            var bosSecretKey = EditorConfig.GetString("BosSecretKey");
            var bosBucket = EditorConfig.GetString("BosBucket");
            var bosEndpoint = EditorConfig.GetString("BosEndpoint");
            var bosDomain = EditorConfig.GetString("BosDomain");
            UploadResult result = new UploadResult() { OriginFileName = uploadFileName, State = UploadState.Unknown };
            try
            {
                BceClientConfiguration clientConfig = new BceClientConfiguration();
                clientConfig.Credentials = new DefaultBceCredentials(bosAccessKey, bosSecretKey);
                clientConfig.Endpoint = bosEndpoint;
                // 设置http最大连接数为10
                clientConfig.ConnectionLimit = 10;
                // 设置tcp连接超时为5000毫秒
                clientConfig.TimeoutInMillis = 5000;
                // 设置读写数据超时的时间为50000毫秒
                clientConfig.ReadWriteTimeoutInMillis = 50000;
                BosClient client = new BosClient(clientConfig);
                client.PutObject(bosBucket, savePath, uploadFileBytes);
                result.Url = bosDomain + "/" + savePath;
                result.State = UploadState.Success;
            }
            catch (Exception e)
            {
                result.State = UploadState.FileAccessError;
                result.ErrorMessage = e.Message;
            }
            finally
            {
            }
            return Task.FromResult(result);
        }

        private bool CheckFileType(UploadConfig config, string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return config.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        private bool CheckFileSize(UploadConfig config, long size)
        {
            return size < config.SizeLimit;
        }
    }
}
