var app = angular.module('app', ['moment-picker', 'ui.router', 'ngAnimate']);

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('UserList', {
            url: '/UserList',
            templateUrl: 'Tasklog/UserList'
        })
        .state('UserDetails', {
            url: '/UserDetails',
            templateUrl: 'Tasklog/UserDetails'
        })
        .state('UsersDetails', {
            url: '/UsersDetails',
            templateUrl: 'Tasklog/UsersDetails'
        })

}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllMonthlyRecordData = (o) => {
        return $http.post('Tasklog/GetAllMonthlyRecordData', o);
    };
    //this.GetProjectItem = (o) => {
    //    return $http.post('Tasklog/GetProjectItem', o);
    //};
    // personal
    this.UpdateProjectTask = (o) => {
        return $http.post('Tasklog/UpdateProjectTask', o);
    };
    //this.GetProjectTask = (o) => {
    //    return $http.post('Tasklog/GetProjectTask', o);
    //};  
    this.GetTasklogData = (o) => {
        return $http.post('Tasklog/GetTasklogData', o);
    };
    this.DeleteProjectTask = (o) => {
        return $http.post('Tasklog/DeleteProjectTask', o);
    };
    //guid
    this.GetTasklogDataByGuid = (o) => {
        return $http.post('Tasklog/GetTasklogDataByGuid', o);
    };
    this.GetUserByGuid = (o) => {
        return $http.post('Tasklog/GetUserByGuid', o);
    };

    // 匯入上月資料 <-- 培文
    this.GetLastMonthData = (o) => {
        return $http.post('Tasklog/GetLastMonthData', o);
    };
    // 取得使用者群組 <-- 培文
    this.GetGroups = (o) => {
        return $http.post('Tasklog/GetGroups', o);
    };
    // 群組篩選 <-- 培文
    this.GetGroupByName = (o) => {
        return $http.post('Tasklog/GetGroupByName', o);
    };
    // 個人詳細內容 <-- 培文
    this.GetUserContent = (o) => {
        return $http.post('Tasklog/GetUserContent', o);
    };
    // 多人詳細內容 <-- 培文
    this.GetMemberContent = (o) => {
        return $http.post('Tasklog/GetMemberContent', o);
    };

}]);

app.factory('myFactory', function () {

    var savedData = {}

    function set(data, data1) {
        savedData.users = data;
        savedData.monthlyRecord = data1;
    }

    function get() {
        return savedData;
    }

    return {
        set: set,
        get: get,
    }

});
app.factory('groupsFactory', function () {

    var savedData = {}

    function set(data) {
        savedData.monthlyRecordData = data;
    }

    function get() {
        return savedData;
    }

    return {
        set: set,
        get: get,
    }

});
app.factory('userDetailsFactory', function () {

    var savedData = {}

    function set(data, data1) {
        savedData.yymms = data;
        savedData.user = data1;
    }

    function get() {
        return savedData;
    }

    return {
        set: set,
        get: get,
    }

});

