window.airGuardCharts = {
    renderForecast: function (ctxId, labels, data) {
        const ctx = document.getElementById(ctxId);
        if (!ctx) return;

        if (ctx.chartInstance) {
            ctx.chartInstance.destroy();
        }

        const chart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'PM2.5 Forecast (µg/m³)',
                    data: data,
                    borderColor: 'rgba(75,192,192,1)',
                    backgroundColor: 'rgba(75,192,192,0.2)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        title: { display: true, text: 'µg/m³' }
                    },
                    x: {
                        title: { display: true, text: 'Time (UTC)' }
                    }
                }
            }
        });

        ctx.chartInstance = chart;
    }
};
