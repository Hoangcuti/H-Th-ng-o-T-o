document.addEventListener('DOMContentLoaded', function() {
    // SCROLL REVEAL ANIMATION
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('active');
            }
        });
    }, observerOptions);

    document.querySelectorAll('.reveal').forEach(el => {
        observer.observe(el);
    });

    // SMOOTH SCROLL FOR ANCHORS
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });

    // TOAST NOTIFICATIONS (IF ELEMENTS EXIST)
    const toasts = document.querySelectorAll('.system-toast');
    toasts.forEach(toast => {
        const type = toast.getAttribute('data-type');
        const msg = toast.getAttribute('data-msg');
        console.log(`[${type}] ${msg}`);
    });
});

/**
 * Live Search functionality for tables
 * @@param {string} inputId - ID of the search input element
 * @@param {string} tableId - ID of the table element
 */
function applyLiveSearch(inputId, tableId) {
    const searchInput = document.getElementById(inputId);
    const table = document.getElementById(tableId);
    
    if (!searchInput || !table) return;

    // Use 'input' event for real-time filtering as user types or pastes
    searchInput.addEventListener('input', function() {
        const filter = searchInput.value.toLowerCase().trim();
        const tbody = table.querySelector('tbody') || table;
        const rows = tbody.querySelectorAll('tr');

        rows.forEach(row => {
            // Check if any cell in the row contains the filter text
            const text = row.textContent.toLowerCase();
            if (text.includes(filter)) {
                row.style.display = ""; // Restore default display
                row.classList.add('animate-fade-in');
            } else {
                row.style.setProperty('display', 'none', 'important');
            }
        });
    });
}
