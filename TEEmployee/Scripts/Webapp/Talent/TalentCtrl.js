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

    // High Performer
    this.HighPerformer = (o) => {
        return $http.post('Talent/HighPerformer', o);
    }
    // 取得年份
    this.GetYears = (o) => {
        return $http.post('Talent/GetYears', o);
    };
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
    // 取得所有員工職等職級
    this.GetSenioritys = function (o) {
        return $http.post('Talent/GetSenioritys', o);
    };
    // 條件篩選
    this.ConditionFilter = function (o) {
        return $http.post('Talent/ConditionFilter', o);
    };
    // 取得員工履歷
    this.Get = function (o) {
        return $http.post('Talent/Get', o);
    };
    // 儲存回覆
    this.SaveResponse = function (o) {
        return $http.post('Talent/SaveResponse', o);
    };
    // 取得測評資料PDF
    this.GetPDF = (o) => {
        return $http.post('Talent/GetPDF', o, config);
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
        user.group_four = data.group_four;
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
            else if (item.group === selectedGroup || item.group_one === selectedGroup || item.group_two === selectedGroup || item.group_three === selectedGroup || item.group_four === selectedGroup) {
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
            appService.HighPerformer({})
                .then(function (ret) {
                    $scope.HighPerformer = ret.data.Item1;
                })

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

    // 取得所有員工職等職級
    appService.GetSenioritys({})
        .then(function (ret) {
            $scope.senioritys = ret.data;
        });

    // 條件篩選
    $scope.educationals = ["", "學士", "碩士", "博士"];
    $scope.filter = {
        age1: 0,
        age2: 100,
        companyYear1: 0,
        companyYear2: 100,
        educational: "",
        seniority: ""
    };
    $scope.ConditionFilter = function (filter) {
        appService.ConditionFilter({ filter: filter, json: angular.toJson($scope.GetAll) })
            .then(function (ret) {
                $scope.data = [];
                for (let item of ret.data) {
                    $scope.data.push(item);
                }
            });
    }

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
            else if (item.group === selectedGroup || item.group_one === selectedGroup || item.group_two === selectedGroup || item.group_three === selectedGroup || item.group_four === selectedGroup) {
                $scope.data.push(item);
            }
        }
    }

    // TalentHighPerformers
    $scope.TalentHighPerformers = function () {
        // High Performer
        appService.HighPerformer({})
            .then(function (ret) {
                if (ret.data.Item2 === "") {
                    $scope.HighPerformer = ret.data.Item1;
                    highperformers.set(ret.data.Item1);
                    $location.path('/TalentHighPerformers');
                }
                else {
                    alert(ret.data.Item2);
                }
            })
            .catch(function (ret) {
                alert(ret.data);
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
                alert('未上傳履歷');
            });
        // 取得年份
        appService.GetYears({})
            .then(function (ret) {
                console.log(ret.data);
                $scope.years = ret.data;
                $scope.selectedYear = $scope.years[0];
            })
    }

    //// 上傳年度績效檔案
    //$(document).on("click", "#btnUpload", function () {
    //    var files = $("#importFile").get(0).files;

    //    var formData = new FormData();
    //    formData.append('importFile', files[0]);

    //    $.ajax({
    //        url: 'Talent/ImportFile',
    //        data: formData,
    //        type: 'POST',
    //        contentType: false,
    //        processData: false,
    //        success: function (data) {
    //            if (data === true) {
    //                alert("更新完成");
    //            }
    //            else {
    //                alert("上傳格式錯誤");
    //            }
    //        }
    //    });
    //});

}]);

