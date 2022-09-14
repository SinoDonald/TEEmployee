let fdata = [];
let chartSet = [];

const painting = document.getElementById('painting')

function DrawEmployeeBarChart(data, selectedCategory) {

    let names = [];
    let titles = [];
    let choices = [];
    //let grades = ['優良', '普通', '尚可', '待加強', 'N/A'];
    let grades = ['N/A', '待加強', '尚可', '普通', '優良'];

    painting.innerHTML = '';

    if (data.length === 0) return;

    const selectedSample = data[0].Responses.filter(response => response.CategoryId === Number(selectedCategory));

    for (const sample of selectedSample) {
        titles.push(sample.Content)
        choices.push([]);
    }

    //for (let i = 0; i !== selectedSample.length; i++) {
    //    titles.push(selectedSample.Content)
    //    choices.push([]);
    //} 
        

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

            // transform "option" to number
            //let value = Number((data[i].Responses[startidx + j].Choice).slice(6))  -1;
            //choices[j].push(grades[value]);            

            //let value = Number((data[i].Responses[startidx + j].Choice).slice(6))  -1;
            //choices[j].push({x:names[i], y: grades[value]});            
        }

    }

    painting.innerHTML = '';
    

    for (let i = 0; i !== count; i++) {

        AddRow(i);

        const chartData = {
            labels: names,
            //xlabels: names,
            //ylabels: grades,
            datasets: [{
                label: titles[i],
                /*data: choices[i],*/
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
                        beginAtZero: true,
                        ticks: {
                            // Include a dollar sign in the ticks
                            callback: (value) => {
                                return grades[value];
                            }
                        }
                    }                     
                },
                //scales: {
                //    x: {
                //        type: 'category',
                //        labels: names
                //    },

                //    y: {
                //        type: 'category',
                //        labels: ['優良', '普通', '尚可', '待加強', 'N/A']
                //    }
                //},
                plugins: {
                    legend: {
                        //position: 'top',
                        display: false
                    },
                    title: {
                        display: true,
                        text: titles[i],                        
                        font: {
                            size: 24
                        }                        
                    }
                }
            },
        };

        chartSet.push(new Chart(document.getElementById('myChart' + i), config));

    }

}

function DrawManagerBarChart(data, selectedManager, selectedCategory) {

    let names = [];
    let titles = [];
    let votes = [];
    let rdata = data.ChartManagerResponses;

    painting.innerHTML = '';

    //if (data.length === 0) return;

    const selectedAssessments = data.ManagerAssessments.filter(assessment => assessment.CategoryId === Number(selectedCategory));

    if (selectedManager !== 'All')
        rdata = data.ChartManagerResponses.filter(item => item.Manager.name === selectedManager);

    for (const item of selectedAssessments) {
        titles.push(item.Content)
        votes.push([[],[],[],[]]);
    }

    //for (let i = 0; i !== selectedSample.length; i++) {
    //    titles.push(selectedSample.Content)
    //    choices.push([]);
    //} 


    let startidx = selectedAssessments[0].Id - 1;
    let count = selectedAssessments.length;

    //if (count === 0)
    //    return;


    // loop name
    for (let i = 0; i !== rdata.length; i++) {

        names.push(rdata[i].Manager.name);

        // push [vote,vote,vote]  
        for (let j = 0; j !== count; j++) {

            // 4 option
            for (let k = 0; k !== 4; k++) {

                votes[j][k].push(rdata[i].Votes[startidx + j][k]);
            }
                        
        }

    }

    painting.innerHTML = '';


    for (let i = 0; i !== count; i++) {

        AddRow(i);

        const chartData = {
            labels: names,
            datasets: [{
                label: '同意',
                data: votes[i][0],
                backgroundColor: [
                    'rgba(255, 99, 132, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 99, 132)'
                ],
                borderWidth: 1
            },
            {
                label: '中立',
                data: votes[i][1],
                backgroundColor: [
                    'rgba(255, 159, 64, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 159, 64)'
                ],
                borderWidth: 1
            },
            {
                label: '不同意',
                data: votes[i][2],
                backgroundColor: [
                    'rgba(255, 205, 86, 0.2)'
                ],
                borderColor: [
                    'rgb(255, 205, 86)'
                ],
                borderWidth: 1
            },
            {
                label: 'N/A',
                data: votes[i][3],
                backgroundColor: [
                    'rgba(75, 192, 192, 0.2)'
                ],
                borderColor: [
                    'rgb(75, 192, 192)'
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
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: '人數'
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: titles[i],
                        font: {
                            size: 24
                        }
                    }
                }
            },
        };
        
        chartSet.push(new Chart(document.getElementById('myChart' + i), config));

    }

    painting.className = '';

}


