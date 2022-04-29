var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {
    this.GetManageAssessments = function (o) {
        return $http.post('Assessment/GetManageAssessments', o);
    };

    this.CreateResponse = function (o) {
        return $http.post('Assessment/CreateResponse', o);
    };

    this.GetResponse = function (o) {
        return $http.post('Assessment/GetResponse', o);
    };
}]);

app.controller('ManageCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.ManageAssessments = {};

    //$scope.response = {
    //    "Id": 7291,
    //    "choices": []
    //}

    $scope.Response = [];

    $scope.CreateResponse = function () {
        appService.CreateResponse($scope.ManageAssessments)
            .then(function (ret) {
                $window.location.href = '/Home';
            });
    }

    appService.GetManageAssessments({})
        .then(function (ret) {
            $scope.ManageAssessments = ret.data;
            $scope.ManageAssessments[0].UserId = 7291
        })
        .catch(function (ret) {
            alert('Error');
        });

    appService.GetResponse({})
        .then(function (ret) {
            $scope.ManageAssessments.forEach(function (item) {

                ret.data.forEach(function (item2) {

                    if (item.Id == item2.Id) {
                        item.Choice = item2.Choice;
                    }
                });
            });
        })
        .catch(function (ret) {
            alert('Error');
        });
}]);