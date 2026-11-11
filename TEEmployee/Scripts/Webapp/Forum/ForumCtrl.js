var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    // multipart/form-data 送出設定 (讓瀏覽器自行帶入 boundary)
    const multipart = { transformRequest: angular.identity, headers: { 'Content-Type': undefined } };

    this.GetAllPosts = (o) => {
        return $http.post('Forum/GetAllPosts', o);
    };
    this.GetPost = (o) => {
        return $http.post('Forum/GetPost', o);
    };
    // o 為 FormData (post.* 欄位 + images)
    this.InsertPost = (o) => {
        return $http.post('Forum/InsertPost', o, multipart);
    };
    // o 為 FormData (reply.* 欄位 + images)
    this.InsertReply = (o) => {
        return $http.post('Forum/InsertReply', o, multipart);
    };
    this.DeletePost = (o) => {
        return $http.post('Forum/DeletePost', o);
    };

    // 培文 --> 我要抱抱取消通知
    this.UpdateDatabase = (o) => {
        return $http.post('Forum/UpdateDatabase', o);
    };
    // 培文 --> 刪除回覆
    this.DeleteReply = (o) => {
        return $http.post('Forum/DeleteReply', o);
    };

}]);

/* =====================================================================
                         圖片上傳共用功能
   限制需與後端 ForumService 一致：最多 5 張、單張 8MB、JPG/PNG/GIF
   ===================================================================== */

const FORUM_IMAGE = {
    maxCount: 5,
    maxBytes: 8 * 1024 * 1024,
    maxTotalBytes: 18 * 1024 * 1024,   // Web.config 單次請求上限 20MB，保留表單欄位空間
    types: ['image/jpeg', 'image/png', 'image/gif'],
};

/** 建立圖片挑選器：管理待上傳檔案、預覽網址與錯誤訊息 */
function createImagePicker() {

    const picker = { items: [], error: '' };

    const totalBytes = () => picker.items.reduce((sum, x) => sum + x.file.size, 0);

    picker.add = (fileList) => {
        picker.error = '';

        Array.from(fileList || []).forEach((f) => {
            const name = f.name || '貼上的圖片';

            if (FORUM_IMAGE.types.indexOf(f.type) < 0) {
                picker.error = `「${name}」不是 JPG、PNG 或 GIF 格式。`;
                return;
            }
            if (f.size > FORUM_IMAGE.maxBytes) {
                picker.error = `「${name}」超過 8 MB。`;
                return;
            }
            if (picker.items.length >= FORUM_IMAGE.maxCount) {
                picker.error = `最多只能附加 ${FORUM_IMAGE.maxCount} 張圖片。`;
                return;
            }
            if (totalBytes() + f.size > FORUM_IMAGE.maxTotalBytes) {
                picker.error = '圖片總大小超過 18 MB，請減少張數或縮小圖片。';
                return;
            }

            picker.items.push({ file: f, name: f.name || 'paste.png', url: URL.createObjectURL(f) });
        });
    };

    picker.remove = (index) => {
        URL.revokeObjectURL(picker.items[index].url);
        picker.items.splice(index, 1);
        picker.error = '';
    };

    picker.clear = () => {
        picker.items.forEach(x => URL.revokeObjectURL(x.url));
        picker.items = [];
        picker.error = '';
    };

    return picker;
}

/** 綁定「選擇檔案 / Ctrl+V 貼上截圖 / 拖曳」到發文視窗 */
function bindImageInputs($scope, picker) {

    const modal = document.getElementById('exampleModal');
    const input = document.getElementById('forumImageInput');
    const zone = document.getElementById('forumDropZone');
    if (!modal || !input || !zone) return;

    // 選擇檔案
    input.addEventListener('change', () => {
        $scope.$apply(() => picker.add(input.files));
        input.value = '';   // 允許再次選同一個檔案
    });

    // Ctrl+V 貼上：只處理純圖片 (例如截圖)。
    // 從 Word/Excel 複製文字時剪貼簿也會附帶圖片，此時視為貼文字，不加入圖片。
    modal.addEventListener('paste', (e) => {
        const items = Array.from((e.clipboardData && e.clipboardData.items) || []);
        if (items.some(it => it.kind === 'string' && it.type === 'text/plain')) return;

        const files = items
            .filter(it => it.kind === 'file' && it.type.indexOf('image/') === 0)
            .map(it => it.getAsFile())
            .filter(f => f);

        if (files.length) {
            e.preventDefault();
            $scope.$apply(() => picker.add(files));
        }
    });

    // 拖曳：整個視窗都可放開，避免瀏覽器直接開啟圖片
    ['dragenter', 'dragover'].forEach(ev => modal.addEventListener(ev, (e) => {
        e.preventDefault();
        zone.classList.add('dragover');
    }));
    ['dragleave', 'drop'].forEach(ev => modal.addEventListener(ev, (e) => {
        e.preventDefault();
        if (ev === 'drop' || !modal.contains(e.relatedTarget)) zone.classList.remove('dragover');
    }));
    modal.addEventListener('drop', (e) => {
        if (e.dataTransfer && e.dataTransfer.files.length)
            $scope.$apply(() => picker.add(e.dataTransfer.files));
    });
}

