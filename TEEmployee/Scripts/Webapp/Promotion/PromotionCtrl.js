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
}]);

app.controller('PromotionCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    //const upload = document.querySelector(`#filepicker`);
    filepicker.addEventListener('change', (e) => {
        //const file = e.target.files[0];
        // todo: use file pointer
        console.log("what have you done");

        if (e.target.files[0]) {

            uploading().then(() => {

                $scope.$apply(function () {
                    $scope.data[$scope.num].filepath = e.target.files[0].name;
                });
            });
        }

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

    // client to server
    //formKpiElem.onsubmit = async (e) => {
    //    e.preventDefault();

    //    // don't add '/' before MVC controller
    //    let response = await fetch('GKpi/UploadKpiFile', {
    //        method: 'POST',
    //        body: new FormData(formKpiElem)
    //    });

    //    let result = await response.json();

    //    alert(result);
    //};


    $scope.num = 0;

    appService.GetByUser({}).then((ret) => {
        $scope.data = ret.data;
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

}]);

