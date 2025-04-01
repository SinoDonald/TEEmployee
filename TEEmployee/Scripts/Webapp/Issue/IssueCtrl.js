var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

    this.GetAuth = (o) => {
        return $http.post('Issue/GetAuthorization', o);
    };
    this.GetAllProjects = (o) => {
        return $http.post('Issue/GetAllProjects', o);
    };
    this.GetProjectsByGroupOne = (o) => {
        return $http.post('Issue/GetProjectsByGroupOne', o);
    };
    this.CreateProject = (o) => {
        return $http.post('Issue/CreateProject', o);
    };
    this.CreateIssue = (o) => {
        return $http.post('Issue/CreateIssue', o);
    };
    this.UpdateIssue = (o) => {
        return $http.post('Issue/UpdateIssue', o);
    };
    this.CreateItem = (o) => {
        return $http.post('Issue/CreateControlledItem', o);
    };
    this.UpdateItem = (o) => {
        return $http.post('Issue/UpdateControlledItem', o);
    };
    
}]);

app.controller('IssueCtrl', ['$scope', '$location', 'appService', '$rootScope', function ($scope, $location, appService, $rootScope) {

    $scope.calendar = null;
    $scope.data = [];
    $scope.projectTypes = ['宣達事項/其他', '計畫執行', '技術深耕', '研發創新'];

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
                    issue.memberObjs = issue.members ? issue.members.split(',') : []; 
                }
            }

            // calendar event data
            let items = $scope.data
                .flatMap(project => project.issues)
                .flatMap(issue => issue.controlledItems);  

            $scope.events = items.map(x => ({
                title: x.todo,
                start: x.deadline,
                item: x,
            }))

            $scope.initCalendar($scope.events);
        })
    };

    $scope.filterProjectType = function (project_type) {
        return $scope.data.filter(function (project) {
            return project.project_type === project_type;
        });
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

        $scope.issueModalHeader = '新增議題';
        $scope.issueModal = { project_id: project.id };
        $scope.issueModal.groupMembers = $scope.groupMembers.map(x => ({
            name: x.name,
            selected: false,            
        }));
    };

    $scope.updateIssueModal = (issue) => {
        $scope.issueModalHeader = '編輯議題';
        $scope.issueModal = { ...issue };
        $scope.issueModal.groupMembers = $scope.groupMembers.map(x => ({
            name: x.name,
            selected: issue.members?.includes(x.name),
        }));
    };

    $scope.upsertIssue = () => {
        
        let newIssue = { ...$scope.issueModal };

        newIssue.members = newIssue.groupMembers.filter(x => x.selected).map(x => x.name).join(',');

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


    $scope.createItemModal = () => {

        $scope.isEditItemModal = false;
        $scope.itemModal = { issue_id: $scope.issueModal.id };
        
        $('#issueModal').modal('hide');
        $('#issueModal').one('hidden.bs.modal', function () {
            $('#itemModal').modal('show');
        });
    };

    $scope.updateItemModal = (item) => {

        $scope.isEditItemModal = true;

        $scope.itemModal = { ...item };
        
    };


    $scope.upsertItem = () => {

        let newItem = { ...$scope.itemModal };
        newItem.deadline = moment(newItem.dateObj).format('YYYY-MM-DD');

        //newIssue.members = newIssue.groupMembers.filter(x => x.selected).map(x => x.name).join(',');

        if (newItem.id) {

            appService.UpdateItem({ item: newItem }).then((ret) => {
                if (ret.data)
                    $scope.getProjectsByGroupOne();
            })

        } else {

            appService.CreateItem({ item: newItem }).then((ret) => {
                if (ret.data)
                    $scope.getProjectsByGroupOne();
            })
        }
    };


    $scope.selectGroup = () => {
        $scope.getProjectsByGroupOne();
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
                $scope.updateItemModal(info.event.extendedProps.item);
                // https://stackoverflow.com/questions/29544263/how-does-scope-apply-work-exactly-in-angularjs
                $scope.$apply(); // handle the event that is outside the angularjs event, tell it to refresh the front end
                $('#itemModal').modal('show');
            }
        });
        $scope.calendar.render();
               
    };

    $('#calendarModal').on('shown.bs.modal', function () {
        if ($scope.calendar) {
            $scope.calendar.updateSize();
        }
    });


    //function initCalendar() {

    //    var calendarEl = document.getElementById('calendar');
    //    var calendar = new FullCalendar.Calendar(calendarEl, {
    //        initialView: 'dayGridMonth',
    //        //events: events,
    //    });

    //    calendar.render();

    //    $('#calendarModal').on('shown.bs.modal', function () {
    //        calendar.updateSize(); // Recalculate layout
    //    });
    //}


}]);