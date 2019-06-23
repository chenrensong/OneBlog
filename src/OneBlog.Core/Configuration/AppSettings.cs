namespace OneBlog.Configuration
{
    public class AppSettings
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public int PostPerPage { get; set; }

        public string Host { get; set; }

        public string Theme { get; set; }

        public bool TestData { get; set; }

        public EditorType EditorType { get; set; }
    }

    public enum EditorType
    {
        Markdown,
        Html
    }
}
