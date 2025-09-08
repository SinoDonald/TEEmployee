var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

    this.GetSensorResourceData = (o) => {
        return $http.post('Facility/GetSensorResourceData', o);
    };

}]);


app.controller('FacilityCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    appService.GetSensorResourceData({}).then((ret) => {
        $scope.data = ret.data.result.map(x => Boolean(Number(x.value)));
    })

}]);