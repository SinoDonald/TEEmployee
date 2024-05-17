var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllChapters = (o) => {
        return $http.post('GEducation/GetAllChapters', o);
    };

    //this.GetAllCourses = (o) => {
    //    return $http.post('Education/GetAllCourses', o);
    //};
    this.GetAllRecordsByUser = (o) => {
        return $http.post('GEducation/GetAllRecordsByUser', o);
    };
    this.GetAuth = (o) => {
        return $http.post('GEducation/GetAuthorization', o);
    };
    this.UpsertRecords = (o) => {
        return $http.post('GEducation/UpsertRecords', o);
    };
    this.UpdateRecordCompleted = (o) => {
        return $http.post('GEducation/UpdateRecordCompleted', o);
    };
    this.UpdateChapterDigitalized = (o) => {
        return $http.post('GEducation/UpdateChapterDigitalized', o);
    };
}]);


app.controller('GEducationCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {



    appService.GetAllChapters({}).then((ret) => {

        $scope.chapters = ret.data;

    })

    const formElem = document.querySelector('#formElem');

    // client to server
    if (formElem) {
        formElem.onsubmit = async (e) => {

            e.preventDefault();

            // don't add '/' before MVC controller
            let response = await fetch('GEducation/UploadCourseFile', {
                method: 'POST',
                body: new FormData(formElem)
            });

            let result = await response.json();

            alert(result);
        };
    }    

}]);

app.controller('AssignCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    $scope.assignOption = {
        value: 'assign',
    };

    $scope.groupCheckboxList = [
        { id: 1, value: '通識', checked: true },
        { id: 2, value: '規劃', checked: true },
        { id: 3, value: '專管', checked: true },
        { id: 4, value: '設計', checked: true },
    ];

    $scope.levelCheckboxList = [
        { id: 1, value: 'G通識', checked: true },
        { id: 2, value: 'A基礎', checked: true },
        { id: 3, value: 'B進階', checked: true },
        { id: 4, value: 'C案例', checked: true },
    ];

    $scope.updatelevelSelected = () => {
        $scope.levelSelected = $scope.levelCheckboxList.filter(x => x.checked === true).map(x => x.value);
    }

    $scope.updatelevelSelected();

    $scope.updateGroupSelected = () => {
        const groupSelected = $scope.groupCheckboxList.filter(x => x.checked === true).map(x => x.value);
        $scope.groupOneSelected = $scope.group_data.filter(x => groupSelected.includes(x.group)).map(x => x.group_one);
    }


    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        let group_array = $scope.auth.Users.map(x => x.group_one);
        $scope.groups = [...new Set(group_array)];
        //$scope.selectedGroup = "";

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

        if ($scope.users.children.length === 1 && $scope.users.children[0].children.length === 1)
            $scope.users.children[0].children[0].selected = true;

    })

    appService.GetAllChapters({}).then((ret) => {

        /*$scope.chapters = _.groupBy(ret.data, 'course_title');*/

        let group_one_array = ret.data.map(x => ({ group: x.course_group, group_one: x.course_group_one, }));

        // remove duplicate object
        group_one_array = group_one_array.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.group === value.group && t.group_one === value.group_one
            ))
        )
        $scope.group_data = group_one_array;
        $scope.updateGroupSelected();

        // First group by "foo"
        const groupedByGroupOne = _.groupBy(ret.data, 'course_group_one');

        // Second group by "bar" within each group of "foo"
        const groupedByChapterScope = _.mapValues(groupedByGroupOne, itemsFoo => {
            return _.mapValues(_.groupBy(itemsFoo, 'chapter_scope'), itemsBar => {
                // Third group by "baz" within each group of "bar"
                return _.groupBy(itemsBar, 'course_title');
            });
        });

        $scope.group_ones = groupedByChapterScope;
    })

    $scope.filterGroup = (key, value) => {
        return $scope.groupOneSelected.includes(key);
    }

    $scope.filterChapter = (chapter) => {
        return $scope.levelSelected.includes(chapter.chapter_type);
    }

    $scope.updateRecords = () => {

        // Retrieving all data selected
        const result = _.flatMapDeep($scope.group_ones, (itemsFoo, course_group_one) => {
            return _.flatMapDeep(itemsFoo, (itemsBar, chapter_scope) => {
                return _.flatMapDeep(itemsBar, (itemsBaz, course_title) => {
                    return _.filter(itemsBaz, { selected: true });
                });
            });
        });

        const selectedUsers = $scope.users.children
            .map(arr => arr.children.filter(x => x.selected))
            .flat();

        let records = [];

        let assigned = $scope.assignOption.value === 'assign';  

        console.log(assigned);

        for (let user of selectedUsers) {
            for (let chapter of result) {
                records.push({
                    chapter: chapter,
                    empno: user.empno,
                    assigned: assigned,
                })
            }
        }

        if (selectedUsers.length > 0 && result.length > 0) {
            appService.UpsertRecords({ records: records }).then((ret) => {

                if (ret.data)
                    alert('成功');
                else
                    alert('失敗');

            });
        }

        

    }

    //$scope.selectGroup = () => {

    //    let employee_array = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup).map(x => x.name);

    //    $scope.employees = employee_array;
    //    $scope.selectedEmployee = $scope.employees[0];
    //    $scope.selectEmployee();

    //}

    //$scope.selectEmployee = () => {

    //    $scope.records = structuredClone(
    //        $scope.auth.Users.find(x => x.name === $scope.selectedEmployee).records
    //    );

    //}


    //appService.GetAllCourses({}).then((ret) => {

    //    const groupData = (d) => {
    //        let g = Object.entries(d.reduce((r, c) => (r[c.course_group_one] = [...r[c.course_group_one] || [], c], r), {}))

    //        return g.reduce((r, c) => (r.children.push(
    //            { name: c[0], children: c[1] }), r), { name: "Children Array", children: [] })
    //    }

    //    $scope.data = groupData(ret.data);

    //})

    //$scope.clickModal = function (c) {
    //    $scope.modalCourses = structuredClone($scope.data);
    //    $scope.modalUsers = structuredClone($scope.users);
    //};

    //$scope.changeColor = function (c) {
    //    if (c.isButtonClicked) {
    //        c.isButtonClicked = false;
    //    }
    //    else {
    //        c.isButtonClicked = true;
    //    }
    //};

    //$scope.upsertRecords = () => {

    //    const selectedUsers = $scope.modalUsers.children
    //        .map(arr => arr.children.filter(x => x.isButtonClicked))
    //        .flat();

    //    const selectedCourses = $scope.modalCourses.children
    //        .map(arr => arr.children.filter(x => x.isButtonClicked))
    //        .flat();

    //    console.log(selectedUsers);
    //    console.log(selectedCourses);

    //    let records = [];

    //    for (let c of selectedCourses) {
    //        for (let u of selectedUsers) {

    //            let record = {
    //                empno: u.empno,
    //                course: { id: c.id },
    //                assigned: true,
    //            }

    //            records.push(record);
    //        }
    //    }

    //    appService.UpsertRecords({ records: records }).then((ret) => {


    //    })

    //}

}]);


