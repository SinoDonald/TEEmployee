using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TEEmployee.Models.Forum
{
    public class ForumService : IDisposable
    {
        private IForumRepository _forumRepository;
        private IUserRepository _userRepository;

        public ForumService()
        {
            _forumRepository = new ForumRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// 取得所有貼文。
        /// </summary>
        /// <returns>包含所有貼文的列舉。</returns>
        public List<Post> GetAllPosts()
        {
            var ret = _forumRepository.GetAllPosts().OrderByDescending(x => x.postDate).ToList();

            var users = _userRepository.GetAll();
            ret.ForEach(x => x.name = users.Find(y => y.empno == x.empno).name);

            return ret;
        }

        /// <summary>
        /// 取得貼文的所有回文。
        /// </summary>
        /// <param name="id">貼文Id</param>
        /// <returns>貼文和回文列舉的數組。</returns>
        public (Post, List<Reply>) GetPost(int id)
        {
            Post post = _forumRepository.GetPost(id);
            List<Reply> replies = _forumRepository.GetReplies(id);

            var users = _userRepository.GetAll();
            post.name = users.Find(y => y.empno == post.empno).name;
            replies.ForEach(x => x.name = users.Find(y => y.empno == x.empno).name);

            return (post, replies);
        }

        /// <summary>
        /// 新增貼文
        /// </summary>
        /// <param name="post">貼文</param>
        /// <param name="empno">員工編號</param>
        /// <returns>是否新增成功</returns>
        public bool InsertPost(Post post, string empno)
        {
            post.empno = empno;
            var ret = _forumRepository.InsertPost(post);

            return ret;
        }

        /// <summary>
        /// 新增回文
        /// </summary>
        /// <param name="reply">回文</param>
        /// <param name="empno">員工編號</param>
        /// <returns>是否新增成功</returns>
        public bool InsertReply(Reply reply, string empno)
        {
            reply.empno = empno;
            var ret = _forumRepository.InsertReply(reply);

            return ret;
        }

        public bool DeleteAll()
        {
            var ret = _forumRepository.DeleteAll();
            return ret;
        }

        public void Dispose()
        {
            _forumRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}