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

    // 取得所有公用裝置
    this.GetDevices = (o) => {
        return $http.post('Facility/GetDevices', o)
    };
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

    // 取得所有公用裝置
    appService.GetDevices({})
        .then(function (ret) {
            $scope.getDevices = ret.data;
            $scope.device = ret.data[0];
            GetEvents(); // 取得裝置行事曆
        });

    // 取得裝置行事曆
    function GetEvents(data) {
        if (data != undefined) {
            $scope.device = data;
        }
        // 用 JavaScript Date 或 moment.js
        appService.GetEvents({ deviceID: $scope.device })
            .then(function (ret) {
                // 先清空行事曆後, 重新生成, 即時更新
                $('#calendar').fullCalendar('destroy');
                $('#calendar').empty();
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
                    events: ret.data
                });
            })
            .catch(function (ret) {

            });;
    };

    // 選擇要看的成員
    $scope.GetEvents = function (data) {
        GetEvents(data) // 取得裝置行事曆
    }

    appService.GetSensorResourceData({}).then((ret) => {
        $scope.data = ret.data.result.map(x => Boolean(Number(x.value)));
    });

}]);