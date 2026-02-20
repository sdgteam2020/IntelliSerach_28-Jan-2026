//let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];
let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];
let selectedFilter = "All"
$(document).ready(function () {
    GetFilter(0);
    $(document).on('click', '.filter', function () {
        // Remove 'active' class from all filters
        $('.filter').removeClass('active');

        // Add 'active' class to the clicked filter
        $(this).addClass('active');

        selectedFilter = $(this).text();
       
        GetFilter(selectedFilter);
    });

    //if ($('#searchInput').val() != "") {
    //    setTimeout(searchContent(), 1000)
    //}
    // Set focus to #searchInput and move cursor to the end
    let input = document.getElementById('searchInput');
    if (input) {
        input.focus();
        let val = input.value;
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
        let query = $('#searchInput').val();
        if ($('#searchInput').val() != "")
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
        let query = $('#searchInput').val();
        if ($('#searchInput').val() != "")
            searchContent();
    });
});

async function searchContent(reset = true) {
    if (reset) { from = 0; document.getElementById('results').innerHTML = ""; }

    const startTime = performance.now(); // Start timing
    const token = $('input[name="__RequestVerificationToken"]').val();
    await fetch('/IntelliSearch/Search/SearchContent', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            "RequestVerificationToken": token   // Pass the CSRF token in the header
        },
        body: JSON.stringify({ DataString: $('#searchInput').val(), size: size, from: from, Filter: selectedFilter })
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
                    const canonical_url = hit._source?.canonical_url || '';
                    const maxScore = data.hits.max_score || 1;
                    const relevance = ((hit._score / maxScore) * 100).toFixed(0);

                    let cleanPath = virtualPath.replace(/\\\\/g, '').replace('\\', '');
                    //${ highlights.map(h => `${h} `).join(' ') }

                    let Mainurl = "";
                    let baseurl = "";
                    let fileurl = "/IntelliSearch/Master/WatermarkPdfWithFolder?fileName="

                    if (realPath.toLowerCase().includes("\\uploadfile")) {
                        // your logic here
                        fileurl = "https://192.168.10.206/dgis_app/uploadfile/";
                    } else if (realPath.toLowerCase().includes("\\uploadfile1111")) {
                        // your logic here
                        fileurl = "https://192.168.10.206/dgis_app/uploadfile111/";
                    }
                       
                    if (cleanPath == "") {
                        Mainurl = canonical_url;
                        cleanPath = canonical_url;
                        fileurl = "";
                        baseurl = getBaseUrl(Mainurl)
                    }
                    else {
                        Mainurl = cleanPath;
                       
                        baseurl = "";//cleanPath.split("\\")[0];
                    }

                    let html = `
    <div class="google-result">
        <a href="${fileurl}${cleanPath}"
           target="_blank"
           class="result-title">
           ${Mainurl}
        </a>

        <div class="result-url">
            <a href="${fileurl}${cleanPath}" target="_blank">
              ${baseurl}
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
function getBaseUrl(input) {
    if (!input) return "";

    // remove "Score:" text if present
    input = input.replace(/Score:\s*\d+/i, '').trim();

    // extract URL
    const match = input.match(/https?:\/\/[^\/\s]+/i);
    return match ? match[0] : "";
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
function goPage(page) { from = (page - 1) * size; searchContent(false); }

async function GetFilter(active) {
    const userdata = new URLSearchParams({
        id: 0
    });
    const token = $('input[name="__RequestVerificationToken"]').val();
    try {
        const response = await fetch('/IntelliSearch/Search/GetFilter', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                "RequestVerificationToken": token   // Pass the CSRF token in the header
            },
            body: userdata
        });

        if (!response.ok) {
            const text = await response.text();
            Swal.fire({
                position: "top-end",
                icon: "error",
                title: "Save failed.\n" + text,
                showConfirmButton: false,
                timer: 1500
            });

            return;
        }
        const data = await response.json();
        if (data.Code === 200) {
            let listItemddl = "";
           
            if (active == 0 || active =="All")
                listItemddl += `<li class="filter active">All</li>`;
            else
                listItemddl += `<li class="filter">All</li>`;
            if (data.Data.Count) {
                let urls = data.Data.Items;

                for (let i = 0; i < data.Data.Count; i++) {
                    let displayLabel = urls[i].Abbr;
                    if (displayLabel != "" ) {
                        if (displayLabel == active)
                            // Using the .Url and .Abbr properties from your FilterItem model
                            listItemddl += `<li class="filter active" title="${urls[i].Url}">${urls[i].Abbr}</li>`;
                        else
                            listItemddl += `<li class="filter" title="${urls[i].Url}">${urls[i].Abbr}</li>`;
                    }
                }
                $(".filters").html(listItemddl);
            }
        }
        else if (data.Code === 4) {
            $("#loading").hide();
            Swal.fire({
                position: "top-end",
                icon: "error",
                title: data.Message + "\n " + data.Data,
                showConfirmButton: false,
                timer: 3500
            });
        } else {
            $("#loading").hide();
            Swal.fire({
                position: "top-end",
                icon: "error",
                title: data.Message,
                showConfirmButton: false,
                timer: 3500
            });
        }
    } catch (error) {
        Swal.fire({
            text: errormsg002
        });
    }
}