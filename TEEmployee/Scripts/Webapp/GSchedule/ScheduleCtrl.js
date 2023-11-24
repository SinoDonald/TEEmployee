var app = angular.module('app', ['ui.router', 'moment-picker', 'angularjs-dropdown-multiselect']);
const { csv, select, selectAll, selection } = d3;

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('Group', {
            url: '/Group',
            templateUrl: 'GSchedule/Group'
        })
        .state('Personal', {
            url: '/Personal',
            templateUrl: 'GSchedule/Personal'
        })
        .state('Future', {
            url: '/Future',
            templateUrl: 'GSchedule/Future'
        })
        .state('Project', {
            url: '/Project',
            templateUrl: 'GSchedule/Project'
        })

}]);


app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllSchedules = (o) => {
        return $http.post('GSchedule/GetAllSchedules', o);
    };
    this.UpdateSchedule = (o) => {
        return $http.post('GSchedule/UpdateSchedule', o);
    };
    this.InsertSchedule = (o) => {
        return $http.post('GSchedule/InsertSchedule', o);
    };
    this.GetAuthorization = (o) => {
        return $http.post('GSchedule/GetAuthorization', o);
    };
    this.DeleteSchedule = (o) => {
        return $http.post('GSchedule/DeleteSchedule', o);
    };
    this.HistoryStringTest = (o) => {
        return $http.post('GSchedule/HistoryStringTest', o);
    };
    this.UpdateAllPercentComplete = (o) => {
        return $http.post('GSchedule/UpdateAllPercentComplete', o);
    }
    this.GetAllFutures = (o) => {
        return $http.post('GSchedule/GetAllFutures', o);
    };
    this.GetAllProjectSchedules = (o) => {
        return $http.post('GSchedule/GetAllProjectSchedules', o);
    };
    this.InsertProjectSchedule = (o) => {
        return $http.post('GSchedule/InsertProjectSchedule', o);
    };
    this.DeleteProjectSchedule = (o) => {
        return $http.post('GSchedule/DeleteProjectSchedule', o);
    };

}]);

app.factory('dataservice', function () {

    var scheduleData = {};
    var authData = {};

    return {
        set: set,
        get: get,
        setAuth: setAuth,
        getAuth: getAuth,
    }

    function set(data) {
        scheduleData = data;
    }

    function get() {
        return scheduleData;
    }

    function setAuth(data) {
        authData = data;
    }

    function getAuth() {
        return authData;
    }
});


app.controller('ScheduleCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {


    dataservice.set(appService.GetAllSchedules({}));

    let promise = appService.GetAuthorization({}).then((ret) => {

        // transform member data for multi-selection
        ret.data.GroupAuthorities.forEach(group => {
            group.Members = group.Members.map((name, index) => ({
                id: index,
                label: name,
            }));
        })

        return ret.data;

    })

    dataservice.setAuth(promise);

    $scope.isActive = (destination) => {
        return destination === $location.path();
    }



    $location.path('/Group');

    //let promiseA = appService.GetAllSchedules({}).then((ret) => {
    //    dataservice.set(ret.data);
    //})
    //let promiseB = appService.GetAuthorization({}).then((ret) => {

    //    // transform member data for multi-selection
    //    ret.data.GroupAuthorities.forEach(group => {
    //        group.Members = group.Members.map((name, index) => ({
    //            id: index,
    //            label: name,
    //        }));
    //    })

    //    dataservice.setAuth(ret.data);

    //})

    //$q.all([promiseA, promiseB]).then((ret) => {

    //    $location.path('/Group');

    //});



    //appService.GetAllSchedules({}).then((ret) => {
    //    dataservice.set(ret.data);
    //})

    //appService.GetAuthorization({}).then((ret) => {

    //    // transform member data for multi-selection
    //    ret.data.GroupAuthorities.forEach(group => {
    //        group.Members = group.Members.map((name, index) => ({
    //            id: index,
    //            label: name,
    //        }));
    //    })

    //    dataservice.setAuth(ret.data);
    //    $location.path('/Skill');
    //})



}]);

