

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
    const painting = document.getElementById('painting');
    painting.innerHTML = '';
    const div = document.createElement('div');

    div.className = 'mb-5';

    div.innerHTML = '<canvas id="myChart"></canvas>';

    painting.appendChild(div);


    const labels = scoreData.map(x => x.content);
    const avgs = scoreData.map(x => {

        if (x.scores.length === 0)
            return 0;
        const sum = x.scores.reduce((a, c) => a + c.score, 0);
        return sum / x.scores.length;
    });

    const backgrounds = avgs.map(x => x >= 2.5 ? 'rgba(54, 162, 235, 0.2)' : 'rgba(255, 99, 132, 0.2)');
    const borders = avgs.map(x => x >= 2.5 ? 'rgba(54, 162, 235)' : 'rgba(255, 99, 132)');

    const data = {
        labels: labels,
        datasets: [{
            //label: 'My First Dataset',
            //data: [65, 59, 80, 81, 56, 55, 40],
            data: avgs,
            backgroundColor: backgrounds,
            borderColor: borders,
            //backgroundColor: [
            //    'rgba(255, 99, 132, 0.2)',
            //    'rgba(255, 159, 64, 0.2)',
            //    'rgba(255, 205, 86, 0.2)',
            //    'rgba(75, 192, 192, 0.2)',
            //    'rgba(54, 162, 235, 0.2)',
            //    'rgba(153, 102, 255, 0.2)',
            //    'rgba(201, 203, 207, 0.2)'
            //],
            //borderColor: [
            //    'rgb(255, 99, 132)',
            //    'rgb(255, 159, 64)',
            //    'rgb(255, 205, 86)',
            //    'rgb(75, 192, 192)',
            //    'rgb(54, 162, 235)',
            //    'rgb(153, 102, 255)',
            //    'rgb(201, 203, 207)'
            //],
            borderWidth: 1
        }]
    };

    const config = {
        type: 'bar',
        data: data,
        options: {
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    min: 0, // minimum value
                    max: 5 // maximum value
                },
            },
        },
    };




    const ctx = document.getElementById('myChart');
    const barChart = new Chart(ctx, config);

    //lineChart.data.labels = obj?.time ?? [];
    //lineChart.data.datasets[0].data = obj?.percent ?? [];

    //lineChart.update();

}

function createScoreScatterChart(scoreData, members) {
        
    const painting = document.getElementById('painting');
    painting.innerHTML = '';
    const div = document.createElement('div');

    div.className = 'mb-5';

    div.innerHTML = '<canvas id="myChart"></canvas>';

    painting.appendChild(div);

    //console.log(members);
    //console.log(scoreData);

    let domain = scoreData.filter(x => x.skill_type === 'domain');
    let core = scoreData.filter(x => x.skill_type === 'core');
    let manage = scoreData.filter(x => x.skill_type === 'manage');

    const professionScores = members.map(x => {

        let domainSum = 0;
        let domainCount = 0;
        let coreSum = 0;
        let coreCount = 0;

        for (let item of domain) {
            let foundScore = item.scores.find(y => y.empno === x.empno);
            if (foundScore) {
                domainSum = domainSum + foundScore.score;
                domainCount++;
            }                
        }

        for (let item of core) {
            let foundScore = item.scores.find(y => y.empno === x.empno);
            if (foundScore) {
                coreSum = coreSum + foundScore.score;
                coreCount++;
            }
        }

        if (domainCount === 0 || coreCount === 0)
            return 0;
        
        return ((domainSum / domainCount) * 3 + (coreSum / coreCount) * 2) / 5 ;
    });

    const manageScores = members.map(x => {

        let manageSum = 0;
        let manageCount = 0;

        for (let item of manage) {
            let foundScore = item.scores.find(y => y.empno === x.empno);
            if (foundScore) {
                manageSum = manageSum + foundScore.score;
                manageCount++;
            }
        }

        if (manageCount === 0)
            return 0;

        return manageSum / manageCount;
    });

    const scores = [];
    for (let i = 0; i < members.length; i++) {
        scores.push({            
            x: manageScores[i],
            y: professionScores[i],
            status: members[i].name,
        });
    }

    const data = {

        datasets: [{            
            data: scores,
            backgroundColor: 'rgb(255, 99, 132)'
        }],

        //datasets: [{
        //    label: 'Scatter Dataset',
        //    data: [{
        //        x: -10,
        //        y: 0
        //    }, {
        //        x: 0,
        //        y: 10
        //    }, {
        //        x: 10,
        //        y: 5
        //    }, {
        //        x: 0.5,
        //        y: 5.5
        //    }],
        //    backgroundColor: 'rgb(255, 99, 132)'
        //}],
    };

    // Draw line and add label after chartjs complete
    // https://www.youtube.com/watch?v=A35nHNLt8nw
    // https://www.youtube.com/watch?v=PNbDrDI97Ng

    const scatterDataLabels = {
        id: 'scatterDataLabels',
        afterDatasetsDraw(chart, args, options) {
            const { ctx, chartArea: { top, bottom, left, right, width, height }, scales: { x, y } } = chart;
            ctx.save();
            ctx.font = '18px sans-serif';

            for (let i = 0; i < chart.config.data.datasets[0].data.length; i++) {

                ctx.fillText(chart.config.data.datasets[0].data[i].status,
                    chart.getDatasetMeta(0).data[i].x + 10,
                    chart.getDatasetMeta(0).data[i].y - 10);

            }

            ctx.fillText('Experienced But Unsure', left + 50, top + 50);
            ctx.fillText('Enthusiastic Engineer', right - 200, bottom - 50);
            ctx.fillText('Lost Traveler', left + 50, bottom - 50);
            ctx.fillText('High Performer', right - 200 , top + 50);

            ctx.beginPath();
            ctx.lineWidth = 5;
            ctx.strokeStyle = 'rgb(0, 0, 0, 0.1)';
            ctx.moveTo(x.getPixelForValue(3), top);
            ctx.lineTo(x.getPixelForValue(3), bottom);
            ctx.moveTo(left, y.getPixelForValue(3));
            ctx.lineTo(right, y.getPixelForValue(3));
            ctx.stroke();

            ctx.restore();

        }
    }

    const config = {
        type: 'scatter',
        data: data,
        options: {
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                x: {
                    title: {
                        //align: 'start',
                        display: true,
                        font: {
                            size: 25,
                        },
                        text: '專業能力',
                        
                    },
                    type: 'linear',
                    //position: {y: 3},
                    min: 0, 
                    max: 5, 
                },
                y: {           
                    title: {
                        //align: 'end',
                        display: true,
                        font: {
                            size: 25,
                        },
                        text: '管理能力',
                        //padding: {
                        //    top: 20,
                        //},
                    },
                    //position: {x: 3},
                    min: 0,
                    max: 5,
                },
            },
            elements: {
                point: {
                    radius: 8,
                    hoverRadius: 12, // ex.: to make it bigger when user hovers put larger number than radius.
                }
            }
            
        },
        plugins: [scatterDataLabels],
    };

    const ctx = document.getElementById('myChart');
    const scatterChart = new Chart(ctx, config);

    //lineChart.data.labels = obj?.time ?? [];
    //lineChart.data.datasets[0].data = obj?.percent ?? [];

    //lineChart.update();

}


