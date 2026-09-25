$(document).ready(async function () {
    let dattt = await getUserActivity("SearchHistory");

    if (dattt) {

        dattt = JSON.parse(dattt);
        $(".spnlastsearchdate").html(dattt.Date);
        $(".spnRecentSearches").html(dattt.Data);
      
    }

});