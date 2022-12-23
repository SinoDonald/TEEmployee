var app = angular.module('app', ['ui.router', 'moment-picker', 'angularjs-dropdown-multiselect']);

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('Group', {
            url: '/Group',
            templateUrl: 'Schedule/Group'
        })
        .state('Individual', {
            url: '/Individual',
            templateUrl: 'Schedule/Individual'
        })
        .state('Future', {
            url: '/Future',
            templateUrl: 'Schedule/Future'
        })

}]);


app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllSchedules = (o) => {
        return $http.post('Schedule/GetAllSchedules', o);
    };
    this.GetAllOwnedSchedules = (o) => {
        return $http.post('Schedule/GetAllOwnedSchedules', o);
    };
    this.GetAllReferredSchedules = (o) => {
        return $http.post('Schedule/GetAllReferredSchedules', o);
    };
    this.UpdateOwnedSchedules = (o) => {
        return $http.post('Schedule/UpdateOwnedSchedules', o);
    };
    this.DeleteOwnedSchedules = (o) => {
        return $http.post('Schedule/DeleteOwnedSchedules', o);
    };
    this.GetAllEmployeesByRole = (o) => {
        return $http.post('Schedule/GetAllEmployeesByRole', o);
    };

}]);

app.factory('dataservice', function () {

    var scheduleData = {};

    return {
        setOwned: setOwned,
        setReferred: setReferred,
        get: get,
    }
    
    function setOwned(data) {
        scheduleData.Owned = data;        
    }
    function setReferred(data) {
        scheduleData.Referred = data;
    }

    function get() {
        return scheduleData;
    }    

});


app.controller('ScheduleCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    appService.GetAllSchedules({}).then((ret) => {
        $scope.data = ret.data;
    })    
    appService.GetAllOwnedSchedules({}).then((ret) => {
        dataservice.setOwned(ret.data);
    })    
    appService.GetAllReferredSchedules({}).then((ret) => {
        dataservice.setReferred(ret.data);
    })    

    $location.path('/Group');
}]);

app.controller('GroupCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {


    $scope.example1model = [];
    $scope.example1data = [{ id: 1, label: "David" }, { id: 2, label: "Jhon" }, { id: 3, label: "Danny" }];
    //$scope.yourEvents = { onInitDone: function (item) { console.log(item); }, onItemDeselect: function (item) { console.log(item); } };
    $scope.yourEvents = {
        onSelectionChanged: function () {
            let names = "";
            for (let i = 0; i !== $scope.example1model.length; i++) {
                names += $scope.example1model[i].label;
                if (i !== $scope.example1model.length - 1)
                    names += ", ";
                $scope.modal.member = names;
            }          

        },
        onDeselectAll: function () {          
                $scope.modal.member = "";            
        }
    };

    $scope.passChosen = () => {
        $scope.example1model = [];
        for (let emp of $scope.employees) {
            if ($scope.modal.member.includes(emp.label))
                $scope.example1model.push(emp);
        }
    }

    
    $scope.data = dataservice.get();

    $scope.deletedIds = { schedule_ids: [], milestones_ids: [] };

    appService.GetAllEmployeesByRole({}).then((ret) => {
        $scope.employees = ret.data;
        let count = 1;
        for (let emp of $scope.employees) {
            emp.id = count;
            emp.label = emp.name;
            count++;
        }
    })  


    $scope.UpdateSchedules = () => {

        //appService.UpdateOwnedSchedules({ schedules: $scope.data.Owned.Group }).then((ret) => {
        //    appService.GetAllOwnedSchedules({}).then((ret2) => {
        //        dataservice.setOwned(ret2.data);
        //    })
        //})

        let promiseA = Promise.resolve('a'), promiseB = Promise.resolve('b');

        if ($scope.data.Owned.Group.length !== 0) 
            promiseA = appService.UpdateOwnedSchedules({ schedules: $scope.data.Owned.Group })
                .then((ret) => {})
                .catch((ret) => { alert('Error'); }
                );
        

        if ($scope.deletedIds.schedule_ids.length !== 0 || $scope.deletedIds.milestones_ids.length !== 0)
            promiseB = appService.DeleteOwnedSchedules({ schedules: $scope.deletedIds.schedule_ids, milestones: $scope.deletedIds.milestones_ids })
                .then((ret) => {})
                .catch((ret) => { alert('Error'); }
                );
        

        $q.all([promiseA, promiseB]).then((ret) => {
            appService.GetAllOwnedSchedules({}).then((ret2) => {
                dataservice.setOwned(ret2.data);
                $scope.deletedIds = { schedule_ids: [], milestones_ids: [] };
            })
            appService.GetAllReferredSchedules({}).then((ret2) => {
                dataservice.setReferred(ret2.data);
            }) 
        });



    }

    $scope.addSchedule = () => {
        $scope.data.Owned.Group.splice($scope.data.Owned.Group.length, 0, { type: 2, milestones: [] });
        $scope.modal = $scope.data.Owned.Group[$scope.data.Owned.Group.length - 1];
    };

    $scope.addMilestone = () => {
        $scope.modal.milestones.splice($scope.modal.milestones.length, 0, { schedule_id: $scope.modal.id });
    };

    $scope.removeSchedule = () => {

        let id = $scope.modal.id;
        if (id) {
            for (let m of $scope.modal.milestones)
                $scope.deletedIds.milestones_ids.push(m.id);
            $scope.deletedIds.schedule_ids.push(id);
        }            
        $scope.data.Owned.Group.splice($scope.data.Owned.Group.indexOf($scope.modal), 1);
    };

    $scope.removeMilestone = (idx) => {

        let id = $scope.modal.milestones[idx].id;
        if (id)
            $scope.deletedIds.milestones_ids.push(id);

        $scope.modal.milestones.splice(idx, 1);

    };

    //*******************
    //Method 2

    //$scope.ModifySchedule = (schedule) => {
    //    $scope.copy = JSON.parse(JSON.stringify(schedule));
    //    $scope.tempSchedule = schedule;
    //}

    //$scope.SaveChange = () => {        
    //    $scope.tempSchedule.content = $scope.copy.content;
    //    $scope.tempSchedule.milestones = $scope.copy.milestones;
    //}

    //$scope.copy = {};
    //******************
}]);

