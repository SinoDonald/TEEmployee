var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

    this.GetAuth = (o) => {
        return $http.post('IssueV2/GetAuthorization', o);
    };
    this.GetAllProjects = (o) => {
        return $http.post('IssueV2/GetAllProjects', o);
    };
    this.GetProjectsByGroupOne = (o) => {
        return $http.post('IssueV2/GetProjectsByGroupOne', o);
    };
    this.CreateProject = (o) => {
        return $http.post('IssueV2/CreateProject', o);
    };
    this.DeleteProject = (o) => {
        return $http.post('IssueV2/DeleteProject', o);
    };
    this.CreateIssue = (o) => {
        return $http.post('IssueV2/CreateIssue', o);
    };
    this.UpdateIssue = (o) => {
        return $http.post('IssueV2/UpdateIssue', o);
    };
    this.DeleteIssue = (o) => {
        return $http.post('IssueV2/DeleteIssue', o);
    };
    this.GetCategoriesByGroupOne = (o) => {
        return $http.post('IssueV2/GetCategoriesByGroupOne', o);
    };
    this.CreateCategory = (o) => {
        return $http.post('IssueV2/CreateCategory', o);
    };
    this.DeleteCategory = (o) => {
        return $http.post('IssueV2/DeleteCategory', o);
    };

}]);

