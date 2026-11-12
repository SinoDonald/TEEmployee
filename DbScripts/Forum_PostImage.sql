-- =====================================================================
-- 我要抱抱：附加圖片資料表 (Forum.db)
--
-- 說明：
--   此資料表在網站第一次開啟「我要抱抱」時，程式就會自動建立
--   (ForumRepository.EnsureImageTable)，正常情況下不需要手動執行。
--   只有在主機不允許網站程式修改資料庫結構、或想事先建立時才使用。
--
--   本腳本只「新增」資料表與索引，不會修改或刪除 Post / Reply 既有資料；
--   重複執行也不會出錯 (IF NOT EXISTS)。
--
-- 執行方式 (擇一)：
--   1. DB Browser for SQLite 開啟 App_Data\Forum.db →「執行 SQL」貼上 → 執行 → 寫入變更
--   2. sqlite3 App_Data\Forum.db < Forum_PostImage.sql
--
-- 執行前請先備份 Forum.db。
-- =====================================================================

CREATE TABLE IF NOT EXISTS PostImage (
    id          INTEGER PRIMARY KEY AUTOINCREMENT,
    postId      INTEGER NOT NULL,      -- 所屬貼文 Post.id
    replyId     INTEGER NULL,          -- 所屬回文 Reply.id；NULL 表示附加在貼文本身
    fileName    TEXT    NOT NULL,      -- 原圖相對路徑 (App_Data\ForumImages 底下)
    thumbName   TEXT    NOT NULL,      -- 縮圖相對路徑
    contentType TEXT    NOT NULL,      -- image/jpeg、image/png、image/gif
    width       INTEGER NOT NULL,
    height      INTEGER NOT NULL,
    size        INTEGER NOT NULL,      -- 位元組
    empno       TEXT    NULL,          -- 上傳者 (僅供管理，不顯示於前台)
    createdAt   TEXT    NOT NULL       -- yyyy-MM-ddTHH:mm:ss
);

CREATE INDEX IF NOT EXISTS IX_PostImage_postId ON PostImage(postId);

-- 確認：應看到 PostImage
-- SELECT name FROM sqlite_master WHERE type = 'table';
