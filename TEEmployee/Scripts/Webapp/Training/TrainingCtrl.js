var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    //this.GetAllRecords = (o) => {
    //    return $http.post('Training/GetAllRecords', o);
    //};
    this.GetAuth = (o) => {
        return $http.post('Training/GetAuthorization', o);
    };

    this.GetAllRecordsByUser = (o) => {
        return $http.post('Training/GetAllRecordsByUser', o);
    };


}]);

app.controller('TrainingCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllRecordsByUser({}).then((ret) => {

        $scope.records = ret.data;
        
    })

    // client to server
    formElem.onsubmit = async (e) => {

        e.preventDefault();

        // don't add '/' before MVC controller
        let response = await fetch('Training/UploadTrainingFile', {
            method: 'POST',
            body: new FormData(formElem)
        });

        let result = await response.json();

        alert(result);
    };


}]);

app.controller('GroupCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    $scope.years = [];
    $scope.employees = [];
    $scope.removedItems = [];

    let curYear = new Date().getFullYear();

    for (let i = curYear; i >= 1996; i--) {
        $scope.years.push(i);
    }

    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        $scope.selectedYear = $scope.years[0].toString();
        
        let group_array = $scope.auth.Users.map(x => x.group_one);
        $scope.groups = [...new Set(group_array)];
        $scope.groups.splice($scope.groups.indexOf(''), 1);
        $scope.selectedGroup = "";
    });


    $scope.selectYear = () => {       
        $scope.selectGroup();
    }


    $scope.selectGroup = () => {

        let employee_array = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup).map(x => ({
            user: x,
            count: x.trainings.filter(y => (y.roc_year + 1911) == $scope.selectedYear).length,
        }));               

        $scope.employees = employee_array;
        $scope.selectedEmployee = $scope.employees[0].user.empno;
        $scope.selectEmployee();
        
    }

    $scope.selectEmployee = () => {

        $scope.records = structuredClone(
            $scope.auth.Users.find(x => x.empno === $scope.selectedEmployee).trainings.filter(x => (x.roc_year + 1911) == $scope.selectedYear)
        );

        for (let i = 0; i !== $scope.records.length; i++) {

            $scope.records[i].names = [];

            for (let user of $scope.auth.Users) {

                if (user.trainings.find(x => x.training_id === $scope.records[i].training_id)) {
                    $scope.records[i].names.push(user.name);
                }
            }

        }

    }
    

}]);

