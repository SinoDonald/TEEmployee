let fdata = [];
let chartSet = [];

const painting = document.getElementById('painting')

function DrawEmployeeBarChart(data, selectedCategory) {

    let names = [];
    let titles = [];
    let choices = [];

    painting.innerHTML = '';

    const selectedSample = data[0].Responses.filter(response => response.CategoryId === Number(selectedCategory));

    for (let i = 0; i !== selectedSample.length; i++) {
        titles.push(selectedSample.Content)
        choices.push([]);
    } 
        

    let startidx = selectedSample[0].Id - 1;
    let count = selectedSample.length;

    if (count === 0)
        return;



    for (let i = 0; i !== data.length; i++) {

        names.push(data[i].Employee.name);

        for (let j = 0; j !== count; j++) {

            // transform "option" to number
            let value = Number((data[i].Responses[startidx + j].Choice).slice(6)) * -1 + 5;
            choices[j].push(value);            
        }

    }

    painting.innerHTML = '';
    

    for (let i = 0; i !== count; i++) {

        AddRow(i);

        const chartData = {
            labels: names,
            datasets: [{
                label: titles[i],
                data: choices[i],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)'
                ],
                borderWidth: 1
            }]
        };

        const config = {
            type: 'bar',
            data: chartData,
            options: {
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            },
        };

        chartSet.push(new Chart(document.getElementById('myChart' + i), config));

    }


    





}

function AddRow(idx) {
    const div = document.createElement('div');

    div.className = 'row';

    div.innerHTML = '<canvas id="myChart' + idx + '"></canvas>';

    painting.appendChild(div);
}