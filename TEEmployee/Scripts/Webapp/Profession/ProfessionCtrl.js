var app = angular.module('app', ['ui.router']);

app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {

    $stateProvider
        .state('Skill', {
            url: '/Skill',
            templateUrl: 'Profession/Skill'
        })
        .state('Score', {
            url: '/Score',
            templateUrl: 'Profession/Score'
        })
        .state('Chart', {
            url: '/Chart',
            templateUrl: 'Profession/Chart'
        })
        .state('Scatter', {
            url: '/Scatter',
            templateUrl: 'Profession/Scatter'
        })
        .state('Personal', {
            url: '/Personal',
            templateUrl: 'Profession/Personal'
        })
}]);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAllSkillsByRole = (o) => {
        return $http.post('Profession/GetAllSkillsByRole', o);
    };
    this.GetAuthorization = (o) => {
        return $http.post('Profession/GetAuthorization', o);
    };
    this.UpsertSkills = (o) => {
        return $http.post('Profession/UpsertSkills', o);
    };
    this.DeleteSkills = (o) => {
        return $http.post('Profession/DeleteSkills', o);
    };
    this.GetAllScoresByRole = (o) => {
        return $http.post('Profession/GetAllScoresByRole', o);
    };
    this.UpsertScores = (o) => {
        return $http.post('Profession/UpsertScores', o);
    };
    this.GetPersonal = (o) => {
        return $http.post('Profession/GetPersonal', o);
    };
    this.UpsertPersonal = (o) => {
        return $http.post('Profession/UpsertPersonal', o);
    };
    this.DeletePersonal = (o) => {
        return $http.post('Profession/DeletePersonal', o);
    };
}]);

app.factory('dataservice', function () {

    var auth = {};
    let temp_group = '';

    return {
        set: set,
        get: get,
        setGroup: setGroup,
        getGroup: getGroup,
    }

    function set(data) {
        auth = data;
    }

    function get() {
        return auth;
    }

    function setGroup(group) {
        temp_group = group;
    }

    function getGroup() {
        return temp_group;
    }
});


app.controller('ProfessionCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    //appService.GetAllSkills({}).then((ret) => {
    //    dataservice.set(ret.data);
    //    $scope.data = dataservice.get();
    //})

    //appService.GetAuthorization({}).then((ret) => {
    //    dataservice.set(ret.data);
    //})

    $scope.isActive = (destination) => {
        return destination === $location.path();
    }

    dataservice.set(appService.GetAuthorization({}));
    $location.path('/Skill');

}]);

