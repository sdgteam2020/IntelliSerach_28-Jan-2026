$(document).ready(async function () {
    let dattt = await getUserActivity("SearchHistory");

    if (dattt) {

        dattt = JSON.parse(dattt);
        $(".spnlastsearchdate").html(timeAgo(dattt.Date));
        $(".spnRecentSearches").html(dattt.Data);

    }
    loadDocWebCount();
});

function loadDocWebCount() {

    $.ajax({
        url: '/IntelliSearch/Search/GetDoc_webCount',
        type: 'POST',
        dataType: 'json',

        success: function (response) {

            console.log(response);

            if (response && response.Data) {

                let responses = response.Data.Responses;

                let documentCount = 0;
                let scrapWebCount = 0;

                if (responses && responses.length > 0) {
                    documentCount =
                        responses[0]?.Hits?.Total?.Value ?? 0;
                }

                if (responses && responses.length > 1) {
                    scrapWebCount =
                        responses[1]?.Hits?.Total?.Value ?? 0;
                }

                $('#totalDocuments').text(documentCount+"+");
                $('#totalScrapWeb').text(scrapWebCount);
            }
            else {
                $('#totalDocuments').text('0');
                $('#totalScrapWeb').text('0');
            }
        },

        error: function (xhr, status, error) {

            console.error(
                'Error loading document/web count:',
                error
            );

            $('#totalDocuments').text('-');
            $('#totalScrapWeb').text('-');
        }
    });
}