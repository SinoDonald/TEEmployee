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

}]);

app.factory('dataservice', function () {

    var auth = {};

    return {
        set: set,
        get: get,
    }

    function set(data) {
        auth = data;
    }

    function get() {
        return auth;
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

    dataservice.set(appService.GetAuthorization({}));

}]);

app.controller('SkillCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', 'dataservice', function ($scope, $location, appService, $rootScope, $q, dataservice) {

    document.documentElement.scrollTop = 0;
    //document.body.style.overflow = "hidden";

    $scope.GetAllSkillsByRole = () => {

        appService.GetAllSkillsByRole({ role: $scope.selectedGroup }).then((ret) => {

            //$scope.data = ret.data;

            $scope.nowHard = ret.data.filter(x => x.skill_type === 'hard' && x.skill_time === 'now').sort((a, b) => a.custom_order - b.custom_order);
            $scope.nowSoft = ret.data.filter(x => x.skill_type === 'soft' && x.skill_time === 'now').sort((a, b) => a.custom_order - b.custom_order);
            $scope.futureHard = ret.data.filter(x => x.skill_type === 'hard' && x.skill_time === 'future').sort((a, b) => a.custom_order - b.custom_order);
            $scope.futureSoft = ret.data.filter(x => x.skill_type === 'soft' && x.skill_time === 'future').sort((a, b) => a.custom_order - b.custom_order);

        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllSkillsByRole();
    });


    // deep clone skills object to modal
    $scope.deletedSkills = [];
    $scope.modal = {};
    $scope.cloneDeepToModal = (skills, skill_time, skill_type) => {

        $scope.modal.data = structuredClone(skills);
        $scope.modal.skill_time = skill_time;
        $scope.modal.skill_type = skill_type;
        $scope.deletedSkills = [];

        //$scope.modal.origin = schedule;
        //$scope.modal.originidx = idx;
        //$scope.modal.createMode = false;

    };

    $scope.addSkill = (newSkill) => {

        if (!newSkill) return;

        $scope.modal.data.push({
            id: 0,
            content: newSkill,
            role: $scope.selectedGroup,
            skill_time: $scope.modal.skill_time,
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

        $scope.members = $scope.auth.GroupAuthorities
            .find(x => x.GroupName === $scope.selectedGroup)
            .Members.sort((a, b) => a.empno - b.empno);

        appService.GetAllScoresByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;
            $scope.hard = [];
            $scope.soft = [];

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

                if (skill.skill_type === 'hard')
                    $scope.hard.push(skill);
                else
                    $scope.soft.push(skill);
            }


        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllScoresByRole();
    });

    $scope.upsertScores = () => {

        let savedScores = [];
        const aniBtn = document.querySelector(".animate-btn");


        for (let skill of $scope.data) {
            for (let emp of skill.scores) {

                let num = Number(emp.score);

                if (num && Number.isInteger(num) && num > 0 && num <= 5) {
                    emp.score = num;
                    savedScores.push(emp);
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


    $scope.GetAllScoresByRole = () => {

        $scope.members = $scope.auth.GroupAuthorities
            .find(x => x.GroupName === $scope.selectedGroup)
            .Members.sort((a, b) => a.empno - b.empno);

        appService.GetAllScoresByRole({ role: $scope.selectedGroup }).then((ret) => {

            $scope.data = ret.data;

            createScoreBarChart($scope.data);
        });
    }

    dataservice.get().then((ret) => {

        $scope.auth = ret.data;

        $scope.selectedGroup = $scope.auth.GroupAuthorities[0].GroupName;
        $scope.GetAllScoresByRole();

        
    });

}]);