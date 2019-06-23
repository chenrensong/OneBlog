namespace OneBlog.Models
{
    /// <summary>
    /// Author profile for json serialization
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// photo url
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }

    }
}