app.controller('CurriculumCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    $scope.groupList = [
        { id: 0, value: '', name: '全部' },
        { id: 1, value: '通識', name: '通識' },
        { id: 2, value: '規劃', name: '規劃' },
        { id: 3, value: '專管', name: '專管' },
        { id: 4, value: '設計', name: '設計' },
    ];

    $scope.levelCheckboxList = [
        { id: 1, value: 'G通識', checked: true },
        { id: 2, value: 'A基礎', checked: true },
        { id: 3, value: 'B進階', checked: true },
        { id: 4, value: 'C案例', checked: true },
    ];

    $scope.updatelevelSelected = () => {
        $scope.levelSelected = $scope.levelCheckboxList.filter(x => x.checked === true).map(x => x.value);
    }

    $scope.updatelevelSelected();

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

        if ($scope.groups.length === 1) {
            $scope.selectedGroup = $scope.groups[0];
            $scope.selectGroup();
        }        

    })

    $scope.selectGroup = () => {

        let employee_array = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup).map(x => x.name);

        $scope.employees = employee_array;

        if ($scope.groups.length === 1) {
            $scope.selectedEmployee = $scope.employees.find(x => x === $scope.auth.User.name);
        }
        else {
            $scope.selectedEmployee = $scope.employees[0];            
        }
        $scope.selectEmployee();        

    }

    $scope.selectEmployee = () => {

        //$scope.records = structuredClone(
        //    $scope.auth.Users.find(x => x.name === $scope.selectedEmployee).records
        //);

        const empno = $scope.auth.Users.find(x => x.name === $scope.selectedEmployee).empno;

        $scope.getAllRecordsByUser(empno);


    }

    $scope.getAllRecordsByUser = (empno) => {

        appService.GetAllRecordsByUser({ empno: empno }).then((ret) => {

            $scope.records = ret.data.filter(x => x.assigned);

            // mapped to chapter
            const chapters = $scope.records.map(x => x.chapter);

            // First group by "course_group"
            const groupedByGroup = _.groupBy(chapters, 'course_group');

            // Second group by "course_group_one" 
            const groupedByChapterScope = _.mapValues(groupedByGroup, itemsGroup => {
                return _.mapValues(_.groupBy(itemsGroup, 'course_group_one'), itemsGroupOne => {
                    // Third group by "chapter_scope"
                    return _.groupBy(itemsGroupOne, 'chapter_scope');
                });
            });

            $scope.groupStructure = groupedByChapterScope;

            $scope.search = {
                chapter: {}
            };

            $scope.search.chapter.course_group = '';
            $scope.selectChapterGroup();

        })
    }


    $scope.updateRecordCompleted = (record, state) => {

        record.completed = state;

        appService.UpdateRecordCompleted({ record: record }).then((ret) => {
            record = ret.data;
        })

    }

    $scope.changeColor = function (c) {
        if (c.isButtonClicked) {
            c.isButtonClicked = false;
        }
        else {
            c.isButtonClicked = true;
        }
    };


    $scope.selectChapterGroup = () => {

        let chapterGroupOne_array = [];

        if ($scope.search.chapter.course_group && $scope.search.chapter.course_group in $scope.groupStructure)
            chapterGroupOne_array = Object.keys($scope.groupStructure[$scope.search.chapter.course_group]);


        $scope.search.chapter.course_group_one = '';
        $scope.search.chapter.chapter_scope = '';

        $scope.chapterGroupOnes = chapterGroupOne_array;
        //$scope.selectedEmployee = $scope.employees[0];
        //$scope.selectEmployee();

    }

    $scope.selectChapterGroupOne = () => {



        let chapterScope_array = [];

        if ($scope.search.chapter.course_group_one && $scope.search.chapter.course_group_one in $scope.groupStructure[$scope.search.chapter.course_group])
            chapterScope_array = Object.keys($scope.groupStructure[$scope.search.chapter.course_group][$scope.search.chapter.course_group_one]);

        $scope.search.chapter.chapter_scope = '';

        $scope.chapterScopes = chapterScope_array;
        //$scope.selectedEmployee = $scope.employees[0];
        //$scope.selectEmployee();

    }

    $scope.showUncompleted = (item) => {

        if ($scope.showUncompletedCheckbox)
            return item.completed === false;
        else
            return true;
    }

    $scope.filterLevel = (item) => {
        if ($scope.levelSelected.includes(item.chapter.chapter_type))
            return true;
        else
            return false;
    }

}]);


