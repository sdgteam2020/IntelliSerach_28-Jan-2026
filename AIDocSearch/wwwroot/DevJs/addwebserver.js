$(document).ready(function () {

    $(".btnEdit").click(function () {

        var id = $(this).data("id");
        var url = $(this).data("url");
        var includes = $(this).data("includes");

        // Set values in form
        $("input[name='Id']").val(id);
        $("input[name='Url']").val(url);
        $("input[name='Includes']").val(includes);

        // Optional: Scroll to form
        $('html, body').animate({
            scrollTop: $("form").offset().top
        }, 500);
    });

});