﻿var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllMonthlyRecord = (o) => {
        return $http.post('Tasklog/GetAllMonthlyRecord', o);
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

    $scope.GetAllMonthlyRecord = () => {

        let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`;

        appService.GetAllMonthlyRecord({yymm: yymm}).then((ret) => {

            $scope.data = ret.data;

        })
    }   

    $scope.GetAllMonthlyRecord();

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
                $scope.projects.push({ logs: [], projno: task.projno });
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
                $scope.projects[projidx].itemno += `${item.itemno}\n`;
                $scope.projects[projidx].workHour += item.workHour;
            }
            else {
                $scope.projects[projidx].itemno = `${item.itemno}\n`;
                $scope.projects[projidx].workHour = item.workHour;
            }

        }


        if ($scope.projects.length === 0)
            $scope.projects.push = { logs: [{}] };

    })

}]);

app.controller('EditCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

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
    $scope.deletedIds = [];

    //appService.GetTasklogData({}).then((ret) => {

    //    $scope.data = ret.data;

    //})

    $scope.projects = [{ logs: [{}]}];
    //$scope.addProjRow = (idx, projno) => {
    //    $scope.rowList.splice(idx + 1, 0, { projno: projno });
    //};
    $scope.addLogRow = (projectidx, idx) => {
        $scope.projects[projectidx].logs.splice(idx + 1, 0, {} );
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
        let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`

        for (let project of $scope.projects) {

            for (let log of project.logs) {

                if (log.content) {

                    projectTasks.push({
                        id: log.id, yymm: yymm, projno: project.projno,
                        content: log.content, endDate: log.endDate, note: log.note
                    });
                }

            }

        }

        if ($scope.deletedIds.length !== 0) {
            appService.DeleteProjectTask($scope.deletedIds)
                .then((ret) => { })
                .catch((ret) => { alert('Error'); }
                );
        }        

        if (projectTasks.length !== 0) {
            appService.UpdateProjectTask(projectTasks)
                .then((ret) => { })
                .catch((ret) => { alert('Error'); }
                );
        }     

        $window.location.href = 'Home';

        //appService.UpdateProjectTask(projectTasks).then((ret) => {
        //    $window.location.href = 'Home';
        //});

    }

    $scope.GetTasklogData = () => {

        let yymm = `${Number($scope.selectedYear) - 1911}${$scope.selectedMonth}`;        

        appService.GetTasklogData({ yymm: yymm }).then((ret) => {

            //$scope.ptdata = ret.data;
            $scope.projects = [];

            const projectItems = ret.data.ProjectItems;
            const projectTasks = ret.data.ProjectTasks;

            // add task first
            for (let task of projectTasks) {
                                
                let projidx = $scope.projects.findIndex(x => x.projno === task.projno);

                if (projidx < 0) {
                    $scope.projects.push({ logs: [], projno: task.projno });
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
                    $scope.projects[projidx].itemno += `${item.itemno}\n`;
                    $scope.projects[projidx].workHour += item.workHour;
                }
                else {
                    $scope.projects[projidx].itemno = `${item.itemno}\n`;
                    $scope.projects[projidx].workHour = item.workHour;
                }
                
            }


            if ($scope.projects.length === 0)
                $scope.projects.push = { logs: [{}] };


        });
    }


    $scope.GetTasklogData();


}]);