app.controller('SkillCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    document.documentElement.scrollTop = 0;
    //document.body.style.overflow = "hidden";

    $scope.GetAllSkillsByRole = () => {

        dataservice.setGroup($scope.selectedGroup);

        $scope.editable = $scope.auth.GroupAuthorities.find(x => x.GroupName === $scope.selectedGroup).Editable;

        appService.GetAllSkillsByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.domain = ret.data.filter(x => x.skill_type === 'domain').sort((a, b) => a.custom_order - b.custom_order);
            $scope.core = ret.data.filter(x => x.skill_type === 'core').sort((a, b) => a.custom_order - b.custom_order);
            $scope.manage = ret.data.filter(x => x.skill_type === 'manage').sort((a, b) => a.custom_order - b.custom_order);
        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        if (dataservice.getGroup()) {
            $scope.selectedGroup = dataservice.getGroup()
        }
        else {
            $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        }
        //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllSkillsByRole();
    });




    // deep clone skills object to modal
    $scope.deletedSkills = [];
    $scope.modal = {};
    $scope.cloneDeepToModal = (skills, skill_type) => {

        $scope.modal.data = structuredClone(skills);
        $scope.modal.skill_type = skill_type;
        $scope.deletedSkills = [];
    };

    $scope.addSkill = (newSkill) => {

        if (!newSkill) return;

        $scope.modal.data.push({
            id: 0,
            content: newSkill,
            role: ($scope.modal.skill_type === 'core' || $scope.modal.skill_type === "manage") ? 'shared' : $scope.selectedGroup,
            skill_type: $scope.modal.skill_type,
        })

        // clear input
        $scope.newSkill = "";
    }

    $scope.deleteSkill = (idx) => {

        if ($scope.modal.data[idx].id !== 0) {
            $scope.deletedSkills.push($scope.modal.data[idx]);
        }

        $scope.modal.data.splice(idx, 1);
    }

    $scope.moveUp = (idx) => {
        if (idx === 0) return;
        [$scope.modal.data[idx], $scope.modal.data[idx - 1]] = [$scope.modal.data[idx - 1], $scope.modal.data[idx]];
    }

    $scope.moveDown = (idx) => {
        if (idx === $scope.modal.data.length - 1) return;
        [$scope.modal.data[idx], $scope.modal.data[idx + 1]] = [$scope.modal.data[idx + 1], $scope.modal.data[idx]];
    }

    $scope.saveChanges = () => {

        // reassign the customOrder
        for (let i = 0; i !== $scope.modal.data.length; i++) {
            $scope.modal.data[i].custom_order = i + 1;
        }

        let promiseA = Promise.resolve('a');
        let promiseB = Promise.resolve('b');

        if ($scope.deletedSkills.length !== 0) {
            promiseA = appService.DeleteSkills({ skills: $scope.deletedSkills })
        }

        if ($scope.modal.data.length !== 0) {
            promiseB = appService.UpsertSkills({ skills: $scope.modal.data });
        }

        $q.all([promiseA, promiseB]).then((ret) => {

            // easy peasy
            $scope.GetAllSkillsByRole();

            //if (!ret[1].data) return;

            //if ($scope.modal.skill_type === 'hard') {
            //    if ($scope.modal.skill_time === 'now')
            //        $scope.nowHard = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
            //    else
            //        $scope.futureHard = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
            //}
            //else {
            //    if ($scope.modal.skill_time === 'now')
            //        $scope.nowSoft = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
            //    else
            //        $scope.futureSoft = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
            //}

        });


        //// delete skills
        //appService.DeleteSkills({ skills: $scope.deletedSkills }).then((ret) => {
        //    if (ret.data) {
        //        $scope.deletedSkills = [];
        //    }
        //});        

        //// upsert skills
        //appService.UpsertSkills({ skills: $scope.modal.data }).then((ret) => {

        //    if (!ret.data) return;

        //    if ($scope.modal.skill_type === 'hard') {
        //        if ($scope.modal.skill_time === 'now')
        //            $scope.nowHard = ret.data.sort((a, b) => a.custom_order - b.custom_order);
        //        else
        //            $scope.futureHard = ret.data.sort((a, b) => a.custom_order - b.custom_order);
        //    }
        //    else {
        //        if ($scope.modal.skill_time === 'now')
        //            $scope.nowSoft = ret.data.sort((a, b) => a.custom_order - b.custom_order);
        //        else
        //            $scope.futureSoft = ret.data.sort((a, b) => a.custom_order - b.custom_order);
        //    }
        //});
    }

}]);


