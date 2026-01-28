// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$("#loading").hide();

// 1) Global loader hooks (for jQuery + fetch)
$(document).ajaxStart(function () {
    $("#loading").show();
}).ajaxStop(function () {
    $("#loading").hide();
});