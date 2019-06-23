using OneBlog.Editor.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using OneBlog.Editor;

namespace OneBlog.Uploader
{
    public class PhysicalFileUploder : IFileUploder
    {
        public async Task<UploadResult> InvokeAsync(string uploadFileName, string savePath, byte[] uploadFileBytes)
        {
            UploadResult result = new UploadResult() { State = UploadState.Unknown };
            result.OriginFileName = uploadFileName;
            var localPath = Path.Combine(EditorConfig.WebRootPath, savePath);
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                }
                await File.WriteAllBytesAsync(localPath, uploadFileBytes);
                result.Url = Path.Combine("/", savePath);
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
