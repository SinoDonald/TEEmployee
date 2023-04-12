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

    appService.GetAllSchedules({}).then((ret) => {
        dataservice.set(ret.data);
    })

    appService.GetAuthorization({}).then((ret) => {

        // transform member data for multi-selection
        ret.data.GroupAuthorities.forEach(group => {
            group.Members = group.Members.map((name, index) => ({
                id: index,
                label: name,
            }));
        })

        dataservice.setAuth(ret.data);
    })

}]);

app.controller('GroupCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$timeout', function ($scope, $location, appService, $rootScope, $q, dataservice, $timeout) {


    $scope.data = dataservice.get();
    $scope.auth = dataservice.getAuth();

    // default group
    $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
    $scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members


    // gantt

    $scope.ganttStartMonth = moment({ day: 1 });

    const plot =
        ganttPlot()
            .width(800)
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
            .type('group');

    $scope.UpdateAllPlots = () => {

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
    $scope.cloneDeepToModal = (schedule, idx) => {

        $scope.modal = deepCopyFunction(schedule);
        $scope.modal.origin = schedule;
        $scope.modal.originidx = idx;
        $scope.modal.createMode = false;

    };

    $scope.updateSchedule = () => {

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
    };



    $scope.createSchedule = () => {

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

                    $timeout(function () {
                        const svg = select('#svg' + groupIndex.toString())
                            .call(plot.type('group').data([$scope.data[groupIndex].Group])); // update Group gantt
                    }, 0);



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

    $scope.removeMilestone = (idx) => {
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
                $timeout(function () {
                    $scope.UpdateAllPlots();
                }, 0);
            }

        })

    };


    $scope.filterMembers = () => {
        let group = $scope.auth.GroupAuthorities.find(x => x.GroupName === $scope.selectedGroup)
        $scope.filteredMembers = group.Members

        $scope.editFilter = group.Editable;
    }

    $scope.scheduleFilter = function (schedule) {

        if (schedule.role !== $scope.selectedGroup)
            return false;

        // if the start/end date is not set, show it
        if (!schedule.start_date || !schedule.end_date)
            return true;


        // end before startMonth or start after endMonth not showing
        if (new Date(schedule.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate())
            return false;

        if (new Date(schedule.end_date) < moment($scope.ganttStartMonth).toDate())
            return false;

        return true;
    }


    // transform all member hours
    $scope.data.forEach(calMemberHours);

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

    // filter 
    $scope.filterMembers();

    // draw all gantt at first
    $timeout(function () {
        $scope.UpdateAllPlots();
    }, 0);

}]);

app.controller('PersonalCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$timeout', function ($scope, $location, appService, $rootScope, $q, dataservice, $timeout) {


    $scope.data = dataservice.get();
    $scope.auth = dataservice.getAuth();

    // default group
    $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
    //$scope.filteredMembers = $scope.auth.GroupAuthorities[0].Members

    // gantt

    $scope.ganttStartMonth = moment({ day: 1 });

    const plot =
        ganttPlot()
            .width(800)
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
            .type('group');

    $scope.UpdateAllPlots = () => {

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

    $scope.updateSchedule = () => {

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
            projno: detailSchedule.projno,
            empno: $scope.auth.User.empno,
        };

        $scope.modal.detailIndex = dIdx;
        $scope.modal.groupIndex = gIdx;
        $scope.modal.createMode = true;
    };

    $scope.createSchedule = () => {

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
                $timeout(function () {
                    $scope.UpdateAllPlots();
                }, 0);
            }

        })

    };

    $scope.addMilestone = () => {
        $scope.modal.milestones.splice($scope.modal.milestones.length, 0, { schedule_id: $scope.modal.id });
    };

    $scope.removeMilestone = (idx) => {
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
    }

    $scope.scheduleFilter = function (schedule) {

        if (schedule.role !== $scope.selectedGroup)
            return false;

        if (!schedule.member.includes($scope.selectedMember))
            return false;

        // if the start/end date is not set, show it
        if (!schedule.start_date || !schedule.end_date)
            return true;


        // end before startMonth or start after endMonth not showing
        if (new Date(schedule.start_date) > moment($scope.ganttStartMonth).add(1, 'y').toDate())
            return false;

        if (new Date(schedule.end_date) < moment($scope.ganttStartMonth).toDate())
            return false;

        return true;
    }

    $scope.personalFilter = function (schedule) {

        if (schedule.member !== $scope.selectedMember)
            return false;

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

    $scope.filterMembers();

    // draw all gantt at first
    $timeout(function () {
        $scope.UpdateAllPlots();
    }, 0);

}]);