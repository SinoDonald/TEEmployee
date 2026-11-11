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

        // PostImage 資料表只需在應用程式啟動後建立一次
        private static bool _schemaReady = false;
        private static readonly object _schemaLock = new object();

        public ForumRepository()
        {
            string forumConnection = ConfigurationManager.ConnectionStrings["ForumConnection"].ConnectionString;
            _conn = new SQLiteConnection(forumConnection);
            EnsureImageTable();
        }

        /// <summary>
        /// 若 PostImage 資料表不存在則自動建立 (不需手動改資料庫)
        /// </summary>
        private void EnsureImageTable()
        {
            if (_schemaReady) return;

            lock (_schemaLock)
            {
                if (_schemaReady) return;

                _conn.Execute(@"
                    CREATE TABLE IF NOT EXISTS PostImage (
                        id          INTEGER PRIMARY KEY AUTOINCREMENT,
                        postId      INTEGER NOT NULL,
                        replyId     INTEGER NULL,
                        fileName    TEXT    NOT NULL,
                        thumbName   TEXT    NOT NULL,
                        contentType TEXT    NOT NULL,
                        width       INTEGER NOT NULL,
                        height      INTEGER NOT NULL,
                        size        INTEGER NOT NULL,
                        empno       TEXT    NULL,
                        createdAt   TEXT    NOT NULL
                    );
                    CREATE INDEX IF NOT EXISTS IX_PostImage_postId ON PostImage(postId);");

                _schemaReady = true;
            }
        }

        //public List<Post> GetAllPosts()
        //{
        //    string sql = @"SELECT * FROM Post";
        //    var ret = _conn.Query<Post>(sql).ToList();

        //    return ret;
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

        // ================= 圖片 =================

        private const string InsertImageSql = @"
            INSERT INTO PostImage (postId, replyId, fileName, thumbName, contentType, width, height, size, empno, createdAt)
            VALUES (@postId, @replyId, @fileName, @thumbName, @contentType, @width, @height, @size, @empno, @createdAt)";

        /// <summary>
        /// 新增貼文與圖片：貼文、實體檔、圖片紀錄在同一交易內完成，任何一步失敗都會回滾
        /// </summary>
        public int InsertPostWithImages(Post post, List<PostImage> images, Action saveFiles)
        {
            _conn.Open();
            try
            {
                using (var tran = _conn.BeginTransaction())
                {
                    _conn.Execute(@"INSERT INTO Post (empno, title, content, postDate, anonymous)
                        VALUES(@empno, @title, @content, @postDate, @anonymous)", post, tran);
                    int postId = (int)_conn.ExecuteScalar<long>("SELECT last_insert_rowid()", null, tran);

                    if (images.Count > 0)
                    {
                        saveFiles?.Invoke();
                        foreach (var img in images)
                        {
                            img.postId = postId;
                            img.replyId = null;
                            _conn.Execute(InsertImageSql, img, tran);
                        }
                    }

                    tran.Commit();
                    return postId;
                }
            }
            finally
            {
                _conn.Close();
            }
        }

        /// <summary>
        /// 新增回文與圖片 (同一交易)
        /// </summary>
        public int InsertReplyWithImages(Reply reply, List<PostImage> images, Action saveFiles)
        {
            _conn.Open();
            try
            {
                using (var tran = _conn.BeginTransaction())
                {
                    _conn.Execute(@"INSERT INTO Reply (postId, empno, replyContent, replyDate, anonymous)
                        VALUES(@postId, @empno, @replyContent, @replyDate, @anonymous)", reply, tran);
                    int replyId = (int)_conn.ExecuteScalar<long>("SELECT last_insert_rowid()", null, tran);

                    if (images.Count > 0)
                    {
                        saveFiles?.Invoke();
                        foreach (var img in images)
                        {
                            img.postId = reply.postId;
                            img.replyId = replyId;
                            _conn.Execute(InsertImageSql, img, tran);
                        }
                    }

                    tran.Commit();
                    return replyId;
                }
            }
            finally
            {
                _conn.Close();
            }
        }

        public List<PostImage> GetImagesByPost(int postId)
        {
            return _conn.Query<PostImage>(@"SELECT * FROM PostImage WHERE postId=@postId ORDER BY id", new { postId }).ToList();
        }

        public List<PostImage> GetImagesByReply(int postId, int replyId)
        {
            return _conn.Query<PostImage>(@"SELECT * FROM PostImage WHERE postId=@postId AND replyId=@replyId", new { postId, replyId }).ToList();
        }

        public PostImage GetImage(int id)
        {
            return _conn.Query<PostImage>(@"SELECT * FROM PostImage WHERE id=@id", new { id }).FirstOrDefault();
        }

        public List<PostImage> GetAllImages()
        {
            return _conn.Query<PostImage>(@"SELECT * FROM PostImage").ToList();
        }

        public Dictionary<int, int> GetPostImageCounts()
        {
            return _conn.Query<ImageCountRow>(@"SELECT postId, COUNT(*) AS cnt FROM PostImage WHERE replyId IS NULL GROUP BY postId")
                        .ToDictionary(x => (int)x.postId, x => (int)x.cnt);
        }

        public class ImageCountRow
        {
            public long postId { get; set; }
            public long cnt { get; set; }
        }

        // ================= 刪除 =================

        public bool DeleteAll()
        {
            _conn.Open();

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(@"DELETE FROM PostImage", null, tran);
                    _conn.Execute(@"DELETE FROM Reply", null, tran);
                    _conn.Execute(@"DELETE FROM Post", null, tran);

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
            string deleteImageSql = @"DELETE FROM PostImage WHERE postId=@id;";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(deleteImageSql, new { post.id }, tran);
                    _conn.Execute(deleteReplySql, new { post.id }, tran);
                    _conn.Execute(deletePostSql, new { post.id }, tran);

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
            string deleteImageSql = @"DELETE FROM PostImage WHERE postId=@postId AND replyId=@id";

            using (var tran = _conn.BeginTransaction())
            {
                try
                {
                    _conn.Execute(deleteImageSql, new { postId, id }, tran);
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
