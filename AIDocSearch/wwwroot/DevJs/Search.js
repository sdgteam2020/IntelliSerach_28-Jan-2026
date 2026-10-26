//let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];
let from = 0, size = 5, currentQuery = '', totalHits = 0, debounceTimer = null, selectedSuggestion = -1, suggestionItems = [];
let selectedFilter = "All"
let webServerList = [];
let SearchableIndex = [];
$(document).ready(async function () {

    GetFilter(0, SearchableIndex);
    //GetWebServerUrlMapping();

    $(document).on('click', '.filter', function () {

        // Prevent clicking again
        if ($(this).hasClass('disabled')) {
            return;
        }

        // Disable all filters (or only this one)
        $('.filter').addClass('disabled').css('pointer-events', 'none');

        $('.filter').removeClass('active');
        $(this).addClass('active');
     
        selectedFilter = $(this).attr('title');

        GetFilter(selectedFilter, SearchableIndex);
         searchContent(true,true);
    });

    if ($('#searchInput').val() != "") {
        searchContent(true);
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
            if ($('#searchInput').val() != "") {
                SearchableIndex = [];
                searchContent(true);
            }
        }
    });
    $('#btnSearch').on('click', function () {
        let query = $('#searchInput').val();
        if ($('#searchInput').val() != "")
        {
            SearchableIndex = [];
            searchContent(true);
        }
        else {
            window.location.href = "/Home/Dashboard";
        }
    });

    
});

