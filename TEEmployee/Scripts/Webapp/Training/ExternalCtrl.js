var app = angular.module('app', ['angularjs-dropdown-multiselect']);
var config = { responseType: 'blob' };  // important

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    //this.GetAllRecords = (o) => {
    //    return $http.post('Training/GetAllRecords', o);
    //};
    this.GetGroupAuth = (o) => {
        return $http.post('Training/GetGroupAuthorization', o);
    };
    this.GetAllRecordsByUser = (o) => {
        return $http.post('Training/GetAllRecordsByUser', o);
    };
    this.DownloadGroupExcel = (o) => {
        return $http.post('Training/DownloadGroupExcel', o, config);
    };
    this.UpdateUserRecords = (o) => {
        return $http.post('Training/UpdateUserRecords', o);
    };
    this.GetExternalTrainings = (o) => {
        return $http.post('Training/GetExternalTrainings', o);
    };
    this.CreateExternalTraining = (o) => {
        return $http.post('Training/CreateExternalTraining', o);
    };
    this.UpdateExternalTraining = (o) => {
        return $http.post('Training/UpdateExternalTraining', o);
    };
    this.DeleteExternalTraining = (o) => {
        return $http.post('Training/DeleteExternalTraining', o);
    };
    this.DownloadFile = (o) => {
        return $http.post('Training/DownloadFile', o, config);
    }; 
    this.SendExternalTrainingMail = (o) => {
        return $http.post('Training/SendExternalTrainingMail', o);
    };
}]);

app.controller('TrainingCtrl', ['$scope', '$window', 'appService', '$rootScope', '$q', function ($scope, $window, appService, $rootScope, $q) {

    let trainingCustomTypeMap = ['', '創新領域', '領域專業', '通用專業', '經驗交流'];

    $scope.trainingCustomTypes = [
        { value: '1', name: '創新領域' },
        { value: '2', name: '領域專業' },
        { value: '3', name: '通用專業' },
        { value: '4', name: '經驗交流' },
    ]

    appService.GetAllRecordsByUser({}).then((ret) => {

        $scope.records = ret.data;

        $scope.records.forEach(x => x.roc_year = Number(x.start_date.substring(0, 4) - 1911));

        $scope.records.forEach(x => x.customTypeStrValue = x.customType > 0 ? x.customType.toString() : '');

        //$scope.records.forEach(x => x.start_date = x.start_date.split(' ')[0]);
        //$scope.records.forEach(x => x.end_date = x.end_date.split(' ')[0]);

    })

    const formElem = document.querySelector('#formElem');

    // client to server
    if (formElem) {

        formElem.onsubmit = async (e) => {

            e.preventDefault();

            // don't add '/' before MVC controller
            let response = await fetch('Training/UploadTrainingFile', {
                method: 'POST',
                body: new FormData(formElem)
            });

            let result = await response.json();

            alert(result);
        };
    }

    $scope.changeCustomType = (record) => {

        if (record.customTypeStrValue) {
            record.customType = Number(record.customTypeStrValue);
            record.updated = true;
        }
    }

    $scope.UpdateUserRecords = () => {

        let updatedRecords = $scope.records.filter(x => x.updated);

        appService.UpdateUserRecords({ records: updatedRecords })
            .then((ret) => {
                if (ret.data)
                    $window.location.reload();
            })

    }


}]);

app.controller('GroupCtrl', ['$scope', '$location', 'appService', '$rootScope', '$q', function ($scope, $location, appService, $rootScope, $q) {

    $scope.years = [];
    $scope.employees = [];
    $scope.removedItems = [];

    let curYear = new Date().getFullYear();

    for (let i = curYear; i >= 1996; i--) {
        $scope.years.push(i);
    }

    appService.GetAuth({}).then((ret) => {

        $scope.auth = ret.data;

        $scope.selectedYear = $scope.years[0].toString();

        let group_array = $scope.auth.Users.map(x => x.group_one);
        $scope.groups = [...new Set(group_array)];
        $scope.selectedGroup = "";
    });


    $scope.selectYear = () => {
        $scope.selectGroup();
    }


    $scope.selectGroup = () => {

        //// roc version
        //let employee_array = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup).map(x => ({
        //    user: x,
        //    count: x.trainings.filter(y => (y.roc_year + 1911) == $scope.selectedYear).length,
        //}));               

        // start date version
        let employee_array = $scope.auth.Users.filter(x => x.group_one === $scope.selectedGroup).map(x => ({
            user: x,
            count: x.trainings.filter(y => (y.start_date.substring(0, 4)) == $scope.selectedYear).length,
        }));


        $scope.employees = employee_array;
        $scope.selectedEmployee = $scope.employees[0].user.empno;
        $scope.selectEmployee();

    }

    $scope.selectEmployee = () => {

        //// roc version
        //$scope.records = structuredClone(
        //    $scope.auth.Users.find(x => x.empno === $scope.selectedEmployee).trainings.filter(x => (x.roc_year + 1911) == $scope.selectedYear)
        //);

        // start date version
        $scope.records = structuredClone(
            $scope.auth.Users.find(x => x.empno === $scope.selectedEmployee).trainings.filter(y => (y.start_date.substring(0, 4)) == $scope.selectedYear)
        );

        for (let i = 0; i !== $scope.records.length; i++) {

            $scope.records[i].names = [];

            for (let user of $scope.auth.Users) {

                if (user.trainings.find(x => x.training_id === $scope.records[i].training_id)) {
                    $scope.records[i].names.push(user.name);
                }
            }

        }

    }

    $scope.downloadGroupExcel = () => {

        appService.DownloadGroupExcel({ year: ($scope.selectedYear - 1911).toString() }).then((ret) => {

            const contentDispositionHeader = ret.headers('Content-Disposition');
            let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
            fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');
            var blob = new Blob([ret.data], { type: 'application/octet-stream' });
            saveAs(blob, fileName);

        });
    }


}]);



