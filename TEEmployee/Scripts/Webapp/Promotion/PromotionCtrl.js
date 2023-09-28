var app = angular.module('app', []);
var config = { responseType: 'blob' };  // important

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {
       
    this.GetByUser = (o) => {
        return $http.post('Promotion/GetByUser', o);
    };
    this.UpdatePromotion = (o) => {
        return $http.post('Promotion/Update', o);
    };
    this.DownloadFile = (o) => {
        return $http.post('Promotion/DownloadFile', o, config);
    };
    this.GetAuthorization = (o) => {
        return $http.post('Promotion/GetAuthorization', o);
    };
    this.DeleteAll = (o) => {
        return $http.post('Promotion/DeleteAll', o);
    };
}]);

app.controller('PromotionCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    //const upload = document.querySelector(`#filepicker`);
    filepicker.addEventListener('change', (e) => {
        //const file = e.target.files[0];
        // todo: use file pointer


        if (!e.target.files[0]) {
            return;            
        }

        const maxAllowedSize = 10 * 1024 * 1024;
        if (e.target.files[0].size > maxAllowedSize) {
            alert("檔案超過10MB");
            return;
        }

        const fileType = 'application/pdf';
        if (e.target.files[0].type !== fileType) {
            alert("檔案限定格式為.pdf");
            return;
        }

        uploading().then(() => {

            $scope.$apply(function () {
                $scope.data[$scope.num].filepath = e.target.files[0].name;
            });
        });

        //$scope.data[$scope.num].filepath = e.target.files[0].name;
    });

    const uploading = async (e) => {

        let form = new FormData(formElem);

        form.append('promotion', JSON.stringify($scope.data[$scope.num]));

        let response = await fetch('Promotion/UploadFile', {
            method: 'POST',
            body: form,
        });

        let result = await response.json();
    }

    $scope.num = 0;

    
    appService.GetAuthorization({}).then((ret) => {

        $scope.auth = ret.data;

        let title_array = $scope.auth.Users.map(x => x.profTitle);
        $scope.titles = [...new Set(title_array)];

        $scope.nextProfTitle = $scope.auth.User.nextProfTitle;

        if ($scope.auth.User.department_manager || $scope.auth.User.group_manager) {
            $scope.upgradeUsers = $scope.auth.Users
            $scope.selectedTitle = $scope.auth.User.profTitle;
        /*$scope.selectTitle();*/
            $scope.names = $scope.upgradeUsers.filter(x => x.profTitle === $scope.selectedTitle).map(x => x.name).sort();
            

            $scope.selectedName = $scope.auth.User.name;
            $scope.selectName();
        }
        else {
            appService.GetByUser({}).then((ret2) => {
                $scope.data = ret2.data;
            })
        }

    })
    
    

    $scope.modal = {};

    $scope.UpdatePromotion = (promotion) => {
        appService.UpdatePromotion({ promotion: promotion }).then((ret) => {

        });
    }

    $scope.copyComment = (promotion) => {
        $scope.modal = structuredClone(promotion);
    }

    $scope.saveComment = () => {

        appService.UpdatePromotion({ promotion: $scope.modal }).then((ret) => {
            if (ret.data) {
                let promotion = $scope.data.find(x => x.condition === $scope.modal.condition);
                promotion.comment = $scope.modal.comment;
            }
        });

    }

    $scope.uploadFile = (idx) => {

        $scope.num = idx;
        filepicker.click();

    }

    $scope.downloadFile = (promotion) => {

        appService.DownloadFile({ promotion: promotion }).then((ret) => {

            // set response type of angular http to 'blob', or the default it string or JSON

            // get the file name from response header
            const contentDispositionHeader = ret.headers('Content-Disposition');

            /*const fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');*/

            let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
            fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');

            // Create blob object with type(optional, used for createObjectURL)
            var blob = new Blob([ret.data], { type: 'application/octet-stream' });
            
            // Call Filesaver.js to save it with filename(type inclueded)
            // Benefit: filename
            saveAs(blob, fileName);

        });
    }

    $scope.selectUpgrade = () => {

        if ($scope.selectedUpgrade)
            $scope.upgradeUsers = $scope.auth.Users.filter(x => x.upgrade === $scope.selectedUpgrade);
        else
            $scope.upgradeUsers = $scope.auth.Users

        let title_array = $scope.upgradeUsers.map(x => x.profTitle);
        $scope.titles = [...new Set(title_array)];

        
            $scope.selectedTitle = $scope.titles[0];
            $scope.selectTitle();

        
    }

    $scope.selectTitle = () => {

        //$scope.names = $scope.auth.Users.filter(x => x.profTitle === $scope.selectedTitle).map(x => x.name).sort();
        $scope.names = $scope.upgradeUsers.filter(x => x.profTitle === $scope.selectedTitle).map(x => x.name).sort();
        $scope.selectedName = $scope.names[0];
        $scope.selectName();

    }

    $scope.selectName = () => {

        if (!$scope.selectedName) {
            $scope.data = [];
            return;
        }

            

        let user = $scope.auth.Users.find(x => x.name === $scope.selectedName);

        $scope.nextProfTitle = user.nextProfTitle;

        appService.GetByUser({empno: user.empno}).then((ret) => {
            $scope.data = ret.data;
        })
    }

    $scope.showPromotion = (item) => {

        if (item.condition < 7)
            return true;
        if (!$scope.selectedUpgrade)
            return false;
        if (item.condition === 7 && ($scope.auth.User.department_manager || $scope.auth.User.group_manager))
            return true;
        return false;
    }

    $scope.deleteAll = () => {

        appService.DeleteAll({}).then((ret) => {
            
        });

    }

}]);

