var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetChartEmployeeData = (o) => {
        return $http.post('Assessment/GetChartEmployeeData', o);
    };
    this.GetChartYearList = (o) => {
        return $http.post('Assessment/GetChartYearList', o);
    };
    this.GetChartGroupList = (o) => {
        return $http.post('Assessment/GetChartGroupList', o);
    };

}]);

app.controller('ChartEmployeeCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    //appService.GetChartYearList({isManagerResponse: false}).then((ret) => {

    //    $scope.years = ret.data;
    //    $scope.selectedYear = $scope.years[0];

    //    appService.GetChartDataByGroupAndYear({ year: $scope.selectedYear }).then((ret) => {

    //        $scope.data = ret.data;

    //    });

    //});

    //appService.GetChartGroupList({}).then((ret) => {

    //    $scope.groups = ret.data;
    //    $scope.selectedGroup = $scope.groups[0];

    //});

    //appService.GetChartYearList({ isManagerResponse: false }).then((ret) => {

    //    $scope.years = ret.data;
    //    $scope.selectedYear = $scope.years[0];

    //    appService.GetChartDataByGroupAndYear({ year: $scope.selectedYear }).then((ret) => {

    //        $scope.data = ret.data;

    //    });

    //});

    //appService.GetChartGroupList({}).then((ret) => {

    //    $scope.groups = ret.data;
    //    $scope.selectedGroup = $scope.groups[0];

    //});

    let promiseA = appService.GetChartYearList({ isManagerResponse: false }).then((ret) => {

        $scope.years = ret.data;
        $scope.selectedYear = $scope.years[0];

        return appService.GetChartEmployeeData({ year: $scope.selectedYear });        
    });


    let promiseB = appService.GetChartGroupList({});

    
    $q.all([promiseA, promiseB]).then((ret) => {

        $scope.data = ret[0].data;
        $scope.groups = ret[1].data;
        // or pass these data in promiseA and B, return empty promise from then() 

        $scope.selectedGroup = $scope.groups[0];
        $scope.FilterDataByGroup($scope.selectedGroup);
        $scope.DrawChart();
    });


    $scope.categories = [
        { name: '工作量', id: '1' },
        { name: '工作效率', id: '2' },
        { name: '工作品質', id: '3' },
        { name: '敬業精神', id: '4' },
        { name: '協調能力', id: '5' },
        { name: '其他貢獻', id: '6' }
    ]

    $scope.selectedCategory = $scope.categories[0].id;
    
    $scope.FilterDataByGroup = function(selectedGroup) {

        fdata = [];

        for (let item of $scope.data) {
            if (item.Employee.group === selectedGroup || item.Employee.group_one === selectedGroup || item.Employee.group_two === selectedGroup || item.Employee.group_three === selectedGroup) {
                fdata.push(item);
            }
        }

        $scope.DrawChart();
    }

    $scope.DrawChart = () => DrawEmployeeBarChart(fdata, $scope.selectedCategory);

    $scope.GetChartData = () => {

        appService.GetChartEmployeeData({ year: $scope.selectedYear }).then((ret) => {

            $scope.data = ret.data;
            $scope.FilterDataByGroup($scope.selectedGroup);
            $scope.DrawChart();
        })

    }


}]);