using Newtonsoft.Json.Linq;

namespace OneBlog.Editor.Handlers
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public class ConfigHandler
    {
        public JObject Process()
        {
            return EditorConfig.Items;
        }
    }
}