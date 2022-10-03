var app = angular.module('app', ['moment-picker', 'ngAnimate']);

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

}]);

app.controller('ListCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

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
    $scope.reverse = true;
    
    $scope.sortBy = function (propertyName) {
        $scope.reverse = ($scope.propertyName === propertyName) ? !$scope.reverse : false;
        $scope.propertyName = propertyName;
    };



    $scope.GetAllMonthlyRecordData = () => {

        //let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`;
        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`;

        appService.GetAllMonthlyRecordData({yymm: yymm}).then((ret) => {

            $scope.data = ret.data;
        })
    }   

   
    $scope.GetAllMonthlyRecordData();

}]);

app.controller('DetailsCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    appService.GetTasklogDataByGuid({ guid: $window.guid }).then((ret) => {

        //$scope.ptdata = ret.data;
        $scope.projects = [];

        const projectItems = ret.data.ProjectItems;
        const projectTasks = ret.data.ProjectTasks;

        // add task first
        for (let task of projectTasks) {

            // add if projectItems owns the same projno

            if (projectItems.findIndex(x => x.projno === task.projno) < 0)
                continue;

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
                $scope.projects[projidx].itemno += `${item.itemno} `;
                $scope.projects[projidx].workHour += item.workHour;
                $scope.projects[projidx].overtime += item.overtime;
            }
            else {
                $scope.projects[projidx].itemno = `${item.itemno} `;
                $scope.projects[projidx].workHour = item.workHour;
                $scope.projects[projidx].overtime = item.overtime;
            }

            //if ($scope.projects[projidx].workHour > 0 || $scope.projects[projidx].overtime > 0)
            //    $scope.projects[projidx].hourStr = $scope.projects[projidx].workHour.toString() + ' + ' + $scope.projects[projidx].overtime.toString();

        }

        for (let i = 0; i < $scope.projects.length; i++) {

            if ($scope.projects[i].workHour || $scope.projects[i].overtime) {

                $scope.projects[i].hourStr = $scope.projects[i].workHour.toString() + ' + ' + $scope.projects[i].overtime.toString();
                               
            }

        }


        //if ($scope.projects.length === 0)
        //    $scope.projects.push = { logs: [{}] };

    })

}]);

app.controller('EditCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', '$timeout', function ($scope, $window, appService, $rootScope, $q, $timeout) {

    // select year and month
    $scope.months = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'];
    $scope.years = [];

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

    $scope.projects = [{ logs: [{}]}];
    //$scope.addProjRow = (idx, projno) => {
    //    $scope.rowList.splice(idx + 1, 0, { projno: projno });
    //};

    //$scope.addLogRow = (projectidx, idx) => {
    //    $scope.projects[projectidx].logs.splice(idx + 1, 0, {} );
    //};

    // add at bottim
    $scope.addLogRow = (projectidx, idx) => {
        $scope.projects[projectidx].logs.splice($scope.projects[projectidx].logs.length, 0, {});
    };

    $scope.removeLogRow = (projectidx, idx) => {

        let id = $scope.projects[projectidx].logs[idx].id;
        if (id)
            $scope.deletedIds.push(id); 

        $scope.projects[projectidx].logs.splice(idx, 1);
    };
    $scope.addProjRow = () => {
        $scope.projects.push({ logs: [{}]});
    };

    $scope.UpdateTasklogData = () => {

        //flatten projects

        let projectTasks = [];
        //let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`
        let yymm = `${Number($scope.ctrl.datepicker.slice(0, 4)) - 1911}${$scope.ctrl.datepicker.slice(5, 7)}`; 

        for (let project of $scope.projects) {

            for (let log of project.logs) {

                if (log.content) {

                    projectTasks.push({
                        id: log.id, yymm: yymm, projno: project.projno, realHour: project.realHour, 
                        content: log.content, endDate: log.endDate, note: log.note
                    });
                }

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
                    $scope.projects[projidx].itemno += `${item.itemno} `;
                    $scope.projects[projidx].workHour += item.workHour;
                    $scope.projects[projidx].overtime += item.overtime;
                }
                else {
                    $scope.projects[projidx].itemno = `${item.itemno} `;
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

                    $scope.projects[i].hourStr = $scope.projects[i].workHour.toString() + ' + ' + $scope.projects[i].overtime.toString();

                    if (!$scope.projects[i].realHour) {
                        $scope.projects[i].realHour = $scope.projects[i].workHour + $scope.projects[i].overtime;
                    }

                }
                
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
                     
                    const textAreaItems = document.querySelectorAll(".autoExpand");
                    for (let elm of textAreaItems) {
                        elm.style.height = "";
                        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
                    }

                    
            }, 0);
            



        });
    }


    $scope.GetTasklogData();

    function onExpandableTextareaInput({ target: elm }) {

        if (!elm.classList.contains('autoExpand') || !elm.nodeName === 'TEXTAREA') return

        elm.style.height = "";
        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
    }

    // global delegated event listener
    document.addEventListener('input', onExpandableTextareaInput)


}]);