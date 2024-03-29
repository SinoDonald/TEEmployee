﻿var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetChartManagerData = (o) => {
        return $http.post('Assessment/GetChartManagerData', o);
    };
    this.GetChartYearList = (o) => {
        return $http.post('Assessment/GetChartYearList', o);
    };
    this.GetChartManagerList = (o) => {
        return $http.post('Assessment/GetChartManagerList', o);
    };

}]);

app.controller('ChartManagerCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {


    $scope.mode = 'bar';

    $scope.categories = [
        { name: '解決問題能力', id: '1' },
        { name: '領導能力', id: '2' },
        { name: '溝通能力', id: '3' },
        { name: '訂定目標能力', id: '4' },
        { name: '組織能力', id: '5' },
        { name: '培育同仁能力', id: '6' },
    ]

    $scope.selectedCategory = $scope.categories[0].id;



    let promiseA = appService.GetChartYearList({ isManagerResponse: true }).then((ret) => {

        $scope.years = ret.data;
        $scope.selectedYear = $scope.years[0];

    });

    //appService.GetChartManagerList({}).then((ret) => {

    //    $scope.managers = ret.data;
    //    $scope.selectedManager = $scope.managers[0];

    //});

    let promiseB = appService.GetChartManagerData({}).then((ret) => {

        $scope.data = ret.data;
        $scope.managers = [];

        for (const item of $scope.data.ChartManagerResponses)
            $scope.managers.push(item.Manager.name);
        $scope.selectedManager = $scope.managers[0];

        if ($scope.managers.length > 1)
            $scope.managers.push('All');
    });


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

    //let promiseA = appService.GetChartYearList({ isManagerResponse: true }).then((ret) => {

    //    $scope.years = ret.data;
    //    $scope.selectedYear = $scope.years[0];

    //    return appService.GetChartDataByGroupAndYear({ year: $scope.selectedYear });
    //});


    //let promiseB = appService.GetChartGroupList({});


    $q.all([promiseA, promiseB]).then((ret) => {

        $scope.DrawChart();
    });



    //$scope.FilterDataByManager = function (selectedGroup) {

    //    fdata = [];

    //    for (let item of $scope.data) {
    //        if (item.Employee.group === selectedGroup || item.Employee.group_one === selectedGroup || item.Employee.group_two === selectedGroup) {
    //            fdata.push(item);
    //        }
    //    }

    //    //$scope.DrawChart();
    //}

    //$scope.DrawChart = () => {

    //    switch ($scope.mode) {
    //        case 'bar':
    //            DrawManagerBarChart($scope.data, $scope.selectedManager, $scope.selectedCategory);
    //            break;
    //        case 'comment':
    //            ShowComment($scope.data, $scope.selectedManager, $scope.selectedCategory);
    //            break;
    //        case 'radar':
    //            DrawManagerRadarChart($scope.data, $scope.selectedManager, $scope.categories);
    //            break;
    //    }

    //}


    // 20230511 Draw New Manager Chart (6 options)
    $scope.DrawChart = () => {

        //switch ($scope.mode) {
        //    case 'bar':
        //        if ($scope.selectedYear > '2022H2')
        //            DrawNewManagerBarChart($scope.data, $scope.selectedManager, $scope.selectedCategory);
        //        else
        //            DrawManagerBarChart($scope.data, $scope.selectedManager, $scope.selectedCategory);
        //        break;
        //    case 'comment':
        //        ShowComment($scope.data, $scope.selectedManager, $scope.selectedCategory);
        //        break;
        //    case 'radar':
        //        if ($scope.selectedYear > '2022H2')
        //            DrawNewManagerRadarChart($scope.data, $scope.selectedManager, $scope.categories);
        //        else
        //            DrawManagerRadarChart($scope.data, $scope.selectedManager, $scope.categories);
        //        break;
        //}


        switch ($scope.mode) {
            case 'bar':
                DrawManagerBarChartByYear($scope.data, $scope.selectedManager, $scope.selectedCategory, $scope.selectedYear);
                break;
            case 'comment':
                ShowComment($scope.data, $scope.selectedManager, $scope.selectedCategory);
                break;
            case 'radar':
                DrawManagerRadarChartByYear($scope.data, $scope.selectedManager, $scope.categories, $scope.selectedYear);
                break;
        }

    }

    // 20230706: get new manager list whenever GetChartData()
    $scope.GetChartData = () => {

        if ($scope.selectedYear > '2023H1') {
            $scope.categories = [
                { name: '解決問題能力', id: '1' },
                { name: '領導能力', id: '2' },
                { name: '溝通能力', id: '3' },
                { name: '訂定目標能力', id: '4' },
                { name: '組織能力', id: '5' },
                { name: '培育同仁能力', id: '6' }
            ]
        }
        else {
            $scope.categories = [
                { name: '解決問題能力', id: '1' },
                { name: '領導能力', id: '2' },
                { name: '溝通能力', id: '3' },
                { name: '訂定目標能力', id: '4' },
                { name: '組織能力', id: '5' },
                { name: '培育同仁能力', id: '6' },
                { name: '人格特質', id: '7' }
            ]
        }

        if ($scope.mode === 'comment') {
            $scope.categories.push({ name: '總評', id: ($scope.categories.length + 1).toString() });
        }

        appService.GetChartManagerData({ year: $scope.selectedYear }).then((ret) => {

            $scope.data = ret.data;

            $scope.managers = [];

            for (const item of $scope.data.ChartManagerResponses)
                $scope.managers.push(item.Manager.name);
            $scope.selectedManager = $scope.managers[0];

            if ($scope.managers.length > 1)
                $scope.managers.push('All');

            $scope.DrawChart();
        })
    }

    // 1006 add 總評 condition
    $scope.SwitchMode = ($event, mode) => {

        $scope.mode = mode;
        const managerSelect = document.querySelector(".manager");

        if (mode === 'comment') {
            //$scope.selectedManager = $scope.managers[0];
            //managerSelect.disabled = true;

            //$scope.categories.push({ name: '總評', id: '8' });
            if (!$scope.categories.find(x => x.name === '總評')) {
                $scope.categories.push({ name: '總評', id: ($scope.categories.length + 1).toString() });
            }
            
        }
        else {
            managerSelect.disabled = false;
            //$scope.categories.splice(7, 1);
            //if ($scope.selectedCategory === '8')
            //    $scope.selectedCategory = '1';

            if ($scope.categories.find(x => x.name === '總評')) {
                if ($scope.selectedCategory === $scope.categories.length.toString())
                    $scope.selectedCategory = '1';
                $scope.categories.splice($scope.categories.length - 1, 1);
            }
        }


        const children = $event.currentTarget.parentElement.children;
        for (const child of children)
            child.style.opacity = 0.2;

        $event.currentTarget.style.opacity = 1.0;
        $scope.DrawChart();
    }

}]);