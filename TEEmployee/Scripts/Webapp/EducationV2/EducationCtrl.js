var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllContents = (o) => {
        return $http.post('EducationV2/GetAllContents', o);
    };
    this.GetAllValidContents = (o) => {
        return $http.post('EducationV2/GetAllValidContents', o);
    };
    this.GetAuth = (o) => {
        return $http.post('EducationV2/GetAuthorization', o);
    };
    this.UpsertAssignments = (o) => {
        return $http.post('EducationV2/UpsertAssignments', o);
    }; 
    this.DeleteAssignments = (o) => {
        return $http.post('EducationV2/DeleteAssignments', o);
    };
    this.GetAssignmentsByAssigner = (o) => {
        return $http.post('EducationV2/GetAssignmentsByAssigner', o);
    };
    this.GetAssignmentsWithRecord = (o) => {
        return $http.post('EducationV2/GetAssignmentsWithRecord', o);
    };
    
}]);

app.controller('EducationCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    appService.GetAllContents({}).then((ret) => {
        $scope.contents = ret.data;
    })

    const formElem = document.querySelector('#formElem');

    // client to server
    if (formElem) {
        formElem.onsubmit = async (e) => {

            e.preventDefault();

            // don't add '/' before MVC controller
            let response = await fetch('EducationV2/UploadCourseFile', {
                method: 'POST',
                body: new FormData(formElem)
            });

            let result = await response.json();

            alert(result);
        };
    }    

}]);

app.controller('AssignCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    $scope.step = 1;

    // users and group
    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        $scope.users = $scope.auth.Users.reduce((acc, user) => {
            const { group_one } = user; // destructing
            if (!acc[group_one]) {
                acc[group_one] = [];
            }
            acc[group_one].push(user);
            return acc;
        }, {});

        //let group_array = $scope.auth.Users.map(x => x.group_one);
        //$scope.groups = [...new Set(group_array)];
        ////$scope.selectedGroup = "";

        //const groupData = (d) => {
        //    let g = Object.entries(d.reduce((r, c) => (r[c.group_one] = [...r[c.group_one] || [], c], r), {}))

        //    return g.reduce((r, c) => (r.children.push(
        //        { name: c[0], children: c[1] }), r), { name: "Children Array", children: [] })
        //}

        //let users = $scope.auth.Users.sort(x => x.group_one).map(x => ({
        //    name: x.name,
        //    empno: x.empno,
        //    group_one: x.group_one,
        //}));

        //$scope.users = groupData(users);

        //if ($scope.users.children.length === 1 && $scope.users.children[0].children.length === 1)
        //    $scope.users.children[0].children[0].selected = true;

    })

    // contents
    appService.GetAllValidContents({}).then((ret) => {

        /*$scope.chapters = _.groupBy(ret.data, 'course_title');*/

        $scope.data = ret.data;

        // 2024 0805 new
        const uniqueGroups = [...new Set(ret.data.map(item => item.course_group))];
        $scope.groupCheckboxList = uniqueGroups.map((name, index) => ({
            id: index + 1,
            value: name,
            checked: true,
        }));

        const uniqueContentTypes = [...new Set(ret.data.map(item => item.content_type))];
        $scope.levelCheckboxList = uniqueContentTypes.map((name, index) => ({
            id: index + 1,
            value: name,
            checked: true,
        }));

        let group_one_array = ret.data.map(x => ({ group: x.course_group, group_one: x.course_group_one, }));

        // remove duplicate object
        group_one_array = group_one_array.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.group === value.group && t.group_one === value.group_one
            ))
        )
        $scope.group_data = group_one_array;
        //$scope.updateGroupSelected();
        //$scope.updatelevelSelected();

        // First group by "foo"
        const groupedByGroupOne = _.groupBy(ret.data, 'course_group_one');

        // Second group by "bar" within each group of "foo"
        const groupedByContentScope = _.mapValues(groupedByGroupOne, itemsFoo => {
            return _.mapValues(_.groupBy(itemsFoo, 'content_scope'), itemsBar => {
                // Third group by "baz" within each group of "bar"
                return _.groupBy(itemsBar, 'course_title');
            });
        });

        $scope.contents = groupedByContentScope;
    })

    $scope.updateRecords = () => {

        // Retrieving all data selected
        const selectedContents = _.flatMapDeep($scope.contents, itemsFoo => {
            return _.flatMapDeep(itemsFoo, itemsBar => {
                return _.flatMapDeep(itemsBar, itemsBaz => {
                    return _.filter(itemsBaz, { selected: true });
                });
            });
        });

        const selectedUsers = _.flatMapDeep($scope.users, itemsFoo => {
            return _.filter(itemsFoo, { selected: true });
        })

        
        let assignments = [];

        for (let user of selectedUsers) {
            for (let content of selectedContents) {
                assignments.push({
                    content_id: content.id,
                    empno: user.empno,
                    //assigned: true,
                })
            }
        }

        //console.log(assignments);

        if (assignments.length > 0) {
            appService.UpsertAssignments({ assignments: assignments }).then((ret) => {

                if (ret.data) {
                    alert('課程指派成功');
                    $window.location.href = 'EducationV2/AssignMenu';
                }
                    
                else
                    alert('課程指派失敗');

            });
        }

    }

    $scope.nextStep = (i) => {
        if ($scope.step == 1) {
            $scope.step++;
        }
        else if ($scope.step == 2) {
            $scope.updateRecords();
        }
    }

    $scope.previousStep = (i) => {
        if ($scope.step == 2) {
            $scope.step--;
        }
    }

}]);


