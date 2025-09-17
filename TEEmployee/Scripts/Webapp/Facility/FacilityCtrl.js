var app = angular.module('app', ['ui.router', 'ngAnimate']);
var config = { responseType: 'blob' };

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
    // 新增裝置
    this.CreateDevice = (o) => {
        return $http.post('Facility/CreateDevice', o)
    };
    // 移除裝置
    this.RemoveDevice = (o) => {
        return $http.post('Facility/RemoveDevice', o)
    };
    // 取得感測器狀態
    this.GetSensorResourceData = (o) => {
        return $http.post('Facility/GetSensorResourceData', o);
    };

}]);

// 時間選擇器
app.directive("datetimepicker", ["$timeout", "$parse", function (n) {
    return {
        require: "?ngModel",
        link: function (t, i, r, u) {
            return n(function () {
                return $(i).datetimepicker({
                    format: "HH:mm"
                }).on("dp.change", function () {
                    var n = $(this).val();
                    u.$setViewValue(n)
                })
            })
        }
    }
}]);

app.controller('FacilityCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    appService.GetSensorResourceData({}).then((ret) => {
        $scope.data = ret.data.result.map(x => Boolean(Number(x.value)));
    });

}]);

app.controller('BorrowCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    $scope.activeTab = 'calendar'; // 預設顯示 calendar

    $scope.showContent = function (tab) {
        $scope.activeTab = tab;
    };

    $(function () {
        $('#startPicker').datetimepicker({
            format: 'HH:mm',     // 只顯示時間
            stepping: 5,         // 每 5 分鐘一個選項
            icons: {
                time: 'glyphicon glyphicon-time',
                date: 'glyphicon glyphicon-calendar',
                up: 'glyphicon glyphicon-chevron-up',
                down: 'glyphicon glyphicon-chevron-down',
                previous: 'glyphicon glyphicon-chevron-left',
                next: 'glyphicon glyphicon-chevron-right',
                today: 'glyphicon glyphicon-screenshot',
                clear: 'glyphicon glyphicon-trash',
                close: 'glyphicon glyphicon-remove'
            }
        }).on('dp.change', function (e) {
            // 同步到 AngularJS model
            var scope = angular.element($('#start')).scope();
            scope.$apply(function () {
                scope.reserve.startTime = e.date.format("HH:mm");
            });
        });
    });

    // 取得當前使用者員編
    appService.CurrentUser({})
        .then(function (ret) {
            $scope.currentUser = ret.data;
        });

    // 取得所有公用裝置
    function GetDevices() {
        appService.GetDevices({})
            .then(function (ret) {
                $scope.devices = ret.data;
                $scope.device = ret.data[0];
                // 找到特定type裡的ID, 並使用...new Set()過濾重複值後排序
                $scope.getDevices = [...new Set(ret.data.filter(x => x.type == "電腦").map(x => x.deviceID))].sort();
                $scope.getTeams = [...new Set(ret.data.filter(x => x.type == "Teams").map(x => x.deviceID))].sort();
                $scope.deviceID = ret.data[0].deviceID;
                $scope.insertID = Math.max(...new Set(ret.data.map(item => item.id))) + 1;
                $scope.deviceIDs = [...new Set(ret.data.map(x => x.deviceID))].sort(); // 移除裝置下拉選單
                GetEvents(); // 取得裝置行事曆
            });
    }
    GetDevices();

    // 取得裝置行事曆
    function GetEvents(data) {
        if (data != undefined) {
            $scope.deviceID = data;
        }
        // 用 JavaScript Date 或 moment.js
        appService.GetEvents({ deviceID: $scope.deviceID })
            .then(function (ret) {
                $scope.device = ret.data[0];
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
                    aspectRatio: 1.25,
                    weekends: !0,
                    eventLimit: !0,
                    timeFormat: "H:mm",
                    firstDay: 1, //每週從星期一開始
                    slotMinutes: 60,
                    editable: true,
                    events: ret.data,

                    // 檢視事件
                    eventClick: function (event) {
                        $scope.reserve = event;
                        $scope.$apply(function () {
                            $scope.bEdit = !0; // eventClick -> 禁用 input
                            $scope.editAuth = $scope.currentUser.empno == event.modifiedUser ? !0 : !1; // 編輯者本人可修改
                        });
                        $("#reserve-modal").modal("show");
                    },
                    // 新增事件
                    dayClick: function (event) {
                        if ($scope.bEdit = !0) {
                            $scope.reserve = {};
                            $scope.reserve.deviceID = $scope.deviceID;
                            $scope.reserve.deviceName = $scope.device.deviceName;
                            $scope.reserve.title = "";
                            $scope.reserve.meetingDate = moment(event._d.toISOString()).format("YYYY/MM/DD");
                            $scope.reserve.modifiedDate = moment(new Date).format("YYYY/MM/DD HH:mm:ss");
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
                    },
                    // 拖曳事件
                    eventDrop: function (event, delta, revertFunc) {
                        var eventData = {
                            id: event.id ,
                            type: event.type,
                            empno: event.empno,
                            name: event.name,
                            contactTel: event.contactTel,
                            startTime: event.startTime,
                            endTime: event.endTime,
                            meetingDate: event.end.format("YYYY/MM/DD"),
                            modifiedDate: moment(new Date).format("YYYY/MM/DD HH:mm:ss"),
                            modifiedUser: $scope.currentUser.empno,
                            num: event.num,
                            deviceID: $scope.deviceID,
                            deviceName: $scope.device.deviceName,
                            title: event.title,
                            available: 1,
                            allDay: event.allDay
                        };
                        appService.Send({ state: 'edit', reserve: eventData })
                            .then(function (ret) {
                                if (ret.data === "") {
                                    GetEvents($scope.deviceID); // 更新新增與修改事件
                                    $scope.reserve = eventData;
                                    $("#reserve-modal").modal("hide");
                                }
                                else {
                                    Swal.fire({
                                        icon: 'error',
                                        title: ret.data,
                                        showCancelButton: false,
                                    }).then((result) => {
                                        if (result.isConfirmed) {
                                            revertFunc(); // 還原
                                        }
                                    })
                                }
                            });
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
                                    $("#calendar").fullCalendar("removeEvents", $scope.reserve._id); // 刪除事件
                                    $("#reserve-modal").modal("hide");
                                });
                        }
                    })
                };
                $scope.Send = function (state) {
                    // 將$scope.reserve物件轉成純資料
                    var eventData = {
                        id: state == 'edit' ? $scope.reserve.id : $scope.insertID,
                        type: $scope.reserve.type,
                        empno: $scope.reserve.empno,
                        name: $scope.reserve.name,
                        contactTel: $scope.reserve.contactTel,
                        startTime: $scope.reserve.startTime,
                        endTime: $scope.reserve.endTime,
                        meetingDate: $scope.reserve.meetingDate,
                        modifiedDate: moment(new Date).format("YYYY/MM/DD HH:mm:ss"),
                        modifiedUser: $scope.currentUser.empno,
                        num: $scope.reserve.num,
                        deviceID: $scope.deviceID,
                        deviceName: $scope.device.deviceName,
                        title: $scope.reserve.title,
                        available: 1,
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
                                    if (ret.data === "") {
                                        Swal.fire({
                                            icon: 'success',
                                            title: '完成',
                                            showCancelButton: false,
                                        }).then((result) => {
                                            if (result.isConfirmed) {
                                                GetEvents($scope.deviceID); // 更新新增與修改事件
                                                $("#reserve-modal").modal("hide")
                                            }
                                        })
                                    }
                                    else {
                                        Swal.fire({
                                            icon: 'error',
                                            title: ret.data,
                                            showCancelButton: false,
                                        }).then((result) => {
                                            if (result.isConfirmed) {

                                            }
                                        })
                                    }
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
    };

    // 新增裝置
    $scope.CreateDevice = function () {
        $scope.createdevice = {};
        if ($scope.insertID != undefined) {
            $scope.createdevice.id = $scope.insertID;
        }
        else {
            $scope.createdevice.id = 1;
        }
        $scope.createdevice.types = ["電腦", "Teams"];
        $scope.createdevice.type = $scope.createdevice.types[0];
        $scope.createdevice.deviceID = "";
        $scope.createdevice.deviceName = "";
        $scope.createdevice.empno = $scope.currentUser.empno;
        $scope.createdevice.name = $scope.currentUser.name;
        $scope.createdevice.contactTel = '0' + $scope.currentUser.empno;
        $scope.createdevice.startTime = moment(new Date).format("HH:mm");
        $scope.createdevice.endTime = moment(new Date).add(1, 'minutes').format("HH:mm");
        $scope.createdevice.meetingDate = moment(new Date).format("YYYY/MM/DD");
        $scope.createdevice.modifiedDate = moment(new Date).format("YYYY/MM/DD HH:mm:ss");
        $scope.createdevice.modifiedUser = $scope.currentUser.empno;
        $scope.createdevice.num = 1;
        $scope.createdevice.title = "新增裝置";
        $scope.createdevice.available = 1;
        $scope.createdevice.allDay = 0;
        $scope.createdevice.contact = $scope.currentUser.empno + " " + $scope.currentUser.name;
        $("#create-device").modal("show");
    };
    $scope.Create = function (data)
    {
        $scope.createdevice = data;
        // 如果重複deviceID, 則不得新增
        isExist = [...new Set($scope.devices.filter(x => x.deviceID == data.deviceID))];
        if (isExist.length > 0) {
            Swal.fire({
                icon: 'error',
                title: '裝置ID重複，請重新輸入',
                showCancelButton: false,
            });
        }
        else {
            appService.CreateDevice({ facility: data })
                .then(function (ret) {
                    GetDevices(); // 取得所有公用裝置
                    $("#create-device").modal("hide")
                });
        }
    }

    // 移除裝置
    $scope.RemoveDevice = function () {
        $scope.removedevice = {};
        $scope.removedevice.deviceIDs = $scope.deviceIDs;
        $scope.removedevice.deviceID = $scope.deviceIDs[0]; // 移除裝置預設值
        $("#remove-device").modal("show");
    };
    $scope.Remove = function (data) {
        Swal.fire({
            icon: 'question',
            title: '確定要刪除預約嗎?',
            text: '刪除後，此裝置過往紀錄將將無法復原',
            showCancelButton: true,
        }).then((result) => {
            if (result.isConfirmed) {
                appService.RemoveDevice({ deviceID: data.deviceID })
                    .then(function (ret) {
                        if (ret.data === "") {
                            Swal.fire({
                                icon: 'success',
                                title: '完成',
                                showCancelButton: false,
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    GetDevices(); // 取得所有公用裝置
                                    $("#remove-device").modal("hide");
                                }
                            })
                        }
                    });
            }
        })
    }

}]);