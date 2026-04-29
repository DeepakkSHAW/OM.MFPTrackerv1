window.chartInterop = {
    renderLineChart: function (canvasId, labels, data) {

        console.log("Chart render called");

        const canvas = document.getElementById(canvasId);

        if (!canvas) {
            console.log("Canvas not found");
            return;
        }

        const ctx = canvas.getContext('2d');

        if (canvas._chartInstance) {
            canvas._chartInstance.destroy();
        }

        canvas._chartInstance = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'Data',
                    data: data,
                    borderWidth: 2,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    },
    renderNavChart_old: function (canvasId, labels, navValues) {

        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');

        if (canvas._chartInstance) {
            canvas._chartInstance.destroy();
        }

        canvas._chartInstance = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: 'NAV Value',
                    data: navValues,
                    borderWidth: 2,
                    borderColor: '#2E86DE',
                    backgroundColor: 'rgba(46, 134, 222, 0.1)',
                    fill: true,
                    tension: 0.3,
                    pointRadius: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: false
                    }
                }
            }
        });
    },
    renderNavWithPurchases: function (canvasId, labels, navValues, purchasePoints) {

        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');

        if (canvas._chartInstance) {
            canvas._chartInstance.destroy();
        }

        canvas._chartInstance = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    // 📈 NAV LINE
                    {
                        label: 'NAV',
                        data: navValues,
                        borderColor: '#2E86DE',
                        borderWidth: 2,
                        tension: 0.3,
                        fill: false
                    },

                    // 🔵 PURCHASE DOTS
                    {
                        label: 'Purchase Price',
                        data: purchasePoints.map(p => ({
                            x: p.index,
                            y: p.value
                        })),
                        type: 'scatter',
                        pointBackgroundColor: 'red',
                        pointRadius: 6
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        type: 'category'
                    }
                }
            }
        });
    },
    renderNavWithPurchasesv1: function (canvasId, labels, navValues, purchasePoints) {

        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext('2d');

        if (canvas._chartInstance) {
            canvas._chartInstance.destroy();
        }

        canvas._chartInstance = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    // NAV line
                    {
                        label: 'NAV',
                        data: navValues,
                        borderColor: '#2E86DE',
                        borderWidth: 2,
                        tension: 0.3
                    },

                    // PURCHASE dots (dynamic size)
                    {
                        label: 'Purchase',
                        type: 'scatter',
                        data: purchasePoints.map(p => ({
                            x: p.index,
                            y: p.value,
                            amount: p.amount
                        })),

                        pointBackgroundColor: 'red',

                        pointRadius: function (context) {
                            const amount = context.raw.amount;

                            if (!amount) return 4;

                            // 🔥 scale logic (adjust as you like)
                            if (amount < 200) return 4;
                            if (amount < 1000) return 8;
                            if (amount < 5000) return 12;
                            return 16;
                        }
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false
            }
        });
    },
    renderNavChart: function (canvasId, labels, navValues) {

            const canvas = document.getElementById(canvasId);
            if (!canvas) return;

            const ctx = canvas.getContext('2d');

            if (canvas._chartInstance) {
                canvas._chartInstance.destroy();
            }

            canvas._chartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [{
                        label: 'NAV',
                        data: navValues,
                        borderColor: '#2E86DE',
                        borderWidth: 2,
                        tension: 0.3,
                        fill: false
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false
                }
            });
        },

        renderNavWithPurchasesv1: function (
            canvasId,
            labels,
            navValues,
            purchasePoints) {

            /* ✅ DEFENSIVE GUARD (FIX #2) ✅ */
            if (!Array.isArray(purchasePoints) || purchasePoints.length === 0) {
                console.log("No purchase data — rendering NAV only");
                window.chartInterop.renderNavChart(
                    canvasId,
                    labels,
                    navValues
                );
                return;
            }

            const canvas = document.getElementById(canvasId);
            if (!canvas) return;

            const ctx = canvas.getContext('2d');

            if (canvas._chartInstance) {
                canvas._chartInstance.destroy();
            }

            canvas._chartInstance = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: labels,
                    datasets: [

                        // NAV line
                        {
                            label: 'NAV',
                            data: navValues,
                            borderColor: '#2E86DE',
                            borderWidth: 2,
                            tension: 0.3,
                            fill: false
                        },

                        // Purchase overlay
                        {
                            label: 'Purchase',
                            type: 'scatter',
                            data: purchasePoints.map(p => ({
                                x: p.index,
                                y: p.value,
                                amount: p.amount
                            })),
                            pointBackgroundColor: 'red',
                            pointRadius: function (context) {
                                const amount = context.raw?.amount ?? 0;

                                if (amount < 200) return 4;
                                if (amount < 1000) return 8;
                                if (amount < 5000) return 12;
                                return 16;
                            }
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        tooltip: {
                            callbacks: {
                                label: function (ctx) {
                                    if (ctx.dataset.type === 'scatter') {
                                        return `Purchase: ₹${ctx.raw.amount}`;
                                    }
                                    return `NAV: ${ctx.raw}`;
                                }
                            }
                        }
                    },
                    scales: {
                        x: { type: 'category' }
                    }
                }
            });
        }
};