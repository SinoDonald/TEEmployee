var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {
    this.GetManageAssessments = function (o) {
        return $http.post('Assessment/GetManageAssessments', o);
    };
}]);

app.controller('ManageCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.ManageAssessments = [];
    appService.GetManageAssessments({})
        .then(function (ret) {
            $scope.ManageAssessments = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

}]);