app.controller('ScoreCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$state', function ($scope, $location, appService, $rootScope, $q, dataservice, $state) {

    document.documentElement.scrollTop = 0;

    $(function () {
        $('[data-toggle="popover"]').popover()
    })

    $scope.GetAllScoresByRole = () => {

        dataservice.setGroup($scope.selectedGroup);

        $scope.members = $scope.auth.GroupAuthorities
            .find(x => x.GroupName === $scope.selectedGroup)
            .Members.sort((a, b) => a.empno - b.empno);

        appService.GetAllScoresByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;
            $scope.domain = [];
            $scope.core = [];
            $scope.manage = [];

            // Create data: fill the blank space of table

            for (let skill of $scope.data) {

                for (let member of $scope.members) {

                    if (!skill.scores.find(x => x.empno === member.empno)) {

                        skill.scores.push({
                            skill_id: skill.id,
                            empno: member.empno,
                            score: '',
                        })
                    }
                }

                skill.scores.sort((a, b) => a.empno - b.empno);

                if (skill.skill_type === 'domain')
                    $scope.domain.push(skill);
                else if (skill.skill_type === 'core')
                    $scope.core.push(skill);
                else
                    $scope.manage.push(skill);
            }


        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        $scope.editableGroup = $scope.auth.GroupAuthorities.filter(x => x.Editable);

        if (dataservice.getGroup() && $scope.editableGroup.find(x => x.GroupName === dataservice.getGroup())) {
            $scope.selectedGroup = dataservice.getGroup()
        }
        else {
            $scope.selectedGroup = $scope.editableGroup[0].GroupName;
        }
        //$scope.selectedGroup = $scope.editableGroup[0].GroupName;

        //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllScoresByRole();
    });

    $scope.IamInvalid = true;

    $scope.isInvalidManageCoreScore = (score) => {

        if (!score)
            return false;

        let num = Number(score);

        if (isNaN(num)) {
            return true;
        }


        if (Number.isInteger(num) && num >= 1 && num <= 5) {
            return false;
        }
        else {
            return true;
        }

    }

    $scope.isInvalidDomainScore = (score) => {

        if (!score)
            return false;

        let num = Number(score);

        if (isNaN(num)) {
            return true;
        }


        if (Number.isInteger(num) && num >= 0 && num <= 5) {
            return false;
        }
        else {
            return true;
        }

    }

    $scope.upsertScores = () => {

        let savedScores = [];
        const aniBtn = document.querySelector(".animate-btn");


        for (let skill of $scope.data) {
            for (let emp of skill.scores) {

                if (!emp.score)
                    continue;

                let num = Number(emp.score);

                if (isNaN(num))
                    continue;

                if (skill.skill_type === 'domain') {
                    if (Number.isInteger(num) && num >= 0 && num <= 5) {
                        emp.score = num;
                        savedScores.push(emp);
                    }
                }
                else {
                    if (Number.isInteger(num) && num >= 1 && num <= 5) {
                        emp.score = num;
                        savedScores.push(emp);
                    }
                }

                
            }
        }



        if (savedScores.length === 0) return;

        // start animation
        //aniBtn.classList.add('onclic');

        appService.UpsertScores({ scores: savedScores }).then((ret) => {

            if (ret.data) {
                /*aniBtn.classList.remove('onclic');*/
                aniBtn.classList.add('onclic');
                /*aniBtn.classList.add('validate');*/

                $scope.GetAllScoresByRole();
                /*$state.reload();*/

                //setTimeout(function () {
                //    aniBtn.classList.remove('validate');
                //}, 2500);

                setTimeout(function () {
                    aniBtn.classList.remove('onclic');
                    aniBtn.classList.add('validate');
                    setTimeout(function () {
                        aniBtn.classList.remove('validate');
                    }, 1500);
                }, 1500);


            }

        });

    }

}]);


app.controller('ChartCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$state', function ($scope, $location, appService, $rootScope, $q, dataservice, $state) {

    document.documentElement.scrollTop = 0;

    $scope.selectedType = 'domain';

    $scope.GetAllScoresByRole = () => {

        dataservice.setGroup($scope.selectedGroup);

        $scope.members = $scope.auth.GroupAuthorities
            .find(x => x.GroupName === $scope.selectedGroup)
            .Members.sort((a, b) => a.empno - b.empno);

        appService.GetAllScoresByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;

            $scope.filterChart($scope.selectedType);
        });
    }

    $scope.filterChart = (skill_type) => {
        $scope.selectedType = skill_type;
        createScoreBarChart($scope.data.filter(x => x.skill_type === skill_type));
    }


    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        $scope.editableGroup = $scope.auth.GroupAuthorities.filter(x => x.Editable);

        if (dataservice.getGroup() && $scope.editableGroup.find(x => x.GroupName === dataservice.getGroup())) {
            $scope.selectedGroup = dataservice.getGroup()
        }
        else {
            $scope.selectedGroup = $scope.editableGroup[0].GroupName;
        }

        //$scope.selectedGroup = $scope.editableGroup[0].GroupName;
        //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllScoresByRole();


    });

}]);

