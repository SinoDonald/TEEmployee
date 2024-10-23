var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {     

    this.GetAllContents = (o) => {
        return $http.post('Education/GetAllContents', o);
    };
   
}]);

app.controller('EducationCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllContents({}).then((ret) => {
        $scope.contents = ret.data;
    })

}]);