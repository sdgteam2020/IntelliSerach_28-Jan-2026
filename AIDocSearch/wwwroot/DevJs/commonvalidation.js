document.addEventListener("DOMContentLoaded", function () {

    document.addEventListener("input", function (event) {

        if (event.target.matches("input[type='text'], textarea")) {

            let value = event.target.value;

            let cleanValue = value.replace(/[@#$'!%^=]/g, "").replace(/ {2,}/g, " ");   // Allow only one consecutive space
            ;

            if (value !== cleanValue) {
                event.target.value = cleanValue;
            }
        }

    });
    $(document).on("blur", "input[type='text']", function () {
        $(this).val($(this).val().trim());
    });
});