
// Deterministic color based on label (for AMC, Funds, etc.)
function colorFromLabel(label) {
    let hash = 0;
    for (let i = 0; i < label.length; i++) {
        hash = label.charCodeAt(i) + ((hash << 5) - hash);
    }
    const hue = Math.abs(hash) % 360;
    return `hsl(${hue}, 65%, 55%)`;
}

window.baseCategoryColors = {
    Equity: "#2E86DE",      // Blue
    Debt: "#27AE60",        // Green
    Hybrid: "#F39C12",      // Orange
    Commodity: "#8E44AD",   // Purple
    Liquid: "#16A085",      // Teal
    ELSS: "#D35400",        // Dark Orange
    Other: "#7F8C8D"        // Gray
};
function hexToHsl(hex) {
    const r = parseInt(hex.substr(1, 2), 16) / 255;
    const g = parseInt(hex.substr(3, 2), 16) / 255;
    const b = parseInt(hex.substr(5, 2), 16) / 255;

    const max = Math.max(r, g, b), min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;

    if (max === min) {
        h = s = 0;
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
            case r: h = (g - b) / d + (g < b ? 6 : 0); break;
            case g: h = (b - r) / d + 2; break;
            case b: h = (r - g) / d + 4; break;
        }
        h /= 6;
    }

    return [h * 360, s * 100, l * 100];
}
function getShadedColor(baseHex, index, total) {
    const [h, s, l] = hexToHsl(baseHex);

    // Spread lightness between 35% – 75%
    const minL = 35;
    const maxL = 75;

    const step = (maxL - minL) / Math.max(1, total - 1);
    const lightness = minL + (index * step);

    return hslToCss(h, s, lightness);
}
function hslToCss(h, s, l) {
    return `hsl(${h}, ${s}%, ${l}%)`;
}
// ============================================================
// ✅ GLOBAL CHART INTEROP (SAFE, EXTENSIBLE, NON-DESTRUCTIVE)
// ============================================================

// Never overwrite an existing object
window.chartInterop = window.chartInterop || {};

// ============================================================
// ✅ SHARED COLOR PALETTE (CATEGORY-CONSCIOUS)
// ============================================================

// Centralized category → color mapping
window.chartColors = {
    Equity: "#2E86DE",        // Blue
    Debt: "#27AE60",          // Green
    Hybrid: "#F39C12",        // Orange
    Commodity: "#8E44AD",     // Purple
    Liquid: "#16A085",        // Teal
    ELSS: "#D35400",          // Dark Orange
    Other: "#7F8C8D"          // Gray
};

// Fallback resolver
window.getCategoryColor = function (label) {
    return window.chartColors[label] || "#95A5A6";
};

// ============================================================
// ✅ BASIC LINE CHART (GENERIC)
// ============================================================

