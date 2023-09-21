var app = angular.module('app', ['ui.router', 'ngAnimate']);
var config = { responseType: 'blob' };

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('TalentOption', {
            url: '/TalentOption',
            templateUrl: 'Talent/TalentOption'
        })
        .state('TalentHighPerformers', {
            url: '/TalentHighPerformers',
            templateUrl: 'Talent/TalentHighPerformers'
        })
        .state('TalentSuccessExample', {
            url: '/TalentSuccessExample',
            templateUrl: 'Talent/TalentSuccessExample'
        })
        .state('TalentRecord', {
            url: '/TalentRecord',
            templateUrl: 'Talent/TalentRecord'
        })

}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    // 取得核心專業盤點的專業與管理能力分數
    this.GetAllScores = (o) => {
        return $http.post('Profession/GetAll', o);
    };
    // High Performer
    this.HighPerformer = (o) => {
        return $http.post('Talent/HighPerformer', o);
    }
    // 取得群組
    this.GetGroupList = (o) => {
        return $http.post('Talent/GetGroupList', o);
    };
    // 儲存選項
    this.SaveChoice = function (o) {
        return $http.post('Talent/SaveChoice', o);
    };
    // 取得所有員工履歷
    this.GetAll = function (o) {
        return $http.post('Talent/GetAll', o);
    };
    // 取得員工履歷
    this.Get = function (o) {
        return $http.post('Talent/Get', o);
    };
    // 儲存回覆
    this.SaveResponse = function (o) {
        return $http.post('Talent/SaveResponse', o);
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
        user.workYears = data.workYears;
        user.companyYears = data.companyYears;
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

app.factory('highperformers', function () {

    var users = {}

    function set(data) {
        users = data;
    }

    function get() {
        return users;
    }

    return {
        set: set,
        get: get,
    }

});

app.controller('TalentCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', function ($scope, $location, $window, appService, $rootScope) {

    $location.path('/TalentOption');

}]);

