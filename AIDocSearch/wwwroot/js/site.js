// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const errormsg002 = "Oops! We couldn’t complete your request. Please try again.";
$("#loading").hide();

// 1) Global loader hooks (for jQuery + fetch)
$(document).ajaxStart(function () {
    $("#loading").show();
}).ajaxStop(function () {
    $("#loading").hide();
});

var currentUrl = window.location.pathname.toLowerCase();

$(".navbar-nav a.nav-link").each(function () {
    var linkUrl = $(this).attr("href").toLowerCase();

    if (currentUrl.startsWith(linkUrl)) {
        $(".navbar-nav a.nav-link").removeClass("active");

        $(this).addClass("active");
    }
});
async function getSalt() {
    const token = $('input[name="__RequestVerificationToken"]').val();
    const response = await fetch('/Account/GetSalt', {
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