app.controller('IssueCtrl', ['$scope', '$location', 'appService', '$rootScope', function ($scope, $location, appService, $rootScope) {

    $scope.calendar = null;
    $scope.data = [];
    $scope.projectTypes = ['宣達事項/其他', '計畫執行', '技術深耕', '研發創新'];
    $scope.importanceOptions = ['1', '2', '3', '4', '5'];
    $scope.statusOptions = ['持續追蹤', '建議結案', '結案'];
    $scope.showClosedIssue = false;

    $scope.hdColors = ['#bf9000', '#e06666', '#6aa84f', '#3c78d8'];
    $scope.pjColors = ['#fff2cc', '#f4cccc', '#d9ead3', '#cfe2f3'];
    
    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;
        let group_array = $scope.auth.Users.map(x => x.group_one);
        $scope.groups = [...new Set(group_array)];
        $scope.selectedGroup = $scope.groups[0];
        $scope.selectGroup();
    });

    $scope.getAllProjects = () => {
        appService.GetAllProjects({}).then((ret) => {
            $scope.data = ret.data;
        })
    };

    $scope.getProjectsByGroupOne = () => {
        appService.GetProjectsByGroupOne({ group_one: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;

            // member string
            for (const project of $scope.data) {
                for (const issue of project.issues) {
                    let memberArr = issue.members ? issue.members.split(',') : [];
                    issue.memberObjs = memberArr.map(name => ({ name })) // shorthand for property name
                }
            }

            // calendar event data

            // callback function

            $scope.events = $scope.data.flatMap(project =>
                project.issues.map(x => ({
                    issue: x,
                    project_name: project.name,
                    title: `${project.name}-${x.category || ''}-${x.members || ''}<br />${x.content}`,
                    start: x.finished_date,
                }))
            );

            // chain 

            //let items = $scope.data
            //    .flatMap(project => project.issues)
            //    .flatMap(issue => issue.controlledItems);  

            //$scope.events = items.map(x => ({
            //    title: `${x.todo}\n${x.}`,
            //    start: x.deadline,
            //    item: x,
            //}))

            $scope.initCalendar($scope.events);
        })
    };

    $scope.getCategoriesByGroupOne = () => {
        appService.GetCategoriesByGroupOne({ group_one: $scope.selectedGroup }).then((ret) => {
            $scope.groupCategories = ret.data;
        })
    };

    $scope.filterProjectType = function (project_type) {
        return $scope.data.filter(function (project) {
            return project.project_type === project_type;
        });
    };

    $scope.excludeClosedIssue = function (item) {
        if ($scope.showClosedIssue)
            return true;
        else
            return item.status !== '結案';
    };

    $scope.createProjectModal = (idx) => {

        $scope.projectModal = {
            name: '',
            project_type: Number(idx),
            group_one: $scope.selectedGroup,
        };

    };

    $scope.createProject = () => {

        appService.CreateProject({ project: $scope.projectModal }).then((ret) => {
            if (ret.data)
                $scope.getProjectsByGroupOne();
        })
    };

    $scope.createIssueModal = (project) => {

        $scope.isEditIssueModal = false;
        $scope.issueModal = { project_id: project.id };
        $scope.issueModal.groupMembers = $scope.groupMembers.map(x => ({
            name: x.name,
            selected: false,
        }));
    };

    $scope.updateIssueModal = (issue) => {

        $scope.isEditIssueModal = true;
        $scope.issueModal = { ...issue };
        $scope.issueModal.groupMembers = $scope.groupMembers.map(x => ({
            name: x.name,
            selected: issue.members?.includes(x.name),
        }));
        $scope.issueModal.dateObj = new Date($scope.issueModal.finished_date);
        $scope.issueModal.importanceStr = $scope.issueModal.importance.toString();
    };

    $scope.upsertIssue = () => {

        let newIssue = { ...$scope.issueModal };

        newIssue.members = newIssue.groupMembers.filter(x => x.selected).map(x => x.name).join(',');
        newIssue.importance = Number($scope.issueModal.importanceStr);
        newIssue.finished_date = moment($scope.issueModal.dateObj).format('YYYY-MM-DD');

        if (newIssue.id) {

            appService.UpdateIssue({ issue: newIssue }).then((ret) => {
                if (ret.data)
                    $scope.getProjectsByGroupOne();
            })

        } else {

            newIssue.registered_date = moment().format('YYYY-MM-DD');

            appService.CreateIssue({ issue: newIssue }).then((ret) => {
                if (ret.data)
                    $scope.getProjectsByGroupOne();
            })
        }
    };


    $scope.confirmDeleteProject = (project) => {

        var result = confirm("確定要刪除這筆計畫(及相關議題)嗎？");

        if (result) {
            appService.DeleteProject({ project: project }).then((ret) => {

                if (ret.data) {
                    $scope.getProjectsByGroupOne();
                }
            })
        }
    }


    $scope.confirmDeleteIssue = () => {

        var result = confirm("確定要刪除這筆議題嗎？");

        if (result) {
            appService.DeleteIssue({ issue: $scope.issueModal }).then((ret) => {

                if (ret.data) {
                    $scope.getProjectsByGroupOne();
                    $('#issueModal').modal('hide');
                }
            })
        }
    }


    $scope.selectGroup = () => {
        $scope.getProjectsByGroupOne();
        $scope.getCategoriesByGroupOne();
        $scope.groupMembers = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup);
    }


    $scope.initCalendar = function (events) {

        if ($scope.calendar) {
            $scope.calendar.destroy();
        }

        $scope.calendar = new FullCalendar.Calendar(document.getElementById('calendar'), {
            initialView: 'dayGridMonth',
            events: events,
            eventClick: function (info) {
                //alert('Event: ' + info.event.extendedProps.dog);
                $scope.updateIssueModal(info.event.extendedProps.issue);
                // https://stackoverflow.com/questions/29544263/how-does-scope-apply-work-exactly-in-angularjs
                $scope.$apply(); // handle the event that is outside the angularjs event, tell it to refresh the front end
                $('#issueModal').modal('show');
            },
            eventContent: function (info) {
                return {
                    html: info.event.title
                };
            }
            //eventDidMount: function (info) {
            //    info.el.querySelector('.fc-event-title').innerHTML = info.event.title;
            //}
        });
        $scope.calendar.render();

    };

    $('#calendarModal').on('shown.bs.modal', function () {
        if ($scope.calendar) {
            $scope.calendar.updateSize();
        }
    });

    // category

    $scope.createCategoryModal = () => {
        $scope.newCategoryName = '';
    }

    $scope.createCategory = () => {

        if (!$scope.newCategoryName) return;
        if ($scope.groupCategories.some(x => x.name === $scope.newCategoryName)) return;

        let newCategory = {
            name: $scope.newCategoryName,
            group_one: $scope.selectedGroup,
        };

        appService.CreateCategory({ category: newCategory }).then((ret) => {

            if (ret.data) {
                $scope.getCategoriesByGroupOne();
                $scope.newCategoryName = '';
            }
        });

    }


    $scope.confirmDeleteCategory = (category) => {

        var result = confirm("確定要刪除這項自訂分類嗎");

        if (result) {
            appService.DeleteCategory({ category: category }).then((ret) => {

                if (ret.data) {
                    $scope.getCategoriesByGroupOne();
                }
            })
        }
    }

}]);