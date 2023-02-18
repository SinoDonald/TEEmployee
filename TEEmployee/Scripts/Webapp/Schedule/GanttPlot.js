const { scaleLinear, extent, axisLeft, axisBottom, symbol, scaleTime, scaleOrdinal, transition, timeFormat } = d3;

const ganttPlot = () => {

    let width;
    let height;
    let data // already parsed string to Date
    let x1Value; // rect start
    let x2Value; // rect end
    let margin;
    let radius;
    let type;

    const my = (selection) => {

        const start = new Date(2022, 0, 1);
        const end = new Date(2022, 11, 31);

        const x = scaleTime()
            .domain(extent([start, end]))
            .range([margin.left, width - margin.right]);

        console.log(start);

        // parsed data transformed to data on svg through scale 
        const marks = data.map(d => ({
            x1: x(x1Value(d)),
            x2: x(x2Value(d)),
            y: height / 3,
            cxy: d.milestones.map(i => ({
                cx: x(i.date),
                cy: height / 3
            })),
            wxy: d.milestones.map(i => ({
                wx: x(i.date),
                wy: height / 4,
                content: i.content
            }))
        }));


        

        // rect
        const rects = selection
            .selectAll('rect')
            .data(marks)
            .join('rect')
            .attr("x", d => d.x1)
            .attr("y", d => d.y)
            .attr("width", d => {
                const w = Math.round(d.x2 - d.x1);
                return w > 0 ? w : 0;
            })
            // {
            //   const w = Math.round(d.end_date - d.start_date);
            //   return w < rectProps.minWidth || isNaN(w) ? rectProps.minWidth : w;
            // })
            .attr("height", height / 3)
            /*.attr("fill", "Orange")*/
            .attr("opacity", 0.3)

        if (type === 'group')
            rects.attr("fill", "Orange");
        else
            rects.attr("fill", "LightGreen");
        
        // group
        const groups = selection
            .selectAll('g')
            .data(marks)
            .join('g');



        // circle
        groups.selectAll('circle')
            .data((d) => d.cxy)
            .join('circle')
            .attr('cx', d => d.cx)
            .attr('cy', d => d.cy + 0.5 * height / 3)
            .attr('r', radius);

        // text
        groups.selectAll('text')
            .data((d) => d.wxy)
            .join('text')
            .attr('x', d => d.wx)
            .attr('y', d => d.wy)
            .text(d => d.content);

        selection
            .selectAll('g.x-axis')
            .data([null])
            .join('g')
            .attr('class', 'x-axis')
            .attr('transform', `translate(0, ${height - margin.bottom})`)
            .call(axisBottom(x).ticks(12).tickFormat(timeFormat('%m')).tickSize(-0.5 * height, 0));
        
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
    my.x1Value = function (_) {
        return arguments.length ? ((x1Value = _), my) : x1Value;
    }
    my.x2Value = function (_) {
        return arguments.length ? ((x2Value = _), my) : x2Value;
    }
    my.margin = function (_) {
        return arguments.length ? ((margin = _), my) : margin;
    }
    my.radius = function (_) {
        return arguments.length ? ((radius = +_), my) : radius;
    }
    my.type = function (_) {
        return arguments.length ? ((type = _), my) : type;
    }

    return my;
}