/** 將物件欄位 (prefix.欄位) 與圖片組成 FormData，供 MVC model binding */
function buildFormData(prefix, obj, picker) {
    const fd = new FormData();
    Object.keys(obj).forEach((k) => {
        if (obj[k] !== undefined && obj[k] !== null) fd.append(prefix + '.' + k, obj[k]);
    });
    picker.items.forEach(x => fd.append('images', x.file, x.name));
    return fd;
}

/** 送出結果處理：成功重新整理，失敗顯示訊息 */
function handleSubmitResult($scope, $window, promise) {
    $scope.submitting = true;
    $scope.submitError = '';

    promise.then((ret) => {
        if (ret.data && ret.data.ok) {
            $window.location.reload();
        } else {
            $scope.submitError = (ret.data && ret.data.message) || '送出失敗，請稍後再試。';
            $scope.submitting = false;
        }
    }, (err) => {
        $scope.submitError = (err && err.status === 404)
            ? '送出失敗：檔案可能太大 (單次上限 20 MB)。'
            : '送出失敗，請稍後再試。';
        $scope.submitting = false;
    });
}

/** 圖片網址 (經 Forum/Image 驗證後讀取) */
function forumImageUrl(img, thumb) {
    return 'Forum/Image/' + img.id + (thumb ? '?thumb=true' : '');
}


app.controller('IndexCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    moment.locale('zh-tw');

    let options = {
        timeZone: 'Asia/Taipei',
        hour12: false, // If you want 24-hour format
    };

    $scope.modal = { title: '', content: '' };
    $scope.picker = createImagePicker();
    $scope.submitting = false;
    $scope.submitError = '';
    $scope.currentPage = 1;
    $scope.pageSize = 10;

    bindImageInputs($scope, $scope.picker);

    $scope.changePage = (page) => {
        $scope.currentPage = page;
    }

    appService.GetAllPosts({}).then((ret) => {

        $scope.posts = ret.data;
        $scope.pageMax = Math.floor(ret.data.length / $scope.pageSize) + 1;

        let now = moment();

        //$scope.posts.forEach(x => x.dateStr = moment(x.postDate, "M/D/YYYY, HH:mm:ss").format("YYYY-MM-DD HH:mm:ss"));

        // post date
        $scope.posts.forEach(x => {

            let date = moment(x.postDate, "M/D/YYYY, HH:mm:ss");
            let diffDays = now.diff(date, 'days');

            if (diffDays > 30) {
                x.dateStr = date.format('MM-DD  HH:mm');  // fallback to standard format
            } else {
                x.dateStr = date.fromNow();  // e.g., "3 hours ago"
            }
        });

        // latest reply date
        $scope.posts.forEach(x => {

            let date = moment(x.latestDate);
            let diffDays = now.diff(date, 'days');

            if (diffDays > 30) {
                x.latestDateStr = date.format('MM-DD  HH:mm');
            } else {
                x.latestDateStr = date.fromNow();
            }
        });


    })

    $scope.createModal = () => {
        $scope.modal = { title: '', content: '' };
        $scope.picker.clear();
        $scope.submitError = '';
    };

    $scope.createPost = () => {

        if ($scope.submitting) return;

        let currentTime = new Date().toLocaleString('en-US', options);

        let post = {
            title: $scope.modal.title,
            content: $scope.modal.content,
            postDate: currentTime,
            anonymous: $scope.modal.anonymous === true,
        }

        handleSubmitResult($scope, $window, appService.InsertPost(buildFormData('post', post, $scope.picker)));
    };

    $scope.filterPage = function (page) {
        return function (item, index) {
            return index >= ($scope.currentPage - 1) * $scope.pageSize;
        };
    };

    $scope.switchPage = function (count) {

        if ($scope.currentPage + count > $scope.pageMax) return;
        if ($scope.currentPage + count < 1) return;
            $scope.currentPage += count;
    };


    // 我要抱抱取消通知 <-- 培文
    appService.UpdateDatabase({ notification: '0' }).then((ret) => {
        if (ret.data) {
            console.log(ret.data);
        }
    });

}]);

