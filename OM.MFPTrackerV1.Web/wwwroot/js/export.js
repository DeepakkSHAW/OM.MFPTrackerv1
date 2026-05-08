window.downloadCsv = (filename, content) => {
    const blob = new Blob([content], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.setAttribute("href", url);
    link.setAttribute("download", filename);
    link.click();
};

// =====================================================
// CORE UNIVERSAL DOWNLOAD FUNCTION
// =====================================================
window.downloadFileUniversal = function (
    filename,
    content,
    isBase64 = false,
    mimeType = "text/csv;charset=utf-8;"
) {
    let href;

    if (isBase64 === true) {
        // ✅ Base64 input
        href = `data:${mimeType};base64,${content}`;
    } else {
        // ✅ Raw content input
        const blob = new Blob([content], { type: mimeType });
        href = URL.createObjectURL(blob);
    }

    const link = document.createElement("a");
    link.href = href;
    link.download = filename;
    link.click();

    // ✅ Important: cleanup blob URLs
    if (!isBase64) {
        setTimeout(() => URL.revokeObjectURL(href), 100);
    }
};

// =====================================================
// CSV EXPORT (Most common in your app)
// =====================================================
window.downloadCsv = function (filename, content) {
    window.downloadFileUniversal(
        filename,
        content,
        false,
        "text/csv;charset=utf-8;"
    );
};


// =====================================================
// BASE64 EXPORT (existing behavior)
// =====================================================
window.downloadBase64 = function (filename, base64, mimeType) {
    window.downloadFileUniversal(
        filename,
        base64,
        true,
        mimeType
    );
};
//window.downloadFileUniversal = function (filename, content, isBase64 = false, mimeType = "text/csv;charset=utf-8;") {

//    let href;

//    if (isBase64 === true) {
//        // ✅ existing behaviour (base64)
//        href = `data:${mimeType};base64,${content}`;
//    } else {
//        // ✅ new behaviour (raw content)
//        const blob = new Blob([content], { type: mimeType });
//        href = URL.createObjectURL(blob);
//    }

//    const link = document.createElement("a");
//    link.href = href;
//    link.download = filename;
//    link.click();

//    // ✅ cleanup (important for blob URLs)
//    if (!isBase64) {
//        setTimeout(() => URL.revokeObjectURL(href), 100);
//    }
//};