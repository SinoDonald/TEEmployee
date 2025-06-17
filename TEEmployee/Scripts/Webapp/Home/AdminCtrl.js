var app = angular.module('app', []);

app.run(['$http', '$window', function ($http, $window) {
    $http.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    $http.defaults.headers.common['__RequestVerificationToken'] = $('input[name=__RequestVerificationToken]').val();
}]);

app.service('appService', ['$http', function ($http) {

    this.InsertProjectItem = (o) => {
        return $http.post('Home/InsertProjectItem', o);
    };

    this.GetAllEmployees = function (o) {
        return $http.post('Assessment/GetAllEmployees', o);
    };

    this.InsertUserExtra = function (o) {
        return $http.post('Home/InsertUserExtra', o);
    };

    this.CreateMonthlyRecord = function (o) {
        return $http.post('Home/CreateMonthlyRecord', o);
    };

    this.UpdateUser = function (o) {
        return $http.post('Home/UpdateUser', o);
    };

    this.UpdateAllPercentComplete = function (o) {
        return $http.post('Home/UpdateAllPercentComplete', o);
    };

    this.InsertKpiModels = function (o) {
        return $http.post('GKpi/InsertKpiModels', o);
    };

    this.DeleteKpiModels = function (o) {
        return $http.post('GKpi/DeleteKpiModels', o);
    };

    this.DeleteSolitaryKpiModels = function (o) {
        return $http.post('GKpi/DeleteSolitaryKpiModels', o);
    };

    this.DeleteAssessemnt = function (o) {
        return $http.post('Assessment/DeleteAll', o);
    };

    this.DeleteProfession = function (o) {
        return $http.post('Profession/DeleteAll', o);
    };

    this.DeleteGSchedule = function (o) {
        return $http.post('GSchedule/DeleteAll', o);
    };

    this.DeletePromotion = function (o) {
        return $http.post('Promotion/DeleteAll', o);
    };

    this.DeleteTasklog = function (o) {
        return $http.post('Tasklog/DeleteAll', o);
    };

    this.DeleteForum = function (o) {
        return $http.post('Forum/DeleteAll', o);
    };

    this.DeleteKpi = function (o) {
        return $http.post('GKpi/DeleteAll', o);
    };

    this.DeleteGEducation = function (o) {
        return $http.post('GEducation/DeleteAll', o);
    };
    // =================== 培文 ===================

    // 刪除群組規劃
    this.DeleteGroupPlan = function (o) {
        return $http.post('GSchedule/DeleteGroupPlan', o);
    };

    // 刪除個人規劃
    this.DeletePersonalPlan = function (o) {
        return $http.post('GSchedule/DeletePersonalPlan', o);
    };

    // 刪除人才資料庫
    this.DeleteTalent = function (o) {
        return $http.post('Talent/DeleteTalent', o);
    };

    // 更新個人規劃簡報
    this.UpdatePersonalPlan = function (o) {
        return $http.post('GSchedule/UpdatePersonalPlan', o);
    };
    // 更新通知
    this.UpdateNotify = function (o) {
        return $http.post('Home/UpdateNotify', o);
    };
    // 刪除通知Log檔
    this.DeleteNotifyLog = function (o) {
        return $http.post('Home/DeleteNotifyLog', o);
    };
    // 檢視user.db
    this.ReviewUserDB = (o) => {
        return $http.post('Home/ReviewUserDB', o);
    };
    // 檢視profession.db
    this.ReviewProfessionDB = (o) => {
        return $http.post('Home/ReviewProfessionDB', o);
    };
    //// 人才培訓資料庫
    //this.TalentUpdate = function (o) {
    //    return $http.post('Home/TalentUpdate', o);
    //};
    this.OneTimeSQL = (o) => {
        //return $http.post('Tasklog/AddCustomOrderColumn', o);
        return $http.post('Tasklog/AddGenerateScheduleColumn', o);
    };

}]);

