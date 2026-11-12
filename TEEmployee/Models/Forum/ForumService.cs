using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace TEEmployee.Models.Forum
{
    /// <summary>
    /// 圖片驗證失敗時拋出，Message 會直接顯示給使用者
    /// </summary>
    public class ForumImageException : Exception
    {
        public ForumImageException(string message) : base(message) { }
    }

    public class ForumService : IDisposable
    {
        private IForumRepository _forumRepository;
        private IUserRepository _userRepository;

        // ===== 圖片限制 (前端 ForumCtrl.js 也有相同設定) =====
        public const int MaxImagesPerItem = 5;                 // 每篇貼文/回文最多張數
        public const int MaxImageBytes = 8 * 1024 * 1024;     // 單張上限 8 MB
        private const long MaxPixels = 50000000;               // 解析度上限 (防止超大圖耗盡記憶體)
        private const int MaxEdge = 1920;                      // 原圖長邊上限，超過自動縮小
        private const int ThumbEdge = 320;                     // 縮圖長邊
        private const long JpegQuality = 85L;

        public ForumService()
        {
            _forumRepository = new ForumRepository();
            _userRepository = new UserRepository();
        }

        /// <summary>
        /// 圖片實體檔根目錄 (App_Data 不允許從網址直接存取)
        /// </summary>
        private static string ImageRoot
        {
            get { return HostingEnvironment.MapPath("~/App_Data/ForumImages"); }
        }

        /// <summary>
        /// 論壇時間的標準格式 (與既有資料相同)：M/d/yyyy, HH:mm:ss
        /// 新增貼文 / 回文時由伺服器產生，不再採用前端傳來的時間
        /// </summary>
        public const string DateFormat = "M/d/yyyy, HH:mm:ss";

        public static string Now()
        {
            return DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// 解析論壇時間；格式異常時不拋例外 (避免一筆資料讓整個列表失敗)，回傳 DateTime.MinValue
        /// </summary>
        public static DateTime ParseForumDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return DateTime.MinValue;

            string[] formats = { DateFormat, "M/d/yyyy, H:mm:ss", "M/d/yyyy H:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss", "yyyy-MM-dd HH:mm", "yyyy/MM/dd HH:mm" };
            if (DateTime.TryParseExact(value.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d)) return d;

            // 舊版瀏覽器午夜可能產生 "24:xx:xx"
            string fixedValue = System.Text.RegularExpressions.Regex.Replace(value.Trim(), @", 24:", ", 00:");
            if (DateTime.TryParseExact(fixedValue, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out d)) return d;

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d : DateTime.MinValue;
        }

        /// <summary>員工姓名；找不到 (例如已離職) 時不拋例外</summary>
        private static string NameOf(List<User> users, string empno)
        {
            return users.Find(y => y.empno == empno)?.name ?? "（離職同仁）";
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
            ret.ForEach(x => x.name = NameOf(users, x.empno));

            foreach (var item in ret)
            {
                if (item.anonymous)
                {
                    item.name = "匿名";
                    item.empno = "0000";
                }

            }

            // 貼文本身的圖片數量 (列表頁顯示圖示)
            var imageCounts = _forumRepository.GetPostImageCounts();
            foreach (var item in ret)
            {
                int cnt;
                item.imageCount = imageCounts.TryGetValue(item.id, out cnt) ? cnt : 0;
            }

            // count, latest name and date, then sort by latest date
            foreach (var item in ret)
            {
                item.count = item.replies.Count;

                if (item.count > 0)
                {
                    var replies = item.replies.Select(x => new
                    {
                        Parsed = ParseForumDate(x.replyDate).ToString("yyyy-MM-ddTHH:mm:ss"),
                        Empno = x.empno,
                        Anonymous = x.anonymous,
                    }).ToList();

                    var latest = replies.OrderByDescending(x => x.Parsed).First();
                    item.latestName = latest.Anonymous ? "匿名" : NameOf(users, latest.Empno);
                    item.latestDate = latest.Parsed;
                }
                else
                {
                    item.latestName = item.name;
                    item.latestDate = ParseForumDate(item.postDate).ToString("yyyy-MM-ddTHH:mm:ss");
                }

            }

            ret = ret.OrderByDescending(x => x.latestDate).ToList();

            // 回文明細不回傳到列表 (列表只需要回覆數、最新回覆者與時間)；
            // 避免匿名回文的員編從列表 API 外洩
            ret.ForEach(x => x.replies = null);

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
            if (post == null) return (null, new List<Reply>());   // 貼文不存在 (例如已被刪除)
            List<Reply> replies = _forumRepository.GetReplies(id);

            var users = _userRepository.GetAll();
            post.name = NameOf(users, post.empno);
            replies.ForEach(x => x.name = NameOf(users, x.empno));

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

            // 附加圖片 (只輸出前端需要的欄位)
            var images = _forumRepository.GetImagesByPost(id).Select(ToPublic).ToList();
            post.images = images.Where(x => x.replyId == null).ToList();
            replies.ForEach(r => r.images = images.Where(x => x.replyId == r.id).ToList());

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
        /// 新增貼文 (可附圖片)。圖片不合格時拋出 ForumImageException。
        /// </summary>
        /// <returns>新貼文 id</returns>
        public int InsertPost(Post post, string empno, IEnumerable<HttpPostedFileBase> files)
        {
            post.empno = empno;
            var processed = PrepareImages(files, empno);
            var written = new List<string>();

            try
            {
                return _forumRepository.InsertPostWithImages(post, processed.Select(x => x.Meta).ToList(), () => WriteFiles(processed, written));
            }
            catch
            {
                DeletePhysicalFiles(written);
                throw;
            }
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

        /// <summary>
        /// 新增回文 (可附圖片)。圖片不合格時拋出 ForumImageException。
        /// </summary>
        /// <returns>新回文 id</returns>
        public int InsertReply(Reply reply, string empno, IEnumerable<HttpPostedFileBase> files)
        {
            if (reply == null || _forumRepository.GetPost(reply.postId) == null)
                throw new ForumImageException("找不到要回覆的貼文，可能已被刪除。");

            reply.empno = empno;
            var processed = PrepareImages(files, empno);
            var written = new List<string>();

            try
            {
                return _forumRepository.InsertReplyWithImages(reply, processed.Select(x => x.Meta).ToList(), () => WriteFiles(processed, written));
            }
            catch
            {
                DeletePhysicalFiles(written);
                throw;
            }
        }

        /// <summary>
        /// 取得圖片實體檔路徑與型別；找不到時回傳 null
        /// </summary>
        public (string path, string contentType) GetImageFile(int id, bool thumb)
        {
            var img = _forumRepository.GetImage(id);
            if (img == null) return (null, null);

            string rel = thumb ? img.thumbName : img.fileName;
            string path = ToSafePath(rel);
            if (path == null || !File.Exists(path)) return (null, null);

            string contentType = thumb ? ContentTypeFromExtension(path) : img.contentType;
            return (path, contentType);
        }

        public bool DeletePost(Post post)
        {
            var images = _forumRepository.GetImagesByPost(post.id);
            var ret = _forumRepository.DeletePost(post);
            if (ret) DeleteImageFiles(images);
            return ret;
        }
        /// <summary>
        /// 刪除回覆
        /// </summary>
        /// <param name="post"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public bool DeleteReply(int postId, int id)
        {
            var images = _forumRepository.GetImagesByReply(postId, id);
            var ret = _forumRepository.DeleteReply(postId, id);
            if (ret) DeleteImageFiles(images);
            return ret;
        }

        public bool DeleteAll()
        {
            var images = _forumRepository.GetAllImages();
            var ret = _forumRepository.DeleteAll();
            if (ret) DeleteImageFiles(images);
            return ret;
        }

        public void Dispose()
        {
            _forumRepository.Dispose();
            _userRepository.Dispose();
        }

        // =====================================================================
        //                              圖片處理
        // =====================================================================

        private class ProcessedImage
        {
            public PostImage Meta;
            public byte[] Full;
            public byte[] Thumb;
        }

        /// <summary>
        /// 驗證並重新編碼所有上傳圖片 (全部在記憶體中完成，任何一張不合格就整筆拒絕)
        /// </summary>
        private List<ProcessedImage> PrepareImages(IEnumerable<HttpPostedFileBase> files, string empno)
        {
            var list = (files ?? Enumerable.Empty<HttpPostedFileBase>())
                .Where(f => f != null && f.ContentLength > 0)
                .ToList();

            if (list.Count > MaxImagesPerItem)
                throw new ForumImageException($"每次最多只能附加 {MaxImagesPerItem} 張圖片。");

            var month = DateTime.Now.ToString("yyyyMM");
            var result = new List<ProcessedImage>();

            foreach (var file in list)
            {
                if (file.ContentLength > MaxImageBytes)
                    throw new ForumImageException($"單張圖片不可超過 {MaxImageBytes / 1024 / 1024} MB。");

                byte[] bytes;
                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                result.Add(ProcessImage(bytes, month, empno));
            }

            return result;
        }

        /// <summary>
        /// 依檔案內容 (非副檔名) 判斷格式，重新繪製成新圖檔：
        /// 1. 移除 EXIF (GPS、拍攝裝置、作者等) 以保護匿名
        /// 2. 依 EXIF 方向轉正手機照片
        /// 3. 長邊超過 1920px 時縮小，並產生縮圖
        /// GIF 保留原檔以維持動畫 (GIF 不含 EXIF)。
        /// </summary>
        private ProcessedImage ProcessImage(byte[] bytes, string month, string empno)
        {
            string kind = DetectImageKind(bytes);
            if (kind == null)
                throw new ForumImageException("只接受 JPG、PNG、GIF 格式的圖片。");

            System.Drawing.Image src;
            try
            {
                src = System.Drawing.Image.FromStream(new MemoryStream(bytes), false, false);
            }
            catch (ArgumentException)
            {
                throw new ForumImageException("圖片檔案已損毀或格式不正確。");
            }

            using (src)
            {
                if ((long)src.Width * src.Height > MaxPixels)
                    throw new ForumImageException("圖片解析度過大，請縮小後再上傳。");

                string guid = Guid.NewGuid().ToString("N");
                byte[] full, thumb;
                string contentType, ext, thumbExt;
                int width, height;

                if (kind == "gif")
                {
                    full = bytes;
                    contentType = "image/gif";
                    ext = ".gif";
                    width = src.Width;
                    height = src.Height;
                    thumb = Render(src, ThumbEdge, false);
                    thumbExt = ".png";
                }
                else
                {
                    bool isJpeg = kind == "jpeg";
                    if (isJpeg) ApplyExifOrientation(src);

                    full = Render(src, MaxEdge, isJpeg);
                    thumb = Render(src, ThumbEdge, isJpeg);
                    contentType = isJpeg ? "image/jpeg" : "image/png";
                    ext = isJpeg ? ".jpg" : ".png";
                    thumbExt = ext;

                    var size = ScaledSize(src.Width, src.Height, MaxEdge);
                    width = size.Width;
                    height = size.Height;
                }

                return new ProcessedImage
                {
                    Full = full,
                    Thumb = thumb,
                    Meta = new PostImage
                    {
                        fileName = month + "/" + guid + ext,
                        thumbName = month + "/" + guid + "_t" + thumbExt,
                        contentType = contentType,
                        width = width,
                        height = height,
                        size = full.Length,
                        empno = empno,
                        createdAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
                    }
                };
            }
        }

        private static string DetectImageKind(byte[] b)
        {
            if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return "jpeg";
            if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47
                              && b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A) return "png";
            if (b.Length >= 6 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8'
                              && (b[4] == '7' || b[4] == '9') && b[5] == 'a') return "gif";
            return null;
        }

        private static void ApplyExifOrientation(System.Drawing.Image img)
        {
            const int OrientationId = 0x0112;
            if (!img.PropertyIdList.Contains(OrientationId)) return;

            var prop = img.GetPropertyItem(OrientationId);
            if (prop.Value == null || prop.Value.Length < 2) return;

            // Windows GDI+ 以 little-endian 回傳；保險起見若值不在 1~8 再以 big-endian 解讀
            int orientation = BitConverter.ToUInt16(prop.Value, 0);
            if (orientation < 1 || orientation > 8) orientation = (prop.Value[0] << 8) | prop.Value[1];

            switch (orientation)
            {
                case 2: img.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                case 3: img.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                case 4: img.RotateFlip(RotateFlipType.Rotate180FlipX); break;
                case 5: img.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                case 6: img.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                case 7: img.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                case 8: img.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
            }
        }

        private static Size ScaledSize(int w, int h, int maxEdge)
        {
            int longest = Math.Max(w, h);
            if (longest <= maxEdge) return new Size(w, h);
            double scale = (double)maxEdge / longest;
            return new Size(Math.Max(1, (int)Math.Round(w * scale)), Math.Max(1, (int)Math.Round(h * scale)));
        }

        /// <summary>
        /// 繪製到全新的 Bitmap 再存檔：新圖不帶任何原檔中繼資料
        /// </summary>
        private static byte[] Render(System.Drawing.Image src, int maxEdge, bool asJpeg)
        {
            var size = ScaledSize(src.Width, src.Height, maxEdge);
            var format = asJpeg ? PixelFormat.Format24bppRgb : PixelFormat.Format32bppArgb;

            using (var bmp = new Bitmap(size.Width, size.Height, format))
            {
                bmp.SetResolution(96, 96);
                using (var g = Graphics.FromImage(bmp))
                {
                    if (asJpeg) g.Clear(Color.White);
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    using (var attr = new ImageAttributes())
                    {
                        attr.SetWrapMode(WrapMode.TileFlipXY); // 避免邊緣出現細線
                        g.DrawImage(src, new Rectangle(0, 0, size.Width, size.Height), 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, attr);
                    }
                }

                using (var outMs = new MemoryStream())
                {
                    if (asJpeg)
                    {
                        var codec = ImageCodecInfo.GetImageEncoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                        using (var ps = new EncoderParameters(1))
                        {
                            ps.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality);
                            bmp.Save(outMs, codec, ps);
                        }
                    }
                    else
                    {
                        bmp.Save(outMs, ImageFormat.Png);
                    }
                    return outMs.ToArray();
                }
            }
        }

        /// <summary>
        /// 在資料庫交易內寫入實體檔 (written 記錄已寫入路徑，失敗時用來清除)
        /// </summary>
        private static void WriteFiles(List<ProcessedImage> processed, List<string> written)
        {
            foreach (var p in processed)
            {
                string full = ToSafePath(p.Meta.fileName);
                string thumb = ToSafePath(p.Meta.thumbName);
                Directory.CreateDirectory(Path.GetDirectoryName(full));

                File.WriteAllBytes(full, p.Full);
                written.Add(full);
                File.WriteAllBytes(thumb, p.Thumb);
                written.Add(thumb);
            }
        }

        private static void DeleteImageFiles(IEnumerable<PostImage> images)
        {
            var paths = new List<string>();
            foreach (var img in images)
            {
                paths.Add(ToSafePath(img.fileName));
                paths.Add(ToSafePath(img.thumbName));
            }
            DeletePhysicalFiles(paths);
        }

        private static void DeletePhysicalFiles(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                try
                {
                    if (path != null && File.Exists(path)) File.Delete(path);
                }
                catch (Exception)
                {
                    // 檔案被鎖定等情況不影響主流程，留待日後清理
                }
            }
        }

        /// <summary>
        /// 相對路徑轉實體路徑，並確保不會跳出 ForumImages 目錄
        /// </summary>
        private static string ToSafePath(string relative)
        {
            if (string.IsNullOrEmpty(relative)) return null;

            string root = Path.GetFullPath(ImageRoot).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));

            return full.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? full : null;
        }

        private static string ContentTypeFromExtension(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".jpg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                default: return "application/octet-stream";
            }
        }

        /// <summary>
        /// 只保留前端需要的欄位，不輸出檔名與上傳者
        /// </summary>
        private static PostImage ToPublic(PostImage x)
        {
            return new PostImage { id = x.id, postId = x.postId, replyId = x.replyId, width = x.width, height = x.height };
        }
    }
}
