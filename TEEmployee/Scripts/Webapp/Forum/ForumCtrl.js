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

}]);

app.controller('IndexCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

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
        }

        appService.InsertPost({ post: post }).then((ret) => {
            if (ret.data)
                $window.location.reload();
        })

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

}]);

app.controller('PostCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    let options = {
        timeZone: 'Asia/Taipei',
        hour12: false, // If you want 24-hour format
    };

    appService.GetPost({ id: $window.postId }).then((ret) => {

        $scope.post = ret.data.Item1;
        $scope.replies = ret.data.Item2;        
            
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
        }

        appService.InsertReply({ reply: reply }).then((ret) => {
            if (ret.data)
                $window.location.reload();
        })

    };

    $scope.back = () => {
        $window.history.back();
    }

}]);
