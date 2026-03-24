// Execute when document is ready
document.addEventListener("DOMContentLoaded", function () {
    // 1. Initialize Toasts from TempData
    const toasts = document.querySelectorAll('.system-toast');
    toasts.forEach(toast => {
        const type = toast.getAttribute('data-type');
        const msg = toast.getAttribute('data-msg');
        showToast(type, msg);
    });

    // 2. Global interactive elements
    const securityToggles = document.querySelectorAll('.security-toggle');
    securityToggles.forEach(toggle => {
        toggle.addEventListener('click', function(e) {
            // Check if clicking inside label to avoid double trigger
            if(e.target.tagName !== 'INPUT' && e.target.tagName !== 'LABEL' && e.target.tagName !== 'SPAN') {
                const cb = this.querySelector('input[type="checkbox"]');
                if(cb) cb.checked = !cb.checked;
            }
            if(e.target.tagName === 'INPUT') {
                const isChecked = e.target.checked;
                showToast('success', isChecked ? 'Đã BẬT thông báo bảo mật qua Gmail!' : 'Đã TẮT thông báo bảo mật qua Gmail!');
            }
        });
    });
});

function showToast(type, message) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `custom-toast toast-${type}`;
    
    // Icon based on type
    const icon = type === 'success' ? '✅' : '❌';
    
    toast.innerHTML = `
        <div class="toast-icon">${icon}</div>
        <div class="toast-content">
            <div class="toast-title">${type === 'success' ? 'Thành công' : 'Lỗi'}</div>
            <div class="toast-message">${message}</div>
        </div>
        <div class="toast-close" onclick="this.parentElement.remove()">✕</div>
    `;
    
    container.appendChild(toast);
    
    // Animate in
    setTimeout(() => toast.classList.add('show'), 10);
    
    // Auto remove after 4s
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 4000);
}
