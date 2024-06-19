using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
