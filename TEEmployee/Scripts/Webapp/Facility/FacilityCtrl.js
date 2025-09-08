var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

    this.GetAuth = (o) => {
        return $http.post('Issue/GetAuthorization', o);
    };
    this.GetAllProjects = (o) => {
        return $http.post('Issue/GetAllProjects', o);
    };

}]);


app.controller('FacilityCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    /*$location.path('/TalentOption');*/
    $(document).ready(function () {
        $('#calendar').fullCalendar({
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay'
            },
            firstDay: 1, //The day that each week begins (Monday=1)
            slotMinutes: 60,
            events: '@Url.RouteUrl(new{ action="GetEvents", controller="Home"})'
        });
    });
}]);