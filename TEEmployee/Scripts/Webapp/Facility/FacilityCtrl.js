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
    // 刪除
    this.Delete = (o) => {
        return $http.post('Facility/Delete', o)
    };
    // 修改與新增
    this.Send = (o) => {
        return $http.post('Facility/Send', o)
    };
    // 取得感測器狀態
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
            $scope.currentUser = ret.data;
        });

    // 取得所有公用裝置
    appService.GetDevices({})
        .then(function (ret) {
            // 找到特定type裡的ID, 並使用...new Set()過濾重複值後排序
            $scope.getDevices = [...new Set(ret.data.filter(x => x.type == "電腦").map(x => x.deviceID))].sort();
            $scope.getTeams = [...new Set(ret.data.filter(x => x.type == "Teams").map(x => x.deviceID))].sort();
            $scope.device = ret.data[0];
            $scope.deviceID = ret.data[0].deviceID;
            $scope.insertID = Math.max(...new Set(ret.data.map(item => item.id))) + 1;
            GetEvents(); // 取得裝置行事曆
        });

    // 取得裝置行事曆
    function GetEvents(data) {
        if (data != undefined) {
            $scope.deviceID = data;
        }
        // 用 JavaScript Date 或 moment.js
        appService.GetEvents({ deviceID: $scope.deviceID })
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
                            $scope.editAuth = $scope.currentUser.empno == info.modifiedUser ? !0 : !1; // 編輯者本人可修改
                        });
                        $("#reserve-modal").modal("show");
                    },
                    // 新增事件
                    dayClick: function (i) {
                        if ($scope.bEdit = !0) {
                            $scope.reserve = {};
                            $scope.reserve.title = "";
                            $scope.reserve.meetingDate = moment(i._d.toISOString()).format("YYYY/MM/DD");
                            $scope.reserve.modifiedDate = moment(new Date).format("YYYY/MM/DD HH:mm:ss"),
                            $scope.reserve.startTime = (new Date).getHours() + 1 + ":00";
                            $scope.reserve.startTime.length != 5 && ($scope.reserve.startTime = "0" + $scope.reserve.startTime);
                            $scope.reserve.endTime = (new Date).getHours() + 2 + ":00";
                            $scope.reserve.endTime.length != 5 && ($scope.reserve.endTime = "0" + $scope.reserve.endTime);
                            $scope.reserve.empno = $scope.currentUser.empno;
                            $scope.reserve.name = $scope.currentUser.name;
                            $scope.reserve.contact = $scope.currentUser.empno + " " + $scope.currentUser.name;
                            $scope.reserve.modifiedUser = $scope.currentUser.empno;
                            $scope.reserve.contactTel = '0' + $scope.currentUser.empno;
                            $scope.reserve.num = 1;
                        };
                        $scope.$apply(function () {
                            $scope.bEdit = !1;  // dayClick -> 恢復可輸入
                        });
                        $("#reserve-modal").modal("show");
                        $("#reserve-modal").on("shown.bs.modal", function () {
                            $("#title").focus()
                        })
                    }
                });
                $scope.Delete = function () {
                    Swal.fire({
                        icon: 'question',
                        title: '確定要刪除預約嗎?',
                        text: '刪除後將無法復原',
                        showCancelButton: true,
                    }).then((result) => {
                        if (result.isConfirmed) {
                            appService.Delete({ id: $scope.reserve.id })
                                .then(function (ret) {
                                    GetEvents(data);
                                    $("#reserve-modal").modal("hide");
                                });
                        }
                    })
                };
                $scope.Send = function (state) {
                    // 將$scope.reserve物件轉成純資料
                    var eventData = {
                        id: state == 'edit' ? $scope.reserve.id : $scope.insertID,
                        type: $scope.deviceID == 'Teams' ? 'Teams' : '電腦',
                        empno: $scope.reserve.empno,
                        name: $scope.reserve.name,
                        contactTel: $scope.reserve.contactTel,
                        startTime: $scope.reserve.startTime,
                        endTime: $scope.reserve.endTime,
                        meetingDate: $scope.reserve.meetingDate,
                        modifiedDate: $scope.reserve.modifiedDate,
                        modifiedUser: $scope.currentUser.empno,
                        num: $scope.reserve.num,
                        deviceID: $scope.deviceID,
                        deviceName: $scope.device.deviceName,
                        title: $scope.reserve.title,
                        available: 1,
                        start: $scope.reserve.start,
                        end: $scope.reserve.end,
                        allDay: $scope.reserve.allDay
                    };
                    //Swal.fire({
                    //    icon: 'question',
                    //    title: state == 'edit' ? '確定要修改嗎?' : '確定要新增嗎?',
                    //    showCancelButton: true,
                    //}).then((result) => {
                    //    if (result.isConfirmed) {
                            appService.Send({ state: state, reserve: eventData })
                                .then(function (ret) {
                                    GetEvents(data);
                                    $("#reserve-modal").modal("hide")
                                });
                    //    }
                    //})
                };
            })
            .catch(function (ret) {

            });;
    };

    // 取得裝置行事曆
    $scope.GetEvents = function (data) {
        GetEvents(data)
    }

    appService.GetSensorResourceData({}).then((ret) => {
        $scope.data = ret.data.result.map(x => Boolean(Number(x.value)));
    });

}]);