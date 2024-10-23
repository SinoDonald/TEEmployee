var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {     

    this.GetAllCourses = (o) => {
        return $http.post('Education/GetAllCourses', o);
    };
    this.GetAllRecords = (o) => {
        return $http.post('Education/GetAllRecords', o);
    };
    this.GetAuth = (o) => {
        return $http.post('Education/GetAuthorization', o);
    };
    this.UpsertRecords = (o) => {
        return $http.post('Education/UpsertRecords', o);
    };
}]);

app.controller('EducationCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllCourses({}).then((ret) => {

        $scope.courses = ret.data;

    })

    // client to server
    formElem.onsubmit = async (e) => {

        e.preventDefault();

        // don't add '/' before MVC controller
        let response = await fetch('Education/UploadCourseFile', {
            method: 'POST',
            body: new FormData(formElem)
        });

        let result = await response.json();

        alert(result);
    };

}]);

app.controller('AssignCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        let group_array = $scope.auth.Users.map(x => x.group_one);
        $scope.groups = [...new Set(group_array)];
        $scope.selectedGroup = "";

        const groupData = (d) => {
            let g = Object.entries(d.reduce((r, c) => (r[c.group_one] = [...r[c.group_one] || [], c], r), {}))

            return g.reduce((r, c) => (r.children.push(
                { name: c[0], children: c[1] }), r), { name: "Children Array", children: [] })
        }

        let users = $scope.auth.Users.sort(x => x.group_one).map(x => ({
            name: x.name,
            empno: x.empno,
            group_one: x.group_one,
        }));

        $scope.users = groupData(users);

    })

    $scope.selectGroup = () => {

        let employee_array = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup).map(x => x.name);

        $scope.employees = employee_array;
        $scope.selectedEmployee = $scope.employees[0];
        $scope.selectEmployee();

    }

    $scope.selectEmployee = () => {

        $scope.records = structuredClone(
            $scope.auth.Users.find(x => x.name === $scope.selectedEmployee).records
        );               

    }


    appService.GetAllCourses({}).then((ret) => {                

        const groupData = (d) => {
            let g = Object.entries(d.reduce((r, c) => (r[c.course_group_one] = [...r[c.course_group_one] || [], c], r), {}))
            
            return g.reduce((r, c) => (r.children.push(
                { name: c[0], children: c[1] }), r), { name: "Children Array", children: [] })
        }

        $scope.data = groupData(ret.data);

    })

    $scope.clickModal = function (c) {
        $scope.modalCourses = structuredClone($scope.data);
        $scope.modalUsers = structuredClone($scope.users);
    };

    $scope.changeColor = function (c) {
        if (c.isButtonClicked) {
            c.isButtonClicked = false;
        }            
        else {
            c.isButtonClicked = true;
        }
    };

    $scope.upsertRecords = () => {
                
        const selectedUsers = $scope.modalUsers.children
            .map(arr => arr.children.filter(x => x.isButtonClicked))
            .flat();

        const selectedCourses = $scope.modalCourses.children
            .map(arr => arr.children.filter(x => x.isButtonClicked))
            .flat();

        console.log(selectedUsers);
        console.log(selectedCourses);

        let records = [];

        for (let c of selectedCourses) {
            for (let u of selectedUsers) {

                let record = {
                    empno: u.empno,
                    course: { id: c.id},
                    assigned: true,
                }

                records.push(record);
            }
        }

        appService.UpsertRecords({ records: records }).then((ret) => {
                        

        })

    }

}]);

