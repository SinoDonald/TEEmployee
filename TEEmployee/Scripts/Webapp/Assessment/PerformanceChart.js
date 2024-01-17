
const ctx = document.getElementById('myChart');

const config = {
    type: 'bar',
    data: {
        labels: [],
        datasets: [{
            /*label: 'My Dataset',*/
            data: [],
            backgroundColor: 'rgba(0, 0, 0, 0)',
            borderColor: 'rgba(0, 0, 0, 0)',
            borderWidth: 1
        }]
    },
    options: {
        scales: {
            y: {
                beginAtZero: true,
                min: 0,
                max: 10,
            }
        },
        plugins: {
            legend: {
                display: false
            },
        },
    },
};

const myChart = new Chart(ctx, config);

function DrawPerformanceChart(performances) {
        
    let sorted_performances = performances.sort(x => x.score);
    let labels = sorted_performances.map(x => x.name);
    let values = sorted_performances.map(x => Number(x.score));

    let colorSets = values.map(x => ({
        background: redBackgroundColors[x - 1],
        border: redBorderColors[x - 1],
    }))

    // update
    myChart.data.labels = labels;
    myChart.data.datasets[0].data = values;
    myChart.data.datasets[0].backgroundColor = colorSets.map(x => x.background);
    myChart.data.datasets[0].borderColor = colorSets.map(x => x.border);

    myChart.update();

}


let redBackgroundColors = [
    'rgba(255, 200, 200, 0.8)',
    'rgba(255, 180, 180, 0.8)',
    'rgba(255, 160, 160, 0.8)',
    'rgba(255, 140, 140, 0.8)',
    'rgba(255, 120, 120, 0.8)',
    'rgba(255, 100, 100, 0.8)',
    'rgba(255, 80, 80, 0.8)',
    'rgba(255, 60, 60, 0.8)',
    'rgba(255, 40, 40, 0.8)',
    'rgba(255, 20, 20, 0.8)'
];

let redBorderColors = [
    'rgba(255, 100, 100, 1)',
    'rgba(255, 90, 90, 1)',
    'rgba(255, 80, 80, 1)',
    'rgba(255, 70, 70, 1)',
    'rgba(255, 60, 60, 1)',
    'rgba(255, 50, 50, 1)',
    'rgba(255, 40, 40, 1)',
    'rgba(255, 30, 30, 1)',
    'rgba(255, 20, 20, 1)',
    'rgba(255, 10, 10, 1)'
];