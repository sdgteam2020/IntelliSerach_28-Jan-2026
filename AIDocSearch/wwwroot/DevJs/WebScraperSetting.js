$(document).ready(function () {

    $(".btnEdit").click(function () {

        var id = $(this).data("id");
        var url = $(this).data("max_pages");
        var includes = $(this).data("max_pdfs");

        // Set values in form
        $("input[name='Id']").val(id);
        $("input[name='max_pages']").val(url);
        $("input[name='max_pdfs']").val(includes);

        // Optional: Scroll to form
        $('html, body').animate({
            scrollTop: $("form").offset().top
        }, 500);
    });

});