using BaiduBce;
using BaiduBce.Auth;
using BaiduBce.Services.Bos;
using OneBlog.Editor;
using OneBlog.Editor.Handlers;
using Qiniu.IO;
using Qiniu.RS;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OneBlog.Uploader
{
    public class QiniuUploader : IFileUploder
    {
        public async Task<UploadResult> InvokeAsync(string uploadFileName, string savePath, byte[] uploadFileBytes)
        {
            var qiniuAccessKey = EditorConfig.GetString("QiniuAccessKey");
            var qiniuSecretKey = EditorConfig.GetString("QiniuSecretKey");
            var qiniuBucket = EditorConfig.GetString("QiniuBucket");
            var qiniuUPHost = EditorConfig.GetString("QiniuUPHost");
            var qiniuDomain = EditorConfig.GetString("QiniuDomain");
            Qiniu.Conf.Config.ACCESS_KEY = qiniuAccessKey;
            Qiniu.Conf.Config.SECRET_KEY = qiniuSecretKey;
            Qiniu.Conf.Config.UP_HOST = qiniuUPHost;
            Qiniu.Conf.Config.RS_HOST = qiniuDomain;
            UploadResult result = new UploadResult() { State = UploadState.Unknown };
            try
            {
                var client = new IOClient();
                using (var stram = new MemoryStream(uploadFileBytes))
                {
                    await client.PutAsync(new PutPolicy(qiniuBucket).Token(), savePath, stram, null);
                }
                result.Url = qiniuDomain + "/" + savePath;
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
            return result;
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
