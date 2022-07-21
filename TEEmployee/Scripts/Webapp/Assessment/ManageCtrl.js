var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {
    this.GetAllManageAssessments = function (o) {
        return $http.post('Assessment/GetAllManageAssessments', o);
    };

    this.CreateManageResponse = function (o) {
        return $http.post('Assessment/CreateManageResponse', o);
    };

    this.GetManageResponse = function (o) {
        return $http.post('Assessment/GetManageResponse', o);
    };
}]);

app.controller('ManageCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.ManageAssessments = [];

    $scope.ManageResponse = [];

    $scope.CreateManageResponse = function () {
        appService.CreateManageResponse($scope.ManageAssessments)
            .then(function (ret) {
                $window.location.href = '/Home';
            });
    }

    appService.GetAllManageAssessments({})
        .then(function (ret) {
            $scope.ManageAssessments = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

    appService.GetManageResponse({})
        .then(function (ret) {
            $scope.ManageAssessments.forEach(function (item) {

                ret.data.forEach(function (item2) {

                    if (item.Id == item2.Id) {
                        item.Choice = item2.Choice;
                        return
                    }
                });
            });
        })
        .catch(function (ret) {
            alert('Error');
        });
}]);