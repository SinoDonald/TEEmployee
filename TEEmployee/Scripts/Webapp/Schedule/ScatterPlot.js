const { scaleLinear, extent, axisLeft, axisBottom, symbol, symbolsFill, scaleOrdinal } = d3;

const scatterPlot = () => {

    let width;
    let height;
    let data;
    let xValue;
    let yValue;
    let symbolValue;
    let margin;
    let radius;
    let size;

    const my = (selection) => {

        const x = scaleLinear()
            .domain(extent(data, xValue))
            .range([margin.left, width - margin.right]);

        const y = scaleLinear()
            .domain(extent(data, yValue))
            .range([height - margin.top, margin.bottom]);

        const symbolScale = scaleOrdinal()
            .domain(data.map(symbolValue))
            .range(symbolsFill);

        const marks = data.map(d => ({
            x: x(xValue(d)),
            y: y(yValue(d)),
            pathD: symbol(symbolScale(symbolValue(d)), size)() // symbol generator => path string
        }));

        selection
            .selectAll('path')
            .data(marks)
            .join('path')
            .attr('d', (item) => item.pathD)
            .attr('transform', (item) => `translate(${item.x}, ${item.y})`);

        // .selectAll('circle')
        // .data(marks)
        // .join('circle')
        // .attr('cx', (d) => d.x)
        // .attr('cy', (d) => d.y)
        // .attr('r', radius)


        selection
            .append('g')
            .attr('transform', `translate(${margin.left}, 0)`)
            .call(axisLeft(y));

        selection
            .append('g')
            .attr('transform', `translate(0, ${height - margin.bottom})`)
            .call(axisBottom(x));


    };

    // Function object & property
    // getter and setter
    my.width = function (_) { // !! arguments is not defined in arrow function
        return arguments.length ? ((width = +_), my) : width; // (,) : comma operator ;  + : plus operator, string to number, defensive programming     
    }
    my.height = function (_) {
        return arguments.length ? ((height = +_), my) : height;
    }
    my.data = function (_) {
        return arguments.length ? ((data = _), my) : data;
    }
    my.xValue = function (_) {
        return arguments.length ? ((xValue = _), my) : xValue;
    }
    my.yValue = function (_) {
        return arguments.length ? ((yValue = _), my) : yValue;
    }
    my.symbolValue = function (_) {
        return arguments.length ? ((symbolValue = _), my) : symbolValueue;
    }
    my.margin = function (_) {
        return arguments.length ? ((margin = _), my) : margin;
    }
    my.radius = function (_) {
        return arguments.length ? ((radius = +_), my) : radius;
    }
    my.size = function (_) {
        return arguments.length ? ((size = +_), my) : size;
    }

    return my;
}