app.controller('AdminCtrl', ['$scope', '$window', 'appService', '$rootScope', function ($scope, $window, appService, $rootScope) {

    $scope.InsertProjectItem = () => {

        appService.InsertProjectItem({})
            .then((ret) => {
                if (ret.data === 'True') alert('Update succeed')
                else alert('No data found')
                $window.location.href = 'Home';
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.UpdateUser = () => {

        appService.UpdateUser({})
            .then((ret) => {
                if (ret.data === 'True') alert('Update succeed')
                else alert('No data found')
                $window.location.href = 'Home';
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.dada = [];

    const input = document.getElementById('input')
    input.addEventListener('change', function () {
        readXlsxFile(input.files[0]).then(function (rows) {
            // `rows` is an array of rows
            // each row being an array of cells.

            if (rows[0][0] !== 'empno') {
                alert('Oops!')
                return;
            }
                

            rows.shift();

            for (let row of rows) {

                let newdata = {};

                //empno
                if (Number.isInteger(row[0])) row[0] = row[0].toString();
                newdata.empno = row[0];

                //group one two three
                row[9] = (row[9] && row[9].length >= 2) ? row[9] : '';
                newdata.group = row[9];

                row[11] = (row[11] && row[11].length >= 2) ? row[11] : '';
                newdata.group_one = row[11];

                row[13] = (row[13] && row[13].length >= 2) ? row[13] : '';
                newdata.group_two = row[13];

                row[15] = (row[15] && row[15].length >= 2) ? row[15] : '';
                newdata.group_three = row[15];

                //manager
                row[8] = (row[8] === 'Y') ? true : false;
                newdata.department_manager = row[8];

                row[10] = (row[10] === 'Y') ? true : false;
                newdata.group_manager = row[10];

                row[12] = (row[12] === 'Y') ? true : false;
                newdata.group_one_manager = row[12];

                row[14] = (row[14] === 'Y') ? true : false;
                newdata.group_two_manager = row[14];

                row[16] = (row[16] === 'Y') ? true : false;
                newdata.group_three_manager = row[16];

                row[17] = (row[17] === 'Y') ? true : false;
                newdata.project_manager = row[17];

                row[18] = (row[18] === 'Y') ? true : false;
                newdata.assistant_project_manager = row[18];

                // new: custom_duty
                row[19] = (row[19]) ? row[19] : '';
                newdata.custom_duty = row[19];

                $scope.dada.push(newdata);
            }



            appService.InsertUserExtra({ usersJson: JSON.stringify($scope.dada) })
                .then((ret) => {
                    if (ret.data === 'True') alert('Successfully updated')
                    //else alert('No data found')
                    $window.location.href = 'Home';
                })
                .catch((ret) => {
                    alert('Error');
                });

            //appService.InsertUserExtra({ users: $scope.dada })
            //    .then((ret) => {
            //        if (ret.data === 'True') alert('Successfully updated')
            //        //else alert('No data found')
            //        $window.location.href = 'Home';
            //    })
            //    .catch((ret) => {
            //        alert('Error');
            //    });

            //rows.forEach(function (item) {

            //    addEmployee(item[3]);
            //})

        })
    })

    /*$scope.Employees = [];*/

    //appService.GetAllEmployees({})
    //    .then(function (ret) {
    //        $scope.Employees = ret.data;
    //    })
    //    .catch(function (ret) {
    //        alert('Error');
    //    });

    $scope.CreateMonthlyRecord = (yymm) => {

        appService.CreateMonthlyRecord({ yymm: yymm })
            .then((ret) => {
                if (ret.data === 'True') alert('Successfully created')
                else alert('No data found')
                $window.location.href = 'Home';
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.UpdateAllPercentComplete = () => {

        appService.UpdateAllPercentComplete({}).then((ret) => {

            if (ret.data) {
                console.log("succeed");
            }

        });
    }

    // 更新個人規劃簡報, 搬移簡報到最近的資料夾內, 並移除錯誤的資料夾(undefined)
    $scope.UpdatePersonalPlan = () => {
        appService.UpdatePersonalPlan({}).then((ret) => {
            if (ret.data) {
                console.log(ret.data);
            }
        });
    }

    $scope.InsertKpiModels = () => {

        appService.InsertKpiModels({})
            .then((ret) => {
                console.log("succeed");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteKpiModels = (year) => {

        appService.DeleteKpiModels({ year: year })
            .then((ret) => {
                console.log("succeed");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteSolitaryKpiModels = () => {

        appService.DeleteSolitaryKpiModels({})
            .then((ret) => {
                console.log("succeed");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    // 更新通知 <-- 培文
    $scope.UpdateNotify = () => {
        appService.UpdateNotify({}).then((ret) => {
            if (ret.data) {
                console.log(ret.data);
            }
        });
    }

    // 刪除通知Log檔 <-- 培文
    $scope.DeleteNotifyLog = () => {
        appService.DeleteNotifyLog({}).then((ret) => {
            if (ret.data) {
                console.log(ret.data);
            }
        });
    }

    // 檢視user.db
    $scope.ReviewUserDB = () => {
        appService.ReviewUserDB({})
            .then((ret) => {
                $scope.userDB = ret.data;
            })
            .catch((ret) => {
                alert(ret.dada);
            });
    }

    // 檢視profession.db
    $scope.ReviewProfessionDB = () => {
        appService.ReviewProfessionDB({})
            .then((ret) => {
                $scope.professionDB = ret.data;
            })
            .catch((ret) => {
                alert(ret.dada);
            });
    }

    //// 人才培訓資料庫 <-- 培文
    //$scope.TalentUpdate = () => {
    //    appService.TalentUpdate({ }).then((ret) => {
    //        if (ret.data) {
    //            console.log("succeed");
    //        }
    //    });
    //}

    // client to server
    formKpiElem.onsubmit = async (e) => {
        e.preventDefault();

        // don't add '/' before MVC controller
        let response = await fetch('GKpi/UploadKpiFile', {
            method: 'POST',
            body: new FormData(formKpiElem)
        });

        let result = await response.json();

        alert(result);
    };

    // Reset Database
    $scope.ResetDB = (dbname) => {
        switch (dbname) {
            case '自評表':
                $scope.DeleteAssessemnt();
                break;
            case '核心專業':
                $scope.DeleteProfession();
                break;
            case '行事曆':
                $scope.DeleteGSchedule();
                break;
            case '升等規劃':
                $scope.DeletePromotion();
                break;
            case '工作紀錄':
                $scope.DeleteTasklog();
                break;
            case '我要抱抱':
                $scope.DeleteForum();
                break;
            case 'KPI':
                $scope.DeleteKpi();
                break;
            case '群組規劃':
                $scope.DeleteGroupPlan();
                break;
            case '個人規劃':
                $scope.DeletePersonalPlan();
                break;
            case '人才資料庫':
                $scope.DeleteTalent();
                break;
            case '拓展專業培養':
                $scope.DeleteGEducation();
                break;
            default:
                console.log('Sorry');
        }
    }


    $scope.DeleteAssessemnt = () => {
        appService.DeleteAssessemnt({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteProfession = () => {
        appService.DeleteProfession({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }


    $scope.DeleteGSchedule = () => {
        appService.DeleteGSchedule({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeletePromotion = () => {
        appService.DeletePromotion({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteTasklog = () => {
        appService.DeleteTasklog({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteForum = () => {
        appService.DeleteForum({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteKpi = () => {
        appService.DeleteKpi({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.DeleteGEducation = () => {
        appService.DeleteGEducation({})
            .then((ret) => {
                alert("Successfully deleted");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    // 刪除群組規劃
    $scope.DeleteGroupPlan = () => {
        appService.DeleteGroupPlan({})
            .then((ret) => {
                if (ret.data == true) alert("刪除成功");
                else alert("刪除失敗");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    // 刪除個人規劃
    $scope.DeletePersonalPlan = () => {
        appService.DeletePersonalPlan({})
            .then((ret) => {
                if (ret.data == true) alert("刪除成功");
                else alert("刪除失敗");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    // 刪除人才資料庫
    $scope.DeleteTalent = () => {
        appService.DeleteTalent({})
            .then((ret) => {
                if (ret.data == true) alert("刪除成功");
                else alert("刪除失敗");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

    $scope.OneTimeSQL = (year) => {

        appService.OneTimeSQL({ year: year })
            .then((ret) => {
                if (ret.data)
                    console.log("succeed");
                else
                    console.log("inner error");
            })
            .catch((ret) => {
                alert('Error');
            });
    }

}]);
