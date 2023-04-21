const { scaleLinear, extent, axisLeft, axisBottom, symbol, scaleTime, scaleOrdinal, transition, timeFormat } = d3;

d3.json("https://cdn.jsdelivr.net/npm/d3-time-format@3.0.0/locale/zh-TW.json").then(locale => {
    d3.timeFormatDefaultLocale(locale);

});



const ganttPlot = () => {

    let width;
    let height;
    let data // already parsed string to Date
    let x1Value; // rect start
    let x2Value; // rect end
    let margin;
    let radius;
    let type;
    let year;
    let startMonth;

    const my = (selection) => {

        //const start = new Date(year, 0, 1);
        //const end = new Date(year, 11, 31);

        const start = startMonth;
        let end = moment(startMonth).add(1, 'y').toDate();

        // future
        if (type === 'future')
            end = moment(startMonth).add(3, 'y').toDate();

        const x = scaleTime()
            .domain(extent([start, end]))
            .range([margin.left, width - margin.right]);

        const markstart = x(start);
        const markend = x(end);

        const t = transition()
            .duration(2000)

        //console.log(start);

        // parsed data transformed to data on svg through scale 
        const marks = data.map(d => ({
            x1: x(x1Value(d)),
            x2: x(x2Value(d)),
            //y: height / 3,
            y: height / 2,
            cxy: d.milestones?.map(i => ({
                //cx: x(i.date),
                cx: x(new Date(i.date)),
                //cy: height / 3,
                cy: height / 2 + height / 10,
            })).sort((a, b) => a.cx - b.cx),
            wxy: d.milestones?.map(i => ({
            /* wx: x(i.date),*/
                wx: x(new Date(i.date)),
                //wy: height / 4,
                wy: height / 2 + height / 5,
                content: i.content
            })).sort((a, b) => a.wx - b.wx)
        }));

        // rect
        const rects = selection
            .selectAll('rect')
            .data(marks)
            .join('rect')
            .transition(t)
            //.attr("x", d => d.x1)
            .attr("x", d => {
                return (d.x1 > markstart) ? d.x1 : markstart;
            })
            .attr("y", d => d.y)
            //.attr("width", d => {               
            //    const w = Math.round(d.x2 - d.x1);
            //    return w > 0 ? w : 0;
            //})
            .attr("width", d => {
                const x1 = (d.x1 > markstart) ? d.x1 : markstart;
                const x2 = (d.x2 < markend) ? d.x2 : markend;
                const w = Math.round(x2 - x1);
                return w > 0 ? w : 0;
            })
            // {
            //   const w = Math.round(d.end_date - d.start_date);
            //   return w < rectProps.minWidth || isNaN(w) ? rectProps.minWidth : w;
            // })
            .attr("height", height / 5)
            //.attr("height", height / 4)
            /*.attr("fill", "Orange")*/
            /*.attr("opacity", 0.4)*/
            .attr("fill", "rgb(142, 177, 199)");


        // color
        //if (type === 'group')
        //    rects.attr("fill", "#003262");
        //else if (type === 'detail')
        //    rects.attr("fill", "#90cef1");
        //else 
        //    rects.attr("fill", "#ffc20e");


        //// color
        //if (type === 'group')
        //    rects.attr("fill", "Orange");
        //else
        //    rects.attr("fill", "LightGreen");

        // group
        const groups = selection
            .selectAll('g')
            .data(marks)
            .join('g');

        //1b5faa
        //90cef1
        //ffc20e
        //003262
        //(142,177,199)  

        // circle
        groups.selectAll('circle')
            .data((d) => d.cxy)
            .join('circle')
            .transition(t)
            .attr('cx', d => d.cx)
            //.attr('cy', d => d.cy + 0.5 * height / 3)
            .attr('cy', d => d.cy)
            .attr('r', radius);

        

        // text
        //groups.selectAll('text')
        //    .data((d) => d.wxy)
        //    .join('text')
        //    .transition(t)
        //    .attr('x', d => d.wx)
        //    .attr('y', d => d.wy)
        //    .text(d => d.content);

        let labelRightBounds = [];

        // text
        groups.selectAll('text')
            .data((d) => d.wxy)
            .join('text')            
            .text(d => d.content)
            .style("font-size", "14px")            
            .attr('y', function (d) {
                //labelRightBounds.push([d.wx, this.getComputedTextLength()]);
                labelRightBounds.push([d.wx, this.getBBox().width]);
                
                return d.wy;
            })
            .transition(t)
            .attr('x', d => d.wx)

        //console.log(labelRightBounds);

        let labelHeights = []
        let labelLeft = 0;
        let prevRightBound = -labelLeft;

        function getLabelHeight(i) {

            if (labelRightBounds.length === 0)
                return;                

            if (i === labelRightBounds.length - 1) {
                labelHeights[i] = -2;
                return -2;
            } else if (labelRightBounds[i][0] + labelRightBounds[i][1] + labelLeft > labelRightBounds[i + 1][0]) {
                labelRightBounds[i + 1][0] = labelRightBounds[i][0] + labelRightBounds[i][1] + labelLeft;
                let nextHeight = getLabelHeight(i + 1);
                let thisHeight = nextHeight - 1;
                labelHeights[i] = thisHeight;
                return thisHeight;
            } else {
                getLabelHeight(i + 1);
                labelHeights[i] = -2;
                return -2;
            }
        }

        getLabelHeight(0);

        // text
        groups.selectAll('text')
            .data((d) => d.wxy)
            .join('text')
            .attr('y', (d, i) => (labelHeights[i]) * 16 + d.wy);
            


        //console.log(labelRightBounds);

        selection
            .selectAll('g.x-axis')
            .data([null])
            .join('g')
            .style("font", "10px times")
            .attr('class', 'x-axis')
            .attr('transform', `translate(0, ${height - margin.bottom})`)
            .transition(t)
            //.call(axisBottom(x).ticks(12).tickFormat(timeFormat('%m')).tickSize(-0.5 * height, 0));
            //.call(axisBottom(x).ticks(12).tickSize(-0.5 * height, 0));
            .call(axisBottom(x).ticks(12).tickSize(-0.4 * height, 0));

        
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
    my.year = function (_) {
        return arguments.length ? ((year = +_), my) : year;
    }
    my.startMonth = function (_) {
        return arguments.length ? ((startMonth = _), my) : startMonth;
    }
    return my;
}