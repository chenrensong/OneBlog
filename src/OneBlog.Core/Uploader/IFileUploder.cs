using System.Threading.Tasks;
using OneBlog.Editor.Handlers;

namespace OneBlog.Uploader
{
    public interface IFileUploder
    {
        Task<UploadResult> InvokeAsync(string uploadFileName, string savePath, byte[] uploadFileBytes);
    }
}