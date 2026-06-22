$(document).ready(function () {

    $(".btnEdit").click(function () {

        let id = $(this).data("id");
        let url = $(this).data("max_pages");
        let includes = $(this).data("max_pdfs");

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