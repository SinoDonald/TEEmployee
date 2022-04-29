var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllSelfAssessments = function (o) {
        return $http.post('Assessment/GetAllSelfAssessments', o);
    };

    this.CreateResponse = function (o) {
        return $http.post('Assessment/CreateResponse', o);
    };

    this.GetResponse = function (o) {
        return $http.post('Assessment/GetResponse', o);
    };
}]);

app.controller('SelfCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.SelfAssessments = [];

    //$scope.response = {
    //    "Id": 7596,
    //    "choices": []
    //}

    $scope.Response = [];

    $scope.CreateResponse = function () {
        appService.CreateResponse($scope.SelfAssessments)
            .then(function (ret) {
                $window.location.href = '/Home';
            });
    }


    appService.GetAllSelfAssessments({})
        .then(function (ret) {
            $scope.SelfAssessments = ret.data;
            $scope.SelfAssessments[0].UserId = 7596
        })
        .catch(function (ret) {
            alert('Error');
        });


    appService.GetResponse({})
        .then(function (ret) {
            $scope.SelfAssessments.forEach(function (item) {

                ret.data.forEach(function (item2){

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