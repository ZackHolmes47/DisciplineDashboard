// =========================================================
// SIDEBAR TOGGLE
// Opens and closes the sidebar when the toggle button is clicked.
// =========================================================
function toggleSidebar(event) {
    if (event) {
        event.stopPropagation();
    }

    document.querySelector(".app-shell").classList.toggle("sidebar-collapsed");
}

// =========================================================
// REOPEN COLLAPSED SIDEBAR
// Lets the user click anywhere on the collapsed sidebar to open it again.
// =========================================================
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

// =========================================================
// SCROLL REVEAL
// Shows dashboard cards as they enter the screen.
// =========================================================
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

// =========================================================
// CHALLENGE STEPPER
// Updates challenge progress with plus and minus buttons.
// =========================================================
function updateChallengeProgress(challengeID, amount) {
    const input = document.getElementById(`challenge-progress-${challengeID}`);

    if (!input) {
        return;
    }

    const currentValue = parseFloat(input.value) || 0;
    const maxValue = parseFloat(input.max) || 0;

    let newValue = currentValue + amount;

    if (newValue < 0) {
        newValue = 0;
    }

    if (maxValue > 0 && newValue > maxValue) {
        newValue = maxValue;
    }

    input.value = newValue;
}

// =========================================================
// MOBILE SIDEBAR
// Opens and closes the sidebar on smaller screens.
// =========================================================
function toggleMobileSidebar() {
    document.body.classList.toggle("mobile-sidebar-open");
}