async function searchContent(reset = true,IsFilter) {

   
     if (reset) { from = 0; document.getElementById('results').innerHTML = ""; }
    $("#loading").show();
    
    const startTime = performance.now(); // Start timing
    const token = $('input[name="__RequestVerificationToken"]').val();

    let dataString = $("#searchInput").val();
    if (
        dataString &&
        dataString.trim() !== "" &&
        dataString.startsWith('"') &&
        dataString.endsWith('"')
    ) {
        $('#ddlSearch').val(3)
    }
    else if (
        dataString &&
        dataString.trim() !== "" &&
        dataString.startsWith('(') &&
        dataString.endsWith(')')
    ) {
        $('#ddlSearch').val(2)
    }
    const parms = { DataString: dataString, size: size, from: from, Filter: selectedFilter, Type: $('#ddlSearch').val() }
   
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
                const hits = data.hits.hits || [];
                // totalHits = data.hits.total.value || 0;
                totalHits = data.aggregations?.unique_results?.value ?? 0;
                const aggregations = data.aggregations.indexes_used.buckets;

                var now = new Date();
               
                const Browsermemory = {
                    Data: $('#searchInput').val(),
                    Date: now
                }
                let DataForStoreinMemory = encryptPayloadDatawithkey(JSON.stringify(Browsermemory), $('#spnUserName').val());
                 saveUserActivity(DataForStoreinMemory, "SearchHistory")
              
                const resultDiv = document.getElementById('results'); resultDiv.innerHTML = '';
                if (hits.length === 0 && from === 0) { resultDiv.innerHTML = "<div class='alert alert-danger'>No results found.</div>"; $("#resultCount").html(""); $('#resultTime').html(""); $("#CountSearch").addClass("d-none"); renderPagination(); return; } else {
                    $("#CountSearch").removeClass("d-none");
                    $("#resultCount").html("Total Result :-" + totalHits + " ");
                    const endTime = performance.now(); // End timing
                    const elapsed = (endTime - startTime).toFixed(2);
                    $('#resultTime').html(`Search completed in ${elapsed} ms`);
                }
                aggregations.forEach(hit => {
                    SearchableIndex.push(hit.key);
                });
                hits.forEach(hit => {
                 
                    const virtualPath = hit._source?.path?.virtual || '';
                    const realPath = hit._source?.path?.real || '';
                     let highlights = hit.highlight?.content || [];
                    // let highlights = [
                    //     ...(hit.highlight?.content || []),
                    //     ...(hit.highlight?.['table_rows.columns.value'] || []),
                    //     ...(hit.highlight?.['table_rows.raw'] || []),
                    //     ...(hit.highlight?.['table_rows.raw_text'] || [])
                    // ];
                    // const content = hit._source?.content?.join(" ... ") || hit._source?.content || "";
                  
                    const score = hit._score ?? 0;
                    const canonical_url = hit._source?.canonical_url || '';
                    const h1 = hit._source?.h1 || '';
                    const job_id = hit._source?.job_id || '';
                    // const headings_h1 = hit._source?.headings || '';
                    const headings_h1 = hit.highlight?.headings || hit._source?.headings || '';
                    const url = hit._source?.url || '';
                    const title = hit._source?.title || '';
                    const maxScore = data.hits.max_score || 1;
                    const relevance = ((hit._score / maxScore) * 100).toFixed(0);
                    const indexName = hit._index;
                    const table_rows = hit._source?.table_rows
                

                    const tableHtml = renderDynamicTables(table_rows, dataString);
                   
                   
              

                    // ========================================================
                    // GET HIGHLIGHTS FROM ALL SEARCH FIELDS
                    // ========================================================

                   let  highlight = hit.highlight || {};

                    const contentHighlights =
                        highlight.content || [];

                    const headingHighlights =
                        highlight.headings || [];

                    const titleHighlights =
                        highlight.title || [];

                    const metaDescriptionHighlights =
                        highlight.meta_description || [];

                    const tableRawTextHighlights =
                        highlight['table_rows.raw_text'] || [];

                    const tableRawHighlights =
                        highlight['table_rows.raw'] || [];

                    const tableLabelHighlights =
                        highlight['table_rows.columns.label'] || [];

                    const tableNameHighlights =
                        highlight['table_rows.columns.name'] || [];

                    const tableValueHighlights =
                        highlight['table_rows.columns.value'] || [];


                    // ========================================================
                    // COMBINE ALL HIGHLIGHTS
                    // ========================================================

                    highlights = [
                        ...headings_h1,//p-1
                        ...highlights,
                      
                        ...titleHighlights,//p-1
                       ...headingHighlights,//p-1
                        ...metaDescriptionHighlights, //p-1
                        ...tableLabelHighlights, // p-2
                        //  ...tableNameHighlights,// p-2
                        //...tableValueHighlights,// p-2
                        ...tableRawTextHighlights,
                        // ...tableRawHighlights,// p-2
                        // ...contentHighlights, //p-1
                         
                         
                    ];
                    const displayTitle =
                        Array.isArray(titleHighlights)
                            ? titleHighlights.find(item => typeof item === "string" && item.trim()) || title
                            : title;
                    let cleanPath = virtualPath.replace(/\\\\/g, '').replace('\\', '');
                    //${ highlights.map(h => `${h} `).join(' ') }
                    if (job_id == undefined || job_id=="") {
                        let Mainurl = "";
                        let baseurl = "";
                        let fileurl = ""
                       

                        let extension = "";
                        let path = "";
                        let fileName = ""
                        if (realPath != null) {
                          
                            fileName = realPath.split('\\').pop().split('/').pop();

                            extension = fileName.includes('.')
                                ? fileName.split('.').pop().toLowerCase()
                                : '';
                        }else if (url != null) {
                            extension = "web";
                            path = url;
                            fileName = url;
                        }


                        const normalizedPath = realPath.replace(/\\/g, "/");

                        let Icon_for_Download_Url = 'fa-solid fa-file';
                        let iconClass = 'text-secondary';

                        switch (extension) {
                            case 'web':
                                Icon_for_Download_Url = 'fa fa-globe me-2';
                                iconClass = 'text-danger';
                                break;
                            case 'pdf':
                                Icon_for_Download_Url = 'fa-solid fa-file-pdf';
                                iconClass = 'text-danger';
                                break;

                            case 'doc':
                            case 'docx':
                                Icon_for_Download_Url = 'fa-solid fa-file-word';
                                iconClass = 'text-primary';
                                break;

                            case 'xls':
                            case 'xlsx':
                                Icon_for_Download_Url = 'fa-solid fa-file-excel';
                                iconClass = 'text-success';
                                break;

                            case 'ppt':
                            case 'pptx':
                                Icon_for_Download_Url = 'fa-solid fa-file-powerpoint';
                                iconClass = 'text-warning';
                                break;

                            case 'txt':
                                Icon_for_Download_Url = 'fa-solid fa-file-lines';
                                iconClass = 'text-dark';
                                break;

                            case 'jpg':
                            case 'jpeg':
                            case 'png':
                            case 'gif':
                            case 'bmp':
                            case 'webp':
                                Icon_for_Download_Url = 'fa-solid fa-file-image';
                                iconClass = 'text-info';
                                break;

                            case 'zip':
                            case 'rar':
                            case '7z':
                                Icon_for_Download_Url = 'fa-solid fa-file-zipper';
                                iconClass = 'text-warning';
                                break;
                        }
                        const isPdf = extension === 'pdf';
                        //let Icon_for_Download_Url = "";
                        // if (extension == "web")
                        //     Icon_for_Download_Url = "fa fa-external-link";
                        // else if (isPdf)
                        //     Icon_for_Download_Url = "fa-solid fa-eye";
                        // else
                        //     Icon_for_Download_Url = "fa-solid fa-download";





                        Mainurl = cleanPath;

                        let html = `
    <div class="google-result">
        <div class="result-header">
            <div class="result-left">

             <span class="text-decoration-none GetCrawlerId"
                               title="${fileName}"
                               data-CrawlerId="${indexName}"
                               data-FileName="${Mainurl}">
                                <i class="${Icon_for_Download_Url} ${iconClass} me-1"></i>
                                ${Mainurl.length > 20 ? Mainurl.substring(0,100) + '...' : Mainurl}

                            </span>
               
            </div>
            <div class="result-right">
                <div class="result-score">
                    <div class="score-label me-2 badge badge-pill badge-success">${indexName.replace("fs-", "")}</div>
                    <span class="score-pill d-none">Score: ${score} MaxScore:${maxScore}  Score ${relevance}</span>
                   
                </div>
            </div>
        </div>

        <div class="result-snippet">
            <ul>
                ${highlights}  ............
            </ul> 
        </div>
    </div>
    `;
                        $('#results').append(html);
                       

                    } else {

                        let html = `
    <div class="google-result">
        <div class="result-header">
            <div class="result-left">
                <a href="${url}" target="_blank" class="result-title" title="${url}"> <i class="fa fa-globe me-2"></i> ${new URL(url.trim()).origin} (${displayTitle})</a>
              
            </div>
            <div class="result-right">
                <div class="result-score">
                     <div class="score-label me-2 badge badge-pill badge-success">${indexName.replace("fs-", "")}</div>
                    <span class="score-pill d-none">Score: ${score} MaxScore:${maxScore}  Score ${relevance}</span>
                   
                </div>
            </div>
        </div>

        <div class="result-snippet">
            <ul>
                 ${highlights}   ............
            </ul>
        </div>
        ${tableHtml}
    </div>
    `;
                        $('#results').append(html);
                    }


                    
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
   GetFilter(selectedFilter, SearchableIndex);
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

async function GetFilter(active, SearchableIndex) {
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
                listItemddl += `<li class="filter active" title="All">All</li>`;
            else
                listItemddl += `<li class="filter" title="All">All</li>`;
            if (data.Data) {
                let urls = data.Data;
                if (SearchableIndex != null)
                urls = data.Data.filter(index => SearchableIndex.includes(index.index));

                for (let i = 0; i < urls.length; i++) {
                    let displayLabel = urls[i].index;
                    if (displayLabel != "" ) {
                        if (displayLabel == active)
                            // Using the .Url and .Abbr properties from your FilterItem model
                            listItemddl += `<li class="filter active" title="${urls[i].index}">${urls[i].index.replace("fs-","")}</li>`;
                        else
                            listItemddl += `<li class="filter filtersWithSearch" title="${urls[i].index}">${urls[i].index.replace("fs-", "") }</li>`;
                    }
                }
                urls = data.Data.filter(index => !SearchableIndex.includes(index.index));

                for (let i = 0; i < urls.length; i++) {
                    let displayLabel = urls[i].index;
                    if (displayLabel != "") {
                     
                            listItemddl += `<li class="filter" title="${urls[i].index}">${urls[i].index.replace("fs-", "")}</li>`;
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
function renderDynamicTables(table_rows, tableValueHighlights) {
    if (!Array.isArray(table_rows) || table_rows.length === 0) {
        return "";
    }

    const groupedTables = {};

    table_rows.forEach(row => {
        const tableIndex = row.table_index ?? 0;

        if (!groupedTables[tableIndex]) {
            groupedTables[tableIndex] = [];
        }

        groupedTables[tableIndex].push(row);
    });

    let finalHtml = "";

    Object.keys(groupedTables).forEach(tableIndex => {
        const rows = groupedTables[tableIndex];
        const headers = [];

        rows.forEach(row => {
            if (!Array.isArray(row.columns)) return;

            row.columns.forEach((col, index) => {
                const header =
                    col.label ||
                    col.name ||
                    `Column ${index + 1}`;

                if (!headers.includes(header)) {
                    headers.push(header);
                }
            });
        });

        if (headers.length === 0) return;

        // Keep only rows that contain a value matching the highlights.
        const matchingRows = rows.filter(row => {
            if (!Array.isArray(row.columns)) return false;

            return row.columns.some(col => {
                const value = col?.value ?? "";
                const highlightedValue =
                    getHighlightedTableValue(value, tableValueHighlights);

                return highlightedValue !== escapeHtml(String(value));
            });
        });

        // If no row in this table matches, do not render this table.
        if (matchingRows.length === 0) return;

        finalHtml += `
            <div class="table-responsive mt-2">
                <table class="table table-bordered table-sm">
                    <thead>
                        <tr>
                            ${headers.map(header => `
                                <th>${escapeHtml(header)}</th>
                            `).join("")}
                        </tr>
                    </thead>
                    <tbody>
        `;

        matchingRows.forEach(row => {
            finalHtml += "<tr>";

            headers.forEach(header => {
                const column = Array.isArray(row.columns)
                    ? row.columns.find((col, index) => {
                        const columnHeader =
                            col.label ||
                            col.name ||
                            `Column ${index + 1}`;

                        return columnHeader === header;
                    })
                    : null;

                const originalValue = column?.value ?? "";
                const displayValue = getHighlightedTableValue(
                    originalValue,
                    tableValueHighlights
                );

                finalHtml += `<td>${displayValue}</td>`;
            });

            finalHtml += "</tr>";
        });

        finalHtml += `
                    </tbody>
                </table>
            </div>
        `;
    });

    return finalHtml || "";
}
function getHighlightedTableValue(originalValue, searchValue) {

    if (!originalValue) {
        return "";
    }

    if (!searchValue) {
        return escapeHtml(originalValue);
    }

    const originalText = String(originalValue);
    const searchText = String(searchValue).trim();

    if (!searchText) {
        return escapeHtml(originalText);
    }

    // Escape RegExp special characters
    const escapedSearch = searchText.replace(
        /[.*+?^${}()|[\]\\]/g,
        '\\$&'
    );

    const regex = new RegExp(
        `(${escapedSearch})`,
        'gi'
    );

    const escapedOriginal = escapeHtml(originalText);

    return escapedOriginal.replace(
        regex,
        '<mark class="marks">$1</mark>'
    );
}
function escapeHtml(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

const STOP_WORDS = new Set([
    "a", "an", "the",
    "is", "am", "are", "was", "were",
    "be", "been", "being",
    "has", "have", "had",
    "do", "does", "did",
    "of", "in", "on", "at", "to", "for",
    "from", "with", "by",
    "and", "or", "but"
]);

function escapeRegex(str) {
    return str.replace(/[-[\]{}()*+?.,\\^$|#\s]/g, '\\$&');
}

// word + common plurals (rank/ranks, army/armies)
function wordPattern(word) {
    const escaped = escapeRegex(word);
    if (/y$/i.test(word)) {
        return escaped.slice(0, -1) + '(?:y|ies)';
    }
    return escaped + '(?:s|es)?';
}

function completeHighlighting(text, searchQuery) {
    if (!text) return '';

    if (Array.isArray(text)) {
        return text.map(item => completeHighlighting(item, searchQuery)).join('');
    }

    // Remove existing <mark> tags to avoid double-wrapping
    const cleanText = String(text).replace(/<\/?mark[^>]*>/gi, '');

    if (!searchQuery || typeof searchQuery !== 'string' || !searchQuery.trim()) {
        return `<li>${cleanText}</li>`;
    }

    const rawQuery = searchQuery.replace(/^["(]+|[")]+$/g, '').trim();
    const words = rawQuery.split(/\s+/).filter(Boolean);

    // Keywords = non-stop words; stop words = only those present in the query
    const keywords = [...new Set(words.filter(w => !STOP_WORDS.has(w.toLowerCase())))];
    const stops = [...new Set(words.filter(w => STOP_WORDS.has(w.toLowerCase())))];

    if (keywords.length === 0) return `<li>${cleanText}</li>`;

    const K = '(?:' + keywords.map(wordPattern).join('|') + ')';
    const S = stops.length
        ? '(?:' + stops.map(s => escapeRegex(s)).join('|') + ')'
        : null;

    // keyword, then repeatedly: (spaces [stopword spaces]* keyword)
    // [ \t]+ (not \s+) so a match never spans a line break
    const gap = S ? `[ \\t]+(?:${S}[ \\t]+)*` : '[ \\t]+';
    const regex = new RegExp(`\\b${K}(?:${gap}${K})*\\b`, 'gi');

    const highlighted = cleanText.replace(regex, m => `<mark class="marks">${m}</mark>`);

    return `${highlighted}`;
}