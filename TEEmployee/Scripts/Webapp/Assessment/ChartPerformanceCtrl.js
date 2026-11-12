var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetPerformanceChart = (o) => {
        return $http.post('Assessment/GetPerformanceChart', o);
    };

}]);

app.controller('ChartPerformanceCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    // year selection

    $scope.years = [];
    let current = new Date();

    for (let i = current.getFullYear(); i >= 2024; i--) {

        if (i === current.getFullYear()) {
            if (current.getMonth() + 1 > 6)
                $scope.years.push(`${i}H2`);
        }
        else {
            $scope.years.push(`${i}H2`);
        }
        $scope.years.push(`${i}H1`);
    }

    $scope.selectedYear = $scope.years[0];

    $scope.selectYear = () => {
        appService.GetPerformanceChart({ year: $scope.selectedYear }).then((ret) => {
            $scope.managers = ret.data || [];
            if ($scope.managers.length === 0) return;   // 該年度沒有資料
            $scope.selectedManager = $scope.managers[0].empno;
            $scope.selectManager();            
        })
    }

    $scope.selectManager = () => {
        let manager = $scope.managers.find(x => x.empno === $scope.selectedManager);
        $scope.groups = (manager && manager.groups) || [];
        if ($scope.groups.length === 0) return;   // 沒有可顯示的群組
        $scope.selectedGroup = $scope.groups[0].group_name;
        $scope.selectGroup();  
    }

    $scope.selectGroup = () => {
        let performances = $scope.groups.find(x => x.group_name === $scope.selectedGroup).performances;
        DrawPerformanceChart(performances);
    }

    $scope.selectYear();

}]);