function toggleSidebar(event) {
    if (event) {
        event.stopPropagation();
    }

    document.querySelector(".app-shell").classList.toggle("sidebar-collapsed");
}

document.addEventListener("DOMContentLoaded", function () {
    const sidebar = document.querySelector(".sidebar");
    const appShell = document.querySelector(".app-shell");

    if (sidebar && appShell) {
        sidebar.addEventListener("click", function (event) {
            if (appShell.classList.contains("sidebar-collapsed")) {
                appShell.classList.remove("sidebar-collapsed");
            }
        });
    }
});
const revealElements = document.querySelectorAll(".reveal");

function revealOnScroll() {
    revealElements.forEach(el => {
        const rect = el.getBoundingClientRect();

        if (rect.top < window.innerHeight - 90) {
            el.classList.add("visible");
        }
    });
}

window.addEventListener("scroll", revealOnScroll);
window.addEventListener("load", revealOnScroll);