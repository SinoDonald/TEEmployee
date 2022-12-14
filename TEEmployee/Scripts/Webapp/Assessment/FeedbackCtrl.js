var app = angular.module('app', ['ui.router', 'ngAnimate']);

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('EmployeeList', {
            url: '/EmployeeList',
            templateUrl: 'Assessment/EmployeeList'
        })       
        .state('AssessEmployee', {
            url: '/AssessEmployee',
            templateUrl: 'Assessment/AssessEmployee'
        })

}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllEmployees = function (o) {
        return $http.post('Assessment/GetAllEmployees', o);
    };

    this.GetAllEmployeesWithState = function (o) {
        return $http.post('Assessment/GetAllEmployeesWithState', o);
    };

    this.GetAllEmployeesWithStateByRole = function (o) {
        return $http.post('Assessment/GetAllEmployeesWithStateByRole', o);
    };

    this.GetResponse = function (o) {
        return $http.post('Assessment/GetResponse', o);
    };

    this.GetAllSelfAssessments = function (o) {
        return $http.post('Assessment/GetAllSelfAssessments', o);
    };

    this.CreateMResponse = function (o) {
        return $http.post('Assessment/CreateMResponse', o);
    };

    this.GetMResponse = function (o) {
        return $http.post('Assessment/GetMResponse', o);
    };

    this.GetMixResponse = function (o) {
        return $http.post('Assessment/GetMixResponse', o);
    };

    this.CreateMixResponse = function (o) {
        return $http.post('Assessment/CreateMixResponse', o);
    };

    this.GetResponseByYear = function (o) {
        return $http.post('Assessment/GetResponseByYear', o);
    };

    this.UpdateFeedback = function (o) {
        return $http.post('Assessment/UpdateFeedback', o);
    };

    this.GetFeedback = function (o) {
        return $http.post('Assessment/GetFeedback', o);
    };

    this.GetAllOtherFeedbacks = function (o) {
        return $http.post('Assessment/GetAllOtherFeedbacks', o);
    };
    
    this.UpdateFeedbackNotification = (o) => {
        return $http.post('Assessment/UpdateFeedbackNotification', o);
    };
}]);

app.factory('myFactory', function () {
    var savedData = {}
    //function set(data, data2) {
    //    savedData.employee = data;
    //    savedData.mixResponse = data2;
    //}

    function set(data, isBoss) {
        savedData.EmployeeInfo = data;
        savedData.isBoss = isBoss;
    }

    function get() {
        return savedData;
    }

    return {
        set: set,
        get: get,
    }

});

app.controller('FeedbackCtrl', ['$scope', '$location', 'appService', '$rootScope', 'myFactory', function ($scope, $location, appService, $rootScope, myFactory) {

    $location.path('/EmployeeList');

}]);

app.controller('EmployeeListCtrl', ['$scope', '$location', 'appService', '$rootScope', 'myFactory', function ($scope, $location, appService, $rootScope, myFactory) {

    $scope.Employees = [];

    $scope.sortBy = function (propertyName) {        
        $scope.propertyName = propertyName;
    };

    //appService.GetAllEmployees({})
    //    .then(function (ret) {
    //        $scope.Employees = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });
    
    //appService.GetAllEmployeesWithState({})
    //    .then(function (ret) {
    //        $scope.Employees = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    appService.GetAllEmployeesWithStateByRole({})
        .then(function (ret) {
            $scope.Employees = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });

    $scope.Review = function (data, isBoss) {
        $location.path('/AssessEmployee');
        myFactory.set(data, isBoss)

    /*myFactory.set(data);*/
           //appService.GetMixResponse(data)
           //    .then(function (ret) {
           //        myFactory.set(data, ret.data)
           //        $location.path('/Review');
           //    })
           //    .catch(function (ret) {
           //        alert('Error');
           //    });
           //$window.location.href = '/Assessment/Review'
           //$window.location.path = '/Review'

    }

}]);

