using Microsoft.AspNetCore.Mvc;

namespace OneBlog.Editor
{
    public class UEditorResponse
    {
       
        public UEditorResponse(string contentType, string result)
        {
            ContentType = contentType;
            Result = result;
        }

        public string ContentType { get; set; }

        public string Result { get; set; }
    }
}
