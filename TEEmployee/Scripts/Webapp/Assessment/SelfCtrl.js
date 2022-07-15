var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllSelfAssessments = function (o) {
        return $http.post('Assessment/GetAllSelfAssessments', o);
    };

    this.CreateResponse = function (o) {
        return $http.post('Assessment/CreateResponse', o);
    };

    this.GetResponse = function (o) {
        return $http.post('Assessment/GetResponse', o);
    };

    this.GetResponseByYear = function (o) {
        return $http.post('Assessment/GetResponseByYear', o);
    };

    this.GetYearList = function (o) {
        return $http.post('Assessment/GetYearList', o);
    };

    this.GetAllFeedbacks = function (o) {
        return $http.post('Assessment/GetAllFeedbacks', o);
    };

    
}]);

app.controller('SelfCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.SelfAssessments = [];
    $scope.state;

    //$scope.response = {
    //    "Id": 7596,
    //    "choices": []
    //}
    $scope.data = {
        model: null,
        availableOptions: [
            //{ id: '2022H1', name: '2022H1' },
            //{ id: '2022H2', name: '2022H2' }
        ]
    };

    $scope.myFunction = function (daystring) {
        alert(daystring);
    }

    appService.GetYearList({}).then(function (ret) {
        $scope.years = ret;
      
        ret.data.forEach(function (item) {

            $scope.data.availableOptions.push({ id: item, name: item })
        });

        $scope.data.model = $scope.data.availableOptions[0].name

    });
    


    $scope.Response = [];

    //$scope.CreateResponse = function () {
    //    appService.CreateResponse($scope.SelfAssessments)
    //        .then(function (ret) {
    //            $window.location.href = '/Home';
    //        });
    //}

    //add save / submit state
    $scope.CreateResponse = function (state) {
        appService.CreateResponse({ assessments: $scope.SelfAssessments, state: state, year: $scope.data.model })
            .then(function (ret) {
                $window.location.href = '/Home';
            });
    }

    $scope.GetResponseByYear = function (year) {
        appService.GetResponseByYear({ year: year })
            .then(function (ret) {
                $scope.state = ret.data.State;
                //$scope.SelfAssessments.forEach(function (item) {

                //    ret.data.Responses.forEach(function (item2) {

                //        if (item.Id === item2.Id) {
                //            item.Choice = item2.Choice;
                //            return
                //        }

                //    });

                //});
                if (ret.data.Responses.length !== 0) {

                    $scope.SelfAssessments = ret.data.Responses

                    appService.GetAllFeedbacks({ year: year })
                        .then(function (ret) {
                            $scope.Feedbacks = ret.data;
                        });
                }
                    
                else {
                    appService.GetAllSelfAssessments({})
                        .then(function (ret) {
                            $scope.SelfAssessments = ret.data;
                            //$scope.SelfAssessments[0].UserId = 7596                            
                        })
                        .catch(function (ret) {
                            alert('Error');
                        });
                }

            })
            .catch(function (ret) {
                alert('Error');
            });

    }


    $scope.GetResponseByYear();

    



    //appService.GetAllSelfAssessments({})
    //    .then(function (ret) {
    //        $scope.SelfAssessments = ret.data;
    //        //$scope.SelfAssessments[0].UserId = 7596
    //        $scope.GetResponseByYear();
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });



    
    //appService.GetAllSelfAssessments({})
    //    .then(function (ret) {
    //        $scope.SelfAssessments = ret.data;
    //        //$scope.SelfAssessments[0].UserId = 7596
    //        $scope.GetResponseByYear();
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });


    //appService.GetResponse({})
    //    .then(function (ret) {
    //        $scope.SelfAssessments.forEach(function (item) {

    //            ret.data.Responses.forEach(function (item2) {

    //                if (item.Id === item2.Id) {
    //                    item.Choice = item2.Choice;
    //                    return
    //                }

    //            });

    //        });


    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });
   


}]);