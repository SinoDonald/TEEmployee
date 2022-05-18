
var chartset = [];

var chartdataset = [];

var configset = [];

function CreateChart(chartData, optionsName) {

    for (var i = 0; i != chartData.length; i++) {

        addRow(i);

        var labels = [];
        
        var scores = []

        for (var j = 0; j != optionsName.length; j++)
            scores.push([])


        chartData.forEach(function (item) {

            if (item.CategoryId == i + 1) {

                item.Charts.forEach(function (item2){

                    labels.push(item2.Content)

                    for (var j = 0; j != optionsName.length; j++) {
                        scores[j].push(item2.Votes[j])
                    }
                    
                })

                
            }

        });

        var datasets = []

        for (var j = 0; j != optionsName.length; j++) {

            datasets.push({

                label: optionsName[j],
                data: scores[j],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)',
                    'rgba(255, 159, 64, 0.2)',
                    'rgba(255, 205, 86, 0.2)',
                    'rgba(75, 192, 192, 0.2)',
                    'rgba(54, 162, 235, 0.2)',
                    'rgba(153, 102, 255, 0.2)',
                    'rgba(201, 203, 207, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)',
                    'rgb(255, 159, 64)',
                    'rgb(255, 205, 86)',
                    'rgb(75, 192, 192)',
                    'rgb(54, 162, 235)',
                    'rgb(153, 102, 255)',
                    'rgb(201, 203, 207)'
                ],
                borderWidth: 1

            })
        }



        var data = {

            labels: labels,
            datasets: datasets
        };

        chartdataset.push(data);


        var config = {
            type: 'bar',
            data: chartdataset[i],
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            },
        };

        configset.push(config);

        chartset.push(new Chart(document.getElementById('myChart' + i), configset[i]))



    }



}





//function CreateChart() {

//    for (var i = 0; i != 6; i++) {

//        addRow(i);

//        var labels = [];
//        var scoreOne = []
//        var scoreTwo = []
//        var scoreThr = []

//        ResponseData.forEach(function (item) {
//            if (item.CategoryId == i+1) {
//                labels.push(item.Content)
//                scoreOne.push(item.OptionOneCount)
//                scoreTwo.push(item.OptionTwoCount)
//                scoreThr.push(item.OptionThrCount)
//            }

//        });


//        var data = {

//            labels: labels,
//            datasets: [{
//                label: 'Option1',
//                data: scoreOne,
//                backgroundColor: [
//                    'rgba(255, 99, 132, 0.2)',
//                    'rgba(255, 159, 64, 0.2)',
//                    'rgba(255, 205, 86, 0.2)',
//                    'rgba(75, 192, 192, 0.2)',
//                    'rgba(54, 162, 235, 0.2)',
//                    'rgba(153, 102, 255, 0.2)',
//                    'rgba(201, 203, 207, 0.2)'
//                ],
//                borderColor: [
//                    'rgb(255, 99, 132)',
//                    'rgb(255, 159, 64)',
//                    'rgb(255, 205, 86)',
//                    'rgb(75, 192, 192)',
//                    'rgb(54, 162, 235)',
//                    'rgb(153, 102, 255)',
//                    'rgb(201, 203, 207)'
//                ],
//                borderWidth: 1
//            },
//                {
//                    label: 'Option2',
//                    data: scoreTwo,
//                    backgroundColor: [
//                        'rgba(255, 99, 132, 0.2)',
//                        'rgba(255, 159, 64, 0.2)',
//                        'rgba(255, 205, 86, 0.2)',
//                        'rgba(75, 192, 192, 0.2)',
//                        'rgba(54, 162, 235, 0.2)',
//                        'rgba(153, 102, 255, 0.2)',
//                        'rgba(201, 203, 207, 0.2)'
//                    ],
//                    borderColor: [
//                        'rgb(255, 99, 132)',
//                        'rgb(255, 159, 64)',
//                        'rgb(255, 205, 86)',
//                        'rgb(75, 192, 192)',
//                        'rgb(54, 162, 235)',
//                        'rgb(153, 102, 255)',
//                        'rgb(201, 203, 207)'
//                    ],
//                    borderWidth: 1
//                },
//                {
//                    label: 'Option3',
//                    data: scoreThr,
//                    backgroundColor: [
//                        'rgba(255, 99, 132, 0.2)',
//                        'rgba(255, 159, 64, 0.2)',
//                        'rgba(255, 205, 86, 0.2)',
//                        'rgba(75, 192, 192, 0.2)',
//                        'rgba(54, 162, 235, 0.2)',
//                        'rgba(153, 102, 255, 0.2)',
//                        'rgba(201, 203, 207, 0.2)'
//                    ],
//                    borderColor: [
//                        'rgb(255, 99, 132)',
//                        'rgb(255, 159, 64)',
//                        'rgb(255, 205, 86)',
//                        'rgb(75, 192, 192)',
//                        'rgb(54, 162, 235)',
//                        'rgb(153, 102, 255)',
//                        'rgb(201, 203, 207)'
//                    ],
//                    borderWidth: 1
//                },

//            ]



//        };

//        dataset.push(data);


//        var config = {
//            type: 'bar',
//            data: dataset[i],
//            options: {
//                scales: {
//                    y: {
//                        beginAtZero: true
//                    }
//                }
//            },
//        };

//        configset.push(config);

//        chartset.push(new Chart(document.getElementById('myChart' + i), configset[i]))



//    }



//}





function addRow(idx) {
    const div = document.createElement('div');

    div.className = 'row';

    div.innerHTML = '<canvas id="myChart' + idx +  '"></canvas>';

    document.getElementById('charts').appendChild(div);
}




//const myChart = new Chart(
//    document.getElementById('myChart'),
//    config
//);


//var labels = [1, 2, 3, 4, 5, 6, 7];
//const data = {
//    labels: labels,
//    datasets: [{
//        label: 'My First Dataset',
//        data: [65, 59, 80, 81, 56, 55, 40],
//        backgroundColor: [
//            'rgba(255, 99, 132, 0.2)',
//            'rgba(255, 159, 64, 0.2)',
//            'rgba(255, 205, 86, 0.2)',
//            'rgba(75, 192, 192, 0.2)',
//            'rgba(54, 162, 235, 0.2)',
//            'rgba(153, 102, 255, 0.2)',
//            'rgba(201, 203, 207, 0.2)'
//        ],
//        borderColor: [
//            'rgb(255, 99, 132)',
//            'rgb(255, 159, 64)',
//            'rgb(255, 205, 86)',
//            'rgb(75, 192, 192)',
//            'rgb(54, 162, 235)',
//            'rgb(153, 102, 255)',
//            'rgb(201, 203, 207)'
//        ],
//        borderWidth: 1
//    }]
//};
//const config = {
//    type: 'bar',
//    data: data,
//    options: {
//        scales: {
//            y: {
//                beginAtZero: true
//            }
//        }
//    },
//};