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

        public List<Post> GetAllPosts()
        {
            var ret = _forumRepository.GetAllPosts();

            var users = _userRepository.GetAll();
            ret.ForEach(x => x.name = users.Find(y => y.empno == x.empno).name);

            return ret;
        }

        public (Post, List<Reply>) GetPost(int id)
        {
            Post post = _forumRepository.GetPost(id);
            List<Reply> replies = _forumRepository.GetReplies(id);

            return (post, replies);
        }

        public bool InsertPost(Post post, string empno)
        {
            post.empno = empno;
            var ret = _forumRepository.InsertPost(post);

            return ret;
        }

        public bool InsertReply(Reply reply, string empno)
        {
            reply.empno = empno;
            var ret = _forumRepository.InsertReply(reply);

            return ret;
        }

        public void Dispose()
        {
            _forumRepository.Dispose();
            _userRepository.Dispose();
        }
    }
}