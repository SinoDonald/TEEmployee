var app = angular.module('app', []);
var config = { responseType: 'blob' };  // important

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.GetAll = (o) => {
        return $http.post('Wish/GetAll', o);
    };
    this.InsertWish = (o) => {
        return $http.post('Wish/InsertWish', o);
    };
    this.GetAuthorization = (o) => {
        return $http.post('Wish/GetAuthorization', o);
    }; 
    this.UpdateWishStatus = (o) => {
        return $http.post('Wish/UpdateWishStatus', o);
    };
    this.DownloadFile = (o) => {
        return $http.post('Wish/DownloadFile', o, config);
    };
}]);

app.controller('IndexCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    $scope.modal = { category: '', purpose: '', detail: '' };
    $scope.viewModal = {};

    $scope.statusLabels = {
        0: '未處理',
        1: '處理中',
        2: '已結案',
    };

    $scope.statusOptions = [
        { value: '0', label: '未處理' },
        { value: '1', label: '處理中' },
        { value: '2', label: '已結案' },
    ];

    appService.GetAuthorization({}).then((ret) => {
        $scope.editable = ret.data.empno === '8477';
    })

    $scope.GetAll = () => {
        appService.GetAll({}).then((ret) => {
            $scope.wishes = ret.data;   
        })
    }

    $scope.GetAll();

    $scope.showCreateModal = () => {
        $scope.modal = { category: '', purpose: '', detail: '' };
    };

    $scope.showViewModal = (item) => {
        $scope.viewModal = { ...item, status: String(item.wish.status)}
    };

    //$scope.createWish = () => {

    //    let wish = { ...$scope.modal };            

    //    appService.InsertWish({ wish: wish }).then((ret) => {
    //        if (ret.data)
    //            $window.location.reload();
    //    })

    //};

    $scope.updateStatus = (viewModal) => {
        let updatedWish = { id: viewModal.wish.id, status: Number(viewModal.status) };
        appService.UpdateWishStatus({ wish: updatedWish }).then((ret) => {
            if (ret.data)
                viewModal.wish.status = Number(viewModal.status);
        })
    }

    $scope.openFileDialog = function () {
        document.querySelector('#wishFile').click();
    };

    document.querySelector('#wishFile').addEventListener('change', function (event) {
        const file = event.target.files[0];
        if (file) {
            $scope.$apply(function () {
                $scope.modal.fakename = file.name;
            });
        }
    });

    $scope.deleteFakename = () => {
        $scope.modal.fakename = '';
    };

   
    $scope.createWish = async function () {

        const fileInput = document.querySelector('#wishFile');
        let file = null;

        if ($scope.modal.fakename)
            file = fileInput.files[0];

        let wish = { ...$scope.modal };

        const formData = new FormData();
        formData.append('file', file);
        formData.append('wish', JSON.stringify(wish)); // Send the object as string

        const response = await fetch('Wish/InsertWish', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            throw new Error('Upload failed');
        }

        const result = await response.json();
        console.log('Upload success:', result);
        $window.location.reload();
    } 


    $scope.downloadFile = (viewModal) => {

        appService.DownloadFile({ wish: viewModal.wish }).then((ret) => {
            const contentDispositionHeader = ret.headers('Content-Disposition');
            let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
            fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');
            var blob = new Blob([ret.data], { type: 'application/octet-stream' });
            saveAs(blob, fileName);
        });
    }

}]);