app.controller('UnassignCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    $scope.step = 1;

    // users and group
    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        $scope.users = $scope.auth.Users.reduce((acc, user) => {
            const { group_one } = user; // destructing
            if (!acc[group_one]) {
                acc[group_one] = [];
            }
            acc[group_one].push(user);
            return acc;
        }, {});

        //let group_array = $scope.auth.Users.map(x => x.group_one);
        //$scope.groups = [...new Set(group_array)];
        ////$scope.selectedGroup = "";

        //const groupData = (d) => {
        //    let g = Object.entries(d.reduce((r, c) => (r[c.group_one] = [...r[c.group_one] || [], c], r), {}))

        //    return g.reduce((r, c) => (r.children.push(
        //        { name: c[0], children: c[1] }), r), { name: "Children Array", children: [] })
        //}

        //let users = $scope.auth.Users.sort(x => x.group_one).map(x => ({
        //    name: x.name,
        //    empno: x.empno,
        //    group_one: x.group_one,
        //}));

        //$scope.users = groupData(users);

        //if ($scope.users.children.length === 1 && $scope.users.children[0].children.length === 1)
        //    $scope.users.children[0].children[0].selected = true;

    })

    // contents
    appService.GetAllValidContents({}).then((ret) => {
        
        $scope.contents = ret.data;
    })

    $scope.deleteAssignments = () => {

        // Retrieving all assignment canceled (marked as false)       

        const cancelledAssignments = _.flatMapDeep($scope.lookup, (item, empno) =>
            _.flatMapDeep(item, (checked, content_id) =>
                !checked
                    ? [{ empno: empno, content_id: content_id }]
                    : []
            )
        );

        if (cancelledAssignments.length > 0) {

            appService.DeleteAssignments({ assignments: cancelledAssignments }).then((ret) => {

                if (ret.data) {
                    alert('取消課程成功');
                    $window.location.href = 'EducationV2/AssignMenu';
                }
                else
                    alert('取消課程失敗');

            });
        }

    }

    $scope.nextStep = (i) => {
        if ($scope.step == 1) {
            
            $scope.selectedUsers = _.flatMapDeep($scope.users, itemsFoo => {
                return _.filter(itemsFoo, { selected: true });
            });

            if ($scope.selectedUsers.length === 0) return;

            appService.GetAssignmentsByAssigner({ empnos: $scope.selectedUsers.map(x => x.empno)}).then((ret) => {
                $scope.assignments = ret.data;

                const contentSet = {};
                $scope.assignments.forEach(x => {
                    contentSet[x.content_id] = true;
                });

                $scope.contentList = $scope.contents.filter(c => contentSet[c.id]);

                $scope.buildLookup();

                $scope.step++;
            })
        }
        else if ($scope.step == 2) {
            $scope.deleteAssignments();
        }
    }

    $scope.previousStep = (i) => {
        if ($scope.step == 2) {
            $scope.step--;
        }
    }

    $scope.buildLookup = () => {
        // build a fast lookup: lookup[empno][conent_id] === true
        $scope.lookup = {};
        $scope.assignments.forEach(function (x) {
            $scope.lookup[x.empno] = $scope.lookup[x.empno] || {};
            $scope.lookup[x.empno][x.content_id] = true;
        });
    }

}]);

app.controller('RecordCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    $scope.step = 1;

    // users and group
    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        $scope.users = $scope.auth.Users.reduce((acc, user) => {
            const { group_one } = user; // destructing
            if (!acc[group_one]) {
                acc[group_one] = [];
            }
            acc[group_one].push(user);
            return acc;
        }, {});

        //let group_array = $scope.auth.Users.map(x => x.group_one);
        //$scope.groups = [...new Set(group_array)];
        ////$scope.selectedGroup = "";

        //const groupData = (d) => {
        //    let g = Object.entries(d.reduce((r, c) => (r[c.group_one] = [...r[c.group_one] || [], c], r), {}))

        //    return g.reduce((r, c) => (r.children.push(
        //        { name: c[0], children: c[1] }), r), { name: "Children Array", children: [] })
        //}

        //let users = $scope.auth.Users.sort(x => x.group_one).map(x => ({
        //    name: x.name,
        //    empno: x.empno,
        //    group_one: x.group_one,
        //}));

        //$scope.users = groupData(users);

        //if ($scope.users.children.length === 1 && $scope.users.children[0].children.length === 1)
        //    $scope.users.children[0].children[0].selected = true;

    })

    // contents
    appService.GetAllValidContents({}).then((ret) => {

        $scope.contents = ret.data;
    })

    $scope.nextStep = (i) => {

        if ($scope.step == 1) {

            $scope.selectedUsers = _.flatMapDeep($scope.users, itemsFoo => {
                return _.filter(itemsFoo, { selected: true });
            });

            if ($scope.selectedUsers.length === 0) return;

            appService.GetAssignmentsWithRecord({ empnos: $scope.selectedUsers.map(x => x.empno) }).then((ret) => {

                $scope.assignments = ret.data;

                const contentSet = {};
                $scope.assignments.forEach(x => {
                    contentSet[x.content_id] = true;
                });

                $scope.contentList = $scope.contents.filter(c => contentSet[c.id]);

                $scope.buildLookup();

                $scope.step++;
            })
        }
        
    }

    $scope.previousStep = (i) => {
        if ($scope.step == 2) {
            $scope.step--;
        }
    }

    $scope.buildLookup = () => {
        // build a fast lookup: lookup[empno][conent_id] === completed or not
        $scope.lookup = {};
        $scope.assignments.forEach((x) => {
            $scope.lookup[x.empno] = $scope.lookup[x.empno] || {};
            $scope.lookup[x.empno][x.content_id] = x.completed;
        });
    }

}]);