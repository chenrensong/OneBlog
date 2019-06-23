using System.Collections.Generic;

namespace OneBlog.Models.ManageViewModels
{
    public class DashboardViewModel
    {
        /// <summary>
        /// Draft posts
        /// </summary>
        public List<PostItem> DraftPosts { get; set; }
        /// <summary>
        /// Post published counter
        /// </summary>
        public int PostPublishedCnt { get; set; }
        /// <summary>
        /// Post drafts counter
        /// </summary>
        public int PostDraftCnt { get; set; }
        /// <summary>
        /// Latest comments
        /// </summary>
        public List<CommentItem> Comments { get; set; }
        /// <summary>
        /// Approved comments counter
        /// </summary>
        public int ApprovedCommentsCnt { get; set; }
        /// <summary>
        /// Pending comments counter
        /// </summary>
        public int PendingCommentsCnt { get; set; }
        /// <summary>
        /// Spam comments counter
        /// </summary>
        public int SpamCommentsCnt { get; set; }
        /// <summary>
        /// Draft pages
        /// </summary>
        //public List<PageItem> DraftPages { get; set; }
        /// <summary>
        /// Published pages counter
        /// </summary>
        public int PagePublishedCnt { get; set; }
        /// <summary>
        /// Draft pages counter
        /// </summary>
        public int PageDraftCnt { get; set; }
        /// <summary>
        /// Trash items counter
        /// </summary>
        public List<TrashItem> Trash { get; set; }


    }
}
