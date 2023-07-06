//const labels = [moment().locale('zh-tw').format('YYYY-MM-DD'),
//    moment().add(1, 'M').locale('zh-tw').format('YYYY-MM-DD'),
//    moment().add(2, 'M').locale('zh-tw').format('YYYY-MM-DD'),
//    moment().subtract(1, 'M').locale('zh-tw').format('YYYY-MM-DD')];


const labels = ['2023-04', '2023-05', '2023-06', '2023-07'];

const count = 10;
let sData = {};
sData.time = [];
sData.percent = [];

sData.time = labels;
sData.percent = [15, 25, 35, 65];

//for (let i = 0; i < count; i++) {
//    sData.label.push(moment().year(2019).month(7).date(i * 7 + 1).startOf('day'))
//    sData.time.push(Math.round(Math.random() * 100))
//}

const data = {
    /*labels: labels,*/
    //datasets: [
    //    {
    //        label: 'Dataset 1',
    //        data: [10, 20, 30, 5],
    //        borderColor: 'rgb(255, 99, 132)',
    //        backgroundColor: 'rgb(255, 99, 132)',
    //        showLine: true,
    //        lineTension: 0.3,
    //    },
    //    {
    //        label: 'Dataset 2',
    //        data: [15, 25, 5, 35],
    //        borderColor: 'rgb(99, 255, 132)',
    //        backgroundColor: 'rgb(99, 255, 132)',
    //        showLine: true,
    //        lineTension: 0.3,
    //    }
    //]
    labels: sData.time,
    datasets: [{
        label: '累計進度',
        data: sData.percent,
        borderColor: 'rgb(255, 99, 132)',
        backgroundColor: 'rgb(255, 99, 132)',
        showLine: true,
        lineTension: 0.3,
    }]
};

const config = {
    type: 'scatter',
    data: data,
    options: {
        responsive: true,
        plugins: {
            legend: {
                position: 'top',
            },
            //title: {
            //    display: true,
            //    text: 'Chart.js Line Chart'
            //}
        },
        scales: {
            x: {
                type: 'time',
                time: {
                    unit: 'month',
                    displayFormats: {
                        month: 'YYYY-MM'
                    }
                }
            },
            y: {
                max: 100,
                min: 0,
            }
            //xAxes: [{
            //    type: 'time',
            //    gridLines: {
            //        display: true
            //    },
            //    time: {
            //        minUnit: 'month'
            //    }
            //}]
        },
    },
};

//function createHistoryLineChart() {

//    const ctx = document.getElementById('myChart');

//    new Chart(ctx, config);
//}

function createScoreBarChart(scoreData) {

    //const labels = ['Mon', 'Mon', 'Mon', 'Mon', 'Mon', 'Mon', 'Mon',];

    const labels = scoreData.map(x => x.content);

    const data = {
        labels: labels,
        datasets: [{
            label: 'My First Dataset',
            data: [65, 59, 80, 81, 56, 55, 40],
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
        }]
    };

    const config = {
        type: 'bar',
        data: data,
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        },
    };


    

    const ctx = document.getElementById('myChart');
    const barChart = new Chart(ctx, config);

    //lineChart.data.labels = obj?.time ?? [];
    //lineChart.data.datasets[0].data = obj?.percent ?? [];

    //lineChart.update();

}




