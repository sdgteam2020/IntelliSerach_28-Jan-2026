// Please see documentation at ###S###learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
let AllliveIndex=[];
const errormsg002 = "Oops! We couldn’t complete your request. Please try again.";
$("#loading").hide();

// 1) Global loader hooks (for jQuery + fetch)
$(document).ajaxStart(function () {
    $("#loading").show();
}).ajaxStop(function () {
    $("#loading").hide();
});
$(document).on('click', '.GetCrawlerId', function () {
    let CrawlerId = $(this).data('crawlerid');
    let FileName = $(this).data('filename');
    
    GetFilesFromCrawler(CrawlerId, FileName)

});
let currentUrl = window.location.pathname.toLowerCase();

$(".navbar-nav a.nav-link").each(function () {
    let linkUrl = $(this).attr("href").toLowerCase();

    if (currentUrl.startsWith(linkUrl)) {
        $(".navbar-nav a.nav-link").removeClass("active");

        $(this).addClass("active");
    }
});
async function getSalt() {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await fetch('/IntelliSearch/Account/GetSalt', {
        method: 'POST',
        headers: {
            'Accept': 'application/json',
            "RequestVerificationToken": token // matches [ValidateAntiForgeryToken]
        }
    });

    if (!response.ok) {
        throw new Error('Failed to fetch salt');
    }

    const data = await response.json();

    return data;
} function encryptPayload(dto, keyB64, ivB64) {
    const key = CryptoJS.enc.Base64.parse(keyB64);
    const iv = CryptoJS.enc.Base64.parse(ivB64);

    const encrypted = CryptoJS.AES.encrypt(
        JSON.stringify(dto),
        key,
        {
            iv: iv,
            mode: CryptoJS.mode.CBC,
            padding: CryptoJS.pad.Pkcs7
        }
    );

    return encrypted.toString(); // Base64
}

// Apply saved theme early to avoid flash of un-themed content.
(function () {
    try {
        var t = localStorage.getItem('theme');
        if (t === 'light') {
            document.documentElement.classList.add('light-theme');
        } else if (t === 'dark') {
            document.documentElement.classList.remove('light-theme');
        } else if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
            document.documentElement.classList.add('light-theme');
        }
    } catch (e) {
        // ignore
    }
})();

