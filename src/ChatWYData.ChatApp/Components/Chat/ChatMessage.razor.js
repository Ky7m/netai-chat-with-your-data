window.ChatWYData = window.ChatWYData || {};
window.ChatWYData.ChatApp = window.ChatWYData.ChatApp || {};

window.ChatWYData.ChatApp.downloadDocument = function (fileBlobUri) {
    const link = document.createElement('a');
    link.href = fileBlobUri;
    link.download = '';
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
