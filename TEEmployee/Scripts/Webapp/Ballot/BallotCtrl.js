var app = angular.module('app', []);
var config = { responseType: 'blob' };  // important

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

    this.GetEvents = (o) => {
        return $http.post('Ballot/GetEvents', o);
    };
    this.GetAllEmployeeCandidates = (o) => {
        return $http.post('Ballot/GetAllEmployeeCandidates', o);
    }; 
    this.CreateBallot = (o) => {
        return $http.post('Ballot/CreateBallot', o);
    }; 
    this.GetBallotByUserAndEvent = (o) => {
        return $http.post('Ballot/GetBallotByUserAndEvent', o);
    };
    this.DownloadEmployeeVoteExcel = (o) => {
        return $http.post('Ballot/DownloadEmployeeVoteExcel', o, config);
    };


}]);


app.controller('BallotCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    $scope.events = [];
    $scope.selected = { code: null };
    $scope.currentEvent = null;
    $scope.data = [];
    $scope.user = null;
    $scope.count = 0;
    $scope.loading = true;
    $scope.sending = false;

    let candidatesPromise = appService.GetAllEmployeeCandidates({}).then((ret) => ret.data || []);
    let loadSeq = 0;   // 快速切換活動時，只採用最後一次的結果

    // 讀取活動清單；網址可帶 ?event=2025emp 指定活動
    appService.GetEvents({}).then((ret) => {
        $scope.events = ret.data.events || [];

        let match = /[?&]event=([^&#]+)/.exec($window.location.search);
        let fromUrl = match ? decodeURIComponent(match[1]) : null;
        let code = $scope.events.some(e => e.code === fromUrl) ? fromUrl : ret.data.defaultCode;

        $scope.selected.code = code;
        loadEvent();
    });

    $scope.changeEvent = () => {
        loadEvent();
    };

    function loadEvent() {
        let seq = ++loadSeq;
        let code = $scope.selected.code;

        $scope.currentEvent = $scope.events.find(e => e.code === code) || null;
        $scope.loading = true;
        $scope.user = null;
        $scope.count = 0;

        if (!$scope.currentEvent) {
            $scope.loading = false;
            return;
        }

        $q.all([
            appService.GetBallotByUserAndEvent({ event_name: code }),
            candidatesPromise,
        ]).then(([ballotRet, candidates]) => {
            if (seq !== loadSeq) return;

            let names = {};
            candidates.forEach(c => names[c.empno] = c.name);

            $scope.data = candidates.map(c => Object.assign({}, c, { selected: false }));
            $scope.user = ballotRet.data || null;

            if ($scope.user) {
                $scope.user.choicesText = ($scope.user.choices || '')
                    .split(';')
                    .filter(x => x)
                    .map(x => names[x] ? `${x} ${names[x]}` : x)
                    .join('、');
            }

            $scope.loading = false;
        });
    }

    $scope.selectCandidate = (item) => {

        if (!item.selected) {
            item.selected = true;
            $scope.count++;
        }            
        else {
            item.selected = false;
            $scope.count--;
        }
    }

    $scope.sendBallot = () => {

        let ballot = {
            event_name: $scope.currentEvent.code,
            choices: $scope.data.filter(x => x.selected).map(x => x.empno).join(';'),
        }

        $scope.sending = true;
        appService.CreateBallot({ ballot: ballot }).then((ret) => {
            if (ret.data && ret.data.success) {
                loadEvent();
            }
            else {
                alert((ret.data && ret.data.message) || '投票失敗，請稍後再試');
                loadEvent();
            }
        }).finally(() => {
            $scope.sending = false;
        });

    }

    $scope.download = () => {

        appService.DownloadEmployeeVoteExcel({ event_name: $scope.currentEvent.code }).then((ret) => {

            const contentDispositionHeader = ret.headers('Content-Disposition');
            let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
            fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');
            var blob = new Blob([ret.data], { type: 'application/octet-stream' });
            saveAs(blob, fileName);

        });
    }

}]);
