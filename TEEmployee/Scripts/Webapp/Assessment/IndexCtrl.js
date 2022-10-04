var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {
    this.GetAllSelfAssessments = function (o) {
        return $http.post('Assessment/GetAllSelfAssessments', o);
    };
}]);

app.controller('IndexCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.SelfAssessments = [];
    appService.GetAllSelfAssessments({})
        .then(function (ret) {
            $scope.SelfAssessments = ret.data;
        })
        .catch(function (ret) {
            alert('Error');            
        });

}]);