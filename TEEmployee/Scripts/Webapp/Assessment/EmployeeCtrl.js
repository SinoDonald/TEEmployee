var app = angular.module('app', ['ui.router']);

app.config(function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('EmpList', {
            url: '/EmpList',
            templateUrl: 'Assessment/EmpList'
        })
        .state('Review', {
            url: '/Review',
            templateUrl: 'Assessment/Review'
        })
        .state('AssessEmp', {
            url: '/AssessEmp',
            templateUrl: 'Assessment/AssessEmp'
        })
        .state('Compare', {
            url: '/Compare',
            templateUrl: 'Assessment/Compare'
        })

});




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

}]);

app.factory('myFactory', function () {

    var savedData = {}
    function set(data, data2) {
        savedData.employee = data;
        savedData.mixResponse = data2;
    }
    function get() {
        return savedData;
    }


    return {
        set: set,
        get: get,        
    }

});


app.controller('EmployeeCtrl', ['$scope', '$location', 'appService', '$rootScope', 'myFactory', function ($scope, $location, appService, $rootScope, myFactory) {

    $location.path('/EmpList');

}]);

app.controller('EmpListCtrl', ['$scope', '$location', 'appService', '$rootScope', 'myFactory', function ($scope, $location, appService, $rootScope, myFactory) {

    $scope.Employees = [];

    $scope.people = [{
        name: "John"
    }, {
        name: "Paul"
    }, {
        name: "George"
    }, {
        name: "Ringo"
    }];

    $scope.choices = {};

    //appService.GetAllEmployees({})
    //    .then(function (ret) {
    //        $scope.Employees = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    appService.GetAllEmployeesWithState({})
        .then(function (ret) {
            $scope.Employees = ret.data;
        })
        .catch(function (ret) {
            alert('Error');
        });


    $scope.Review = function (data) {
        /*myFactory.set(data);*/

        appService.GetMixResponse(data)
            .then(function (ret) {
                
                myFactory.set(data, ret.data)
                $location.path('/Review');
            })
            .catch(function (ret) {
                alert('Error');
            });


        //$window.location.href = '/Assessment/Review'
        //$window.location.path = '/Review'
        
    }


}]);



app.controller('ReviewCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

    //$scope.Response = {}

    //$scope.Employee = myFactory.get();

    $scope.Response = myFactory.get().mixResponse;


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



}]);

app.controller('AssessEmpCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

    $scope.Response = myFactory.get().mixResponse;
    $scope.Employee = myFactory.get().employee;

    $scope.CreateMixResponse = function () {
        appService.CreateMixResponse({ mixResponses: $scope.Response, employee: $scope.Employee })
            .then(function (ret) {
                $window.location.href = '/Home';
            });
    }

    //$scope.Response = {}

    //$scope.SelfAssessments = [];

    




    //$scope.CreateMResponse = function () {
    //    appService.CreateMResponse({ assessments: $scope.SelfAssessments, employee: $scope.Employee })
    //        .then(function (ret) {
    //            $window.location.href = '/Home';
    //        });
    //}

    //appService.GetAllSelfAssessments({})
    //    .then(function (ret) {
    //        $scope.SelfAssessments = ret.data;            
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    //appService.GetMResponse($scope.Employee)
    //    .then(function (ret) {

    //        for (let i in $scope.SelfAssessments) {

    //            for (let j in ret.data) {

    //                if ($scope.SelfAssessments[i].Id == ret.data[j].Id) {
    //                    $scope.SelfAssessments[i].Choice = ret.data[j].Choice;
    //                    break;
    //                }
    //            }
    //        }


    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });



}]);



app.controller('CompareCtrl', ['$scope', '$window', 'appService', '$rootScope', 'myFactory', function ($scope, $window, appService, $rootScope, myFactory) {

    $scope.Response = myFactory.get().mixResponse;
    $scope.Employee = myFactory.get().employee;

    $scope.CreateMixResponse = function () {
        appService.CreateMixResponse({ mixResponses: $scope.Response, employee: $scope.Employee })
            .then(function (ret) {
                $window.location.href = '/Home';
            });
    }


}]);