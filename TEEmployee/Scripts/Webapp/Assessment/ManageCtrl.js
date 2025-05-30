﻿var app = angular.module('app', ['ui.router', 'ngAnimate']);

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('ManagerOption', {
            url: '/ManagerOption',
            templateUrl: 'Assessment/ManagerOption'
        })
        .state('ManagerSuggest', {
            url: '/ManagerSuggest',
            templateUrl: 'Assessment/ManagerSuggest'
        })
        //.state('SetManager', {
        //    url: '/SetManager',
        //    templateUrl: 'Assessment/SetManager'
        //})

}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    // 要填寫的主管名單
    this.UserManagers = function (o) {
        return $http.post('Assessment/UserManagers', o);
    };

    // 取得所有要評核的主管
    this.GetScorePeople = function (o) {
        return $http.post('Assessment/GetScorePeople', o);
    };

    // 取得所有的問卷年份
    this.GetManageYearList = function (o) {
        return $http.post('Assessment/GetManageYearList', o);
    };

    // 選擇年份顯示問卷結果
    this.GetAllManageAssessments = function (o) {
        return $http.post('Assessment/GetAllManageAssessments', o);
    };

    // 儲存檔案或寄送
    this.CreateManageResponse = function (o) {
        return $http.post('Assessment/CreateManageResponse', o);
    };

    // 取得所有員工名單
    //this.SetScorePeople = function (o) {
    //    return $http.post('Assessment/SetScorePeople', o);
    //}

    // Check states of all employee, return true if any of it is 'sent'
    this.ManageResponseStateCheck = function (o) {
        return $http.post('Assessment/ManageResponseStateCheck', o);
    }

    // Get All Managers(include project managers) to be selected
    this.GetAllScoreManagers = function (o) {
        return $http.post('Assessment/GetAllScoreManagers', o);
    }

    // Update Score Managers that are selected
    this.UpdateScoreManagers = function (o) {
        return $http.post('Assessment/UpdateScoreManagers', o);
    }

}]);

app.factory('myFactory', function () {

    var savedData = {}

    function set(data) {
        savedData.manager = data;
    }

    function get() {
        return savedData;
    }

    return {
        set: set,
        get: get,
    }

});

app.controller('ManageCtrl', ['$scope', '$location', 'appService', '$rootScope', function ($scope, $location, appService, $rootScope) {

    $location.path('/ManagerOption');

}]);