app.controller('ListCtrl', ['$scope', '$window', 'appService', '$rootScope', '$location', function ($scope, $window, appService, $rootScope, $location) {

    $location.path('/UserList');

}]);
app.controller('UserListCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', '$q', 'myFactory', 'groupsFactory', 'userDetailsFactory', function ($scope, $location, $window, appService, $rootScope, $q, myFactory, groupsFactory, userDetailsFactory) {

    // select year and month
    $scope.months = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'];
    $scope.years = [];

    const date = new Date();
    const [month, year] = [date.getMonth(), date.getFullYear()];

    for (let i = year; i !== 2019; i--) {
        $scope.years.push(i.toString());
    }

    $scope.selectedYear = $scope.years[0];
    $scope.selectedMonth = $scope.months[month];

    $scope.data = [];

    $scope.ctrl = {};
    $scope.ctrl.datepicker = moment().add(-1, 'months').locale('zh-tw').format('YYYY-MM');

    $scope.propertyName = 'User.group_one';
    $scope.reverse = false;

    $scope.sortBy = function (propertyName) {
        $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
        $scope.propertyName = propertyName;
    };

    $scope.GetAllMonthlyRecordData = () => {
        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`;

        appService.GetAllMonthlyRecordData({ yymm: yymm }).then((ret) => {
            $scope.groups = []; // 重置groups
            $scope.data = ret.data;
            groupsFactory.set(ret.data);
            // 取得使用者群組 <-- 培文
            appService.GetGroups({ json: angular.toJson($scope.data) }).then((ret) => {
                ret.data.forEach(function (item) {
                    $scope.groups.push({ id: item, name: item });
                });
                $scope.group = $scope.groups[0].name;
            })
        })
    }
    //$scope.GetAllMonthlyRecordData();

    // 全選
    $(document).ready(function () {
        $("#CheckAll").click(function () {
            if ($("#CheckAll").prop("checked")) {
                $("input[name='checkboxs']").prop("checked", true);
                $scope.data.forEach(function (item) {
                    item.selected = true;
                });
            } else {
                $("input[name='checkboxs']").prop("checked", false);
                $scope.data.forEach(function (item) {
                    item.selected = false;
                });
            }
        })
    })

    // 群組篩選 <-- 培文
    $scope.GetGroupByName = function (name) {
        appService.GetGroupByName({ json: angular.toJson(groupsFactory.get().monthlyRecordData), groupName: name })
            .then(function (ret) {
                $("#CheckAll").prop("checked", false); // 預設取消全選
                $scope.data = ret.data;
            });
    }

    // 個人詳細內容 <-- 培文
    $scope.GetUserContent = function (data) {
        // 近三個月
        let yymms = [];
        for (let i = 1; i < 4; i++) {
            yymm = moment().add(-i, 'months').locale('zh-tw').format('YYYY-MM');
            yymm = `${Number(yymm.slice(0, 4)) - 1911}${yymm.slice(5, 7)}`;
            yymms.push(yymm);
        }

        userDetailsFactory.set(yymms, data);
        $location.path('/UserDetails');
    }

    // 多人詳細內容 <-- 培文
    $scope.GetMemberContent = function (data) {
        let selectedUsers = $scope.data.filter(x => x.selected === true);
        let monthlyRecord = [];
        let users = [];
        for (let i = 0; i < selectedUsers.length; i++) {
            monthlyRecord.push(selectedUsers[i].MonthlyRecord);
            users.push(selectedUsers[i].User);
        }

        try {
            myFactory.set(users, selectedUsers[0].MonthlyRecord.yymm);
            $location.path('/UsersDetails');
        }
        catch {
            alert('請選取要查詢的人員');
        }
    }
}]);
app.controller('UserDetailsCtrl', ['$scope', '$location', '$window', 'appService', '$rootScope', '$q', 'myFactory', 'userDetailsFactory', function ($scope, $location, $window, appService, $rootScope, $q, myFactory, userDetailsFactory) {

    $scope.data = [];
    $scope.ctrl = {};
    $scope.ctrl.datepicker = moment().add(-3, 'months').locale('zh-tw').format('YYYY-MM');
    $scope.ctrl.datepicker1 = moment().add(-1, 'months').locale('zh-tw').format('YYYY-MM');
    $scope.user = userDetailsFactory.get().user;    

    let projectTypeMap = ['', '計畫執行', '研發創新', '技術深根'];
    
    $scope.GetUserAllMonthlyRecordData = () => {
        // 取得個人各月詳細工作項目
        appService.GetUserContent({ startMonth: $scope.ctrl.datepicker, endMonth: $scope.ctrl.datepicker1, user: $scope.user }).then((ret) => {
            $scope.data = ret.data;
            $scope.projectList = [];

            for (let data of ret.data) {

                $scope.projects = [];
                $scope.projects.user = data.User;
                $scope.projects.yymm = data.yymm;
                const projectItems = data.ProjectItems;
                const projectTasks = data.ProjectTasks;

                // add task first
                for (let task of projectTasks) {
                    let projidx = $scope.projects.findIndex(x => x.projno === task.projno);
                    if (projidx < 0) {
                        $scope.projects.push({ logs: [], projno: task.projno, realHour: task.realHour });
                        projidx = $scope.projects.length - 1;
                    }
                    $scope.projects[projidx].logs.push({ user: data.User, yymm: data.yymm, id: task.id, content: task.content, endDate: task.endDate, note: task.note, projectType: projectTypeMap[task.projectType] });
                }

                // fill in project item
                for (let item of projectItems) {
                    let projidx = $scope.projects.findIndex(x => x.projno === item.projno);
                    if (projidx < 0) {
                        $scope.projects.push({ logs: [{}], projno: item.projno });
                        projidx = $scope.projects.length - 1;
                    }
                    if ($scope.projects[projidx].itemno) {
                        $scope.projects[projidx].itemno += `${item.itemno}, `;
                        $scope.projects[projidx].workHour += item.workHour;
                        $scope.projects[projidx].overtime += item.overtime;
                    }
                    else {
                        $scope.projects[projidx].itemno = `${item.itemno}, `;
                        $scope.projects[projidx].workHour = item.workHour;
                        $scope.projects[projidx].overtime = item.overtime;
                    }
                }

                for (let i = 0; i < $scope.projects.length; i++) {
                    if ($scope.projects[i].workHour || $scope.projects[i].overtime) {
                        $scope.projects[i].hourStr = `${$scope.projects[i].workHour} + ${$scope.projects[i].overtime} = ${$scope.projects[i].workHour + $scope.projects[i].overtime}`;
                    }

                    // 20240304 Update
                    if (!$scope.projects[i].realHour) {
                        $scope.projects[i].realHour = $scope.projects[i].workHour + $scope.projects[i].overtime;
                    }

                    if ($scope.projects[i].itemno)
                        $scope.projects[i].itemno = $scope.projects[i].itemno.slice(0, $scope.projects[i].itemno.length - 2);
                }

                $scope.projectList.push({ projects: $scope.projects });
            }
        })
            .catch(function (ret) {
                alert('Error');
            });
    }

    $scope.GetUserAllMonthlyRecordData();

}]);
app.controller('UsersDetailsCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

    $scope.ctrl = {};
    $scope.ctrl.datepicker = `${Number(myFactory.get().monthlyRecord.slice(0, 3)) + 1911}-${myFactory.get().monthlyRecord.slice(3, 5)}`;

    let projectTypeMap = ['', '計畫執行', '研發創新', '技術深根'];

    $scope.GetMemberAllMonthlyRecordData = () => {
        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`;

        appService.GetAllMonthlyRecordData({ yymm: yymm }).then((ret) => {
            $scope.data = ret.data;
            $scope.users = myFactory.get().users;

            appService.GetMemberContent({ yymm: yymm, users: $scope.users })
                .then(function (ret) {

                    $scope.yymm = ret.data[0].yymm;
                    $scope.projectList = [];

                    for (let data of ret.data) {

                        $scope.projects = [];
                        $scope.projects.user = data.User;
                        $scope.projects.yymm = data.yymm;
                        const projectItems = data.ProjectItems;
                        const projectTasks = data.ProjectTasks;

                        // add task first
                        for (let task of projectTasks) {
                            let projidx = $scope.projects.findIndex(x => x.projno === task.projno);
                            if (projidx < 0) {
                                $scope.projects.push({ logs: [], projno: task.projno, realHour: task.realHour });
                                projidx = $scope.projects.length - 1;
                            }
                            $scope.projects[projidx].logs.push({ user: data.User, yymm: data.yymm, id: task.id, content: task.content, endDate: task.endDate, note: task.note, projectType: projectTypeMap[task.projectType] });
                        }

                        // fill in project item
                        for (let item of projectItems) {
                            let projidx = $scope.projects.findIndex(x => x.projno === item.projno);
                            if (projidx < 0) {
                                $scope.projects.push({ logs: [{}], projno: item.projno });
                                projidx = $scope.projects.length - 1;
                            }
                            if ($scope.projects[projidx].itemno) {
                                $scope.projects[projidx].itemno += `${item.itemno}, `;
                                $scope.projects[projidx].workHour += item.workHour;
                                $scope.projects[projidx].overtime += item.overtime;
                            }
                            else {
                                $scope.projects[projidx].itemno = `${item.itemno}, `;
                                $scope.projects[projidx].workHour = item.workHour;
                                $scope.projects[projidx].overtime = item.overtime;
                            }
                        }

                        for (let i = 0; i < $scope.projects.length; i++) {
                            if ($scope.projects[i].workHour || $scope.projects[i].overtime) {
                                $scope.projects[i].hourStr = $scope.projects[i].workHour.toString() + ' + ' + $scope.projects[i].overtime.toString();
                            }
                            if ($scope.projects[i].itemno)
                                $scope.projects[i].itemno = $scope.projects[i].itemno.slice(0, $scope.projects[i].itemno.length - 2);
                        }

                        $scope.projectList.push({ projects: $scope.projects });
                    }
                })
                .catch(function (ret) {
                    alert('Error');
                });
        })
    }

    $scope.GetMemberAllMonthlyRecordData();

}]);
app.controller('DetailsCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    appService.GetUserByGuid({ guid: $window.guid }).then((ret) => {
        $scope.user = ret.data;
        document.title = $scope.user.name + "-工作紀錄管控表"
    })

    appService.GetTasklogDataByGuid({ guid: $window.guid }).then((ret) => {

        //$scope.ptdata = ret.data;
        $scope.projects = [];

        const projectItems = ret.data.ProjectItems;
        const projectTasks = ret.data.ProjectTasks;

        $scope.yymm = projectTasks[0].yymm;

        // add task first
        for (let task of projectTasks) {

            // add if projectItems owns the same projno
            // 1109 update: show all project even if it dosent'n own 
            //if (projectItems.findIndex(x => x.projno === task.projno) < 0)
            //    continue;

            let projidx = $scope.projects.findIndex(x => x.projno === task.projno);

            if (projidx < 0) {
                $scope.projects.push({ logs: [], projno: task.projno, realHour: task.realHour });
                projidx = $scope.projects.length - 1;
            }

            $scope.projects[projidx].logs.push({ id: task.id, content: task.content, endDate: task.endDate, note: task.note });

        }

        // fill in project item
        for (let item of projectItems) {

            let projidx = $scope.projects.findIndex(x => x.projno === item.projno);

            if (projidx < 0) {
                $scope.projects.push({ logs: [{}], projno: item.projno });
                projidx = $scope.projects.length - 1;
            }

            if ($scope.projects[projidx].itemno) {
                $scope.projects[projidx].itemno += `${item.itemno}, `;
                $scope.projects[projidx].workHour += item.workHour;
                $scope.projects[projidx].overtime += item.overtime;
            }
            else {
                $scope.projects[projidx].itemno = `${item.itemno}, `;
                $scope.projects[projidx].workHour = item.workHour;
                $scope.projects[projidx].overtime = item.overtime;
            }

            //if ($scope.projects[projidx].workHour > 0 || $scope.projects[projidx].overtime > 0)
            //    $scope.projects[projidx].hourStr = $scope.projects[projidx].workHour.toString() + ' + ' + $scope.projects[projidx].overtime.toString();

        }

        for (let i = 0; i < $scope.projects.length; i++) {

            if ($scope.projects[i].workHour || $scope.projects[i].overtime) {
                $scope.projects[i].hourStr = $scope.projects[i].workHour.toString() + ' + ' + $scope.projects[i].overtime.toString();

                // 20240304 Update
                if (!$scope.projects[i].realHour) {
                    $scope.projects[i].realHour = $scope.projects[i].workHour + $scope.projects[i].overtime;
                }
            }

            if ($scope.projects[i].itemno)
                $scope.projects[i].itemno = $scope.projects[i].itemno.slice(0, $scope.projects[i].itemno.length - 2);

        }

        //if ($scope.projects.length === 0)
        //    $scope.projects.push = { logs: [{}] };

    })

}]);
app.controller('EditCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', '$timeout', function ($scope, $window, appService, $rootScope, $q, $timeout) {

    // select year and month
    $scope.months = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'];
    $scope.years = [];

    $scope.projectTypes = [
        { value: '1', name: '計畫執行' },
        { value: '2', name: '研發創新' },
        { value: '3', name: '技術深根' },
    ]

    $scope.ctrl = {};
    $scope.ctrl.datepicker = moment().locale('zh-tw').format('YYYY-MM');
    /*$scope.ctrl.datepicker = "";*/

    const date = new Date();
    const [month, year] = [date.getMonth(), date.getFullYear()];

    var limit = 250; // textarea height limit

    for (let i = year; i !== 2019; i--) {
        $scope.years.push(i.toString());
    }

    $scope.selectedYear = $scope.years[0];
    $scope.selectedMonth = $scope.months[month];

    $scope.data = [];
    $scope.deletedIds = [];

    //appService.GetTasklogData({}).then((ret) => {

    //    $scope.data = ret.data;

    //})

    $scope.projects = [{ logs: [{}] }];
    //$scope.addProjRow = (idx, projno) => {
    //    $scope.rowList.splice(idx + 1, 0, { projno: projno });
    //};

    //$scope.addLogRow = (projectidx, idx) => {
    //    $scope.projects[projectidx].logs.splice(idx + 1, 0, {} );
    //};

    // add at bottom
    $scope.addLogRow = (projectidx, idx) => {
        $scope.projects[projectidx].logs.splice($scope.projects[projectidx].logs.length, 0, {});
    };

    $scope.removeLogRow = (projectidx, idx) => {

        // add apparent hint that projected item cannot be removed, only can clear
        if ($scope.projects[projectidx].itemno !== undefined && $scope.projects[projectidx].logs.length === 1) {
            $scope.projects[projectidx].logs[idx].content = '';
            $scope.projects[projectidx].logs[idx].endDate = '';
            $scope.projects[projectidx].logs[idx].note = '';
            $scope.projects[projectidx].logs[idx].projectType = '';
        }
        else {

            let id = $scope.projects[projectidx].logs[idx].id;
            if (id)
                $scope.deletedIds.push(id);

            $scope.projects[projectidx].logs.splice(idx, 1);

        }

    };

    $scope.moveUpLogRow = (projectidx, idx) => {

        let logs = $scope.projects[projectidx].logs;

        if (logs.length >= 1 && idx > 0) {
            [logs[idx - 1], logs[idx]] = [logs[idx], logs[idx - 1]];
        }        
    };

    $scope.moveDownLogRow = (projectidx, idx) => {

        let logs = $scope.projects[projectidx].logs;

        if (logs.length >= 1 && idx < logs.length - 1) {
            [logs[idx + 1], logs[idx]] = [logs[idx], logs[idx + 1]];
        }
    };


    $scope.addProjRow = () => {
        $scope.projects.push({ logs: [{}] });
    };

    $scope.UpdateTasklogData = () => {

        //flatten projects

        let projectTasks = [];
        //let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`
        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`;

        for (var project of $scope.projects) {

            let customOrder = 0;

            for (let log of project.logs) {

                //if (log.content) {

                //    projectTasks.push({
                //        id: log.id, yymm: yymm, projno: project.projno, realHour: project.realHour, 
                //        content: log.content, endDate: log.endDate, note: log.note
                //    });
                //}


                // 20240304 update: If realHour is equal to workhour plus overtime, save it as 0 in DB
                // This can avoid people saving tasklog too early causing wrong realHour
                if (project.realHour === project.workHour + project.overtime)
                    project.realHour = 0;


                projectTasks.push({
                    id: log.id, yymm: yymm, projno: project.projno, realHour: project.realHour,
                    content: log.content, endDate: log.endDate, note: log.note, projectType: Number(log.projectType),
                    custom_order: customOrder++, generate_schedule: log.generate_schedule,
                });

            }
        }

        //if ($scope.deletedIds.length !== 0) {
        //    appService.DeleteProjectTask($scope.deletedIds)
        //        .then((ret) => {

        //        })
        //        .catch((ret) => { alert('Error'); }
        //        );
        //}        

        //if (projectTasks.length !== 0) {
        //    appService.UpdateProjectTask(projectTasks)
        //        .then((ret) => { })
        //        .catch((ret) => { alert('Error'); }
        //        );
        //}     

        //$scope.GetTasklogData();

        //$scope.succeed = true;
        //$timeout(function () {
        //    $scope.succeed = false;
        //}, 2000);


        //let promiseA = new Promise((resolve, reject) => { });
        //let promiseB = new Promise((resolve, reject) => { });
        let promiseA = Promise.resolve('a');
        let promiseB = Promise.resolve('b');

        if ($scope.deletedIds.length !== 0) {
            promiseA = appService.DeleteProjectTask($scope.deletedIds)
                .then((ret) => {

                })
                .catch((ret) => { alert('Error'); }
                );
        }

        if (projectTasks.length !== 0) {
            promiseB = appService.UpdateProjectTask(projectTasks)
                .then((ret) => { })
                .catch((ret) => { alert('Error'); }
                );
        }

        //$scope.GetTasklogData();

        //$scope.succeed = true;
        //$timeout(function () {
        //    $scope.succeed = false;
        //}, 2000);

        $q.all([promiseA, promiseB]).then((ret) => {

            $scope.GetTasklogData();

            $scope.succeed = true;
            $timeout(function () {
                $scope.succeed = false;
            }, 2000);
        });

        //alert('已儲存');

        //$window.location.reload();

        /*$window.location.href = 'Tasklog/Index';*/

        //appService.UpdateProjectTask(projectTasks).then((ret) => {
        //    $window.location.href = 'Home';
        //});

    }

    $scope.GetTasklogData = () => {

        //let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`;

        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`;
        $scope.selectedMonth = $scope.ctrl.datepicker.slice(5, 7);
        appService.GetTasklogData({ yymm: yymm }).then((ret) => {

            //$scope.ptdata = ret.data;
            $scope.projects = [];

            const projectItems = ret.data.ProjectItems;
            const projectTasks = ret.data.ProjectTasks;

            // add task first
            for (let task of projectTasks) {

                let projidx = $scope.projects.findIndex(x => x.projno === task.projno);

                if (projidx < 0) {
                    $scope.projects.push({ logs: [], projno: task.projno, realHour: task.realHour });
                    projidx = $scope.projects.length - 1;
                }

                $scope.projects[projidx].logs.push({
                    id: task.id, content: task.content, endDate: task.endDate,
                    note: task.note, projectType: (task.projectType) > 0 ? task.projectType.toString() : '',
                    generate_schedule: task.generate_schedule,
                });

            }

            // fill in project item
            for (let item of projectItems) {

                let projidx = $scope.projects.findIndex(x => x.projno === item.projno);

                if (projidx < 0) {
                    $scope.projects.push({ logs: [{}], projno: item.projno });
                    projidx = $scope.projects.length - 1;
                }

                if ($scope.projects[projidx].itemno) {
                    $scope.projects[projidx].itemno += `${item.itemno}, `;
                    $scope.projects[projidx].workHour += item.workHour;
                    $scope.projects[projidx].overtime += item.overtime;
                }
                else {
                    $scope.projects[projidx].itemno = `${item.itemno}, `;
                    $scope.projects[projidx].workHour = item.workHour;
                    $scope.projects[projidx].overtime = item.overtime;
                }

                //if ($scope.projects[projidx].workHour > 0 || $scope.projects[projidx].overtime > 0)
                //    $scope.projects[projidx].hourStr = $scope.projects[projidx].workHour.toString() + ' + ' + $scope.projects[projidx].overtime.toString();                

                //if (!$scope.projects[projidx].realHour) {
                //    $scope.projects[projidx].realHour = $scope.projects[projidx].workHour + $scope.projects[projidx].overtime;
                //}

            }

            for (let i = 0; i < $scope.projects.length; i++) {

                if ($scope.projects[i].workHour || $scope.projects[i].overtime) {

                    //$scope.projects[i].hourStr = $scope.projects[i].workHour.toString() + ' + ' + $scope.projects[i].overtime.toString();
                    $scope.projects[i].hourStr = `${$scope.projects[i].workHour} + ${$scope.projects[i].overtime} = ${$scope.projects[i].workHour + $scope.projects[i].overtime}`;


                    if (!$scope.projects[i].realHour) {
                        $scope.projects[i].realHour = $scope.projects[i].workHour + $scope.projects[i].overtime;
                    }
                }

                if ($scope.projects[i].itemno)
                    $scope.projects[i].itemno = $scope.projects[i].itemno.slice(0, $scope.projects[i].itemno.length - 2);

            }

            //if ($scope.projects[projidx].workHour > 0 || $scope.projects[projidx].overtime > 0)
            //    $scope.projects[projidx].hourStr = $scope.projects[projidx].workHour.toString() + ' + ' + $scope.projects[projidx].overtime.toString();

            //if (!$scope.projects[projidx].realHour) {
            //    $scope.projects[projidx].realHour = $scope.projects[projidx].workHour + $scope.projects[projidx].overtime;
            //}

            if ($scope.projects.length === 0)
                $scope.projects.push({ logs: [{}] });

            // modify textarea height in the beginning

            $timeout(function () {

                var pp = document.querySelectorAll(".autoExpand");
                for (var elm of pp) {
                    elm.style.height = "";
                    elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
                }

            }, 0);

        });
    }

    $scope.GetTasklogData();

    // 匯入上月資料 <-- 培文
    $scope.GetLastMonthData = () => {
        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`;

        // 撈取本月與上月資料
        appService.GetLastMonthData({ yymm: yymm }).then((ret) => {
            $scope.projects = [];
            const projectItems = ret.data.ProjectItems;
            const projectTasks = ret.data.ProjectTasks;

            // add task first
            for (let task of projectTasks) {
                let projidx = $scope.projects.findIndex(x => x.projno === task.projno);
                if (projidx < 0) {
                    $scope.projects.push({ logs: [], projno: task.projno, realHour: task.realHour });
                    projidx = $scope.projects.length - 1;
                }
                $scope.projects[projidx].logs.push({ id: task.id, content: task.content, endDate: task.endDate, projectType: task.projectType.toString(), note: task.note });
            }

            // fill in project item
            for (let item of projectItems) {
                let projidx = $scope.projects.findIndex(x => x.projno === item.projno);

                if (projidx < 0) {
                    $scope.projects.push({ logs: [{}], projno: item.projno });
                    projidx = $scope.projects.length - 1;
                }

                if ($scope.projects[projidx].itemno) {
                    $scope.projects[projidx].itemno += `${item.itemno}, `;
                    $scope.projects[projidx].workHour += item.workHour;
                    $scope.projects[projidx].overtime += item.overtime;
                }
                else {
                    //$scope.projects[projidx].itemno = `${item.itemno}, `; // 修正【匯入上月資料】後無法刪除項目的bug
                    $scope.projects[projidx].workHour = item.workHour;
                    $scope.projects[projidx].overtime = item.overtime;
                }
            }

            for (let i = 0; i < $scope.projects.length; i++) {

                if ($scope.projects[i].workHour || $scope.projects[i].overtime) {

                    $scope.projects[i].hourStr = $scope.projects[i].workHour.toString() + ' + ' + $scope.projects[i].overtime.toString();

                    if (!$scope.projects[i].realHour) {
                        $scope.projects[i].realHour = $scope.projects[i].workHour + $scope.projects[i].overtime;
                    }
                }

                if ($scope.projects[i].itemno)
                    $scope.projects[i].itemno = $scope.projects[i].itemno.slice(0, $scope.projects[i].itemno.length - 2);
            }

            if ($scope.projects.length === 0)
                $scope.projects.push({ logs: [{}] });

            $timeout(function () {

                var pp = document.querySelectorAll(".autoExpand");
                for (var elm of pp) {
                    elm.style.height = "";
                    elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
                }

            }, 0);
        });
    }

    function onExpandableTextareaInput({ target: elm }) {

        if (!elm.classList.contains('autoExpand') || !elm.nodeName === 'TEXTAREA') return

        elm.style.height = "";
        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
    }

    // global delegated event listener
    document.addEventListener('input', onExpandableTextareaInput)

}]);