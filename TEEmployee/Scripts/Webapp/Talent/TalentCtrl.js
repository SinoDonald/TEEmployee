var app = angular.module('app', ['ui.router', 'ngAnimate']);
var config = { responseType: 'blob' };

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('TalentOption', {
            url: '/TalentOption',
            templateUrl: 'Talent/TalentOption'
        })
        .state('TalentRecord', {
            url: '/TalentRecord',
            templateUrl: 'Talent/TalentRecord'
        })

}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    // 取得群組
    this.GetGroupList = (o) => {
        return $http.post('Talent/GetGroupList', o);
    };
    // 取得所有員工履歷
    this.GetAll = function (o) {
        return $http.post('Talent/GetAll', o);
    };
    // 取得員工履歷
    this.Get = function (o) {
        return $http.post('Talent/Get', o);
    };
    
}]);

app.factory('dataservice', function () {

    var user = {}

    function set(data) {
        user.empno = data.empno;
        user.name = data.name;
        user.birthday = data.birthday;
        user.age = data.age;
        user.educational = data.educational;
        user.experience = data.experience;
        user.project = data.project;
        user.license = data.license;
        user.planning = data.planning;
        user.test = data.test;
        user.advantage = data.advantage;
        user.developed = data.developed;
        user.future = data.future;
    }

    function get() {
        return user;
    }

    return {
        set: set,
        get: get,
    }

});


app.controller('TalentCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    $location.path('/TalentOption');

}]);

app.controller('TalentOptionCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, $window, appService, $rootScope, $q, dataservice) {

     // 取得群組
    let groups = appService.GetGroupList({});
    $q.all([groups]).then((ret) => {
        $scope.groups = ret[0].data;
        $scope.selectedGroup = $scope.groups[0];
        $scope.FilterDataByGroup($scope.selectedGroup);
    });

    // 取得所有員工履歷
    appService.GetAll({ })
        .then(function (ret) {
            $scope.GetAll = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

    // 選擇要看的成員
    $scope.TalentRecord = function (data) {
        // 取得員工履歷
        appService.Get({ empno: data })
            .then(function (ret) {
                $scope.Get = ret.data;
                dataservice.set(ret.data[0]);
                $location.path('/TalentRecord');
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

}]);

app.controller('TalentRecordCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', 'dataservice', function ($scope, $location, $window, appService, $rootScope, dataservice) {

    $scope.user = dataservice.get(); // 取得員工履歷

}]);