let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];

$(document).ready(function () {

//Handle suggestion

    $('#searchInput').on('input', function (event) {
    const query = $(this).val().trim();
    if (query.length < 2) {
        $('#suggestions').hide();
        return;
    }
    if (event.key === 'Enter') {
        event.preventDefault();
        document.getElementById('searchBtn').click();
    }
        const token = $('input[name="__RequestVerificationToken"]').val();
    fetch('/Search/SearchContent', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            "RequestVerificationToken": token   // Pass the CSRF token in the header

        },
        body: JSON.stringify({ DataString: $('#searchInput').val(), size: size, from: from })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data1 => {
            $(".msgerror").addClass("d-none");
            $(".msgerror").html("");
            if (data1.Code == 200) {
                const data = JSON.parse(data1.Data);
                const hits = data.hits?.hits || [];
                totalHits = data.hits?.total?.value || 0;

                let html = '<ul class="search-highlight-list">';

                hits.forEach(hit => {

                    // ✅ Use highlight if present
                    if (hit.highlight?.content?.length) {
                        hit.highlight.content.forEach(snippet => {
                            html += `<li class="search-highlight-item">${snippet}</li>`;
                        });
                    }
                    // ✅ Fallback when highlight is missing
                    else if (hit._source?.content) {
                        const text = hit._source.content.substring(0, 150);
                        html += `<li class="search-highlight-item">${text}...</li>`;
                    }

                });

                html += '</ul>';

                $('#suggestions').html(html).show();
            } else {
                $(".msgerror").removeClass("d-none");
                $(".msgerror").html(data1.Data);
            }

            })
            .catch(console.error);



});

// Handle click on suggestion item
$('#suggestions').on('click', 'li', function () {

    // Replace this line:
    // $('#searchInput').val($(this).text());
    // With this line to assign only the plain text (no HTML tags) to searchInput and replace \n with space:
    $('#searchInput').val($(this).text().replace(/<[^>]*>/g, ' ').replace(/\n/g, ' '));
   
    $('#suggestions').hide();
    // Optionally trigger search here
});

// Hide suggestions when clicking outside
$(document).click(function (e) {
    if (!$(e.target).closest('#searchBox, #suggestions').length) {
        $('#suggestions').hide();
    }
});



// ENd Handle suggestion
});
