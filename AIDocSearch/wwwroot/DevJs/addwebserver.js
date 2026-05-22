$(document).ready(function () {

    $(".btnEdit").click(function () {

        let id = $(this).data("id");
        let url = $(this).data("url");
        let includes = $(this).data("includes");
        let Index_Name = $(this).data("index_name");
       
        // Set values in form
        $("input[name='Id']").val(id);
        $("input[name='Url']").val(url);
        $("input[name='Includes']").val(includes);
        $("input[name='Index_Name']").val(Index_Name);

        // Optional: Scroll to form
        $('html, body').animate({
            scrollTop: $("form").offset().top
        }, 500);
    });

});