app.controller('PostCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    moment.locale('zh-tw');

    let options = {
        timeZone: 'Asia/Taipei',
        hour12: false, // If you want 24-hour format
    };

    let picMap = new Map();

    let now = moment();

    $scope.modal = { content: '' };
    $scope.picker = createImagePicker();
    $scope.submitting = false;
    $scope.submitError = '';
    $scope.imgUrl = forumImageUrl;

    bindImageInputs($scope, $scope.picker);

    const getLocaleDateStr = (itemDate) => {
        let date = moment(itemDate, "M/D/YYYY, HH:mm:ss");
        let diffDays = now.diff(date, 'days');

        if (diffDays > 30) {
            return date.format('YYYY/MM/DD  HH:mm');
        } else {
            return date.fromNow();
        }
    }

    appService.GetPost({ id: $window.postId }).then((ret) => {
        $scope.post = ret.data.Item1;
        $scope.replies = ret.data.Item2;

        // set map
        picMap.set($scope.post.empno, Math.floor(Math.random() * 5) + 1);
        $scope.replies.forEach(x => picMap.set(x.empno, Math.floor(Math.random() * 5) + 1));
        // get map
        $scope.post.pic = picMap.get($scope.post.empno);
        $scope.replies.forEach(x => x.pic = picMap.get(x.empno));

        // date moment js
        $scope.post.dateStr = getLocaleDateStr($scope.post.postDate);
        $scope.replies.forEach(x => x.dateStr = getLocaleDateStr(x.replyDate));

    })

    $scope.createModal = () => {
        $scope.modal = { content: '' };
        $scope.picker.clear();
        $scope.submitError = '';
    };

    $scope.createReply = () => {

        if ($scope.submitting) return;

        let currentTime = new Date().toLocaleString('en-US', options);

        let reply = {
            postId: $window.postId,
            replyContent: $scope.modal.content,
            replyDate: currentTime,
            anonymous: $scope.modal.anonymous === true,
        }

        handleSubmitResult($scope, $window, appService.InsertReply(buildFormData('reply', reply, $scope.picker)));
    };

    $scope.back = () => {
        $window.history.back();
    }

    /* ---------- 點圖放大 ---------- */

    $scope.viewer = null;

    $scope.openImage = (list, index) => {
        $scope.viewer = { list: list, index: index };
    };
    $scope.closeImage = () => {
        $scope.viewer = null;
    };
    $scope.stepImage = (step, $event) => {
        if ($event) $event.stopPropagation();
        if (!$scope.viewer) return;
        const n = $scope.viewer.list.length;
        $scope.viewer.index = ($scope.viewer.index + step + n) % n;
    };

    document.addEventListener('keydown', (e) => {
        if (!$scope.viewer) return;
        if (e.key === 'Escape') $scope.$apply($scope.closeImage);
        else if (e.key === 'ArrowLeft') $scope.$apply(() => $scope.stepImage(-1));
        else if (e.key === 'ArrowRight') $scope.$apply(() => $scope.stepImage(1));
    });

    /// 刪除貼文 <-- 培文
    $scope.confirmDeletePost = (post) => {
        var result = confirm("確定要刪除這則貼文嗎？");

        if (result) {
            appService.DeletePost({ post: { id: post.id } }).then((ret) => {
                if (ret.data) {
                    $window.location.href = indexUrl;
                }
            })
        }
    }
    /// 刪除留言 <-- 培文
    $scope.deleteReply = function (postId, id) {
        Swal.fire({
            icon: 'question',
            title: '確定要刪除留言嗎?',
            text: '刪除後將無法復原',
            showCancelButton: true,
        }).then((result) => {
            if (result.isConfirmed) {
                appService.DeleteReply({ postId: postId, id: id }).then((ret) => {
                    if (ret.data) {
                        $window.location.href = indexUrl;
                    }
                })
            }
        })
    };
}]);
