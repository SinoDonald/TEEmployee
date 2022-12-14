var app = angular.module('app', ['ui.router']);

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

    $scope.data = dataservice.get();

}]);

app.controller('IndividualCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    $scope.data = dataservice.get();

}]);

app.controller('FutureCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    $scope.data = dataservice.get();

}]);

