var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    //this.GetAllRecords = (o) => {
    //    return $http.post('Training/GetAllRecords', o);
    //};

    this.GetAllRecordsByUser = (o) => {
        return $http.post('Training/GetAllRecordsByUser', o);
    };

}]);

app.controller('TrainingCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllRecordsByUser({}).then((ret) => {

        $scope.records = ret.data;
        
    })

    // client to server
    formElem.onsubmit = async (e) => {

        e.preventDefault();

        // don't add '/' before MVC controller
        let response = await fetch('Training/UploadTrainingFile', {
            method: 'POST',
            body: new FormData(formElem)
        });

        let result = await response.json();

        alert(result);
    };


}]);

