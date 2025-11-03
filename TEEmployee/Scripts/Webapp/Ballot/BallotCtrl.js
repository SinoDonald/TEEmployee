var app = angular.module('app', []);
var config = { responseType: 'blob' };  // important

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}])

app.service('appService', ['$http', function ($http) {

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


app.controller('BallotCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    

    appService.GetBallotByUserAndEvent({ event_name: '2025emp'}).then((ret) => {

        $scope.user = ret.data;

        if (!$scope.user) {
            appService.GetAllEmployeeCandidates({}).then((ret) => {
                $scope.data = ret.data;
            })
        }
        else {
            $scope.user.choices = $scope.user.choices.replaceAll(';', ', ');
        }
    })

    $scope.count = 0;
    $scope.count

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
            event_name: '2025emp',
            choices: $scope.data.filter(x => x.selected).map(x => x.empno).join(';'),
        }

        appService.CreateBallot({ ballot: ballot }).then((ret) => {
            if (ret) {
                $window.location.reload();
            }
        })

    }

    $scope.download = () => {

        appService.DownloadEmployeeVoteExcel({}).then((ret) => {

            const contentDispositionHeader = ret.headers('Content-Disposition');
            let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
            fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');
            var blob = new Blob([ret.data], { type: 'application/octet-stream' });
            saveAs(blob, fileName);

        });
    }

}]);