var app = angular.module('app', ['ui.router']);

app.config(function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('ManagerOption', {
            url: '/ManagerOption',
            templateUrl: 'Assessment/ManagerOption'
        })
        .state('ManagerSuggest', {
            url: '/ManagerSuggest',
            templateUrl: 'Assessment/ManagerSuggest'
        })

});

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetManagers = function (o) {
        return $http.post('Assessment/GetManagers', o);
    };

    this.GetAllManageAssessments = function (o) {
        return $http.post('Assessment/GetAllManageAssessments', o);
    };

    this.CreateManageResponse = function (o) {
        return $http.post('Assessment/CreateManageResponse', o);
    };

}]);

app.factory('myFactory', function () {
    var savedData = {}

    function set(data) {
        savedData.manager = data;
    }

    function get() {
        return savedData;
    }

    return {
        set: set,
        get: get,
    }

});

app.controller('ManageCtrl', ['$scope', '$location', 'appService', '$rootScope', function ($scope, $location, appService, $rootScope) {

    $location.path('/ManagerOption');

}]);

app.controller('ManagerOptionCtrl', ['$scope', '$location', 'appService', '$rootScope', 'myFactory', function ($scope, $location, appService, $rootScope, myFactory) {

    appService.GetManagers({})
        .then(function (ret) {
            $scope.GetManagers = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

    $scope.ManagerSuggest = function (data) {
        $location.path('/ManagerSuggest');
        myFactory.set(data)
    }

}]);

app.controller('ManagerSuggestCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

    $scope.ManageAssessments = [];

    $scope.manager = myFactory.get().manager; // 選擇要評分的主管

    appService.GetAllManageAssessments({ manager: $scope.manager })
        .then(function (ret) {
            $scope.ManageAssessments = ret.data.Responses;
            $scope.state = ret.data.State;
        })
        .catch(function (ret) {
            alert('Error');
        });

    $scope.CreateManageResponse = function (data) {
        appService.CreateManageResponse({ assessments: $scope.ManageAssessments, state: data, manager: $scope.manager })
            .then(function (ret) {
                alert('儲存完成');
                $window.location.href = '/Assessment/Manage#!/ManagerOption';
            });
    }

    $scope.SuperLink = function () {
        $window.location.href = '/Assessment/Manage#!/ManagerOption';
    }

}]);