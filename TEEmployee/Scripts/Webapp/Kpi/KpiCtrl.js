var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllKpiModels = (o) => {
        return $http.post('Kpi/GetAllKpiModels', o);
    };

    this.UpdateKpiItems = (o) => {
        return $http.post('Kpi/UpdateKpiItems', o);
    };

}]);

app.controller('KpiCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    const aniBtn = document.querySelector(".animate-btn");
    $scope.showLast = false;
    $scope.years = [], $scope.employees = [];
    $scope.removedItems = [];

    let curYear = new Date().getFullYear();
    let curMonth = new Date().getMonth();

    for (let i = curYear; i >= 2023; i--) {
        $scope.years.push(i);
    }
    if (curMonth > 5)
        $scope.showLast = true;


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

        // reset removed items
        $scope.removedItems = [];
    }

    $scope.selectedYear = $scope.years[0].toString();
    $scope.selectYear();

    // add at bottom
    $scope.addKpiItem = () => {

        if ($scope.datum) {
            $scope.datum.items.push({
                id: 0,
                kpi_id: $scope.datum.id,
                content: '',
                target: '',
                weight: 0,
                h1_employee_check: false,
                h1_manager_check: false,
                h1_reason: '',
                h2_employee_check: false,
                h2_manager_check: false,
                h2_reason: '',
                consensual: false,
            });
        }
    };

    $scope.removeKpiItem = (idx) => {
        $scope.removedItems.push($scope.datum.items[idx]);
        $scope.datum.items.splice(idx, 1);
    };

    $scope.updateKpiItems = () => {

        // Upsert and delete kpi items
        appService.UpdateKpiItems({ items: $scope.datum.items, removedItems: $scope.removedItems }).then((ret) => {



            if (ret.data) {
                $scope.datum.items = ret.data;
                $scope.removedItems = [];

                aniBtn.classList.add('onclic');

                setTimeout(function () {
                    aniBtn.classList.remove('onclic');
                    aniBtn.classList.add('validate');
                    setTimeout(function () {
                        aniBtn.classList.remove('validate');
                    }, 1500);
                }, 1500);

            }

        });
    };

    $scope.sumScore = () => {

        if (!$scope.datum) return 0;

        //console.log($scope.datum.items);

        let scores = $scope.datum.items.map(x => (x.consensual ? x.weight : 0));
        return scores.reduce((a, c) => (a + c)).toFixed(2);

        //return $scope.datum.items.reduce((a, c) => (a.consensual ? a.weight : 0) + (c.consensual ? c.weight : 0)).toFixed(2);
    }

}]);