app.controller('DigitalCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    $scope.groupCheckboxList = [
        { id: 1, value: '通識', checked: true },
        { id: 2, value: '規劃', checked: true },
        { id: 3, value: '專管', checked: true },
        { id: 4, value: '設計', checked: true },
    ];

    $scope.levelCheckboxList = [
        { id: 1, value: 'G通識', checked: true },
        { id: 2, value: 'A基礎', checked: true },
        { id: 3, value: 'B進階', checked: true },
        { id: 4, value: 'C案例', checked: true },
    ];

    $scope.updatelevelSelected = () => {
        $scope.levelSelected = $scope.levelCheckboxList.filter(x => x.checked === true).map(x => x.value);
    }

    $scope.updatelevelSelected();

    $scope.updateGroupSelected = () => {
        const groupSelected = $scope.groupCheckboxList.filter(x => x.checked === true).map(x => x.value);
        $scope.groupOneSelected = $scope.group_data.filter(x => groupSelected.includes(x.group)).map(x => x.group_one);
    }


    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

    })

    appService.GetAllChapters({}).then((ret) => {


        let group_one_array = ret.data.map(x => ({ group: x.course_group, group_one: x.course_group_one, }));

        // remove duplicate object
        group_one_array = group_one_array.filter((value, index, self) =>
            index === self.findIndex((t) => (
                t.group === value.group && t.group_one === value.group_one
            ))
        )
        $scope.group_data = group_one_array;
        $scope.updateGroupSelected();

        // First group by "foo"
        const groupedByGroupOne = _.groupBy(ret.data, 'course_group_one');

        // Second group by "bar" within each group of "foo"
        const groupedByChapterScope = _.mapValues(groupedByGroupOne, itemsFoo => {
            return _.mapValues(_.groupBy(itemsFoo, 'chapter_scope'), itemsBar => {
                // Third group by "baz" within each group of "bar"
                return _.groupBy(itemsBar, 'course_title');
            });
        });

        $scope.group_ones = groupedByChapterScope;
    })

    $scope.filterGroup = (key, value) => {
        return $scope.groupOneSelected.includes(key);
    }

    $scope.filterChapter = (chapter) => {
        return $scope.levelSelected.includes(chapter.chapter_type);
    }

    $scope.showDigitalized = (chapter) => {

        if ($scope.showDigitalizedCheckbox)
            return chapter.digitalized === true;
        else
            return true;
    }


    $scope.updateChapterDigitalized = (chapter) => {

        console.log(chapter);

        //record.completed = state;

        appService.UpdateChapterDigitalized({ chapter: chapter }).then((ret) => {
            chapter = ret.data;
        })

    }

}]);