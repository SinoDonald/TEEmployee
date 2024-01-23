using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Web;

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

        public List<Post> GetAllPosts()
        {            
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                string sql = @"SELECT * FROM Post";

                var ret = _conn.Query<Post>(sql).ToList();

                ret.ForEach(x => x.count = CountReplies(x.id));

                tran.Commit();

                return ret;
            }

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
            string sql = @"INSERT INTO Post (empno, title, content, postDate) 
                    VALUES(@empno, @title, @content, @postDate)";

            var ret = _conn.Execute(sql, post);

            return ret > 0;
        }

        public bool InsertReply(Reply reply)
        {
            string sql = @"INSERT INTO Reply (postId, empno, replyContent, replyDate) 
                    VALUES(@postId, @empno, @replyContent, @replyDate)";

            var ret = _conn.Execute(sql, reply);

            return ret > 0;
        }

        public void Dispose()
        {
            _conn.Close();
            _conn.Dispose();
            return;
        }

    }
}