window.chartInterop.renderLineChart = function (canvasId, labels, data) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    canvas._chartInstance = new Chart(ctx, {
        type: "line",
        data: {
            labels,
            datasets: [{
                label: "Data",
                data,
                borderWidth: 2,
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });
};

// ============================================================
// ✅ NAV LINE CHART (NO PURCHASES)
// ============================================================

window.chartInterop.renderNavChart = function (canvasId, labels, navValues) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    canvas._chartInstance = new Chart(ctx, {
        type: "line",
        data: {
            labels,
            datasets: [{
                label: "NAV",
                data: navValues,
                borderColor: "#2E86DE",
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
};

// ============================================================
// ✅ NAV CHART WITH PURCHASE OVERLAY
// ============================================================

window.chartInterop.renderNavWithPurchasesv1 = function (
    canvasId,
    labels,
    navValues,
    purchasePoints
) {
    // Defensive guard – fallback to NAV-only
    if (!Array.isArray(purchasePoints) || purchasePoints.length === 0) {
        window.chartInterop.renderNavChart(canvasId, labels, navValues);
        return;
    }

    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    canvas._chartInstance = new Chart(ctx, {
        type: "line",
        data: {
            labels,
            datasets: [
                {
                    label: "NAV",
                    data: navValues,
                    borderColor: "#2E86DE",
                    borderWidth: 2,
                    tension: 0.3,
                    fill: false
                },
                {
                    label: "Purchase",
                    type: "scatter",
                    data: purchasePoints.map(p => ({
                        x: p.index,
                        y: p.value,
                        amount: p.amount
                    })),
                    pointBackgroundColor: "red",
                    pointRadius: function (context) {
                        const amt = context.raw?.amount ?? 0;
                        if (amt < 200) return 4;
                        if (amt < 1000) return 8;
                        if (amt < 5000) return 12;
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
                        label: ctx => {
                            if (ctx.dataset.type === "scatter") {
                                return `Purchase: ₹${ctx.raw.amount}`;
                            }
                            return `NAV: ${ctx.raw}`;
                        }
                    }
                }
            },
            scales: {
                x: { type: "category" }
            }
        }
    });
};

// ============================================================
// ✅ PIE CHART (CATEGORY ALLOCATION, SHARED COLORS)
// ============================================================

window.chartInterop.renderPieChartV1 = function (
    canvasId,
    labels,
    values
) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    const colors = labels.map(label =>
        window.getCategoryColor(label)
    );

    canvas._chartInstance = new Chart(ctx, {
        type: "pie",
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: colors
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: "right"
                },
                tooltip: {
                    callbacks: {
                        label: ctx => {
                            const total =
                                ctx.dataset.data.reduce((a, b) => a + b, 0);

                            const value = ctx.raw;
                            const percent =
                                ((value / total) * 100).toFixed(2);

                            return `${ctx.label}: ₹${value.toLocaleString()} (${percent}%)`;
                        }
                    }
                }
            }
        }
    });
};

window.chartInterop.renderPieChartv2 = function (
    canvasId,
    labels,
    values
) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    // ------------------------------------------------------------
    // Helper: deterministic distinct color per label (AMC, Fund)
    // ------------------------------------------------------------
    function colorFromLabel(label) {
        let hash = 0;
        for (let i = 0; i < label.length; i++) {
            hash = label.charCodeAt(i) + ((hash << 5) - hash);
        }
        const hue = Math.abs(hash) % 360;
        return `hsl(${hue}, 65%, 55%)`;
    }

    // ------------------------------------------------------------
    // Detect whether this is a CATEGORY pie or a GENERIC (AMC) pie
    // ------------------------------------------------------------

    // Count subcategories per parent (for category pies)
    const groups = {};
    labels.forEach(label => {
        const parent = label.split("-")[0].trim();
        groups[parent] = (groups[parent] || 0) + 1;
    });

    // If ANY parent exists in baseCategoryColors,
    // we treat this as a Category pie
    const isCategoryPie = labels.some(label => {
        const parent = label.split("-")[0].trim();
        return window.baseCategoryColors &&
            window.baseCategoryColors[parent];
    });

    const indexes = {};

    const colors = labels.map(label => {
        const parent = label.split("-")[0].trim();

        // ✅ Category pie → base color + shades
        if (isCategoryPie && window.baseCategoryColors[parent]) {
            indexes[parent] = indexes[parent] ?? 0;

            const color = getShadedColor(
                window.baseCategoryColors[parent],
                indexes[parent],
                groups[parent]
            );

            indexes[parent]++;
            return color;
        }

        // ✅ AMC pie / generic pie → distinct color per label
        return colorFromLabel(label);
    });

    // ------------------------------------------------------------
    // Render chart
    // ------------------------------------------------------------
    canvas._chartInstance = new Chart(ctx, {
        type: "pie",
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: colors
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: "right"
                },
                tooltip: {
                    callbacks: {
                        label: ctx => {
                            const total =
                                ctx.dataset.data.reduce((a, b) => a + b, 0);
                            const percent =
                                ((ctx.raw / total) * 100).toFixed(2);

                            return `${ctx.label}: ₹${ctx.raw.toLocaleString()} (${percent}%)`;
                        }
                    }
                }
            }
        }
    });
};

