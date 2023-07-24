var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllKpiModels = (o) => {
        return $http.post('Kpi/GetAllKpiModels', o);
    };

}]);

app.controller('KpiCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {


    $scope.years = [], $scope.employees = [];
    $scope.kpiModels = [];

    let curYear = new Date().getFullYear();
    for (let i = curYear; i >= 2023; i--) {
        $scope.years.push(i);
    }    

    $scope.selectYear = () => {

        appService.GetAllKpiModels({ year: Number($scope.selectedYear) }).then((ret) => {

            $scope.data = ret.data;

            let group_array = $scope.data.map(x => x.group_name);
            $scope.groups = [...new Set(group_array)]
        });
    }

    $scope.selectGroup = () => {

        if (!$scope.data) return;

        let employee_array = $scope.data.filter(x => x.group_name === $scope.selectedGroup).map(x => x.name);
        $scope.employees = [...new Set(employee_array)]
        $scope.selectedEmployee = $scope.employees[0];
        $scope.selectEmployee();
    }

    $scope.selectEmployee = () => {

        if (!$scope.data) return;

        $scope.kpiTypes = $scope.data
            .filter(x => x.group_name === $scope.selectedGroup
                && x.name === $scope.selectedEmployee).map(x => x.kpi_type);

        $scope.selectedType = $scope.kpiTypes[0];
        $scope.selectType();

    }

    $scope.selectType = () => {

        if (!$scope.data) return;

        $scope.datum = $scope.data
            .find(x => x.name === $scope.selectedEmployee
                && x.group_name === $scope.selectedGroup
                && x.kpi_type === $scope.selectedType);
    }

    $scope.selectedYear = $scope.years[0].toString();
    $scope.selectYear();

}]);
