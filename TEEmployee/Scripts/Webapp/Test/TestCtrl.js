var app = angular.module('app', []);
var config = { responseType: 'blob' };

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.DownloadMyPage = (o) => {
        return $http.post('Test/DownloadMyPage', o, config);
    };

}]);

app.controller('TestCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    var reader = new FileReader();

    $scope.downloadMyPage = () => {
        appService.DownloadMyPage({ name: $scope.name }).then((ret) => {

            // set response type of angular http to 'blob', or the default it string or JSON

            // get the file name from response header
            const contentDispositionHeader = ret.headers('Content-Disposition');
            const fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');

            // Create blob object with type(optional, used for createObjectURL)
            var blob = new Blob([ret.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });

            // Call Filesaver.js to save it with filename(type inclueded)
            // Benefit: filename
            saveAs(blob, fileName);

            // Other method - use URL.createObjectURL or FileReader
            // Cons: Can not define filename

            // Method 2
            //var objectUrl = URL.createObjectURL(blob);
            //window.open(objectUrl);

            // Method 3
            //reader.readAsDataURL(new Blob([ret.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }));
        })
    }

    // Method 3
    //reader.onload = function (e) {
    //    window.open(decodeURIComponent(reader.result), '_self', '', false);
    //}



}]);