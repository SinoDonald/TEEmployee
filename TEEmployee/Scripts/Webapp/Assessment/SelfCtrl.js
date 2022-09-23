var app = angular.module('app', ['ngAnimate']);

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

app.controller('SelfCtrl', ['$scope', '$window', 'appService', '$rootScope', '$timeout', function ($scope, $window, appService, $rootScope, $timeout) {

    $scope.SelfAssessments = [];
    $scope.state;

    const optionText = ['優良', '普通', '尚可', '待加強', 'N/A'];


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
                //$window.location.href = 'Assessment';
                if (state === 'save') {
                    /* alert('已儲存');*/
                    $scope.succeed = true;
                    $timeout(function () {
                        $scope.succeed = false;
                    }, 2000);
                    
                }
                else {
                    //alert('已送出');
                    $window.location.href = 'Assessment';
                }
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

                // 
                if (ret.data.Responses.length !== 0) {

                    if ($scope.state === "submit") {
                        for (let m = 0; m !== ret.data.Responses.length; m++) {
                            if (ret.data.Responses[m].Choice.includes('option'))
                                ret.data.Responses[m].Choice = optionText[Number(ret.data.Responses[m].Choice.slice(6)) - 1];
                        }
                    }


                    var feedbacksByCategory = [];

                    var category_count = Math.max(...ret.data.Responses.map(o => Number(o.CategoryId)))

                    //$scope.SelfAssessments = ret.data.Responses

                    appService.GetAllFeedbacks({ year: year })
                        .then(function (ret2) {

                            for (var i = 0; i < (category_count) + 1; i++) {

                                var feedback = [];

                                for (var j = 0; j < ret2.data.length; j++) {                                                  

                                    if (ret2.data[j].Text[i] !== '') {
                                        feedback.push({ Name: ret2.data[j].Name, Text: ret2.data[j].Text[i] });
                                    }
                                }
                                feedbacksByCategory.push(feedback);
                            }

                            var count = 0;

                            for (var k = 0; k < ret.data.Responses.length; k++) {

                                

                                if (ret.data.Responses[k].Content === '自評摘要') {

                                    feedbacksByCategory[count].forEach(function (item) {
                                        ret.data.Responses.splice(k + 1, 0, { CategoryId: ret.data.Responses[k].CategoryId, Content: '意見回饋', Choice: item.Text, Name: item.Name });
                                    });

                                    count++;
                                }
                            }

                            //$scope.Responses = ret.data.Responses 
                            $scope.SelfAssessments = ret.data.Responses


                            $scope.Feedbacks = feedbacksByCategory[count];
                        });
                                        

                }

                // load empty form if no record in database
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