app.controller('ScatterCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$state', function ($scope, $location, appService, $rootScope, $q, dataservice, $state) {

    document.documentElement.scrollTop = 0;

    $scope.GetAllScoresByRole = () => {

        dataservice.setGroup($scope.selectedGroup);

        $scope.members = $scope.auth.GroupAuthorities
            .find(x => x.GroupName === $scope.selectedGroup)
            .Members.sort((a, b) => a.empno - b.empno);

        appService.GetAllScoresByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;
            createScoreScatterChart($scope.data, $scope.members);
        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        $scope.editableGroup = $scope.auth.GroupAuthorities.filter(x => x.Editable);

        if (dataservice.getGroup() && $scope.editableGroup.find(x => x.GroupName === dataservice.getGroup())) {
            $scope.selectedGroup = dataservice.getGroup()
        }
        else {
            $scope.selectedGroup = $scope.editableGroup[0].GroupName;
        }
        //$scope.selectedGroup = $scope.editableGroup[0].GroupName;
        //$scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllScoresByRole();

    });

}]);

app.controller('PersonalCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    document.documentElement.scrollTop = 0;

    $(function () {
        $('[data-toggle="popover"]').popover()
    })

    let data;

    //$scope.items = [{
    //    label: '0分 「非所需技能」',
    //    value: 0
    //}, {
    //    label: '1分「安排學習技能」',
    //    value: 1
    //}, {
    //    label: '2分「學習中」',
    //    value: 2
    //}, {
    //    label: '3分「可獨立作業」',
    //    value: 3
    //}, {
    //    label: '4分「精通且可指導他人」',
    //    value: 4
    //}, {
    //    label: '5分「專家，從事專業達五年以上',
    //    value: 5
    //},];
    $scope.items = [];

    const DomainItems = [{
        label: '0分 「非所需技能」',
        value: 0
    }, {
        label: '1分「安排學習技能」',
        value: 1
    }, {
        label: '2分「學習中」',
        value: 2
    }, {
        label: '3分「可獨立作業」',
        value: 3
    }, {
        label: '4分「精通且可指導他人」',
        value: 4
    }, {
        label: '5分「專家，從事專業達五年以上」',
        value: 5
    },];

    const ManageCoreItems = [{
        label: '1分 「非常不熟悉」',
        value: 1
    }, {
        label: '2分「不熟悉」',
        value: 2
    }, {
        label: '3分「普通」',
        value: 3
    }, {
        label: '4分「熟悉」',
        value: 4
    }, {
        label: '5分「非常熟悉」',
        value: 5
    },];



    $scope.GetAllSkillsByRole = () => {

        dataservice.setGroup($scope.selectedGroup);

        $scope.selectGroup();

        appService.GetAllSkillsByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;
            data = ret.data;

            $scope.domain = ret.data.filter(x => x.skill_type === 'domain').sort((a, b) => a.custom_order - b.custom_order);
            $scope.core = ret.data.filter(x => x.skill_type === 'core').sort((a, b) => a.custom_order - b.custom_order);
            $scope.manage = ret.data.filter(x => x.skill_type === 'manage').sort((a, b) => a.custom_order - b.custom_order);

            // new place
            $scope.GetPersonal($scope.selectedMember);
        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        if (dataservice.getGroup()) {
            $scope.selectedGroup = dataservice.getGroup()
        }
        else {
            $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        }

        $scope.GetAllSkillsByRole();
    });

    $scope.GetPersonal = (empno) => {

        $scope.editable = ($scope.auth.User.empno === $scope.selectedMember);

        // version 1: Add personal skill yourself

        //appService.GetPersonal({ empno: empno }).then((ret) => {

        //    $scope.personal = ret.data;

        //    $scope.personalDomain = ret.data.filter(x => x.skill_type === 'domain' && x.role === $scope.selectedGroup).sort((a, b) => a.custom_order - b.custom_order);
        //    $scope.personalCore = ret.data.filter(x => x.skill_type === 'core').sort((a, b) => a.custom_order - b.custom_order);
        //    $scope.personalManage = ret.data.filter(x => x.skill_type === 'manage').sort((a, b) => a.custom_order - b.custom_order);

        //});

        // version 2: Add ALL by default

        appService.GetPersonal({ empno: empno }).then((ret) => {

            $scope.personal = ret.data;

            for (let datum of $scope.data) {

                if ($scope.personal.find(x => x.skill_id === datum.id)) {
                    
                    continue;
                }

                $scope.personal.push({
                    comment: '',
                    content: datum.content,
                    custom_order: datum.custom_order,
                    empno: $scope.auth.User.empno,
                    role: datum.role,
                    score: 0,
                    skill_id: datum.id,
                    skill_type: datum.skill_type,
                })

            }

            //$scope.personalDomain = ret.data.filter(x => x.skill_type === 'domain' && x.role === $scope.selectedGroup).sort((a, b) => a.custom_order - b.custom_order);
            //$scope.personalCore = ret.data.filter(x => x.skill_type === 'core').sort((a, b) => a.custom_order - b.custom_order);
            //$scope.personalManage = ret.data.filter(x => x.skill_type === 'manage').sort((a, b) => a.custom_order - b.custom_order);

            $scope.personalDomain = $scope.personal.filter(x => x.skill_type === 'domain' && x.role === $scope.selectedGroup).sort((a, b) => a.custom_order - b.custom_order);
            $scope.personalCore = $scope.personal.filter(x => x.skill_type === 'core').sort((a, b) => a.custom_order - b.custom_order);
            $scope.personalManage = $scope.personal.filter(x => x.skill_type === 'manage').sort((a, b) => a.custom_order - b.custom_order);

        });

    }

    $scope.selectGroup = () => {
        $scope.members = $scope.auth.GroupAuthorities.find(x => x.GroupName === $scope.selectedGroup).Members;
        $scope.selectedMember = $scope.members[0].empno;
        // old place
        //$scope.GetPersonal($scope.selectedMember);
    }



    // deep clone skills object to modal
    $scope.deletedSkills = [];
    $scope.modal = {};
    $scope.cloneDeepToModal = (skills, skill_type, skillSet) => {

        $scope.newSkill = '';
        $scope.modal.data = structuredClone(skills);
        $scope.modal.skill_type = skill_type;
        $scope.deletedSkills = [];
        let skillids = skills.map(x => x.skill_id);
        $scope.skillSet = skillSet;

        if (skill_type === 'domain')
            $scope.items = DomainItems;
        else
            $scope.items = ManageCoreItems;

        //$scope.skillSet = skillSet.filter(x => !skillids.includes(x.id));
    };

    $scope.isInSkillSet = (id) => {
        if ($scope.modal.data.find(x => x.skill_id === id)) return true;
        return false;
    }

    $scope.addSkill = (newSkill) => {

        if (!newSkill) return;

        let cloneSkill = structuredClone(data.find(x => x.id === Number(newSkill)));
        cloneSkill.skill_id = cloneSkill.id;
        cloneSkill.empno = $scope.selectedMember;

        $scope.modal.data.push(cloneSkill);

        /*$scope.skillSet.splice($scope.skillSet.findIndex(x => x.id === Number(newSkill)), 1);*/

        // clear input
        $scope.newSkill = '';
    }

    $scope.deleteSkill = (idx) => {

        if ($scope.modal.data[idx].id !== 0) {
            $scope.deletedSkills.push($scope.modal.data[idx]);
        }

        $scope.modal.data.splice(idx, 1);
    }

    //$scope.moveUp = (idx) => {
    //    if (idx === 0) return;
    //    [$scope.modal.data[idx], $scope.modal.data[idx - 1]] = [$scope.modal.data[idx - 1], $scope.modal.data[idx]];
    //}

    //$scope.moveDown = (idx) => {
    //    if (idx === $scope.modal.data.length - 1) return;
    //    [$scope.modal.data[idx], $scope.modal.data[idx + 1]] = [$scope.modal.data[idx + 1], $scope.modal.data[idx]];
    //}

    $scope.saveChanges = () => {

        // reassign the customOrder
        //for (let i = 0; i !== $scope.modal.data.length; i++) {
        //    $scope.modal.data[i].custom_order = i + 1;
        //}

        let promiseA = Promise.resolve('a');
        let promiseB = Promise.resolve('b');

        if ($scope.deletedSkills.length !== 0) {
            promiseA = appService.DeletePersonal({ personals: $scope.deletedSkills })
        }

        if ($scope.modal.data.length !== 0) {
            promiseB = appService.UpsertPersonal({ personals: $scope.modal.data });
        }

        $q.all([promiseA, promiseB]).then((ret) => {

            // easy peasy
            $scope.GetPersonal($scope.selectedMember);

        });

    }

}]);



