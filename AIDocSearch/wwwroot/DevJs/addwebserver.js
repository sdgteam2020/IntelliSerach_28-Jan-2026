$(document).ready(function () {
    $('#tblData').DataTable({
        "order": [],
        "paging": true,
        "searching": true,
        "info": true,
        "autoWidth": false,
        "responsive": true,
        "select": true
    });
    $("#SaveForm").on("submit", async function (e) {
        e.preventDefault();

        const isValid = false; await CheckValidation();
        if (isValid) {
            this.submit();
        }
    });

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
async function CheckValidation() {
    const form = $("#SaveForm");
    if (!form.valid()) {
        return false;
    }

    let formData = {};

    form.serializeArray().forEach(function (item) {
        formData[item.name] = item.value;
    });

    let jsonData = JSON.stringify(formData);

    let encrypted = encryptPayloadData(jsonData);
    $("#EncryptedData").val(encrypted);

    $("#SaveForm")[0].submit(); // native submit
    return true;
}