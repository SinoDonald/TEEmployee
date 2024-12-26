var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAuth = (o) => {
        return $http.post('GKpi/GetAuth', o);
    };

    this.GetEmployeeKpiModels = (o) => {
        return $http.post('GKpi/GetEmployeeKpiModels', o);
    };

    this.GetManagerKpiModels = (o) => {
        return $http.post('GKpi/GetManagerKpiModels', o);
    };

    this.UpdateKpiItems = (o) => {
        return $http.post('GKpi/UpdateKpiItems', o);
    };

}]);

app.controller('FillinCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', '$timeout', function ($scope, $window, appService, $rootScope, $q, $timeout) {

    appService.GetAuth({}).then((ret) => {
        $scope.auth = ret.data;
    });

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
    if (curMonth >= 10)
        $scope.years.unshift(curYear + 1);


    $scope.selectYear = () => {

        appService.GetEmployeeKpiModels({ year: Number($scope.selectedYear) }).then((ret) => {

            $scope.data = ret.data;

            let role_array = $scope.data.map(x => x.role);
            $scope.roles = [...new Set(role_array)];

            $scope.selectedRole = $scope.roles[0];
            $scope.selectRole();

            //let group_array = $scope.data.map(x => x.group_name);
            //$scope.groups = [...new Set(group_array)];

            //$scope.selectedGroup = $scope.groups[0];
            //$scope.selectGroup();
        });
    }

    $scope.selectRole = () => {

        if (!$scope.data) return;

        //$scope.kpiTypes = $scope.data
        //    .filter(x => x.role === $scope.selectedRole).map(x => x.kpi_type);        

        //$scope.selectedType = $scope.kpiTypes[0];
        //$scope.selectType();

        let group_array = $scope.data.filter(x => x.role === $scope.selectedRole).map(x => x.group_name);
        $scope.groups = [...new Set(group_array)];

        $scope.selectedGroup = $scope.groups[0];
        $scope.selectGroup();

    }

    $scope.selectGroup = () => {

        if (!$scope.data) return;

        //let employee_array = $scope.data.filter(x => x.group_name === $scope.selectedGroup).map(x => x.name);
        //$scope.employees = [...new Set(employee_array)]
        //$scope.selectedEmployee = $scope.employees[0];
        //$scope.selectEmployee();

        //let role_array = $scope.data.filter(x => x.group_name === $scope.selectedGroup).map(x => x.role);
        //$scope.roles = [...new Set(role_array)]
        //$scope.selectedRole = $scope.roles[0];
        //$scope.selectRole();

        $scope.kpiTypes = $scope.data
            .filter(x => x.group_name === $scope.selectedGroup && x.role === $scope.selectedRole).map(x => x.kpi_type);

        $scope.selectedType = $scope.kpiTypes[0];
        $scope.selectType();
    }


    //$scope.selectEmployee = () => {

    //    if (!$scope.data) return;

    //    $scope.kpiTypes = $scope.data
    //        .filter(x => x.group_name === $scope.selectedGroup
    //            && x.name === $scope.selectedEmployee).map(x => x.kpi_type);

    //    $scope.selectedType = $scope.kpiTypes[0];
    //    $scope.selectType();

    //}

    $scope.selectType = () => {

        if (!$scope.data) return;

        //$scope.datum = $scope.data
        //    .find(x => x.kpi_type === $scope.selectedType);

        $scope.datum = $scope.data
            .find(x => x.kpi_type === $scope.selectedType && x.role === $scope.selectedRole && x.group_name === $scope.selectedGroup);

        destructFeedbacks();

        $timeout(function () {

            var pp = document.querySelectorAll(".autoExpand");
            for (var elm of pp) {
                elm.style.height = "";
                elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
            }

        }, 0);

        // reset removed items
        //$scope.removedItems = [];
    }

    const destructFeedbacks = () => {
        //destruct feedback to self and other
        if ($scope.datum) {

            for (let item of $scope.datum.items) {

                if (item.h1_feedback) {
                    item.h1_feedbacks = item.h1_feedback.split('EOF');
                    item.h1_feedbacks.splice(item.h1_feedbacks.length - 1, 1);
                }

                if (item.h2_feedback) {
                    item.h2_feedbacks = item.h2_feedback.split('EOF');
                    item.h2_feedbacks.splice(item.h2_feedbacks.length - 1, 1);
                }
            }
        }
    }





    //$scope.selectedYear = $scope.years[0].toString();
    $scope.selectedYear = curYear.toString();
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
                //h1_manager_check: false,
                h1_reason: '',
                h2_employee_check: false,
                //h2_manager_check: false,
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
                destructFeedbacks();
                $scope.removedItems = [];

                aniBtn.classList.add('onclic');

                setTimeout(function () {
                    aniBtn.classList.remove('onclic');
                    aniBtn.classList.add('validate');
                    setTimeout(function () {
                        aniBtn.classList.remove('validate');
                    }, 1500);
                }, 1500);


                $timeout(function () {

                    var pp = document.querySelectorAll(".autoExpand");
                    for (var elm of pp) {
                        elm.style.height = "";
                        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
                    }

                }, 0);

            }

        });
    };

    $scope.sumScore = () => {

        if (!$scope.datum) return 0;
        if ($scope.datum.items.length === 0) return 0;

        //console.log($scope.datum.items);

        let scores = $scope.datum.items.map(x => (x.consensual ? x.weight : 0));
        return scores.reduce((a, c) => (a + c)).toFixed(2);

        //return $scope.datum.items.reduce((a, c) => (a.consensual ? a.weight : 0) + (c.consensual ? c.weight : 0)).toFixed(2);
    }

    var limit = 250;

    function onExpandableTextareaInput({ target: elm }) {

        if (!elm.classList.contains('autoExpand') || !elm.nodeName === 'TEXTAREA') return

        elm.style.height = "";
        elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
    }

    // global delegated event listener
    document.addEventListener('input', onExpandableTextareaInput);

    $scope.autoExpand = () => {
        $timeout(function () {

            var pp = document.querySelectorAll(".autoExpand");
            for (var elm of pp) {
                elm.style.height = "";
                elm.style.height = Math.min(elm.scrollHeight, limit) + "px";
            }

        }, 0);
    }

    

    // client to server
    const formKpiElem = document.querySelector('#formKpiElem');

    if (formKpiElem) {

        formKpiElem.onsubmit = async (e) => {
            e.preventDefault();

            // don't add '/' before MVC controller
            let response = await fetch('GKpi/UploadKpiFile', {
                method: 'POST',
                body: new FormData(formKpiElem)
            });

            let result = await response.json();

            alert(result);

            $window.location.reload()
        };

    }   

}]);