function AddRow(idx) {
    const div = document.createElement('div');

    div.className = 'row  mb-5';

    div.innerHTML = '<canvas id="myChart' + idx + '"></canvas>';

    painting.appendChild(div);
}

function ShowComment(data, selectedManager, selectedCategory) {

    painting.innerHTML = '';

    const rdata = data.ChartManagerResponses.filter(item => item.Manager.name === selectedManager)[0];

    const ul = document.createElement('ul');

    for (let item of rdata.Responses[Number(selectedCategory) - 1]) {
        const li = document.createElement('li');
        li.textContent = item;
        li.className = 'list-group-item';
        li.style.whiteSpace = 'pre-line';
        ul.appendChild(li);
    }

    ul.className = 'list-group';
    painting.appendChild(ul);
    painting.className = '';
}

function DrawManagerRadarChart(data, selectedManager, categories) {

    let numOfQuestions = [];
    let labels = [];

    for (const item of categories) {
        let num = data.ManagerAssessments.filter(assessment => assessment.CategoryId === Number(item.id)).length;
        numOfQuestions.push(num);
        labels.push(item.name);
    }

    let score = [];
    let names = [];    
    let rdata = data.ChartManagerResponses;

    painting.innerHTML = '';

    if (selectedManager !== 'All')
        rdata = data.ChartManagerResponses.filter(item => item.Manager.name === selectedManager);


    // loop name
    for (let i = 0; i !== rdata.length; i++) {

        names.push(rdata[i].Manager.name);
        score.push([]);

        let idx = 0;
        const numOfVotes = rdata[i].Votes[0].reduce((previousValue, currentValue) => previousValue + currentValue);
        if (!numOfVotes) continue;

        // sum up score

        for (let num of numOfQuestions) {

            let sum = 0;
            let numOfInvalid = 0;

            for (let j = idx; j !== (idx + num); j++) {

                const votes = rdata[i].Votes[j]
                sum += (votes[0] * 2 + votes[1] * 1);    
                numOfInvalid += votes[3];
                
            }
            //score[i].push((sum / numOfVotes /(num * 2) * 5)); 

            score[i].push((sum / ((num * numOfVotes - numOfInvalid) * 2) * 5));            

            idx += num;           

        }

    }

    painting.innerHTML = '';

    // draw radar per manager

    for (let i = 0; i !== names.length; i++) {

        AddRow(i);

        const chartData = {
            labels: labels,
            datasets: [{
                label: '表現',
                data: score[i],
                fill: true,
                backgroundColor: 'rgba(255, 99, 132, 0.2)',
                borderColor: 'rgb(255, 99, 132)',
                pointBackgroundColor: 'rgb(255, 99, 132)',
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: 'rgb(255, 99, 132)'
            }]
        };

        const config = {
            type: 'radar',
            data: chartData,
            options: {
                elements: {
                    line: {
                        borderWidth: 3
                    }
                },
                scales: {
                    r: {
                        beginAtZero: true,
                        pointLabels: {
                            font: {
                                size: 20
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        position: 'top',
                    },
                    title: {
                        display: true,
                        text: names[i]
                    }
                }
            },
        };

        chartSet.push(new Chart(document.getElementById('myChart' + i), config));
        
    }
    painting.className = 'radar';
}