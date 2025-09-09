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
        return $http.post('Facility/GetEvents', o)
    };
    this.GetSensorResourceData = (o) => {
        return $http.post('Facility/GetSensorResourceData', o);
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
        var date = new Date();
        var d = date.getDate();
        var m = date.getMonth();
        var y = date.getFullYear();
        appService.GetEvents({ start: date, end: date })
            .then(function (ret) {
                if (ret.data.size != 0) {
                    $('#calendar').fullCalendar({
                        header: {
                            left: 'prev,next today',
                            center: 'title',
                            right: 'month,agendaWeek,agendaDay'
                        },
                        views: {
                            month: {
                                buttonText: "月"
                            },
                            week: {
                                buttonText: "週"
                            },
                            day: {
                                buttonText: "日"
                            }
                        },
                        firstDay: 1, //每週從星期一開始
                        slotMinutes: 60,
                        editable: true,
                        //events: [
                        //    {
                        //        title: 'All Day Event',
                        //        start: new Date(y, m, 1)
                        //    },
                        //    {
                        //        title: 'Long Event',
                        //        start: new Date(y, m, d - 5),
                        //        end: new Date(y, m, d - 2)
                        //    },
                        //    {
                        //        id: 999,
                        //        title: 'Repeating Event',
                        //        start: new Date(y, m, d - 3, 16, 0),
                        //        allDay: false
                        //    },
                        //    {
                        //        id: 999,
                        //        title: 'Repeating Event',
                        //        start: new Date(y, m, d + 4, 16, 0),
                        //        allDay: false
                        //    },
                        //    {
                        //        title: 'Meeting',
                        //        start: new Date(y, m, d, 10, 30),
                        //        allDay: false
                        //    },
                        //    {
                        //        title: 'Lunch',
                        //        start: new Date(y, m, d, 12, 0),
                        //        end: new Date(y, m, d, 14, 0),
                        //        allDay: false
                        //    },
                        //    {
                        //        title: 'Birthday Party',
                        //        start: new Date(y, m, d + 1, 19, 0),
                        //        end: new Date(y, m, d + 1, 22, 30),
                        //        allDay: false
                        //    },
                        //    {
                        //        title: 'Click for Google',
                        //        start: new Date(y, m, 28),
                        //        end: new Date(y, m, 29),
                        //        url: 'http://google.com/'
                        //    }
                        //]
                        events: ret.data
                    });
                }
                else {

                }
            });
    };
    GetEvents(); // 取得裝置行事曆

    //$(document).ready(function () {
    //    $('#calendar').fullCalendar({
    //        header: {
    //            left: 'prev,next today',
    //            center: 'title',
    //            right: 'month,agendaWeek,agendaDay'
    //        },
    //        firstDay: 1, //The day that each week begins (Monday=1)
    //        slotMinutes: 60,
    //        events: '@Url.RouteUrl(new{ action="GetEvents", controller="Home"})'
    //    });
    //});

    appService.GetSensorResourceData({}).then((ret) => {
        $scope.data = ret.data.result.map(x => Boolean(Number(x.value)));
    });

}]);