const { scaleLinear, extent, axisLeft, axisBottom, symbol, scaleTime, scaleOrdinal, transition, timeFormat } = d3;

//d3.json("https://cdn.jsdelivr.net/npm/d3-time-format@3.0.0/locale/zh-TW.json").then(locale => {
//    d3.timeFormatDefaultLocale(locale);

//});



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

        const lineX = x(moment().toDate());
        //const p = d3.line()([[lineX, height / 2 - height / 16], [lineX, height - height / 8]]);
        const p = d3.line()([[lineX, height - margin.bottom - 0.4 * height], [lineX, height - margin.bottom]]);
        
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
                content: `${i.content} - ${i.date}`,
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
        //groups.selectAll('circle')
        //    .data((d) => d.cxy)
        //    .join('circle')
        //    .transition(t)
        //    .attr('cx', d => d.cx)
        //    //.attr('cy', d => d.cy + 0.5 * height / 3)
        //    .attr('cy', d => d.cy)
        //    .attr('r', radius)


        groups.selectAll('circle')
            .data((d) => d.cxy)
            .join(
                (enter) => {
                    enter
                        .append('circle')
                        .attr('r', radius)
                        .call((enter) =>
                            enter
                                .transition(t)
                                .attr('cx', d => d.cx)
                                .attr('cy', d => d.cy)
                        )
                        .append('title')
                        .text((d) => d.content)
                },
                (update) => {
                    update
                        .call((update) =>
                            update
                                .transition(t)
                                .attr('cx', d => d.cx)
                                .attr('cy', d => d.cy)
                        )
                        .select('title')
                        .text((d) => d.content)
                }
            )






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


        let key = -2;

        // right to left ascending => left to right ascending
        for (let i = 0; i !== labelHeights.length; i++) {

            if (labelHeights[i] !== key) {

                let count = key - labelHeights[i];

                for (let j = 0; j !== Math.floor(((count + 1) / 2)); j++) {
                    [labelHeights[i + j], labelHeights[i + count - j]] = [labelHeights[i + count - j], labelHeights[i + j]];                    
                }

                i = i + count;

            }
        }

        // get the index of point which is the closest to today

        //if (marks[0].wxy) {
        //    console.log(marks.wxy.length);
        //}

        let closestIdx;

        for (let i = 0; i !== marks[0].wxy.length; i++) {
            
            if (marks[0].wxy[i].wx >= lineX) {
                closestIdx = i;
                break;
            }            
            
        }

        if (closestIdx && labelHeights[closestIdx] !== -2) {
            let numBefore = -2 - labelHeights[closestIdx];

            // before idx, send it to the outer space
            for (let i = 1; i <= numBefore; i++) {
                labelHeights[closestIdx - i] = -5;
            }

            // first three after index, assign -2 -3 -4 if not equal to -2
            for (let i = 0; i !== 3; i++) {
                if (labelHeights[closestIdx + i]) {
                    labelHeights[closestIdx + i] = -2 - i;
                }

            }
        }


        //console.log(labelHeights);
        //console.log('close idx is :' + closestIdx);

        // text
        groups.selectAll('text')
            .data((d) => d.wxy)
            .join('text')
            .attr('y', (d, i) => (labelHeights[i]) * 16 + d.wy);
            

        // line 
        selection.selectAll('path')
            .data([null])
            .join('path')
            .attr("stroke", "rgb(80, 71, 70)")
            //.attr("stroke", "#885A5A")
            .attr("stroke-width", 3)
            .transition(t)
            .attr('d', p);


        //console.log(labelRightBounds);

        selection
            .selectAll('g.x-axis')
            .data([null])
            .join('g')
            //.style("font", "10px times")
            .style("font", "12px Verdana")
            .attr('class', 'x-axis')
            .attr('transform', `translate(0, ${height - margin.bottom})`)
            .transition(t)
            //.call(axisBottom(x).ticks(12).tickFormat(timeFormat('%m')).tickSize(-0.5 * height, 0));
            //.call(axisBottom(x).ticks(12).tickSize(-0.5 * height, 0));
            //.call(axisBottom(x).ticks(12).tickSize(-0.4 * height, 0));
            .call(axisBottom(x).ticks(12).tickFormat(function (date) {
                if (d3.timeYear(date) < date) {
                    return d3.timeFormat('%m')(date);
                } else {
                    return d3.timeFormat('%Y')(date) + '.' + d3.timeFormat('%m')(date);
                }
            }).tickSize(-0.4 * height, 0));


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