//app.controller('SkillCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

//    document.documentElement.scrollTop = 0;
//    //document.body.style.overflow = "hidden";

//    $scope.GetAllSkillsByRole = () => {

//        appService.GetAllSkillsByRole({ role: $scope.selectedGroup }).then((ret) => {

//            //$scope.data = ret.data;

//            $scope.nowHard = ret.data.filter(x => x.skill_type === 'hard' && x.skill_time === 'now').sort((a, b) => a.custom_order - b.custom_order);
//            $scope.nowSoft = ret.data.filter(x => x.skill_type === 'soft' && x.skill_time === 'now').sort((a, b) => a.custom_order - b.custom_order);
//            $scope.futureHard = ret.data.filter(x => x.skill_type === 'hard' && x.skill_time === 'future').sort((a, b) => a.custom_order - b.custom_order);
//            $scope.futureSoft = ret.data.filter(x => x.skill_type === 'soft' && x.skill_time === 'future').sort((a, b) => a.custom_order - b.custom_order);

//        });
//    }

//    dataservice.get().then((ret) => {

//        $scope.auth = ret.data;

//        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
//        $scope.GetAllSkillsByRole();
//    });


//    // deep clone skills object to modal
//    $scope.deletedSkills = [];
//    $scope.modal = {};
//    $scope.cloneDeepToModal = (skills, skill_time, skill_type) => {