app.controller('ManagerOptionCtrl', ['$scope', '$location', 'appService', '$rootScope', 'myFactory', '$window', function ($scope, $location, appService, $rootScope, myFactory, $window) {

    // 要填寫的主管名單
    appService.UserManagers({})
        .then(function (ret) {
            $scope.UserManagers = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

    // 取得所有要評核的主管
    appService.GetScorePeople({})
        .then(function (ret) {
            $scope.GetScorePeople = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

    // 顯示要評分的主管問卷
    $scope.ManagerSuggest = function (data) {
        $location.path('/ManagerSuggest');
        myFactory.set(data)
    }

    // 設定要被評核的人員
    $scope.SetManager = function () {
        $location.path('/SetManager');
    }

    // Check employees who have not finished any assessments
    $scope.CheckState = () => {

        $scope.loading = true;

        appService.ManageResponseStateCheck({})
            .then((ret) => {
                $scope.manageResponseStates = ret.data;
                $scope.loading = false;
                $scope.groupList = [...new Set(ret.data.map(x => x.Employee.group).filter(x => x !== ''))]; // For all department

                if ($scope.groupList.includes('設計') && $scope.groupList.includes('規劃') && $scope.groupList.includes('專管') && $scope.groupList.includes('營運'))
                    $scope.groupList = ['設計', '規劃', '專管', '營運', '行政'];
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    // Get All Managers(include project managers) to be selected
    $scope.GetAllScoreManagers = () => {
        if (!$scope.scoreManagers) {
            appService.GetAllScoreManagers({})
                .then((ret) => {
                    $scope.scoreManagers = ret.data;
                    for (let item of $scope.scoreManagers) {
                        item.selected = true;
                    }
                })
                .catch((ret) => alert('Error'));
        }        
    }

    // Update Score Managers that are selected
    $scope.UpdateScoreManagers = () => {

        let selectedManagers = $scope.scoreManagers.filter(x => x.selected === true);

        appService.UpdateScoreManagers({ selectedManagers: selectedManagers})
            .then((ret) => {
                $window.location.href = 'Assessment/Manage';
            })
            .catch((ret) => {
                alert('Error');
            });

    }

}]);

app.controller('ManagerSuggestCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', '$timeout', function ($scope, $window, appService, $rootScope, myFactory, $timeout) {

    $scope.ManageAssessments = [];

    $scope.manager = myFactory.get().manager; // 選擇要評分的主管

    $scope.data = {
        model: null,
        availableOptions: [
            //{ id: '2022H1', name: '2022H1' },
            //{ id: '2022H2', name: '2022H2' }
        ]
    };

    $scope.clicked = false;
    $scope.clickclick = () => {
        $scope.clicked = true;
    }

    let limit = 250; //height limit

    // 取得所有的問卷年份
    appService.GetManageYearList({}).then(function (ret) {
        $scope.years = ret;
        ret.data.forEach(function (item) {
            $scope.data.availableOptions.push({ id: item, name: item })
        });
        $scope.data.model = $scope.data.availableOptions[0].name;
    });

    // 選擇年份顯示問卷結果
    $scope.GetAllManageAssessments = function (year) {
        appService.GetAllManageAssessments({ year: $scope.data.model, manager: $scope.manager })
            .then(function (ret) {
                $scope.ManageAssessments = ret.data.Responses;
                $scope.state = ret.data.State;
                // 2023H2 model = 2, 5 options
                $scope.model = 2;
                // 判斷問卷, 2023之前選項使用模式0(4個選項), 之後使用模式1(6個選項)
                if (year !== null) {
                    //year = year.substring(0, 4);
                    //if (year < 2023) {
                    //    $scope.model = 0;
                    //}
                    if (year < '2023H1')
                        $scope.model = 0;
                    if (year < '2023H2')
                        $scope.model = 1;
                }

                // set textarea height in beginning
                $timeout(function () {

                    var textAreaItems = document.querySelectorAll(".autoExpand");
                    for (let elm of textAreaItems) {
                        elm.style.height = "";
                        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
                    }
                }, 0);

            })
            .catch(function (ret) {
                alert('Error');
            });
    }
    $scope.GetAllManageAssessments();

    // 儲存檔案或寄送
    $scope.CreateManageResponse = function (data) {
        appService.CreateManageResponse({ assessments: $scope.ManageAssessments, state: data, year: $scope.data.model, manager: $scope.manager })
            .then(function (ret) {
                if (data === 'save') {
                /*alert('已儲存');*/
                    $scope.succeed = true;
                    $timeout(function () {
                        $scope.succeed = false;
                    }, 2000);
                }
                else {
                    /*alert('已送出');*/
                    $window.location.href = 'Assessment/Manage';
                }
            });
    }

    // 回列表
    $scope.ToManage = function () {
        //$window.location.href = 'Assessment/Index';
        $window.location.href = 'Assessment/Manage';
    }

    // textarea auto expand
    function onExpandableTextareaInput({ target: elm }) {

        if (!elm.classList.contains('autoExpand') || !elm.nodeName === 'TEXTAREA') return

        elm.style.height = "";
        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
    }

    // global delegated event listener
    document.addEventListener('input', onExpandableTextareaInput)

}]);
//app.controller('SetManagerCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

//    // 取得所有員工名單
//    appService.SetScorePeople({})
//        .then(function (ret) {
//            $scope.SetScorePeople = ret.data;
//        })
//        .catch(function (ret) {
//            alert('Error');
//        });

//}]);