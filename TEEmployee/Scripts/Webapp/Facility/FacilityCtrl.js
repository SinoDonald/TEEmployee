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

    // 取得當前使用者員編
    this.CurrentUser = (o) => {
        return $http.post('Facility/CurrentUser', o)
    };
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

    var facility = {}

    function set(data) {
        facility.id = data.id;
        facility.type = data.type;
        facility.empno = data.empno;
        facility.name = data.name;
        facility.contactTel = data.contactTel;
        facility.startTime = data.startTime;
        facility.endTime = data.endTime;
        facility.meetingDate = data.meetingDate;
        facility.modifiedDate = data.modifiedDate;
        facility.modifiedUser = data.modifiedUser;
        facility.num = data.num;
        facility.deviceID = data.deviceID;
        facility.deviceName = data.deviceName;
        facility.title = data.title;
        facility.available = data.available;
        facility.start = data.start;
        facility.end = data.end;
        facility.allDay = data.allDay;
    }

    function get() {
        return facility;
    }

    return {
        set: set,
        get: get,
    }

});

app.controller('FacilityCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', 'dataservice', function ($scope, $location, $window, appService, $rootScope, dataservice) {

    // 取得當前使用者員編
    appService.CurrentUser({})
        .then(function (ret) {
            $scope.currentUser = ret.data.empno;
        });

    // 取得所有公用裝置
    appService.GetDevices({})
        .then(function (ret) {
            // 找到特定type裡的ID, 並使用...new Set()過濾重複值後排序
            $scope.getDevices = [...new Set(ret.data.filter(x => x.type == "電腦").map(x => x.deviceID))].sort();
            $scope.getTeams = [...new Set(ret.data.filter(x => x.type == "Teams").map(x => x.deviceID))].sort();
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
                $scope.bEdit = !1;
                $scope.editAuth = !1;
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
                    events: ret.data,


                    // 檢視事件
                    eventClick: function (info) {
                        $scope.reserve = info;
                        $scope.$apply(function () {
                            $scope.bEdit = !0; // eventClick -> 禁用 input
                            $scope.editAuth = $scope.currentUser == info.modifiedUser ? !0 : !1;
                        });
                        $("#reserve-modal").modal("show")
                    },
                    // 新增事件
                    dayClick: function (i) {
                        $scope.reserve = [];
                        $scope.$apply(function () {
                            $scope.bEdit = !1;  // dayClick -> 恢復可輸入
                        });
                        $("#reserve-modal").modal("show");
                        $("#reserve-modal").on("shown.bs.modal", function () {
                            $("#title").focus()
                        })
                    }
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