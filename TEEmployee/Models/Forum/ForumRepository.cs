using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using TEEmployee.Models.Profession;

namespace TEEmployee.Models.Forum
{
    public class ForumRepository : IForumRepository
    {
        private IDbConnection _conn;

        public ForumRepository()
        {
            string forumConnection = ConfigurationManager.ConnectionStrings["ForumConnection"].ConnectionString;
            _conn = new SQLiteConnection(forumConnection);
        }

        //public List<Post> GetAllPosts()
        //{
        //    string sql = @"SELECT * FROM Post";
        //    var ret = _conn.Query<Post>(sql).ToList();

        //    return ret;
        //}

        //public List<Post> GetAllPosts()
        //{            
        //    _conn.Open();

        //    using (var tran = _conn.BeginTransaction())
        //    {
        //        string sql = @"SELECT * FROM Post";

        //        var ret = _conn.Query<Post>(sql).ToList();

        //        ret.ForEach(x => x.count = CountReplies(x.id));

        //        tran.Commit();

        //        return ret;
        //    }

        //}

        public List<Post> GetAllPosts()
        {
           
            var lookup = new Dictionary<int, Post>();
            _conn.Query<Post, Reply, Post>(@"
                SELECT p.*, r.*
                FROM Post AS p
                LEFT JOIN Reply AS r ON p.id = r.postId",
                (p, r) =>
                {
                    Post post;

                    if (!lookup.TryGetValue(p.id, out post))
                        lookup.Add(p.id, post = p);
                    if (post.replies == null)
                        post.replies = new List<Reply>();
                    if (r != null)
                        post.replies.Add(r);
                    return post;
                }).AsQueryable();
            var resultList = lookup.Values;

            return resultList.ToList();

        }

        public Post GetPost(int id)
        {
            string sql = @"SELECT * FROM Post WHERE id=@id";
            var ret = _conn.Query<Post>(sql, new { id }).FirstOrDefault();

            return ret;
        }

        public List<Reply> GetReplies(int postId)
        {
            string sql = @"SELECT * FROM Reply WHERE postId=@postId";
            var ret = _conn.Query<Reply>(sql, new { postId }).ToList();

            return ret;
        }

        private int CountReplies(int postId)
        {
            string sql = @"SELECT COUNT(*) FROM Reply WHERE postId=@postId";
            var ret = _conn.ExecuteScalar<int>(sql, new { postId });

            return ret;
        }

        public bool InsertPost(Post post)
        {            
            string sql = @"INSERT INTO Post (empno, title, content, postDate, anonymous) 
                    VALUES(@empno, @title, @content, @postDate, @anonymous)";

            var ret = _conn.Execute(sql, post);

            return ret > 0;
        }

        public bool InsertReply(Reply reply)
        {
            string sql = @"INSERT INTO Reply (postId, empno, replyContent, replyDate, anonymous) 
                    VALUES(@postId, @empno, @replyContent, @replyDate, @anonymous)";

            var ret = _conn.Execute(sql, reply);

            return ret > 0;
        }

        public bool DeleteAll()
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(@"DELETE FROM Reply", tran);
                    _conn.Execute(@"DELETE FROM Post", tran);

                    tran.Commit();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }

        }

        public bool DeletePost(Post post)
        {
            _conn.Open();

            bool ret = true;
            string deletePostSql = @"DELETE FROM Post WHERE id=@id;";
            string deleteReplySql = @"DELETE FROM Reply WHERE postId=@id;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(deleteReplySql, post);
                    _conn.Execute(deletePostSql, post);

                    tran.Commit();
                }
                catch (Exception)
                {
                    ret = false;
                }

            }

            return ret;
        }
        /// <summary>
        /// 刪除回覆
        /// </summary>
        /// <param name="post"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteReply(int postId, int id)
        {
            _conn.Open();

            string deleteReplySql = @"DELETE FROM Reply WHERE postId=@postId AND id=@id";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    int rows = _conn.Execute(deleteReplySql, new { postId, id }, tran);
                    tran.Commit();
                    return rows > 0; // 是否真的刪除到資料
                }
                catch
                {
                    return false;
                }
            }
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }
    }
}