app.controller('FeedbackCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAuth({}).then((ret) => {
        $scope.auth = ret.data;
    });

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
    if (curMonth >= 10)
        $scope.years.unshift(curYear + 1);


    $scope.selectYear = () => {

        appService.GetManagerKpiModels({ year: Number($scope.selectedYear) }).then((ret) => {

            $scope.data = ret.data;

            let role_array = $scope.data.map(x => x.role);
            $scope.roles = [...new Set(role_array)];

            $scope.selectedRole = $scope.roles[0];
            $scope.selectRole();


            //let group_array = $scope.data.map(x => x.group_name);
            //$scope.groups = [...new Set(group_array)];

            //$scope.selectedGroup = $scope.groups[0];
            //$scope.selectGroup();
        });
    }

    $scope.selectRole = () => {

        if (!$scope.data) return;

        let group_array = $scope.data.filter(x => x.role === $scope.selectedRole).map(x => x.group_name);
        $scope.groups = [...new Set(group_array)];

        $scope.selectedGroup = $scope.groups[0];
        $scope.selectGroup();

        //$scope.kpiTypes = $scope.data
        //    .filter(x => x.role === $scope.selectedRole).map(x => x.kpi_type);

        //$scope.selectedType = $scope.kpiTypes[0];
        //$scope.selectType();
    }



    $scope.selectGroup = () => {

        if (!$scope.data) return;

        let employee_array = $scope.data.filter(x => x.role === $scope.selectedRole
            && x.group_name === $scope.selectedGroup).map(x => x.name);
        $scope.employees = [...new Set(employee_array)]
        $scope.selectedEmployee = $scope.employees[0];
        $scope.selectEmployee();
    }

    $scope.selectEmployee = () => {

        if (!$scope.data) return;

        $scope.kpiTypes = $scope.data
            .filter(x => x.role === $scope.selectedRole
                && x.group_name === $scope.selectedGroup
                && x.name === $scope.selectedEmployee).map(x => x.kpi_type);

        $scope.selectedType = $scope.kpiTypes[0];
        $scope.selectType();

    }

    $scope.selectType = () => {

        if (!$scope.data) return;

        $scope.datum = $scope.data
            .find(x => x.role === $scope.selectedRole
                && x.name === $scope.selectedEmployee
                && x.group_name === $scope.selectedGroup
                && x.kpi_type === $scope.selectedType);

        //destruct feedback to self and other
        destructFeedbacks();


        // reset removed items
        /*$scope.removedItems = [];*/
    }


    const destructFeedbacks = () => {
        //destruct feedback to self and other
        if ($scope.datum) {

            let name = $scope.auth[1];

            for (let item of $scope.datum.items) {

                if (item.h1_feedback) {

                    item.h1_feedbacks = item.h1_feedback.split('EOF');
                    item.h1_feedbacks.splice(item.h1_feedbacks.length - 1, 1);

                    let idx = item.h1_feedbacks.findIndex(x => x.includes(name));

                    if (idx > -1) {
                        item.h1_self_feedback = item.h1_feedbacks[idx];
                        item.h1_self_feedback = item.h1_self_feedback.slice(item.h1_self_feedback.indexOf(':') + 2);
                        item.h1_feedbacks.splice(idx, 1);
                    }

                }

                if (item.h2_feedback) {

                    item.h2_feedbacks = item.h2_feedback.split('EOF');
                    item.h2_feedbacks.splice(item.h2_feedbacks.length - 1, 1);

                    let idx = item.h2_feedbacks.findIndex(x => x.includes(name));

                    if (idx > -1) {
                        item.h2_self_feedback = item.h2_feedbacks[idx];
                        item.h2_self_feedback = item.h2_self_feedback.slice(item.h2_self_feedback.indexOf(':') + 2);
                        item.h2_feedbacks.splice(idx, 1);
                    }

                }

            }
        }
    }

    const composeFeedbacks = () => {

        // integrate feedbacks 
        if ($scope.datum) {

            let name = $scope.auth[1];

            for (let item of $scope.datum.items) {

                item.h1_feedback = '';

                if (item.h1_self_feedback) {
                    item.h1_feedback += `${name}: ${item.h1_self_feedback}EOF`;
                }

                if (item.h1_feedbacks?.length)
                    for (let f of item.h1_feedbacks)
                        item.h1_feedback += `${f}EOF`;
            }

            for (let item of $scope.datum.items) {

                item.h2_feedback = '';

                if (item.h2_self_feedback) {
                    item.h2_feedback += `${name}: ${item.h2_self_feedback}EOF`;
                }

                if (item.h2_feedbacks?.length)
                    for (let f of item.h2_feedbacks)
                        item.h2_feedback += `${f}EOF`;
            }

        }


    }












    //$scope.selectedYear = $scope.years[0].toString();
    $scope.selectedYear = curYear.toString();
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
                //h1_manager_check: false,
                h1_reason: '',
                h2_employee_check: false,
                //h2_manager_check: false,
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

        composeFeedbacks();

        // Upsert and delete kpi items
        appService.UpdateKpiItems({ items: $scope.datum.items, removedItems: $scope.removedItems }).then((ret) => {



            if (ret.data) {
                $scope.datum.items = ret.data;
                destructFeedbacks();
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
        if ($scope.datum.items.length === 0) return 0;

        //console.log($scope.datum.items);

        let scores = $scope.datum.items.map(x => (x.consensual ? x.weight : 0));
        return scores.reduce((a, c) => (a + c)).toFixed(2);

        //return $scope.datum.items.reduce((a, c) => (a.consensual ? a.weight : 0) + (c.consensual ? c.weight : 0)).toFixed(2);
    }

    

}]);