//        $scope.modal.data = structuredClone(skills);
//        $scope.modal.skill_time = skill_time;
//        $scope.modal.skill_type = skill_type;
//        $scope.deletedSkills = [];

//        //$scope.modal.origin = schedule;
//        //$scope.modal.originidx = idx;
//        //$scope.modal.createMode = false;

//    };

//    $scope.addSkill = (newSkill) => {

//        if (!newSkill) return;

//        $scope.modal.data.push({
//            id: 0,
//            content: newSkill,
//            role: $scope.selectedGroup,
//            skill_time: $scope.modal.skill_time,
//            skill_type: $scope.modal.skill_type,
//        })

//        // clear input
//        $scope.newSkill = "";
//    }

//    $scope.deleteSkill = (idx) => {

//        if ($scope.modal.data[idx].id !== 0) {
//            $scope.deletedSkills.push($scope.modal.data[idx]);
//        }

//        $scope.modal.data.splice(idx, 1);
//    }

//    $scope.moveUp = (idx) => {
//        if (idx === 0) return;
//        [$scope.modal.data[idx], $scope.modal.data[idx - 1]] = [$scope.modal.data[idx - 1], $scope.modal.data[idx]];
//    }

//    $scope.moveDown = (idx) => {
//        if (idx === $scope.modal.data.length - 1) return;
//        [$scope.modal.data[idx], $scope.modal.data[idx + 1]] = [$scope.modal.data[idx + 1], $scope.modal.data[idx]];
//    }

//    $scope.saveChanges = () => {

//        // reassign the customOrder
//        for (let i = 0; i !== $scope.modal.data.length; i++) {
//            $scope.modal.data[i].custom_order = i + 1;
//        }

//        let promiseA = Promise.resolve('a');
//        let promiseB = Promise.resolve('b');

//        if ($scope.deletedSkills.length !== 0) {
//            promiseA = appService.DeleteSkills({ skills: $scope.deletedSkills })
//        }

//        if ($scope.modal.data.length !== 0) {
//            promiseB = appService.UpsertSkills({ skills: $scope.modal.data });
//        }

//        $q.all([promiseA, promiseB]).then((ret) => {

//            // easy peasy
//            $scope.GetAllSkillsByRole();

//            //if (!ret[1].data) return;

