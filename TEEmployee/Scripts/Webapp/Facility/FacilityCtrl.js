var app = angular.module('app', ['ui.router', 'ngAnimate']);
var config = { responseType: 'blob' };

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    $stateProvider
        .state('Borrow', {
            url: '/Borrow',
            templateUrl: 'Facility/Borrow'
        })
}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

    // 取得裝置行事曆
    this.GetEvents = (o) => {
        return $http.post('Facility/GetEvents', o);
    };

}]);

app.factory('dataservice', function () {

    var user = {}

    function set(data) {
        user.empno = data.empno;
        user.name = data.name;
        user.group = data.group;
        user.group_one = data.group_one;
        user.group_two = data.group_two;
        user.group_three = data.group_three;
        user.birthday = data.birthday;
        user.age = data.age;
        user.pic = data.pic;
        user.workYears = data.workYears;
        user.companyYears = data.companyYears;
        user.seniority = data.seniority.split('\n');
        user.educational = data.educational.split('\n');
        user.performance = data.performance.split('\n');
        user.experience = data.experience.split('\n');
        user.project = data.project.split('\n');
        user.license = data.license.split('\n');
        user.domainSkill = data.domainSkill.split('\n');
        user.coreSkill = data.coreSkill.split('\n');
        user.manageSkill = data.manageSkill.split('\n');
        user.planning = data.planning.split('\n');
        user.test = data.test;
        user.advantage = data.advantage;
        user.disadvantage = data.disadvantage;
        user.developed = data.developed;
        user.future = data.future;
    }

    function get() {
        return user;
    }

    return {
        set: set,
        get: get,
    }

});


app.controller('FacilityCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    function GetEvents() {
        // 用 JavaScript Date 或 moment.js
        var startDate = new Date();
        var endDate = new Date();
        endDate.setDate(endDate.getDate() + 7); // 七天後
        appService.GetEvents({ start: startDate, end: endDate })
            .then(function (ret) {
                if (ret.data.size != 0) {
                    $('#calendar').fullCalendar({
                        header: {
                            left: 'prev,next today',
                            center: 'title',
                            right: 'month,agendaWeek,agendaDay'
                        },
                        firstDay: 1, //每週從星期一開始
                        slotMinutes: 60,
                }
                else {

                }
            });
    }
    GetEvents(); // 取得裝置行事曆

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