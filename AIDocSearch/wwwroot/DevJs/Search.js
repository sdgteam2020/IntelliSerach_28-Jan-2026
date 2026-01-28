//let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];

$(document).ready(function () {

   

        if ($('#searchInput').val() != "") {
            setTimeout(searchContent(), 1000)
        }
        // Set focus to #searchInput and move cursor to the end
        var input = document.getElementById('searchInput');
        if (input) {
            input.focus();
            var val = input.value;
            input.value = '';
            input.value = val;
        }
        document.getElementById('searchInput').addEventListener('keydown', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                if ($('#searchInput').val() != "")
                    searchContent();
            }
        });
        $('#btnSearch').on('click', function () {
            var query = $('#searchInput').val();
            if ($('#searchInput').val()!="")
            searchContent();
        });

    document.getElementById('searchInput').addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            if ($('#searchInput').val() != "")
                searchContent();
        }
    });
    $('#btnSearch').on('click', function () {
        var query = $('#searchInput').val();
        if ($('#searchInput').val()!="")
        searchContent();
    });
});

async function searchContent(reset = true) {
    if (reset) { from = 0; document.getElementById('results').innerHTML = ""; }

    const startTime = performance.now(); // Start timing
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
            const hits = data.hits.hits || []; totalHits = data.hits.total.value || 0;


            const resultDiv = document.getElementById('results'); resultDiv.innerHTML = '';
            if (hits.length === 0 && from === 0) { resultDiv.innerHTML = "<div class='alert alert-danger'>No results found.</div>"; $("#resultCount").html(""); $('#resultTime').html(""); $("#CountSearch").addClass("d-none"); renderPagination(); return; } else {
                $("#CountSearch").removeClass("d-none");
                $("#resultCount").html("Total Result :-" + totalHits + " ");
                const endTime = performance.now(); // End timing
                const elapsed = (endTime - startTime).toFixed(2);
                $('#resultTime').html(`Search completed in ${elapsed} ms`);
            }
            hits.forEach(hit => {
                const virtualPath = hit._source?.path?.virtual || '';
                const realPath = hit._source?.path?.real || '';
                const highlights = hit.highlight?.content || [];
                // const content = hit._source?.content?.join(" ... ") || hit._source?.content || "";
                const pathhh = hit._index;
                const score = hit._score ?? 0;

                const maxScore = data.hits.max_score || 1;
                const relevance = ((hit._score / maxScore) * 100).toFixed(0);

                const cleanPath = virtualPath.replace(/\\\\/g, '').replace('\\', '');
                //${ highlights.map(h => `${h} `).join(' ') } 
                let html = `
    <div class="google-result">
        <a href="Master/WatermarkPdfWithFolder?fileName=${cleanPath}"
           target="_blank" 
           class="result-title">
           ${cleanPath} 
        </a>

        <div class="result-url">
            <a href="Master/WatermarkPdfWithFolder?fileName=${cleanPath}" target="_blank">
               ${window.location.origin}
            </a>
        </div>

        <!-- SCORE DISPLAY -->
        <div class="result-score">
                    <span class="badge bg-primary">
                        Score: ${relevance}
                    </span>
                </div>
         <!-- END SCORE DISPLAY -->
        <div class="result-snippet">
            <ul>
                ${highlights}
            </ul>
        </div>
    </div>
    `;
                
                $('#results').append(html);

                renderPagination();
            });
            } else {
                $("#CountSearch").addClass("d-none");
                const resultDiv = document.getElementById('results'); resultDiv.innerHTML = '';
                $(".msgerror").removeClass("d-none");
                $(".msgerror").html(data1.Data);
            }
        }).catch(console.error);
}
function renderPagination() {

    const pageSize = Number(size);
    const offset = Number(from);
    const total = Number(totalHits);

    if (!pageSize || total <= pageSize) {
        document.getElementById('paginationTop').innerHTML = '';
        document.getElementById('paginationBottom').innerHTML = '';
        return;
    }

    const pages = Math.ceil(total / pageSize);
    const currentPage = Math.floor(offset / pageSize) + 1;

    const buildButtons = () => {
        const btns = [];

        // ◀ Prev
        if (currentPage > 1) {
            btns.push(`<button class="page-btn" data-page="${currentPage - 1}">‹</button>`);
        }

        // 1
        btns.push(`<button class="page-btn ${currentPage === 1 ? 'active' : ''}" data-page="1">1</button>`);

        if (currentPage > 4) btns.push(`<span class="dots">…</span>`);

        // Middle
        const start = Math.max(2, currentPage - 2);
        const end = Math.min(pages - 1, currentPage + 2);

        for (let i = start; i <= end; i++) {
            btns.push(`
                <button class="page-btn ${i === currentPage ? 'active' : ''}"
                        data-page="${i}">
                    ${i}
                </button>
            `);
        }

        if (currentPage < pages - 3) btns.push(`<span class="dots">…</span>`);

        // Last
        if (pages > 1) {
            btns.push(`
                <button class="page-btn ${currentPage === pages ? 'active' : ''}"
                        data-page="${pages}">
                    ${pages}
                </button>
            `);
        }

        // ▶ Next
        if (currentPage < pages) {
            btns.push(`<button class="page-btn" data-page="${currentPage + 1}">›</button>`);
        }

        return btns.join('');
    };

    document.getElementById('paginationTop').innerHTML = buildButtons();
    document.getElementById('paginationBottom').innerHTML = buildButtons();
}


document.addEventListener('click', function (e) {

    if (!e.target.classList.contains('page-btn')) return;

    const page = parseInt(e.target.dataset.page, 10);
    if (isNaN(page)) return;

    from = (page - 1) * size;
   
    searchContent(false);
});

function highlightQuery(text, query) {
    const escapedQuery = query.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&'); // Escape regex
    const regex = new RegExp(`(${escapedQuery})`, 'gi'); // case-insensitive
    return text.replace(regex, '<strong class="highlightQuery">$1</strong>');
}
function goPage(page) { from = (page - 1) * size;  searchContent(false); }