app.controller('GroupCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$timeout', function ($scope, $location, appService, $rootScope, $q, dataservice, $timeout) {


    //$scope.niceNoah = () => {

    //    appService.UpdateAllPercentComplete({}).then((ret) => {

    //        if (ret.data) {
    //            console.log("succeed");
    //        }

    //    });
    //}

    // checkbox: show complete projects
    $scope.checkboxModel = {
        value: false
    };

    // line chart
    $scope.lineChartName = "";
    const ctx = document.getElementById('myChart');
    const lineChart = new Chart(ctx, config);

    $scope.createHistoryLineChart = (schedule) => {
        $scope.lineChartName = schedule.content
        createHistoryLineChart(lineChart, JSON.parse(schedule.history));
    }


    //$scope.data = dataservice.get();
    //$scope.auth = dataservice.getAuth();

    //// default group
    //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
    //$scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members


    // gantt
    //$scope.ganttStartMonth = moment({ day: 1 });
    $scope.ganttStartMonth = moment({ day: 1 }).subtract(5, 'M');
    $scope.updateMonthRange = () => {
        let rangeStart = $scope.ganttStartMonth.local('zh-tw').format('YYYY-MM');
        let rangeEnd = moment($scope.ganttStartMonth).add(1, 'y').local('zh-tw').format('YYYY-MM');
        $scope.monthRange = `${rangeStart} ~ ${rangeEnd}`;
    }
    $scope.updateMonthRange();

    const plot =
        ganttPlot()
            .width(800)
            .height(110)
            .x1Value((d) => new Date(d.start_date))
            .x2Value((d) => new Date(d.end_date))
            .margin({
                top: 10,
                right: 10,
                bottom: 20,
                left: 50
            })
            .radius(5)
            .type('group');




    const UpdateAllPlots = () => {

        const groupSchedules = $scope.data;

        plot.startMonth($scope.ganttStartMonth.toDate());

        // parse date in gantt.js

        // group schedule
        for (let i = 0; i !== groupSchedules.length; i++) {

            const svg = select('#svg' + i.toString())
                .call(plot.type('group').data([groupSchedules[i].Group]));

            // detail schedule
            const detailSchedules = groupSchedules[i].Details;

            for (let j = 0; j !== detailSchedules.length; j++) {

                const svgChild = select(`#svgChild${i}-${j}`)
                    .call(plot.type('detail').data([detailSchedules[j].Detail]));
            }

        }

        // version 1: checkbox and filter crossing
        // default 

        //let count = 0;
        //let groupProjectNames = groupSchedules.filter(x => !(
        //    new Date(x.Group.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate() ||
        //    new Date(x.Group.end_date) < moment($scope.ganttStartMonth).toDate())
        //    ).map(x => {
        //    count++;
        //    return {
        //        id: count,
        //        label: x.Group.projno,
        //        finished: x.Group.percent_complete === 100,
        //    }
        //})


        //if (!$scope.selection_filtered_projects) {
        //    $scope.selection_filtered_projects = [];
        //    for (let name of groupProjectNames) {
        //        $scope.selection_filtered_projects.push(name);
        //        if (!name.finished)
        //            $scope.multi_selected_projects.push(name);
        //    }

        //}
        //else {
        //    let temp_range_projects = $scope.selection_filtered_projects;
        //    $scope.selection_filtered_projects = [];
        //    let temp_selected_projects = $scope.multi_selected_projects;
        //    $scope.multi_selected_projects = [];

        //    for (let name of groupProjectNames) {
        //        $scope.selection_filtered_projects.push(name);

        //        if (temp_range_projects.find(x => x.label === name.label)) {

        //            if (temp_selected_projects.find(x => x.label === name.label)) {
        //                $scope.multi_selected_projects.push(name);
        //                continue;
        //            }

        //        }
        //        else {

        //            if ($scope.checkboxModel.value) {
        //                $scope.multi_selected_projects.push(name);
        //            }
        //            else {
        //                if (!name.finished)
        //                    $scope.multi_selected_projects.push(name);
        //            }

        //        }
        //    }
        //}





    }

    $scope.UpdateAllPlots = (redundant) => {
        if (!redundant)
            $scope.updateProjectFilter();
        $timeout(function () {
            UpdateAllPlots();
        }, 0);
    }


    $scope.updateProjectFilter = () => {
        // version 2: checkbox prior to filter

        let count = 0;
        let groupProjectNames = $scope.data
            .filter(x => x.Group.role === $scope.selectedGroup)
            .filter(x => !(
                new Date(x.Group.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate() ||
                new Date(x.Group.end_date) < moment($scope.ganttStartMonth).toDate()))
            .map(x => {
                count++;
                return {
                    id: count,
                    label: x.Group.projno
                }
            })

        if (!$scope.selection_filtered_projects) {
            $scope.selection_filtered_projects = [];
            for (let name of groupProjectNames) {
                $scope.selection_filtered_projects.push(name);
                $scope.multi_selected_projects.push(name);
            }
        }
        else {
            let temp_range_projects = $scope.selection_filtered_projects;
            let temp_selected_projects = $scope.multi_selected_projects;
            $scope.selection_filtered_projects = [];
            $scope.multi_selected_projects = [];

            for (let name of groupProjectNames) {
                $scope.selection_filtered_projects.push(name);

                if (temp_range_projects.find(x => x.label === name.label)) {

                    if (temp_selected_projects.find(x => x.label === name.label)) {
                        $scope.multi_selected_projects.push(name);
                        continue;
                    }
                }
                else {
                    $scope.multi_selected_projects.push(name);
                }
            }
        }

    }



    // multi-select
    $scope.multiselectedmodel = [];
    $scope.yourEvents = {
        onSelectionChanged: function () {
            let names = "";
            $scope.multiselectedmodel.sort((a, b) => a.id - b.id);
            for (let i = 0; i !== $scope.multiselectedmodel.length; i++) {
                names += $scope.multiselectedmodel[i].label;
                if (i !== $scope.multiselectedmodel.length - 1)
                    names += ", ";
                $scope.modal.member = names;
            }
        },
        onDeselectAll: function () {
            $scope.modal.member = "";
        }
    };

    // project multi-select
    $scope.multi_selected_projects = [];
    $scope.multi_selected_project_event = {
        onSelectionChanged: function () {
            $scope.UpdateAllPlots('yes');
        },
        onDeselectAll: function () {
            $scope.UpdateAllPlots('yes');
        }
    };


    // trasform members string into checkbox array object
    $scope.passChosen = () => {
        $scope.multiselectedmodel = [];
        for (let emp of $scope.filteredMembers) {
            if ($scope.modal.member?.includes(emp.label))
                $scope.multiselectedmodel.push(emp);
        }
    }


    // deep clone schedule object to modal
    $scope.cloneDeepToModal = (schedule, idx, groupSchedule) => {

        $scope.modal = deepCopyFunction(schedule);
        $scope.modal.origin = schedule;
        $scope.modal.originidx = idx;
        $scope.modal.createMode = false;
        //$scope.modal.history = JSON.stringify(sData);


        if (groupSchedule) {
            // show group schedule content in detail schedule modal 
            $scope.modal.parentContent = groupSchedule.content;
            // update projno from group schedule
            $scope.modal.projno = groupSchedule.projno;
        }


        // MEMBER FILTER

        if (schedule.type === 1) {
            $scope.selectionFilteredMembers = $scope.filteredMembers;
        }
        else {
            $scope.selectionFilteredMembers = [];

            for (let emp of $scope.filteredMembers) {
                if (groupSchedule.member?.includes(emp.label))
                    $scope.selectionFilteredMembers.push(emp);
            }
        }



    };

    $scope.updateSchedule = () => {

        transHistory();

        appService.UpdateSchedule({ schedule: $scope.modal }).then((ret) => {

            if (ret.data) {

                ret.data.milestones = ret.data.milestones ?? [];

                // pass modal value back to origin
                for (const property in $scope.modal.origin) {
                    $scope.modal.origin[property] = ret.data[property];
                }

                if (ret.data.type === 1) {

                    let groupIndex = $scope.modal.originidx;

                    calMemberHours($scope.data[groupIndex]); // update member hours                    
                    //const svg = select('#svg' + groupIndex.toString())
                    //    .call(plot.data([$scope.data[groupIndex].Group])); // update Group gantt
                    const svg = select('#svg' + groupIndex.toString())
                        .call(plot.type('group').data([$scope.modal.origin])); // update Group gantt
                }
                else if (ret.data.type === 2) {

                    let groupIndex = $scope.data.findIndex(x => x.Group.id === ret.data.parent_id);
                    let detailIndex = $scope.modal.originidx;

                    //const svgChild = select(`#svgChild${groupIndex}-${detailIndex}`)
                    //    .call(plot.data([$scope.data[groupIndex].Details[detailIndex].Detail])); // update Detail gantt

                    const svgChild = select(`#svgChild${groupIndex}-${detailIndex}`)
                        .call(plot.type('detail').data([$scope.modal.origin])); // update Detail gantt

                }

            }
        })
    }

    // create empty group schedule object to modal
    $scope.createGroupScheduleModal = () => {

        $scope.modal = {
            type: 1,
            member: '',
            milestones: [],
            start_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            end_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            role: $scope.selectedGroup,
        };

        $scope.multiselectedmodel = []
        $scope.modal.createMode = true;

        // MEMBER FILTER
        $scope.selectionFilteredMembers = $scope.filteredMembers;

    };

    $scope.createDetailScheduleModal = (groupSchedule) => {

        $scope.modal = {
            type: 2,
            member: '',
            milestones: [],
            start_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            end_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            role: $scope.selectedGroup,
            parent_id: groupSchedule.id,
            projno: groupSchedule.projno,
        };

        $scope.multiselectedmodel = []
        $scope.modal.createMode = true;
        $scope.modal.parentContent = groupSchedule.content;

        // set default time of detail schedule equal to group schedule
        $scope.modal.start_date = groupSchedule.start_date;
        $scope.modal.end_date = groupSchedule.end_date;

        // MEMBER FILTER
        $scope.selectionFilteredMembers = [];

        for (let emp of $scope.filteredMembers) {
            if (groupSchedule.member?.includes(emp.label))
                $scope.selectionFilteredMembers.push(emp);
        }

    };



    $scope.createSchedule = () => {

        transHistory();

        appService.InsertSchedule($scope.modal).then((ret) => {

            if (ret.data) {

                // create empty milestone list
                ret.data.milestones = ret.data.milestones ?? [];

                // group or detail
                if (ret.data.type === 1) {

                    let groupIndex = $scope.data.length;

                    $scope.data.splice(groupIndex, 0, {
                        Group: ret.data,
                        Details: [],
                        ManHours: [],
                    });

                    calMemberHours($scope.data[groupIndex]);


                    //
                    //$timeout(function () {
                    //    const svg = select('#svg' + groupIndex.toString())
                    //        .call(plot.type('group').data([$scope.data[groupIndex].Group])); // update Group gantt
                    //}, 0);

                    // why not just update all?
                    $scope.UpdateAllPlots();

                }
                else if (ret.data.type === 2) {
                    let parent = $scope.data.find(x => x.Group.id === ret.data.parent_id);

                    let groupIndex = $scope.data.findIndex(x => x.Group.id === ret.data.parent_id);
                    let detailIndex = parent.Details.length;

                    parent.Details.splice(detailIndex, 0, {
                        Detail: ret.data,
                        Personals: [],
                    });

                    $timeout(function () {
                        const svgChild = select(`#svgChild${groupIndex}-${detailIndex}`)
                            .call(plot.type('detail').data([parent.Details[detailIndex].Detail])); // update Detail gantt
                    }, 0);

                }

            }
        })
    };

    $scope.addMilestone = () => {
        $scope.modal.milestones.splice($scope.modal.milestones.length, 0, { schedule_id: $scope.modal.id });
    };

    $scope.removeMilestone = (milestone) => {
        const idx = $scope.modal.milestones.indexOf(milestone);
        $scope.modal.milestones.splice(idx, 1);
    };

    // delete all corresponding sub schedules and milestones
    $scope.deleteSchedule = () => {

        $('#exampleModalLong').modal('hide')

        appService.DeleteSchedule($scope.modal).then((ret) => {
            if (ret.data) {

                // group or detail
                if ($scope.modal.type === 1) {

                    let idx = $scope.data.findIndex(x => x.Group.id === $scope.modal.id);
                    $scope.data.splice(idx, 1);
                }
                else {

                    let parent = $scope.data.find(x => x.Group.id === $scope.modal.parent_id);
                    let idx = parent.Details.findIndex(x => x.Detail.id === $scope.modal.id);
                    parent.Details.splice(idx, 1);
                }

                // redraw all gantt
                //$timeout(function () {
                //    $scope.UpdateAllPlots();
                //}, 0);

                $scope.UpdateAllPlots();
            }

        })

    };


    $scope.filterMembers = () => {
        let group = $scope.auth.GroupAuthorities.find(x => x.GroupName === $scope.selectedGroup)
        $scope.filteredMembers = group.Members
        $scope.selectionFilterMembers = $scope.filteredMembers;

        $scope.editFilter = group.Editable;

        // gantt charts that are hidden because of group filter
        // will cause overlapping gantt text when showing it
        // redraw charts
        $scope.UpdateAllPlots();
    }

    $scope.scheduleFilter = function (schedule) {

        if (schedule.role !== $scope.selectedGroup)
            return false;

        // if the start/end date is not set, show it
        if (!schedule.start_date || !schedule.end_date)
            return true;

        // (Detail only) if checkbox is true, hide completed detail schedule

        if (schedule.type === 2) {
            if (!$scope.checkboxModel.value && schedule.percent_complete === 100)
                return false;
        }

        // (Group only) end before startMonth or start after endMonth not showing
        // 0704 update: hide 100% group schedule
        if (schedule.type === 1) {
            if (new Date(schedule.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate())
                return false;

            if (new Date(schedule.end_date) < moment($scope.ganttStartMonth).toDate())
                return false;

            if (!$scope.checkboxModel.value && schedule.percent_complete === 100)
                return false;
        }

        // (Group Only) Additional filter for comparing specific projexts       
        if (schedule.type === 1) {
            if (!$scope.multi_selected_projects.find(x => x.label === schedule.projno))
                return false;
        }

        return true;
    }


    //transform all member hours
    //$scope.data.forEach(calMemberHours);

    function calMemberHours(groupSchedule) {

        let yymm = moment().add(-1, 'months').locale('zh-tw').format('YYYYMM');
        const yy = +yymm.slice(0, 4) - 1911;
        const mm = yymm.slice(4);
        yymm = `${yy}${mm}`;

        const projectHours = groupSchedule.ManHours;
        const project = groupSchedule.Group;

        let memberHours = '';
        let monthlyHours = 0;
        let totalHours = 0;

        if (project.member) {

            // member hours
            const names = project.member.split(', ');
            for (let name of names) {
                const manHour = projectHours.find(x => x.name === name && x.yymm === yymm);
                let hour = (manHour) ? manHour.hours : 0;
                memberHours += `${name}(${hour})\n`
                    ;
            }

            if (memberHours)
                memberHours = memberHours.slice(0, memberHours.length - 1);

            // monthly hours
            monthlyHours = projectHours
                .filter(x => x.yymm === yymm)
                .reduce((a, c) => a + c.hours, 0) ?? 0;

            // total hours
            totalHours = projectHours
                .reduce((a, c) => a + c.hours, 0) ?? 0;

        }

        project.memberHours = memberHours;
        project.monthlyHours = monthlyHours;
        project.totalHours = totalHours;

    }

    function transHistory() {

        let historyObj;
        $scope.modal.percent_complete = Number($scope.modal.percent_complete);
        $scope.modal.last_percent_complete = Number($scope.modal.last_percent_complete);

        if (!$scope.modal.history) {
            historyObj = { time: [], percent: [] };
        }
        else {
            historyObj = JSON.parse($scope.modal.history);
        }

        if ($scope.modal.last_percent_complete) {

            let yymm = moment({ day: 1 }).subtract(1, 'M').format('YYYY-MM');
            let hIdx = historyObj?.time.indexOf(yymm);
            if (hIdx >= 0)
                historyObj.percent[hIdx] = $scope.modal.last_percent_complete;
            else {
                historyObj.time.push(yymm);
                historyObj.percent.push($scope.modal.last_percent_complete);
            }
        }

        if ($scope.modal.percent_complete) {

            let yymm = moment({ day: 1 }).format('YYYY-MM');
            let hIdx = historyObj?.time.indexOf(yymm);
            if (hIdx >= 0)
                historyObj.percent[hIdx] = $scope.modal.percent_complete;
            else {
                historyObj.time.push(yymm);
                historyObj.percent.push($scope.modal.percent_complete);
            }
        }

        // sort again 

        //1) combine the arrays:
        let templist = [];
        for (let j = 0; j < historyObj.time.length; j++)
            templist.push({ 'time': historyObj.time[j], 'percent': historyObj.percent[j] });

        //2) sort:
        templist.sort(function (a, b) {
            return ((a.time < b.time) ? -1 : ((a.time === b.time) ? 0 : 1));
        });

        //3) separate them back out:
        for (let k = 0; k < templist.length; k++) {
            historyObj.time[k] = templist[k].time;
            historyObj.percent[k] = templist[k].percent;
        }


        $scope.modal.history = JSON.stringify(historyObj);

    }



    function deepCopyFunction(inputObject) {
        // Return the value if inputObject is not an Object data
        // Need to notice typeof null is 'object'
        if (typeof inputObject !== 'object' || inputObject === null) {
            return inputObject;
        }

        // Create an array or object to hold the values
        const outputObject = Array.isArray(inputObject) ? [] : {};

        // Recursively deep copy for nested objects, including arrays
        for (let key in inputObject) {
            const value = inputObject[key];
            outputObject[key] = deepCopyFunction(value);
        }

        return outputObject;
    }


    let promiseA = dataservice.get();
    let promiseB = dataservice.getAuth();

    $q.all([promiseA, promiseB]).then((ret) => {

        $scope.data = ret[0].data;
        $scope.auth = ret[1];

        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members

        $scope.data.forEach(calMemberHours);
        $scope.filterMembers();
        $scope.UpdateAllPlots();
    });



    // default group
    //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
    //$scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members


    // filter 
    //$scope.filterMembers();

    // draw all gantt at first

    //$timeout(function () {
    //    $scope.UpdateAllPlots();
    //}, 0);


    //$scope.UpdateAllPlots();

}]);

app.controller('PersonalCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$timeout', function ($scope, $location, appService, $rootScope, $q, dataservice, $timeout) {


    // checkbox
    $scope.checkboxModel = {
        value: false
    };

    // line chart
    $scope.lineChartName = "";
    const ctx = document.getElementById('myChart');
    const lineChart = new Chart(ctx, config);

    $scope.createHistoryLineChart = (schedule) => {
        $scope.lineChartName = schedule.content
        createHistoryLineChart(lineChart, JSON.parse(schedule.history));
    }


    //$scope.data = dataservice.get();
    //$scope.auth = dataservice.getAuth();

    // default group
    //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
    //$scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members

    // project multi-select
    $scope.multi_selected_projects = [];
    $scope.multi_selected_project_event = {
        onSelectionChanged: function () {
            $scope.UpdateAllPlots('yes');
        },
        onDeselectAll: function () {
            $scope.UpdateAllPlots('yes');
        }
    };


    // gantt

    //$scope.ganttStartMonth = moment({ day: 1 });
    $scope.ganttStartMonth = moment({ day: 1 }).subtract(5, 'M');
    $scope.updateMonthRange = () => {
        let rangeStart = $scope.ganttStartMonth.local('zh-tw').format('YYYY-MM');
        let rangeEnd = moment($scope.ganttStartMonth).add(1, 'y').local('zh-tw').format('YYYY-MM');
        $scope.monthRange = `${rangeStart} ~ ${rangeEnd}`;
    }
    $scope.updateMonthRange();


    const plot =
        ganttPlot()
            .width(800)
            .height(110)
            .x1Value((d) => new Date(d.start_date))
            .x2Value((d) => new Date(d.end_date))
            .margin({
                top: 10,
                right: 10,
                bottom: 20,
                left: 50
            })
            .radius(5)
            .type('group');



    const UpdateAllPlots = () => {

        const groupSchedules = $scope.data;

        plot.startMonth($scope.ganttStartMonth.toDate());

        // parse date in gantt.js

        // group schedule
        for (let i = 0; i !== groupSchedules.length; i++) {

            const svg = select('#svg' + i.toString())
                .call(plot.type('group').data([groupSchedules[i].Group]));

            // detail schedule
            const detailSchedules = groupSchedules[i].Details;

            for (let j = 0; j !== detailSchedules.length; j++) {

                const svgChild = select(`#svgChild${i}-${j}`)
                    .call(plot.type('detail').data([detailSchedules[j].Detail]));

                // personal schedule
                const personalSchedules = detailSchedules[j].Personals;

                for (let k = 0; k !== personalSchedules.length; k++) {

                    const svgGrandChild = select(`#svgGrandChild${i}-${j}-${k}`)
                        .call(plot.type('personal').data([personalSchedules[k]]));

                }
            }

        }

    }

    $scope.UpdateAllPlots = (redundent) => {
        if (!redundent)
            $scope.updateProjectFilter();
        $timeout(function () {
            UpdateAllPlots();
        }, 0);
    }


    $scope.updateProjectFilter = () => {
        // version 2: checkbox prior to filter

        let count = 0;
        let groupProjectNames = $scope.data
            .filter(x => x.Group.role === $scope.selectedGroup)
            .filter(x => x.Group.member?.includes($scope.selectedMember))
            .filter(x => !(
                new Date(x.Group.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate() ||
                new Date(x.Group.end_date) < moment($scope.ganttStartMonth).toDate()))
            .map(x => {
                count++;
                return {
                    id: count,
                    label: x.Group.projno
                }
            })

        if (!$scope.selection_filtered_projects) {
            $scope.selection_filtered_projects = [];
            for (let name of groupProjectNames) {
                $scope.selection_filtered_projects.push(name);
                $scope.multi_selected_projects.push(name);
            }
        }
        else {
            let temp_range_projects = $scope.selection_filtered_projects;
            let temp_selected_projects = $scope.multi_selected_projects;
            $scope.selection_filtered_projects = [];
            $scope.multi_selected_projects = [];

            for (let name of groupProjectNames) {
                $scope.selection_filtered_projects.push(name);

                if (temp_range_projects.find(x => x.label === name.label)) {

                    if (temp_selected_projects.find(x => x.label === name.label)) {
                        $scope.multi_selected_projects.push(name);
                        continue;
                    }
                }
                else {
                    $scope.multi_selected_projects.push(name);
                }
            }
        }

    }


    $scope.updateSchedule = () => {

        transHistory();

        appService.UpdateSchedule({ schedule: $scope.modal }).then((ret) => {

            if (ret.data) {

                ret.data.milestones = ret.data.milestones ?? [];

                // pass modal value back to origin
                for (const property in $scope.modal.origin) {
                    $scope.modal.origin[property] = ret.data[property];
                }

                let personalIndex = $scope.modal.originidx;

                $timeout(function () {
                    const svg = select(`#svgGrandChild${$scope.modal.groupIndex}-${$scope.modal.detailIndex}-${personalIndex}`)
                        .call(plot.type('personal').data([$scope.modal.origin])); // update Group gantt
                }, 0);

                //if (ret.data.type === 1) {

                //    let groupIndex = $scope.modal.originidx;

                //    calMemberHours($scope.data[groupIndex]); // update member hours                    
                //    const svg = select('#svg' + groupIndex.toString())
                //        .call(plot.data([$scope.data[groupIndex].Group])); // update Group gantt

                //    const svgChild = select(`#svgChild${groupIndex}-${detailIndex}`)
                //        .call(plot.data([$scope.data[groupIndex].Details[detailIndex].Detail])); // update Detail gantt
                //}
                //else if (ret.data.type === 2) {

                //    let groupIndex = $scope.data.findIndex(x => x.Group.id === ret.data.parent_id);
                //    let detailIndex = $scope.modal.originidx;



                //}

            }
        })
    }



    // deep clone schedule object to modal
    $scope.cloneDeepToModal = (schedule, idx, dIdx, gIdx) => {

        $scope.modal = deepCopyFunction(schedule);
        $scope.modal.origin = schedule;
        $scope.modal.originidx = idx;
        $scope.modal.createMode = false;
        $scope.modal.detailIndex = dIdx;
        $scope.modal.groupIndex = gIdx;

        // update projno from group schedule
        let groupSchedule = $scope.data[$scope.modal.groupIndex].Group
        $scope.modal.projno = groupSchedule.projno;
    };

    $scope.createPersonalScheduleModal = (detailSchedule, dIdx, gIdx) => {

        $scope.modal = {
            type: 3,
            member: $scope.auth.User.name,
            milestones: [],
            start_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            end_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            role: detailSchedule.role,
            parent_id: detailSchedule.id,
            //projno: detailSchedule.projno,
            empno: $scope.auth.User.empno,
        };

        $scope.modal.detailIndex = dIdx;
        $scope.modal.groupIndex = gIdx;
        $scope.modal.createMode = true;

        // set default time of personal schedule equal to detail schedule
        $scope.modal.start_date = detailSchedule.start_date;
        $scope.modal.end_date = detailSchedule.end_date;

        // update projno from group schedule
        let groupSchedule = $scope.data[$scope.modal.groupIndex].Group;
        $scope.modal.projno = groupSchedule.projno;
    };

    $scope.createSchedule = () => {

        transHistory();

        appService.InsertSchedule($scope.modal).then((ret) => {

            if (ret.data) {

                // create empty milestone list
                ret.data.milestones = ret.data.milestones ?? [];

                // group or detail


                let personals = $scope.data[$scope.modal.groupIndex].Details[$scope.modal.detailIndex].Personals;
                let personalIndex = personals.length;

                personals.splice(personalIndex, 0, ret.data);

                $timeout(function () {
                    const svg = select(`#svgGrandChild${$scope.modal.groupIndex}-${$scope.modal.detailIndex}-${personalIndex}`)
                        .call(plot.type('personal').data([personals[personalIndex]])); // update Group gantt
                }, 0);

            }
        })
    };

    // delete all corresponding sub schedules and milestones
    $scope.deleteSchedule = () => {

        $('#exampleModalLong').modal('hide')

        appService.DeleteSchedule($scope.modal).then((ret) => {
            if (ret.data) {

                let personals = $scope.data[$scope.modal.groupIndex].Details[$scope.modal.detailIndex].Personals;
                personals.splice($scope.modal.originidx, 1);

                // redraw all gantt
                //$timeout(function () {
                //    $scope.UpdateAllPlots();
                //}, 0);
                $scope.UpdateAllPlots();
            }

        })

    };

    $scope.addMilestone = () => {
        $scope.modal.milestones.splice($scope.modal.milestones.length, 0, { schedule_id: $scope.modal.id });
    };

    $scope.removeMilestone = (milestone) => {
        const idx = $scope.modal.milestones.indexOf(milestone);
        $scope.modal.milestones.splice(idx, 1);
    };

    $scope.filterMembers = () => {
        let group = $scope.auth.GroupAuthorities.find(x => x.GroupName === $scope.selectedGroup)
        $scope.filteredMembers = group.Members

        let user = $scope.filteredMembers.find(x => x.label === $scope.auth.User.name);

        if (user) {
            $scope.selectedMember = user.label;
        }
        else {
            $scope.selectedMember = $scope.filteredMembers[0].label;
        }

        $scope.changeMember();
    }

    $scope.changeMember = () => {
        $scope.editFilter = $scope.selectedMember === $scope.auth.User.name;

        $scope.UpdateAllPlots();
    }

    $scope.scheduleFilter = function (schedule) {

        if (schedule.role !== $scope.selectedGroup)
            return false;

        if (!schedule.member?.includes($scope.selectedMember))
            return false;

        // if the start/end date is not set, show it
        if (!schedule.start_date || !schedule.end_date)
            return true;


        // (Group only) end before startMonth or start after endMonth not showing
        if (schedule.type === 1) {
            if (new Date(schedule.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate())
                return false;

            if (new Date(schedule.end_date) < moment($scope.ganttStartMonth).toDate())
                return false;

            if (!$scope.checkboxModel.value && schedule.percent_complete === 100)
                return false;
        }

        // (Group Only) Additional filter for comparing specific projexts       
        if (schedule.type === 1) {
            if (!$scope.multi_selected_projects.find(x => x.label === schedule.projno))
                return false;
        }


        // end before startMonth or start after endMonth not showing
        //if (new Date(schedule.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate())
        //    return false;

        //if (new Date(schedule.end_date) < moment($scope.ganttStartMonth).toDate())
        //    return false;

        return true;
    }

    $scope.personalFilter = function (schedule) {

        if (schedule.member !== $scope.selectedMember)
            return false;

        // (Personal only) if checkbox is true, hide completed detail schedule


        if (!$scope.checkboxModel.value && schedule.percent_complete === 100)
            return false;


        return true;
    }

    function transHistory() {

        let historyObj;
        $scope.modal.percent_complete = Number($scope.modal.percent_complete);
        $scope.modal.last_percent_complete = Number($scope.modal.last_percent_complete);

        if (!$scope.modal.history) {
            historyObj = { time: [], percent: [] };
        }
        else {
            historyObj = JSON.parse($scope.modal.history);
        }

        if ($scope.modal.last_percent_complete) {

            let yymm = moment({ day: 1 }).subtract(1, 'M').format('YYYY-MM');
            let hIdx = historyObj?.time.indexOf(yymm);
            if (hIdx >= 0)
                historyObj.percent[hIdx] = $scope.modal.last_percent_complete;
            else {
                historyObj.time.push(yymm);
                historyObj.percent.push($scope.modal.last_percent_complete);
            }
        }

        if ($scope.modal.percent_complete) {

            let yymm = moment({ day: 1 }).format('YYYY-MM');
            let hIdx = historyObj?.time.indexOf(yymm);
            if (hIdx >= 0)
                historyObj.percent[hIdx] = $scope.modal.percent_complete;
            else {
                historyObj.time.push(yymm);
                historyObj.percent.push($scope.modal.percent_complete);
            }
        }

        // sort again 

        //1) combine the arrays:
        let templist = [];
        for (let j = 0; j < historyObj.time.length; j++)
            templist.push({ 'time': historyObj.time[j], 'percent': historyObj.percent[j] });

        //2) sort:
        templist.sort(function (a, b) {
            return ((a.time < b.time) ? -1 : ((a.time === b.time) ? 0 : 1));
        });

        //3) separate them back out:
        for (let k = 0; k < templist.length; k++) {
            historyObj.time[k] = templist[k].time;
            historyObj.percent[k] = templist[k].percent;
        }

        $scope.modal.history = JSON.stringify(historyObj);

    }

    function deepCopyFunction(inputObject) {
        // Return the value if inputObject is not an Object data
        // Need to notice typeof null is 'object'
        if (typeof inputObject !== 'object' || inputObject === null) {
            return inputObject;
        }

        // Create an array or object to hold the values
        const outputObject = Array.isArray(inputObject) ? [] : {};

        // Recursively deep copy for nested objects, including arrays
        for (let key in inputObject) {
            const value = inputObject[key];
            outputObject[key] = deepCopyFunction(value);
        }

        return outputObject;
    }

    let promiseA = dataservice.get();
    let promiseB = dataservice.getAuth();

    $q.all([promiseA, promiseB]).then((ret) => {

        $scope.data = ret[0].data;
        $scope.auth = ret[1];

        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;

        $scope.filterMembers();
        $scope.UpdateAllPlots();
    });


    //$scope.filterMembers();

    // draw all gantt at first
    //$timeout(function () {
    //    $scope.UpdateAllPlots();
    //}, 0);
    //$scope.UpdateAllPlots();


}]);


app.controller('FutureCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$timeout', function ($scope, $location, appService, $rootScope, $q, dataservice, $timeout) {

    //appService.GetAllFutures({}).then((ret) => {
    //    $scope.data = ret.data;
    //    $scope.UpdateAllPlots();
    //})

    //$scope.auth = dataservice.getAuth();

    //// default group
    //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
    //$scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members

    // gantt

    $scope.ganttStartMonth = moment({ month: 0, day: 1 });
    $scope.updateMonthRange = () => {
        let rangeStart = $scope.ganttStartMonth.local('zh-tw').format('YYYY-MM');
        let rangeEnd = moment($scope.ganttStartMonth).add(3, 'y').local('zh-tw').format('YYYY-MM');
        $scope.monthRange = `${rangeStart} ~ ${rangeEnd}`;
    }
    $scope.updateMonthRange();

    const plot =
        ganttPlot()
            .width(1200)
            .height(80)
            .x1Value((d) => new Date(d.start_date))
            .x2Value((d) => new Date(d.end_date))
            .margin({
                top: 10,
                right: 10,
                bottom: 20,
                left: 50
            })
            .radius(5)
            .type('future');


    const UpdateAllPlots = () => {

        const groupSchedules = $scope.data;

        plot.startMonth($scope.ganttStartMonth.toDate());

        // parse date in gantt.js

        // group schedule
        for (let i = 0; i !== groupSchedules.length; i++) {

            const svg = select('#svg' + i.toString())
                .call(plot.data([groupSchedules[i].Group]));

            // detail schedule
            const detailSchedules = groupSchedules[i].Details;

            for (let j = 0; j !== detailSchedules.length; j++) {

                const svgChild = select(`#svgChild${i}-${j}`)
                    .call(plot.data([detailSchedules[j].Detail]));
            }

        }

    }

    $scope.UpdateAllPlots = () => {
        $timeout(function () {
            UpdateAllPlots();
        }, 0);
    }


    // multi-select
    $scope.multiselectedmodel = [];
    $scope.yourEvents = {
        onSelectionChanged: function () {
            let names = "";
            $scope.multiselectedmodel.sort((a, b) => a.id - b.id);
            for (let i = 0; i !== $scope.multiselectedmodel.length; i++) {
                names += $scope.multiselectedmodel[i].label;
                if (i !== $scope.multiselectedmodel.length - 1)
                    names += ", ";
                $scope.modal.member = names;
            }
        },
        onDeselectAll: function () {
            $scope.modal.member = "";
        }
    };

    // trasform members string into checkbox array object
    $scope.passChosen = () => {
        $scope.multiselectedmodel = [];
        for (let emp of $scope.filteredMembers) {
            if ($scope.modal.member?.includes(emp.label))
                $scope.multiselectedmodel.push(emp);
        }
    }


    // deep clone schedule object to modal
    $scope.cloneDeepToModal = (schedule, idx, parentContent) => {

        $scope.modal = deepCopyFunction(schedule);
        $scope.modal.origin = schedule;
        $scope.modal.originidx = idx;
        $scope.modal.createMode = false;
        $scope.modal.parentContent = parentContent;

    };

    $scope.filterMembers = () => {
        let group = $scope.auth.GroupAuthorities.find(x => x.GroupName === $scope.selectedGroup)
        $scope.filteredMembers = group.Members

        $scope.editFilter = group.Editable;

        // overlapping gantt text must be redraw
        //$scope.UpdateAllPlots();
    }

    $scope.updateSchedule = () => {

        appService.UpdateSchedule({ schedule: $scope.modal }).then((ret) => {

            if (ret.data) {

                ret.data.milestones = ret.data.milestones ?? [];

                // pass modal value back to origin
                for (const property in $scope.modal.origin) {
                    $scope.modal.origin[property] = ret.data[property];
                }

                // lazy 
                $scope.UpdateAllPlots();

            }
        })
    }

    // create empty group schedule object to modal
    $scope.createGroupScheduleModal = () => {

        $scope.modal = {
            type: 4,
            member: '',
            milestones: [],
            start_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            end_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            role: $scope.selectedGroup,
        };

        $scope.multiselectedmodel = []
        $scope.modal.createMode = true;
    };

    $scope.createDetailScheduleModal = (groupSchedule) => {

        $scope.modal = {
            type: 5,
            member: '',
            milestones: [],
            start_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            end_date: moment().locale('zh-tw').format('YYYY-MM-DD'),
            role: $scope.selectedGroup,
            parent_id: groupSchedule.id,
        };

        $scope.multiselectedmodel = []
        $scope.modal.createMode = true;
        $scope.modal.parentContent = groupSchedule.content;
    };



    $scope.createSchedule = () => {


        appService.InsertSchedule($scope.modal).then((ret) => {

            if (ret.data) {

                // create empty milestone list
                ret.data.milestones = ret.data.milestones ?? [];

                // group or detail
                if (ret.data.type === 4) {

                    let groupIndex = $scope.data.length;

                    $scope.data.splice(groupIndex, 0, {
                        Group: ret.data,
                        Details: [],
                    });

                    // lazy 
                    $scope.UpdateAllPlots();

                    //$timeout(function () {
                    //    const svg = select('#svg' + groupIndex.toString())
                    //        .call(plot.type('group').data([$scope.data[groupIndex].Group])); // update Group gantt
                    //}, 0);



                }
                else if (ret.data.type === 5) {
                    let parent = $scope.data.find(x => x.Group.id === ret.data.parent_id);

                    let groupIndex = $scope.data.findIndex(x => x.Group.id === ret.data.parent_id);
                    let detailIndex = parent.Details.length;

                    parent.Details.splice(detailIndex, 0, {
                        Detail: ret.data,
                    });

                    // lazy 
                    $scope.UpdateAllPlots();

                    //$timeout(function () {
                    //    const svgChild = select(`#svgChild${groupIndex}-${detailIndex}`)
                    //        .call(plot.type('detail').data([parent.Details[detailIndex].Detail])); // update Detail gantt
                    //}, 0);

                }

            }
        })
    };

    // delete all corresponding sub schedules and milestones
    $scope.deleteSchedule = () => {

        $('#exampleModalLong').modal('hide')

        appService.DeleteSchedule($scope.modal).then((ret) => {
            if (ret.data) {

                // group or detail
                if ($scope.modal.type === 4) {

                    let idx = $scope.data.findIndex(x => x.Group.id === $scope.modal.id);
                    $scope.data.splice(idx, 1);
                }
                else {

                    let parent = $scope.data.find(x => x.Group.id === $scope.modal.parent_id);
                    let idx = parent.Details.findIndex(x => x.Detail.id === $scope.modal.id);
                    parent.Details.splice(idx, 1);
                }

                // lazy 
                $scope.UpdateAllPlots();

                // redraw all gantt
                //$timeout(function () {
                //    $scope.UpdateAllPlots();
                //}, 0);
                //$scope.UpdateAllPlots();
            }

        })

    };

    $scope.scheduleFilter = function (schedule) {

        if (schedule.role !== $scope.selectedGroup)
            return false;

        // if the start/end date is not set, show it
        if (!schedule.start_date || !schedule.end_date)
            return true;

        //// (Detail only) if checkbox is true, hide completed detail schedule

        //if (schedule.type === 2) {
        //    if ($scope.checkboxModel.value && schedule.percent_complete === 100)
        //        return false;
        //}

        // (Group only) end before startMonth or start after endMonth not showing
        if (schedule.type === 4) {
            if (new Date(schedule.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate())
                return false;

            if (new Date(schedule.end_date) < moment($scope.ganttStartMonth).toDate())
                return false;
        }

        return true;
    }



    function deepCopyFunction(inputObject) {
        // Return the value if inputObject is not an Object data
        // Need to notice typeof null is 'object'
        if (typeof inputObject !== 'object' || inputObject === null) {
            return inputObject;
        }

        // Create an array or object to hold the values
        const outputObject = Array.isArray(inputObject) ? [] : {};

        // Recursively deep copy for nested objects, including arrays
        for (let key in inputObject) {
            const value = inputObject[key];
            outputObject[key] = deepCopyFunction(value);
        }

        return outputObject;
    }


    let promiseA = appService.GetAllFutures({});
    let promiseB = dataservice.getAuth();

    $q.all([promiseA, promiseB]).then((ret) => {

        $scope.data = ret[0].data;
        $scope.auth = ret[1];

        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members

        $scope.filterMembers();
        $scope.UpdateAllPlots();
    });



    //// filter 
    //$scope.filterMembers();


}]);

app.controller('ProjectCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$timeout', function ($scope, $location, appService, $rootScope, $q, dataservice, $timeout) {

    dataservice.getAuth().then((ret) => {
        $scope.auth = ret;
        $scope.editable = $scope.auth.User.department_manager || $scope.auth.User.group_manager;
    });

    const reg = /^\d{4}[a-zA-Z]$/;
    $scope.modal = {};

    $scope.alertContent = '';

    const imageInput = document.querySelector("#imageInput");
    const tempImage = document.querySelector('#tempImage');

    imageInput.addEventListener("change", (e) => {
        const file = e.target.files[0]; // this Object holds a reference to the file on disk
        const url = URL.createObjectURL(file); // this points to the File object we just created
        tempImage.src = url;
    })    

    $scope.getAll = () => {
        appService.GetAllProjectSchedules({}).then((ret) => {
            $scope.data = ret.data;
        })
    }

    let checkProject = (projno) => {

        if (reg.test(projno) === false) {
            $scope.alertContent = '不符合計畫編號格式';
            return false;
        }

        if ($scope.data.find(x => x.projno === projno)) {
            $scope.alertContent = '計畫編號已存在';
            return false;
        }

        return true;
    }

    $scope.getAll();

    $scope.createProjectSchedule = () => {
        $scope.modal = {};
        $scope.alertContent = '';
    }

    $scope.insertProjectSchedule = () => {

        $scope.modal.projno = $scope.modal.projno.toUpperCase();

        if (checkProject($scope.modal.projno) === false) return;

        appService.InsertProjectSchedule($scope.modal).then((ret) => {
            if (ret.data) {       
                $('#projectModal').modal('toggle');
                $scope.getAll();
            }
        })
    }

    $scope.deleteProjectSchedule = (projectSchedule) => {        
        appService.DeleteProjectSchedule(projectSchedule).then((ret) => {
            if (ret.data) {
                let idx = $scope.data.findIndex(x => x.projno === projectSchedule.projno);
                $scope.data.splice(idx, 1);
            }
        })
    }

    $scope.clearUpload = () => {
        tempImage.src = '';
        imageInput.value = null;
        document.querySelector('.custom-file-label').textContent = '請上傳圖片';
    }

    // Add file name on bs custom file input
    $('#imageInput').on('change', function () {
        var fileName = $(this).val();
        $(this).next('.custom-file-label').html(fileName);
    })

    // Upsert project schedule (projno & image)client to server

    $scope.uploadProjectSchedule = () => {

        uploading().then(() => {
            $scope.getAll();
        })
    }

    const uploading = async () => {

        let form = new FormData(formElem);
        form.append('projectSchedule', JSON.stringify($scope.modal));

        let response = await fetch('GSchedule/UploadProjectSchedule', {
            method: 'POST',
            body: form,
        });

        let result = await response.json();
    }

    $scope.clickPicture = (item) => {
        if (!item.filepath) return;
        $scope.modal = item;
        $('#pictureModal').modal('toggle');
    }

    $scope.clickDelete = (e, item) => {
        e.stopPropagation();
        $scope.modal = item;
        $('#deleteModal').modal('toggle');
        
    }

    $scope.clickUpload = (e, item) => {
        e.stopPropagation();
        $scope.clearUpload();
        $scope.modal = item;
        $('#uploadModal').modal('toggle');        
    }

}]);