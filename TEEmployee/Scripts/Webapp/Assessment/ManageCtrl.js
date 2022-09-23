var app = angular.module('app', ['ui.router', 'ngAnimate']);

app.config(function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('ManagerOption', {
            url: '/ManagerOption',
            templateUrl: 'Assessment/ManagerOption'
        })
        .state('ManagerSuggest', {
            url: '/ManagerSuggest',
            templateUrl: 'Assessment/ManagerSuggest'
        })
        .state('SetManager', {
            url: '/SetManager',
            templateUrl: 'Assessment/SetManager'
        })

});

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

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
    this.SetScorePeople = function (o) {
        return $http.post('Assessment/SetScorePeople', o);
    }

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

}]);
app.controller('SetManagerCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

    // 取得所有員工名單
    appService.SetScorePeople({})
        .then(function (ret) {
            $scope.SetScorePeople = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

}]);