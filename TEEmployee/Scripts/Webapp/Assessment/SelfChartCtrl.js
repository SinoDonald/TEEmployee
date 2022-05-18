var app = angular.module('app', []);

var optionName = ['Good','Gooood','Goooood'];


app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllSelfAssessments = function (o) {
        return $http.post('Assessment/GetAllSelfAssessments', o);
    };

    this.GetAllResponses = function (o) {
        return $http.post('Assessment/GetAllResponses', o);
    };

    this.GetAllCategorySelfAssessmentCharts = function (o) {
        return $http.post('Assessment/GetAllCategorySelfAssessmentCharts', o);
    };

}]);

app.controller('SelfChartCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {


    appService.GetAllCategorySelfAssessmentCharts({})
        .then(function (ret) {
            CreateChart(ret.data, optionName);
        })
        //.catch(function (ret) {
        //    alert('Error');
        //});

    //appService.GetAllSelfAssessments({})
    //    .then(function (assessments) {

    //        appService.GetAllResponses({})
    //            .then(function (responses) {
                    
    //                assessments.data.forEach(function (assessment) {

    //                    var dataCollect = {}
    //                    var OptionOneCount = 0
    //                    var OptionTwoCount = 0
    //                    var OptionThrCount = 0

    //                    responses.data.forEach(function (response) {

    //                        if (assessment.Id != response.Id)
    //                            return

    //                        if (response.Choice == 'option1')
    //                            OptionOneCount++
    //                        else if (response.Choice == 'option2')
    //                            OptionTwoCount++
    //                        else if (response.Choice == 'option3')
    //                            OptionThrCount++
    //                    })

    //                    dataCollect.OptionOneCount = OptionOneCount
    //                    dataCollect.OptionTwoCount = OptionTwoCount
    //                    dataCollect.OptionThrCount = OptionThrCount
    //                    dataCollect.CategoryId = assessment.CategoryId
    //                    dataCollect.Content = assessment.Content

    //                    ResponseData.push(dataCollect)

    //                })
    //                CreateChart();
    //            })
    //            .catch(function (ret) {
    //                alert('Error');
    //            });
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });


    




    //$scope.SelfAssessments = [];
    //$scope.AllResponses = [];

    //appService.GetAllSelfAssessments({})
    //    .then(function (ret) {
    //        $scope.SelfAssessments = ret.data;            
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    //appService.GetAllResponses({})
    //    .then(function (ret) {
    //        $scope.AllResponses = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    //$scope.SelfAssessments.forEach(function (assessment) {

    //    var dataCollect = {}
    //    var ObjectOneCount = 0

    //    $scope.AllResponses.forEach(function (response) {

    //        if (assessment.Id != response.Id)
    //            return

    //        if (response.Choice == 'object1')
    //            objectOneCount++

    //    })

    //    dataCollect.ObjectOneCount = ObjectOneCount
    //    dataCollect.Content = assessment.Content

    //    chartset.push(dataCollect)

    //})





}]);