window.chartInterop.renderPieChart = function (
    canvasId,
    labels,
    values
) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    // ------------------------------------------------------------
    // Base blue color (single hue)
    // ------------------------------------------------------------
    const baseBlue = "#2E86DE";

    // ------------------------------------------------------------
    // Helpers: HEX ➜ HSL
    // ------------------------------------------------------------
    function hexToHsl(hex) {
        const r = parseInt(hex.substr(1, 2), 16) / 255;
        const g = parseInt(hex.substr(3, 2), 16) / 255;
        const b = parseInt(hex.substr(5, 2), 16) / 255;

        const max = Math.max(r, g, b), min = Math.min(r, g, b);
        let h, s, l = (max + min) / 2;

        if (max === min) {
            h = s = 0;
        } else {
            const d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }
            h /= 6;
        }

        return [h * 360, s * 100, l * 100];
    }

    function hslCss(h, s, l) {
        return `hsl(${h}, ${s}%, ${l}%)`;
    }

    // ------------------------------------------------------------
    // Value-based lightness calculation
    // ------------------------------------------------------------
    const minValue = Math.min(...values);
    const maxValue = Math.max(...values);

    // Lightness range (in %)
    const minL = 35; // darkest (biggest investment)
    const maxL = 75; // lightest (smallest investment)

    const [h, s] = hexToHsl(baseBlue);

    const colors = values.map(v => {
        // Normalize value to 0–1
        const ratio =
            maxValue === minValue
                ? 0.5
                : (v - minValue) / (maxValue - minValue);

        // Invert so higher value = darker color
        const lightness = maxL - ratio * (maxL - minL);

        return hslCss(h, s, lightness);
    });

    // ------------------------------------------------------------
    // Render chart
    // ------------------------------------------------------------
    canvas._chartInstance = new Chart(ctx, {
        type: "pie",
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: colors,
                overOffset: 14
            }]
        },
        options: {
            responsive: true,
            interaction: {
                mode: "nearest",
                intersect: false
            },
            elements: {
                arc: {
                    hoverOffset: 4
                }
            },
            plugins: {
                legend: {
                    position: "right"
                },
                tooltip: {
                    callbacks: {
                        label: ctx => {
                            const total =
                                ctx.dataset.data.reduce((a, b) => a + b, 0);
                            const percent =
                                ((ctx.raw / total) * 100).toFixed(2);

                            return `${ctx.label}: ₹${ctx.raw.toLocaleString()} (${percent}%)`;
                        }
                    }
                }
            }
        }
    });
};


// ===============================
// Bubble chart renderer
// ===============================

window.chartInterop.renderBubbleChartFromPointData = function (canvasId, datasets) {

    console.log("✅ renderBubbleChartFromPointData (with date labels)", datasets);

    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    const ctx = canvas.getContext("2d");
    canvas._chartInstance?.destroy();

    //function colorFromKey(key) {
    //    const hue = Math.abs(key * 37) % 360;
    //    return `hsl(${hue}, 65%, 55%)`;
    //}
    function colorFromIndex(index, total) {
        const hueStep = 360 / Math.max(total, 1);
        const hue = Math.round(index * hueStep);
        return `hsl(${hue}, 65%, 55%)`;
    }
    canvas._chartInstance = new Chart(ctx, {
        type: "bubble",
        data: {
            datasets: datasets.map((d, i) => ({
                label: d.label,
                data: d.data,
                backgroundColor: colorFromIndex(i, datasets.length)
            }))
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                x: {
                    type: "linear",
                    title: { display: true, text: "Transaction Date" },
                    ticks: {
                        callback: value =>
                            new Date(value).toLocaleDateString(undefined, {
                                year: "numeric",
                                month: "short"
                            })
                    }
                },
                y: {
                    title: { display: true, text: "NAV" }
                }
            },
            plugins: {
                legend: {
                    position: "bottom"
                },
                tooltip: {
                    callbacks: {
                        label: ctx => {
                            const d = ctx.raw;
                            return [
                                ctx.dataset.label,
                                `Investment: ₹${d.investment.toLocaleString()}`,
                                `Date: ${new Date(d.x).toLocaleDateString()}`
                            ];
                        }
                    }
                }
            }
        }
    });
};

window.downloadFile = function (filename, base64) {
    const link = document.createElement("a");
    link.href = "data:text/csv;base64," + base64;
    link.download = filename;
    link.click();
};
// ============================================================
// ✅ DEBUG AID (OPTIONAL – SAFE TO KEEP)
// ============================================================

console.log(
    "chartInterop loaded with functions:",
    Object.keys(window.chartInterop)
);