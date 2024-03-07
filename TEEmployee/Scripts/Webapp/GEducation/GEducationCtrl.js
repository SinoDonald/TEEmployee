var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllCourses = (o) => {
        return $http.post('Education/GetAllCourses', o);
    };
    //this.GetAllRecords = (o) => {
    //    return $http.post('Education/GetAllRecords', o);
    //};
    //this.GetAuth = (o) => {
    //    return $http.post('Education/GetAuthorization', o);
    //};
    //this.UpsertRecords = (o) => {
    //    return $http.post('Education/UpsertRecords', o);
    //};
}]);

app.controller('GEducationCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllCourses({}).then((ret) => {

        $scope.courses = ret.data;

    })

    // client to server
    formElem.onsubmit = async (e) => {

        e.preventDefault();

        // don't add '/' before MVC controller
        let response = await fetch('GEducation/UploadCourseFile', {
            method: 'POST',
            body: new FormData(formElem)
        });

        let result = await response.json();

        alert(result);
    };

}]);

