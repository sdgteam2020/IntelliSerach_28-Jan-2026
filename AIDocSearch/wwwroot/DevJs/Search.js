//let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];
let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];
let selectedFilter = "All"
let webServerList = [];
$(document).ready(function () {
    GetFilter(0);
    GetWebServerUrlMapping();
    $(document).on('click', '.filter', function () {

        // Prevent clicking again
        if ($(this).hasClass('disabled')) {
            return;
        }

        // Disable all filters (or only this one)
        $('.filter').addClass('disabled').css('pointer-events', 'none');

        $('.filter').removeClass('active');
        $(this).addClass('active');

        selectedFilter = $(this).text();

        GetFilter(selectedFilter);
    });

    if ($('#searchInput').val() != "") {
        searchContent();
    }
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

    
});

async function searchContent(reset = true) {
    if (reset) { from = 0; document.getElementById('results').innerHTML = ""; }
    $("#loading").show();
   
    const startTime = performance.now(); // Start timing
    const token = $('input[name="__RequestVerificationToken"]').val();
    const parms = { DataString: $('#searchInput').val(), size: size, from: from, Filter: selectedFilter, Type: $('#ddlSearch').val() }
   
    await fetch('/IntelliSearch/Search/SearchContent', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            "RequestVerificationToken": token   // Pass the CSRF token in the header
        },
        body: JSON.stringify(
            {
                Data: encryptPayloadData(JSON.stringify(parms))
            })
        
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            $("#loading").hide();
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
                    const h1 = hit._source?.h1 || '';
                    const headings_h1 = hit._source?.headings_h1 || '';
                    const maxScore = data.hits.max_score || 1;
                    const relevance = ((hit._score / maxScore) * 100).toFixed(0);

                    let cleanPath = virtualPath.replace(/\\\\/g, '').replace('\\', '');
                    //${ highlights.map(h => `${h} `).join(' ') }

                    let Mainurl = "";
                    let baseurl = "";
                    let fileurl = "/IntelliSearch/Master/WatermarkPdfWithFolder?fileName="

                   

                    const normalizedPath = realPath.replace(/\\/g, "/");

                    const match = webServerList.find(item =>
                        normalizedPath.includes(item.Includes)
                    );

                    if (match) {
                        fileurl = match.Url;
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
                     <span class="badge bg-secondary">
                        Path: ${normalizedPath}
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
                    $("#loading").hide();
                });
            } else {
                $("#CountSearch").addClass("d-none");
                const resultDiv = document.getElementById('results'); resultDiv.innerHTML = '';
                $(".msgerror").removeClass("d-none");
                $(".msgerror").html(data1.Data ?? data1.Message);
                $("#loading").hide();
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
                title: "Save failed1.\n" + text,
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
            if (data.Data) {
                let urls = data.Data;

                for (let i = 0; i < data.Data.length; i++) {
                    let displayLabel = urls[i].index;
                    if (displayLabel != "" ) {
                        if (displayLabel == active)
                            // Using the .Url and .Abbr properties from your FilterItem model
                            listItemddl += `<li class="filter active" title="${urls[i].index}">${urls[i].index}</li>`;
                        else
                            listItemddl += `<li class="filter" title="${urls[i].index}">${urls[i].index}</li>`;
                    }
                }
                $(".filters").html(listItemddl);

                if ($('#searchInput').val() != "")
                    searchContent();
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


async function GetWebServerUrlMapping() {
    const userdata = new URLSearchParams({
        id: 0
    });
    const token = $('input[name="__RequestVerificationToken"]').val();
    try {
        const response = await fetch('/IntelliSearch/Master/GetAllWebServer', {
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
                title: "Save failed2.\n" + text,
                showConfirmButton: false,
                timer: 1500
            });

            return;
        }
        const data = await response.json();
        if (data !=null) {
            if (data && Array.isArray(data)) {
                webServerList = data;   // ✅ Store globally
            }
        }
    } catch (error) {
        Swal.fire({
            text: errormsg002
        });
    }
}