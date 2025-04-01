
// offical example

document.addEventListener('DOMContentLoaded', function () {
    var calendarEl = document.getElementById('calendar');
    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'dayGridMonth',
        events: [
            {
                title: 'Y36、Y39進C版提送，B版已於114.05.03提供計畫',
                start: '2025-04-05'
            },
            {
                title: 'Lunch with Client',
                start: '2025-04-05'
            },
            {
                title: 'All-Day Task',
                start: '2025-04-05' // all-day event
            }
        ],
        eventDisplay: 'block',
       
        eventClick: function (info) {
            alert('Event: ' + info.event.title);
            alert('Coordinates: ' + info.jsEvent.pageX + ',' + info.jsEvent.pageY);
            alert('View: ' + info.view.type);

            // change the border color just for fun
            info.el.style.borderColor = 'red';
        }
    });
    calendar.render();

    // Show calendar when modal is fully shown
    $('#calendarModal').on('shown.bs.modal', function () {
        //calendar.render(); // Recalculate layout
        calendar.updateSize(); // better
    });

});