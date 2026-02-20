// Optional: basic reference id (don’t expose stack traces)
document.getElementById("y").textContent = new Date().getFullYear();
// If your server sets a header or query like ?rid=..., you could read it here.
// Fallback: lightweight pseudo ID (client-side only; replace with server value in ASP.NET).
const path = window.location.pathname;
const parts = path.split('/').filter(Boolean);
const guid = parts[parts.length - 1];

document.getElementById("ref-id").textContent = guid;

document.addEventListener("DOMContentLoaded", function () {
    const btn = document.getElementById("btnGoBack");
    if (btn) {
        btn.addEventListener("click", function () {
            window.history.back();
        });
    }
});