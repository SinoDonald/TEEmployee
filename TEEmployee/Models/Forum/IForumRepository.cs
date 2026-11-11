using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace TEEmployee.Models.Forum
{
    interface IForumRepository : IDisposable
    {
        List<Post> GetAllPosts();
        Post GetPost(int id);
        List<Reply> GetReplies(int postId);
        bool InsertPost(Post post);
        bool InsertReply(Reply reply);
        bool DeleteAll();
        bool DeletePost(Post post);
        bool DeleteReply(int postId, int id);

        // ===== 圖片 =====

        /// <summary>新增貼文與其圖片 (同一交易)，回傳新貼文 id；saveFiles 在交易內寫入實體檔</summary>
        int InsertPostWithImages(Post post, List<PostImage> images, Action saveFiles);
        /// <summary>新增回文與其圖片 (同一交易)，回傳新回文 id</summary>
        int InsertReplyWithImages(Reply reply, List<PostImage> images, Action saveFiles);
        /// <summary>取得某貼文 (含其所有回文) 的圖片</summary>
        List<PostImage> GetImagesByPost(int postId);
        /// <summary>取得單張圖片</summary>
        PostImage GetImage(int id);
        /// <summary>各貼文本身的圖片數量 (postId → count)</summary>
        Dictionary<int, int> GetPostImageCounts();
        /// <summary>取得某回文的圖片</summary>
        List<PostImage> GetImagesByReply(int postId, int replyId);
        /// <summary>取得全部圖片</summary>
        List<PostImage> GetAllImages();
    }
}
