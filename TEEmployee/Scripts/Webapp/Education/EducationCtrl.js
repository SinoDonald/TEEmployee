var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {     

    this.GetAllCourses = (o) => {
        return $http.post('Education/GetAllCourses', o);
    };

}]);

app.controller('EducationCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllCourses({}).then((ret) => {

        $scope.courses = ret.data;

    })

    // client to server
    formElem.onsubmit = async (e) => {

        e.preventDefault();

        // don't add '/' before MVC controller
        let response = await fetch('Education/UploadCourseFile', {
            method: 'POST',
            body: new FormData(formElem)
        });

        let result = await response.json();

        alert(result);
    };

}]);