app.controller('AssessEmployeeCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', '$timeout', function ($scope, $window, appService, $rootScope, myFactory, $timeout) {

    const optionText = ['優良', '普通', '尚可', '待加強', 'N/A'];
    const limit = 250; // textarea height limit

    $scope.name = myFactory.get().EmployeeInfo.Employee.name;
    $scope.isBoss = myFactory.get().isBoss;

    $scope.GetResponseByYear = function (year) {
        appService.GetResponseByYear({ year: year, empno: myFactory.get().EmployeeInfo.Employee.empno })
            .then(function (ret) {

                
                for (let m = 0; m !== ret.data.Responses.length; m++) {
                    if (ret.data.Responses[m].Choice.includes('option'))
                        ret.data.Responses[m].Choice = optionText[Number(ret.data.Responses[m].Choice.slice(6)) - 1];
                }                

                //$scope.state = ret.data.State;
                for (var i = 0; i < ret.data.Responses.length; i++) {
                    if (ret.data.Responses[i].Content === '自評摘要') {
                        ret.data.Responses.splice(i + 1, 0, { CategoryId: ret.data.Responses[i].CategoryId, Content: '意見回饋' });
                    }
                }

                $scope.Responses = ret.data.Responses; 

                if ($scope.isBoss)
                    return appService.GetAllOtherFeedbacks({ empno: myFactory.get().EmployeeInfo.Employee.empno })
                else
                    return appService.GetFeedback({ empno: myFactory.get().EmployeeInfo.Employee.empno })
            })
            .then(function (ret) {
                //$scope.state = ret.data.State;                
                //$scope.feedback = ret.data.Text

                if ($scope.isBoss) {
                    $scope.state = 'submit'

                    let count = 0;

                    for (var i = 0; i < $scope.Responses.length; i++) {
                        if ($scope.Responses[i].Content === '意見回饋') {
                            $scope.Responses[i].Choice = '';
                            for (let fed of ret.data) {
                                $scope.Responses[i].Choice += ('\n' + fed.Name + ':\n' + fed.Text[count] + '\n');
                            }
                            
                            count++;
                        }
                    }

                    $scope.feedback = ''
                    for (let fed of ret.data) {                        
                        $scope.feedback += ('\n' + fed.Name + ':\n' + fed.Text[count] + '\n');
                    }
                    
                }

                else {
                    $scope.state = ret.data.State;

                    let count = 0;

                    for (let i = 0; i < $scope.Responses.length; i++) {
                        if ($scope.Responses[i].Content === '意見回饋') {
                            $scope.Responses[i].Choice = ret.data.Text[count];
                            count++;
                        }
                    }

                    $scope.feedback = ret.data.Text[count];
                }
                    
                
                

                // set textarea height in beginning
                $timeout(function () {

                    var textAreaItems = document.querySelectorAll(".autoExpand");
                    for (let elm of textAreaItems) {
                        elm.style.height = "";
                        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
                    }
                }, 0);


            })
            .catch(function (ret) {
                alert('Error');
            });
            //.catch(function (ret) {
            //    alert('Error');
            //});
            


        //appService.GetFeedback({ empno: myFactory.get().EmployeeInfo.Employee.empno })
        //    .then(function (ret) {
        //        //$scope.state = ret.data.State;                
        //        //$scope.feedback = ret.data.Text
        //        $scope.state = ret.data.State

        //        var count = 0;

        //        for (var i = 0; i < $scope.Responses.length; i++) {
        //            if ($scope.Responses[i].Content === '意見回饋') {
        //                $scope.Responses[i].Choice = ret.data.Text[count];
        //                count++;
        //            }
        //        }

        //        $scope.feedback = ret.data.Text[count];

        //    })
        //    .catch(function (ret) {
        //        alert('Error');
        //    });

    }

    $scope.GetResponseByYear();

    $scope.UpdateFeedback = function (state) {
        var feedbacks = [];
        for (var i = 0; i < $scope.Responses.length; i++) {
            if ($scope.Responses[i].Content === '意見回饋') {
                feedbacks.push($scope.Responses[i].Choice);
            }
        }

        feedbacks.push($scope.feedback);

        appService.UpdateFeedback({ feedbacks: feedbacks, state: state, empno: myFactory.get().EmployeeInfo.Employee.empno })
            .then(function (ret) {
                //$window.location.href = 'Assessment/Feedback';
                if (state === 'save') {
                /*alert('已儲存');*/
                    $scope.succeed = true;
                    $timeout(function () {
                        $scope.succeed = false;
                    }, 2000);
                }
                else {
                    appService.UpdateFeedbackNotification({ empno: myFactory.get().EmployeeInfo.Employee.empno }).then((ret) => {
                        $window.location.href = 'Assessment/Feedback';
                    })

                    /*alert('已送出');*/
                    //$window.location.href = 'Assessment/Feedback';
                }
            });
    }

    

    //$scope.Response = {}

    //$scope.Employee = myFactory.get();

    //$scope.Response = myFactory.get().mixResponse;

    //appService.GetResponse($scope.Employee)
    //    .then(function (ret) {
    //        $scope.Response = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    //appService.GetResponse($scope.Employee.UserId)
    //    .then(function (ret) {
    //        $scope.Response = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    function onExpandableTextareaInput({ target: elm }) {

        if (!elm.classList.contains('autoExpand') || !elm.nodeName === 'TEXTAREA') return

        elm.style.height = "";
        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
    }

    // global delegated event listener
    document.addEventListener('input', onExpandableTextareaInput)




}]);

