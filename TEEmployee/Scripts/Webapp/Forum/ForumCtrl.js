var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllPosts = (o) => {
        return $http.post('Forum/GetAllPosts', o);
    };
    this.GetPost = (o) => {
        return $http.post('Forum/GetPost', o);
    };
    this.InsertPost = (o) => {
        return $http.post('Forum/InsertPost', o);
    };
    this.InsertReply = (o) => {
        return $http.post('Forum/InsertReply', o);
    };
    this.DeletePost = (o) => {
        return $http.post('Forum/DeletePost', o);
    };

    // 培文 --> 我要抱抱取消通知
    this.UpdateDatabase = (o) => {
        return $http.post('Forum/UpdateDatabase', o);
    };

}]);

app.controller('IndexCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    moment.locale('zh-tw');

    let options = {
        timeZone: 'Asia/Taipei',
        hour12: false, // If you want 24-hour format
    };

    $scope.modal = { title: '', content: '' };
    $scope.currentPage = 1;
    $scope.pageSize = 10;

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
    };

    $scope.createPost = () => {

        let currentTime = new Date().toLocaleString('en-US', options);

        let post = {
            title: $scope.modal.title,
            content: $scope.modal.content,
            postDate: currentTime,
            anonymous: $scope.modal.anonymous,
        }

        appService.InsertPost({ post: post }).then((ret) => {
            if (ret.data)
                $window.location.reload();
        })

        // 我要抱抱通知大家 <-- 培文
        appService.UpdateDatabase({ notification: '1' }).then((ret) => {
            if (ret.data) {
                console.log(ret.data);
            }
        });

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
    };

    $scope.createReply = () => {

        let currentTime = new Date().toLocaleString('en-US', options);

        let reply = {
            postId: $window.postId,
            replyContent: $scope.modal.content,
            replyDate: currentTime,
            anonymous: $scope.modal.anonymous,
        }

        appService.InsertReply({ reply: reply }).then((ret) => {
            if (ret.data)
                $window.location.reload();
        })

    };

    $scope.back = () => {
        $window.history.back();
    }


    $scope.confirmDeletePost = (post) => {

        var result = confirm("確定要刪除這則貼文嗎？");

        if (result) {
            appService.DeletePost({ post: post }).then((ret) => {

                if (ret.data) {
                    $window.location.href = indexUrl;
                }
            })
        }
    }

    // 我要抱抱通知大家 <-- 培文
    appService.UpdateDatabase({ notification: '1' }).then((ret) => {
        if (ret.data) {
            console.log(ret.data);
        }
    });

}]);