function GetFilesFromCrawler(CrawlerId, FileName) {
    const data = {
        CrawlerId: CrawlerId,
        FileName: FileName
    };
    $("#loading").show();

    fetch('/IntelliSearch/Files/GetFilesFromCrawler', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
        .then(async  response => {

            if (!response.ok) {
                const errorData = await response.json();
                Swal.fire({
                    icon: "error",
                    title: "Oops...",
                    text: errorData.Message,
                    customClass: {
                        popup: "swal-high-z"
                    }
                });
                return;
            }
            $("#loading").hide();
            return response.blob();
        })
        .then(blob => {

            const url = window.URL.createObjectURL(blob);
            window.open(url);
            $("#loading").hide();
        })
        .catch(error => {
            console.error(error);
            $("#loading").hide();
        });
}

function GetCrawlerStatus(CrawlerId) {

    const data = {
        CrawlerId: CrawlerId
    };


    fetch('/IntelliSearch/Files/GetCrawlerStatus', {

        method: 'POST',

        headers: {
            'Content-Type': 'application/json'
        },

        body: JSON.stringify(data)

    })
        .then(response => response.json())

        .then(result => {


            if (result.Code === 200) {
                // Connected

                $("#crawlerStatusText")
                    .html(result.Message)
                    .removeClass("text-danger")
                    .addClass("text-success");


                $("#crawlerStatusDot")
                    .css("background", "#22c55e");


            }
            else {
                // Not connected

                $("#crawlerStatusText")
                    .html("Crawler Not Connected")
                    .removeClass("text-success")
                    .addClass("text-danger");


                $("#crawlerStatusDot")
                    .css("background", "#dc2626");

            }


        })

        .catch(error => {


            console.error(error);


            $("#crawlerStatusText")
                .text("Crawler Status Error")
                .addClass("text-danger");


            $("#crawlerStatusDot")
                .css("background", "#dc2626");

        });
}

async function GetAllCrawlerStatus() {


    valuesArray = []; 

    fetch('/IntelliSearch/Files/GetAllConnections', {

        method: 'POST',

        headers: {
            'Content-Type': 'application/json'
        }

      

    })
        .then(response => response.json())

        .then(result => {

            let key = Object.keys(result)[0]; // Gets 'fs-kapoor_40'
            let value = result[key];          // Gets 'mJ_67PoK8gust5w4v_3rSQ'

            // Push what you need using regular parentheses ()
            AllliveIndex.push(key); 

        })

        .catch(error => {


            console.error("No rec");



        });
}
function timeAgo(dateTime) {
    let now = new Date();
    let past = new Date(dateTime);

    let diffMs = now - past;

    let diffMinutes = Math.floor(diffMs / (1000 * 60));
    let diffHours = Math.floor(diffMinutes / 60);
    let diffDays = Math.floor(diffHours / 24);

    if (diffDays > 0) {
        return diffDays + " day" + (diffDays > 1 ? "s" : "") + " ago";
    }
    else if (diffHours > 0) {
        return diffHours + " hour" + (diffHours > 1 ? "s" : "") + " ago";
    }
    else if (diffMinutes > 0) {
        return diffMinutes + " minute" + (diffMinutes > 1 ? "s" : "") + " ago";
    }
    else {
        return "Just now";
    }
}
function formatDateTime(dateTime) {
    let date = new Date(dateTime);

    let day = String(date.getDate()).padStart(2, '0');
    let month = String(date.getMonth() + 1).padStart(2, '0');
    let year = String(date.getFullYear()).slice(-2);

    let hours = String(date.getHours()).padStart(2, '0');
    let minutes = String(date.getMinutes()).padStart(2, '0');
    let seconds = String(date.getSeconds()).padStart(2, '0');

    return `${day}/${month}/${year} ${hours}:${minutes}:${seconds}`;
}
function animateCounter(elementId, targetValue, duration = 1000, suffix = "") {

    const element = $("#" + elementId);

    if (!element.length)
        return;


    let startTime = null;


    function updateCounter(currentTime) {

        if (!startTime)
            startTime = currentTime;


        let progress = Math.min(
            (currentTime - startTime) / duration,
            1
        );


        let currentValue = Math.floor(
            progress * targetValue
        );


        element.text(
            currentValue.toLocaleString() + suffix
        );


        if (progress < 1) {
            requestAnimationFrame(updateCounter);
        }
        else {
            element.text(
                targetValue.toLocaleString() + suffix
            );
        }

    }


    requestAnimationFrame(updateCounter);
}
function animateProgressBar(elementId, targetValue, duration = 1000) {

    const element = $("#" + elementId);

    let startTime = null;


    function updateProgress(currentTime) {

        if (!startTime)
            startTime = currentTime;


        let progress = Math.min(
            (currentTime - startTime) / duration,
            1
        );


        let currentValue = Math.floor(
            progress * targetValue
        );


        element.css(
            "width",
            currentValue + "%"
        );


        if (progress < 1) {
            requestAnimationFrame(updateProgress);
        }
        else {
            element.css(
                "width",
                targetValue + "%"
            );
        }

    }


    requestAnimationFrame(updateProgress);
}
function formatFileSize(bytes) {

    if (!bytes || bytes === 0)
        return "0";


    const sizes = [
        "Bytes",
        "KB",
        "MB",
        "GB",
        "TB"
    ];


    const i = Math.floor(
        Math.log(bytes) / Math.log(1024)
    );


    return (
        parseFloat(
            (bytes / Math.pow(1024, i))
                .toFixed(2)
        )
        +
        " "
        +
        sizes[i]
    );
}
function getFileIcon(extension) {

    extension = extension.toLowerCase().replace(".", "");


    switch (extension) {

        case "pdf":
            return "fa-solid fa-file-pdf text-danger";


        case "doc":
        case "docx":
            return "fa-solid fa-file-word text-primary";


        case "xls":
        case "xlsx":
            return "fa-solid fa-file-excel text-success";


        case "ppt":
        case "pptx":
            return "fa-solid fa-file-powerpoint text-warning";


        case "txt":
            return "fa-solid fa-file-lines text-dark";


        case "png":
        case "jpg":
        case "jpeg":
        case "bmp":
        case "tif":
        case "tiff":
            return "fa-solid fa-file-image text-info";


        default:
            return "fa-solid fa-file text-secondary";
    }
}