app.controller('ExternalCtrl', ['$scope', '$window', 'appService', '$rootScope', '$timeout', function ($scope, $window, appService, $rootScope, $timeout) {

    $scope.trainingTypes = ['國內培訓', '儲備訓練', '專題演講'];
    const empnoMap = {};
    


    appService.GetGroupAuth({}).then((ret) => {

        $scope.auth = ret.data;

        $scope.selectByGroupData = $scope.auth.Users.map((x, idx) => ({
            id: idx + 1,
            empno: x.empno,
            group_one: x.group_one,
            label: x.name,
        }));

        
        $scope.auth.Users.forEach(user => {
            empnoMap[user.empno] = user.name;
        });

        $scope.getExternalTrainings();

    });


    $scope.selectByGroupModel = [];

    //$scope.selectByGroupData = [
    //    { id: 1, label: "David", gender: 'M' },
    //    { id: 2, label: "Jhon", gender: 'M' },
    //    { id: 3, label: "Lisa", gender: 'F' },
    //    { id: 4, label: "Nicole", gender: 'F' },
    //    { id: 5, label: "Danny", gender: 'M' },
    //    { id: 6, label: "Unknown", gender: 'O' }
    //];
    $scope.selectByGroupSettings = {
        //selectByGroups: ['F', 'M'],
        groupBy: 'group_one',
        scrollableHeight: '300px',
        scrollable: true,
    };

    $scope.selectMemberTranslation = {
        checkAll: '全選',
        uncheckAll: '取消全選',
        buttonDefaultText: '請選擇',
        dynamicButtonTextSuffix: '位'

    }


    //$scope.selectByGroupData = [
    //    { id: 1, label: "David", gender: 'M' },
    //    { id: 2, label: "Jhon", gender: 'M' },
    //    { id: 3, label: "Lisa", gender: 'F' },
    //    { id: 4, label: "Nicole", gender: 'F' },
    //    { id: 5, label: "Danny", gender: 'M' },
    //    { id: 6, label: "Unknown", gender: 'O' }
    //];
    //$scope.selectByGroupSettings = {
    //    selectByGroups: ['F', 'M'],
    //    groupBy: 'gender',
    //};

    //$scope.example13model = []; $scope.example13data = [{ id: 1, label: "David" }, { id: 2, label: "Jhon" }, { id: 3, label: "Lisa" }, { id: 4, label: "Nicole" }, { id: 5, label: "Danny" }]; $scope.example13settings = { smartButtonMaxItems: 3, smartButtonTextConverter: function (itemText, originalItem) { if (itemText === 'Jhon') { return 'Jhonny!'; } return itemText; } };

    $scope.getExternalTrainings = () => {
        appService.GetExternalTrainings({}).then((ret) => {
            $scope.data = ret.data;

            $scope.data.forEach(x => x.memberArr = x.members ? x.members.split(',') : []);
            $scope.data.forEach(x => x.memberStr = x.memberArr.map(name => empnoMap[name]).join());

        });

    };
    


    //$scope.createTraining = () => {

    //    let newTraining = { ...$scope.trainingModal };
    //    newTraining.group_name = $scope.auth.User.group;
    //    newTraining.roc_year = newTraining.startDateObj.getFullYear() - 1911;
    //    newTraining.start_date = moment(newTraining.startDateObj).format('YYYY/MM/DD');
    //    newTraining.end_date = moment(newTraining.endDateObj).format('YYYY/MM/DD');
    //    newTraining.members = $scope.selectByGroupModel.map(x => x.empno).join();

    //    appService.CreateExternalTraining({ training: newTraining }).then((ret) => {
    //        if (ret.data)
    //            $window.location.reload();
    //    })

    //    const uploading = async (e) => {

    //        let form = new FormData(formElem);

    //        form.append('promotion', JSON.stringify($scope.data[$scope.num]));

    //        let response = await fetch('Promotion/UploadFile', {
    //            method: 'POST',
    //            body: form,
    //        });

    //        let result = await response.json();
    //    }

    //};

    $scope.openFileDialog = function () {
        document.querySelector('#trainingFile').click();
    };

    document.querySelector('#trainingFile').addEventListener('change', function (event) {
        const file = event.target.files[0];
        if (file) {
            $scope.$apply(function () {
                $scope.trainingModal.fakename = file.name;
            });
        }
    });

    $scope.deleteFakename = () => {
        $scope.trainingModal.fakename = '';
    };


    $scope.upsertTraining = () => {

        if ($scope.isEditTrainingModal) {
            $scope.updateTraining();
        } else {
            $scope.createTraining();
        }
    }

    $scope.createTraining = async function () {
        const fileInput = document.querySelector('#trainingFile');
        let file = null;

        if ($scope.trainingModal.fakename)
            file = fileInput.files[0];

        let newTraining = {
            group_name: $scope.auth.User.group,
            roc_year: $scope.trainingModal.startDateObj.getFullYear() - 1911,
            training_type: $scope.trainingModal.training_type,
            title: $scope.trainingModal.title,
            organization: $scope.trainingModal.organization,
            start_date: moment($scope.trainingModal.startDateObj).format('YYYY/MM/DD'),
            end_date: moment($scope.trainingModal.endDateObj).format('YYYY/MM/DD'),
            members: $scope.selectByGroupModel.map(x => x.empno).join(),
        };

        const formData = new FormData();
        formData.append('file', file);
        formData.append('training', JSON.stringify(newTraining)); // Send the object as string

        const response = await fetch('Training/CreateExternalTraining', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            throw new Error('Upload failed');
        }

        const result = await response.json();
        console.log('Upload success:', result);
        //$window.location.reload();
        $scope.getExternalTrainings();
    } 


    $scope.updateTraining = async function () {
        const fileInput = document.querySelector('#trainingFile');
        let file = null;

        if ($scope.trainingModal.fakename)
            file = fileInput.files[0];

        let updatedTraining = {
            id: $scope.trainingModal.id,
            roc_year: $scope.trainingModal.startDateObj.getFullYear() - 1911,
            training_type: $scope.trainingModal.training_type,
            title: $scope.trainingModal.title,
            organization: $scope.trainingModal.organization,
            start_date: moment($scope.trainingModal.startDateObj).format('YYYY/MM/DD'),
            end_date: moment($scope.trainingModal.endDateObj).format('YYYY/MM/DD'),
            members: $scope.selectByGroupModel.map(x => x.empno).join(),
            filepath: $scope.trainingModal.filepath,
        };

        const formData = new FormData();
        formData.append('file', file);
        formData.append('training', JSON.stringify(updatedTraining)); // Send the object as string
        formData.append('fakename', $scope.trainingModal.fakename);

        const response = await fetch('Training/UpdateExternalTraining', {
            method: 'POST',
            body: formData,
        });

        if (!response.ok) {
            throw new Error('Upload failed');
        }

        const result = await response.json();
        console.log('Upload success:', result);
        //$window.location.reload();
        $scope.getExternalTrainings();

    } 


    $scope.createTrainingModal = () => {

        document.querySelector('#trainingFile').value = "";
        $scope.isEditTrainingModal = false;
        $scope.trainingModal = {};
        $scope.selectByGroupModel = [];        

    };

    $scope.updateTrainingModal = (training) => {

        document.querySelector('#trainingFile').value = "";
        $scope.isEditTrainingModal = true;
        $scope.trainingModal = { ...training };
        $scope.trainingModal.startDateObj = new Date($scope.trainingModal.start_date.replace(/\//g, "-"));
        $scope.trainingModal.endDateObj = new Date($scope.trainingModal.end_date.replace(/\//g, "-"));
        $scope.trainingModal.fakename = training.filepath;
        //$scope.issueModal.groupMembers = $scope.groupMembers.map(x => ({
        //    name: x.name,
        //    selected: issue.members?.includes(x.name),
        //}));
        $scope.selectByGroupModel = $scope.selectByGroupData.filter(x => training.members.includes(x.empno));

    };


    $scope.confirmDeleteTraining = (training) => {

        var result = confirm("確定要刪除這筆培訓指派嗎？");

        if (result) {
            appService.DeleteExternalTraining({ training: training }).then((ret) => {

                if (ret.data) {
                    $scope.getExternalTrainings();
                }
            })
        }
    }

    $scope.downloadFile = (training) => {

        appService.DownloadFile({ training: training }).then((ret) => {
            const contentDispositionHeader = ret.headers('Content-Disposition');
            let fileName = contentDispositionHeader.split(';')[1].trim().split('=')[1].replace(/"/g, '');
            fileName = decodeURIComponent(fileName).replace(`UTF-8''`, '');
            var blob = new Blob([ret.data], { type: 'application/octet-stream' });
            saveAs(blob, fileName);
        });
    }

    $scope.sendMail = (training) => {

        appService.SendExternalTrainingMail({ training: training }).then((ret) => {

            if (ret.data)
                alert(`已成功寄送通知給${training.memberArr.length}位同仁。`);

        });
        
    }


}]);


app.directive('tooltipInit', function ($timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            $timeout(function () {
                $(element).tooltip();
            });
        }
    };
});