app.controller('TalentOptionCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', '$q', 'dataservice', 'highperformers', function ($scope, $location, $window, appService, $rootScope, $q, dataservice, highperformers) {

    // 搜尋功能
    $scope.SearchByName = function (selectedGroup, name) {
        $scope.data = [];
        for (let item of $scope.GetAll) {
            if (selectedGroup === 'High Performer') {
                for (let highPerformer of $scope.HighPerformer) {
                    if (item.empno === highPerformer.empno) {
                        if (item.empno === name || item.name.includes(name)) {
                            $scope.data.push(item);
                        }
                    }
                }
            }
            else if (selectedGroup === '全部顯示') {
                if (item.empno === name || item.name.includes(name)) {
                    $scope.data.push(item);
                }
            }
            else if (item.group === selectedGroup || item.group_one === selectedGroup || item.group_two === selectedGroup || item.group_three === selectedGroup) {
                if (item.empno === name || item.name.includes(name)) {
                    $scope.data.push(item);
                }
            }
        }
    }

    // 取得所有員工履歷
    appService.GetAll({})
        .then(function (ret) {
            $scope.GetAll = ret.data;

            // High Performer
            appService.GetAllScores({})
                .then(function (ret) {
                    appService.HighPerformer({ getAllScores: ret.data })
                        .then(function (ret) {
                            $scope.HighPerformer = ret.data;
                        })
                })
                .catch(function (ret) {
                    alert('Error');
                });

            // 取得群組
            let groups = appService.GetGroupList({});
            $q.all([groups]).then((ret) => {
                $scope.groups = ret[0].data;
                $scope.selectedGroup = $scope.groups[0];
                $scope.FilterDataByGroup($scope.selectedGroup);
            });
        })
        .catch(function (ret) {
            alert('Error');
        });

    // 依群組顯示
    $scope.FilterDataByGroup = function (selectedGroup) {
        $scope.data = [];
        for (let item of $scope.GetAll) {
            if (selectedGroup === 'High Performer') {
                for (let highPerformer of $scope.HighPerformer) {
                    if (item.empno === highPerformer.empno) {
                        $scope.data.push(item);
                    }
                }
            }
            else if (selectedGroup === '全部顯示') {
                $scope.data.push(item);
            }
            else if (item.group === selectedGroup || item.group_one === selectedGroup || item.group_two === selectedGroup || item.group_three === selectedGroup) {
                $scope.data.push(item);
            }
        }
    }

    // TalentHighPerformers
    $scope.TalentHighPerformers = function (data) {
        // High Performer
        appService.GetAllScores({})
            .then(function (ret) {
                appService.HighPerformer({ getAllScores: ret.data })
                    .then(function (ret) {
                        $scope.HighPerformer = ret.data;
                        highperformers.set(ret.data);
                        $location.path('/TalentHighPerformers');
                    })
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

    // 成功典範
    $scope.TalentSuccessExample = function (data) {
        $location.path('/TalentSuccessExample');
    }

    // 選擇要看的成員
    $scope.TalentRecord = function (data) {
        // 取得員工履歷
        appService.Get({ empno: data })
            .then(function (ret) {
                $scope.Get = ret.data;
                dataservice.set(ret.data[0]);
                $location.path('/TalentRecord');
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

    // 上傳年度績效檔案
    $(document).on("click", "#btnUpload", function () {
        var files = $("#importFile").get(0).files;

        var formData = new FormData();
        formData.append('importFile', files[0]);

        $.ajax({
            url: '/Talent/ImportFile',
            data: formData,
            type: 'POST',
            contentType: false,
            processData: false,
            success: function (data) {
                if (data === true) {
                    alert("更新完成");
                }
                else {
                    alert("上傳格式錯誤");
                }
            }
        });
    });

}]);

app.controller('TalentHighPerformersCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', 'dataservice', 'highperformers', function ($scope, $location, $window, appService, $rootScope, dataservice, highperformers) {

    $scope.GetAll = highperformers.get(); // 取得員工履歷
    $scope.positions = ['', '技術經理', '計畫經理', '組長'];
    $scope.groups = ['全部顯示', '技術經理', '計畫經理', '組長'];
    $scope.selectedGroup = $scope.groups[0];

    // 依群組顯示
    $scope.FilterDataByGroup = function (selectedGroup) {
        $scope.data = [];
        for (let item of $scope.GetAll) {
            if (selectedGroup === '全部顯示') {
                $scope.data.push(item);
            }
            else if (item.position === selectedGroup) {
                $scope.data.push(item);
            }
        }
    }
    $scope.FilterDataByGroup($scope.selectedGroup); // 預設全選

    // 即時監測checkbox三個勾選後才開放下拉選單
    function onExpandableTextareaInput({ target: elem }) {

        // 統計checkbox勾選數量
        const count = 0;
        if (elem.id.includes('inlineCheckbox')) {
            var index = elem.id.substr(-1); // 點選checkbox的index
            for (let i = 1; i <= 5; i++) {
                var elemId = 'inlineCheckbox' + i + index;
                var targetElem = document.getElementById(elemId);
                if (targetElem.checked) {
                    count += 1;
                }
            }
            if (count >= 3) { user.selectPosition = false; }
            else { user.selectPosition = true; }
        }
    }

    // global delegated event listener
    document.addEventListener('input', onExpandableTextareaInput)

    // 三個勾選才開放下拉選單
    $scope.isChecked = function (user) {
        user.selectPosition = true;
        var count = 0;
        if (user.choice1 === true) {count++;}
        if (user.choice2 === true) {count++;}
        if (user.choice3 === true) {count++;}
        if (user.choice4 === true) {count++;}
        if (user.choice5 === true) { count++; }
        if (count >= 3) { user.selectPosition = false; }
        else { user.selectPosition = true; }
    }

    // 回上頁
    $scope.ToTalent = function () {
        $window.location.href = 'Talent#!/TalentOption';
    }

    // 選擇要看的成員
    $scope.TalentRecord = function (data) {
        // 取得員工履歷
        appService.Get({ empno: data })
            .then(function (ret) {
                $scope.Get = ret.data;
                dataservice.set(ret.data[0]);
                $location.path('/TalentRecord');
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

    // 儲存
    $scope.SaveChoice = function (data) {
        // 取得員工履歷
        appService.SaveChoice({ users: data })
            .then(function (ret) {
                if (ret.data === true) { alert('儲存成功'); }
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

}]);

app.controller('TalentSuccessExampleCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', 'dataservice', 'highperformers', function ($scope, $location, $window, appService, $rootScope, dataservice, highperformers) {

    // 回上頁
    $scope.ToTalent = function () {
        $window.location.href = 'Talent#!/TalentOption';
    }

}]);

app.controller('TalentRecordCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', 'dataservice', function ($scope, $location, $window, appService, $rootScope, dataservice) {

    $scope.user = dataservice.get(); // 取得員工履歷
    $scope.planning = []; // 規劃進程
    for (let i = 0; i < $scope.user.planning.length; i++) {
        if ($scope.planning.length < 6) {
                $scope.planning.push({ plan: $scope.user.planning[i] });
        }
    }
    for (let i = 0; i < 6; i++) {
        if ($scope.planning.length < 6) {
            $scope.planning.push({ plan: '' });
        }
        else {
            break;
        }
    }

    // 上傳測評資料檔案
    $(document).on("click", "#btnPDFUpload", function () {
        var files = $("#importPDFFile").get(0).files;

        var formData = new FormData();
        formData.append('importPDFFile', files[0]);

        $.ajax({
            url: '/Talent/ImportPDFFile',
            data: formData,
            type: 'POST',
            contentType: false,
            processData: false,
            success: function (data) {
                if (data != "") {
                    $scope.user.advantage = data[0].advantage;
                    $scope.user.disadvantage = data[0].disadvantage;
                    $scope.user.test = data[0].test;
                    $scope.$apply(); // 用$apply強制刷新數據
                    alert("更新完成");
                }
                else {
                    alert("上傳格式錯誤");
                }
            }
        });
    });

    // 回上頁
    $scope.ToTalent = function () {
        $window.location.href = 'Talent#!/TalentOption';
    }

    // 儲存
    $scope.SaveResponse = function (data) {

        // 解析回傳的planning字串
        let planning = '';
        for (let i = 0; i < $scope.planning.length; i++) {
            planning += $scope.planning[i].plan + '\n';
        }        
        // 取得員工履歷
        appService.SaveResponse({ userCV: data, planning: planning })
            .then(function (ret) {
                if (ret.data === false) { alert('僅限協理儲存'); }
                else { alert('儲存成功'); }
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

}]);