app.controller('IndividualCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {
        
    $scope.data = dataservice.get();

    $scope.deletedIds = { schedule_ids: [], milestones_ids: [] };

    $scope.addSchedule = (parent_id) => {
        $scope.data.Owned.Individual.splice($scope.data.Owned.Individual.length, 0, { type: 3, milestones: [], parent_id: parent_id });
        $scope.modal = $scope.data.Owned.Individual[$scope.data.Owned.Individual.length - 1];
    };

    $scope.addMilestone = () => {
        $scope.modal.milestones.splice($scope.modal.milestones.length, 0, { schedule_id: $scope.modal.id });
    };

    $scope.removeSchedule = () => {

        let id = $scope.modal.id;
        if (id) {
            for (let m of $scope.modal.milestones)
                $scope.deletedIds.milestones_ids.push(m.id);
            $scope.deletedIds.schedule_ids.push(id);
        }
        $scope.data.Owned.Individual.splice($scope.data.Owned.Individual.indexOf($scope.modal), 1);
    };

    $scope.removeMilestone = (idx) => {

        let id = $scope.modal.milestones[idx].id;
        if (id)
            $scope.deletedIds.milestones_ids.push(id);

        $scope.modal.milestones.splice(idx, 1);

    };

    $scope.UpdateSchedules = () => {

       
        let promiseA = Promise.resolve('a'), promiseB = Promise.resolve('b');

        if ($scope.data.Owned.Individual.length !== 0)
            promiseA = appService.UpdateOwnedSchedules({ schedules: $scope.data.Owned.Individual })
                .then((ret) => { })
                .catch((ret) => { alert('Error'); }
                );


        if ($scope.deletedIds.schedule_ids.length !== 0 || $scope.deletedIds.milestones_ids.length !== 0)
            promiseB = appService.DeleteOwnedSchedules({ schedules: $scope.deletedIds.schedule_ids, milestones: $scope.deletedIds.milestones_ids })
                .then((ret) => { })
                .catch((ret) => { alert('Error'); }
                );


        $q.all([promiseA, promiseB]).then((ret) => {
            appService.GetAllOwnedSchedules({}).then((ret2) => {
                dataservice.setOwned(ret2.data);
                $scope.deletedIds = { schedule_ids: [], milestones_ids: [] };
            })            
        });
    }

}]);

app.controller('FutureCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    $scope.data = dataservice.get();

}]);