app.controller('TalentHighPerformersCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', 'dataservice', 'highperformers', function ($scope, $location, $window, appService, $rootScope, dataservice, highperformers) {

    $scope.GetAll = highperformers.get(); // 取得員工履歷
    // 直接開啟網址或重新整理時沒有資料，回到人員篩選頁
    if (!Array.isArray($scope.GetAll)) {
        $location.path('/TalentOption');
        return;
    }
    //$scope.positions = ['', '技術經理', '計畫經理', '組長'];
    //$scope.groups = ['全部顯示', '技術經理', '計畫經理', '組長'];
    $scope.groups = ['全部顯示', '技術經理', '技術組長', '一般計畫經理', '重大計畫經理', '重大計畫副理', '計畫組長', '研發案主持人'];
    $scope.positions = ['', '技術經理', '技術組長', '一般計畫經理', '重大計畫經理', '重大計畫副理', '計畫組長', '研發案主持人'];

    $scope.agesRange = {
        selectedRange: '20,99',
        rangeOptions: [
            { name: '全部顯示', value: '20,99' },
            { name: '60以上', value: '60,99' },
            { name: '50 ~ 59', value: '50,59' },
            { name: '40 ~ 49', value: '40,49' },
            { name: '30 ~ 39', value: '30,39' },
            { name: '20 ~ 29', value: '20,29' },            
        ]
    };

    // get age    
    appService.GetAll({}).then((ret) => {

        for (let item of $scope.GetAll) {
            item.age = ret.data.find(x => x.empno === item.empno).age;
        }  
    })
        
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

    // 勾選數量 (0~5)
    $scope.ChoiceCount = function (user) {
        return (user.choice1 ? 1 : 0) + (user.choice2 ? 1 : 0) + (user.choice3 ? 1 : 0) + (user.choice4 ? 1 : 0) + (user.choice5 ? 1 : 0);
    }
    // 勾選 3 項以上才開放職位下拉選單
    // 舊版用 document 的 change 事件、以勾選框 id 的「最後一個字元」當索引去改 $scope.data[索引]：
    //   1. 第 10 位以後 (索引兩位數) 只取到個位數，例如第 11 位 (索引 10) 會被當成第 1 位
    //   2. 畫面經過年齡篩選與年齡排序，畫面上的順序和 $scope.data 的順序不同，即使前 10 位也可能改到別人
    // 改為直接傳入該員工物件，不再依賴索引
    $scope.UpdateSelectable = function (user) {
        user.selectPosition = $scope.ChoiceCount(user) < 3;
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

    $scope.ageFilter = (user) => {

        let [lowerNumber, upperNumber] = $scope.agesRange.selectedRange.split(',').map(Number);
        if (user.age >= lowerNumber && user.age <= upperNumber)
            return true;
        return false;
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
    // 直接開啟網址或重新整理時沒有選取員工，回到人員篩選頁
    if (!$scope.user || !$scope.user.empno) {
        $location.path('/TalentOption');
        return;
    }
    // 是否為本人頁面 (selfEmpno 由 TalentRecord.cshtml 以 ng-init 帶入，員編忽略前導 0)
    $scope.isSelf = function () {
        const norm = (x) => String(x || '').trim().replace(/^0+/, '');
        return !!$scope.selfEmpno && norm($scope.selfEmpno) === norm($scope.user.empno);
    }

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

    // 取得年份
    appService.GetYears({})
        .then(function (ret) {
            $scope.years = ret.data;
            $scope.selectedYear = $scope.years[0];
        })

    // 上傳測評資料檔案
    $scope.uploadFile = () => {
        filepicker.click();
    }
    try {
        filepicker.addEventListener('change', (e) => {
            if (!e.target.files[0]) {
                return;
            }
            const maxAllowedSize = 10 * 1024 * 1024;
            if (e.target.files[0].size > maxAllowedSize) {
                alert("檔案超過10MB");
                return;
            }
            const fileType = 'application/pdf';
            if (e.target.files[0].type !== fileType) {
                alert("檔案限定格式為.pdf");
                return;
            }
            uploading(e).then(() => {
                $scope.$apply(function (ret) {
                    alert("更新完成");
                });
            });
        });
    }
    catch(error){

    }
    const uploading = async (e) => {
        let form = new FormData(formElem);
        let response = await fetch('Talent/ImportPDFFile', {
            method: 'POST',
            body: form,
        });

        let result = await response.json();
        $scope.user.advantage = result[0].advantage;
        $scope.user.disadvantage = result[0].disadvantage;
        $scope.user.test = result[0].test;
    }

    // 依年份顯示
    $scope.FilterDataByYear = function (selectedYear) {
        $scope.selectedYear = selectedYear;
        GetPDF(false); // 取得PDF
    }

    //// 上傳測評資料檔案
    //$(document).on("click", "#btnPDFUpload", function () {
    //    var files = $("#importPDFFile").get(0).files;

    //    var formData = new FormData();
    //    formData.append('file', files[0]);

    //    $.ajax({
    //        url: 'Talent/ImportPDFFile',
    //        data: formData,
    //        type: 'POST',
    //        contentType: false,
    //        processData: false,
    //        success: function (data) {
    //            if (data != "") {
    //                $scope.user.advantage = data[0].advantage;
    //                $scope.user.disadvantage = data[0].disadvantage;
    //                $scope.user.test = data[0].test;
    //                $scope.$apply(); // 用$apply強制刷新數據
    //                alert("更新完成");
    //            }
    //            else {
    //                alert("上傳格式錯誤");
    //            }
    //        }
    //    });
    //});

    // 上傳測評資料PDF：存到「目前頁面的員工」(後端：部門主管可上傳任何人，其他人只能上傳自己)
    // 舊版用 $(document).on 綁定，每進入一次頁面就多綁一次，會重複上傳，且舊的綁定仍指向之前看過的員工；改為 ng-click
    $scope.UploadPDF = function () {
        var files = $("#importDDIPDFFile").get(0).files;
        if (!files || files.length === 0) { alert("請先選擇 PDF 檔案"); return; }
        if (files[0].size > 20 * 1024 * 1024) { alert("檔案超過20MB"); return; }

        var formData = new FormData();
        formData.append('file', files[0]);
        formData.append('year', $scope.selectedYear);
        formData.append('empno', $scope.user.empno);

        $.ajax({
            url: 'Talent/UploadPDFFile',
            enctype: "multipart/form-data",
            data: formData,
            type: 'POST',
            contentType: false,
            processData: false,
            success: function (data) {
                if (typeof data === 'string' && !data.includes("Error")) {
                    GetPDF(false); // 取得PDF
                    $("#importDDIPDFFile").val('');
                    alert("上傳成功");
                }
                else {
                    alert(data);
                }
            },
            error: function (data) {
                alert(data.responseText);
            }
        });
    }

    function GetPDF(show) {
        appService.GetPDF({ year: $scope.selectedYear, empno: $scope.user.empno })
            .then(function (ret) {
                if (ret.data.size != 0) {
                    const contentDispositionHeader = ret.headers('Content-Disposition');
                    let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
                    fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');
                    var pdfData = new Blob([ret.data], { type: 'application/pdf' });
                    $scope.Show = 1;
                    //$scope.GetPDF = ret.data;
                    framePDF.src = URL.createObjectURL(pdfData);
                }
                else {
                    $scope.Show = 0;
                    if (show === true) {
                        alert("簡報尚未上傳");
                    }
                    else {
                        framePDF.src = "";
                    }
                }
            });
    }
    GetPDF(false); // 取得DDI PDF

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
                if (ret.data === false) { alert('儲存失敗：沒有儲存權限'); }
                else { alert('儲存成功'); }
            })
            .catch(function (ret) {
                alert('Error');
            });
    }

}]);