﻿using System;
using System.Collections.Generic;
using System.Globalization;
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
            //var ret = _forumRepository.GetAllPosts().OrderByDescending(x => x.postDate).ToList();
            var ret = _forumRepository.GetAllPosts();

            var users = _userRepository.GetAll();
            ret.ForEach(x => x.name = users.Find(y => y.empno == x.empno).name);

            foreach (var item in ret)
            {
                if (item.anonymous)
                {
                    item.name = "匿名";
                    item.empno = "0000";
                }
                
            }

            // count, latest name and date, then sort by latest date
            foreach (var item in ret)
            {
                item.count = item.replies.Count;

                if (item.count > 0)
                {
                    var replies = item.replies.Select(x => new
                    {
                        Parsed = DateTime.ParseExact(x.replyDate, "M/d/yyyy, HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Empno = x.empno,
                        Anonymous = x.anonymous,
                    }).ToList();

                    var latest = replies.OrderByDescending(x => x.Parsed).First();                    
                    item.latestName = latest.Anonymous ? "匿名" : users.Find(x => x.empno == latest.Empno).name;
                    item.latestDate = latest.Parsed;
                }
                else
                {
                    item.latestName = item.name;
                    item.latestDate = DateTime.ParseExact(item.postDate, "M/d/yyyy, HH:mm:ss", CultureInfo.InvariantCulture).ToString("yyyy-MM-ddTHH:mm:ss");
                }

            }

            ret = ret.OrderByDescending(x => x.latestDate).ToList();

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

            if (post.anonymous)
            {
                post.name = "匿名";
                post.empno = "0000";
            }
            
            foreach (var item in replies)
            {
                if (item.anonymous)
                {
                    item.name = "匿名";
                    item.empno = "0000";
                }

            }


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

        public bool DeletePost(Post post)
        {
            var ret = _forumRepository.DeletePost(post);
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