//            //if ($scope.modal.skill_type === 'hard') {
//            //    if ($scope.modal.skill_time === 'now')
//            //        $scope.nowHard = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
//            //    else
//            //        $scope.futureHard = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
//            //}
//            //else {
//            //    if ($scope.modal.skill_time === 'now')
//            //        $scope.nowSoft = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
//            //    else
//            //        $scope.futureSoft = ret[1].data.sort((a, b) => a.custom_order - b.custom_order);
//            //}

//        });


//        //// delete skills
//        //appService.DeleteSkills({ skills: $scope.deletedSkills }).then((ret) => {
//        //    if (ret.data) {
//        //        $scope.deletedSkills = [];
//        //    }
//        //});        

//        //// upsert skills
//        //appService.UpsertSkills({ skills: $scope.modal.data }).then((ret) => {

//        //    if (!ret.data) return;

//        //    if ($scope.modal.skill_type === 'hard') {
//        //        if ($scope.modal.skill_time === 'now')
//        //            $scope.nowHard = ret.data.sort((a, b) => a.custom_order - b.custom_order);
//        //        else
//        //            $scope.futureHard = ret.data.sort((a, b) => a.custom_order - b.custom_order);
//        //    }
//        //    else {
//        //        if ($scope.modal.skill_time === 'now')
//        //            $scope.nowSoft = ret.data.sort((a, b) => a.custom_order - b.custom_order);
//        //        else
//        //            $scope.futureSoft = ret.data.sort((a, b) => a.custom_order - b.custom_order);
//        //    }
//        //});
//    }

//}]);

//app.controller('ScoreCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', '$state', function ($scope, $location, appService, $rootScope, $q, dataservice, $state) {

//    document.documentElement.scrollTop = 0;

//    $(function () {
//        $('[data-toggle="popover"]').popover()
//    })

//    $scope.GetAllScoresByRole = () => {

//        $scope.members = $scope.auth.GroupAuthorities
//            .find(x => x.GroupName === $scope.selectedGroup)
//            .Members.sort((a, b) => a.empno - b.empno);

//        appService.GetAllScoresByRole({ role: $scope.selectedGroup }).then((ret) => {

//            $scope.data = ret.data;
//            $scope.hard = [];
//            $scope.soft = [];

//            // Create data: fill the blank space of table

//            for (let skill of $scope.data) {

//                for (let member of $scope.members) {

//                    if (!skill.scores.find(x => x.empno === member.empno)) {

//                        skill.scores.push({
//                            skill_id: skill.id,
//                            empno: member.empno,
//                            score: '',
//                        })
//                    }
//                }

//                skill.scores.sort((a, b) => a.empno - b.empno);

//                if (skill.skill_type === 'hard')
//                    $scope.hard.push(skill);
//                else
//                    $scope.soft.push(skill);
//            }


//        });
//    }

//    dataservice.get().then((ret) => {

//        $scope.auth = ret.data;

//        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
//        $scope.GetAllScoresByRole();
//    });

//    $scope.upsertScores = () => {

//        let savedScores = [];
//        const aniBtn = document.querySelector(".animate-btn");


//        for (let skill of $scope.data) {
//            for (let emp of skill.scores) {

//                let num = Number(emp.score);

//                if (num && Number.isInteger(num) && num > 0 && num <= 5) {
//                    emp.score = num;
//                    savedScores.push(emp);
//                }
//            }
//        }

//        if (savedScores.length === 0) return;

//        // start animation
//        //aniBtn.classList.add('onclic');

//        appService.UpsertScores({ scores: savedScores }).then((ret) => {

//            if (ret.data) {
//                /*aniBtn.classList.remove('onclic');*/
//                aniBtn.classList.add('onclic');
//                /*aniBtn.classList.add('validate');*/

//                $scope.GetAllScoresByRole();
//                /*$state.reload();*/

//                //setTimeout(function () {
//                //    aniBtn.classList.remove('validate');
//                //}, 2500);

//                setTimeout(function () {
//                    aniBtn.classList.remove('onclic');
//                    aniBtn.classList.add('validate');
//                    setTimeout(function () {
//                        aniBtn.classList.remove('validate');
//                    }, 1500);
//                }, 1500);


//            }

//        });




//    }




//}]);
