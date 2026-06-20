// dashboard.js

document.addEventListener('DOMContentLoaded', () => {
    // Sidebar Toggle
    const sidebar = document.getElementById('sidebar');
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');

    if (mobileMenuBtn && sidebar) {
        mobileMenuBtn.addEventListener('click', () => {
            sidebar.classList.toggle('-translate-x-full');
        });
    }

    // Modal Logic
    window.cardToDelete = null;
    const modals = document.querySelectorAll('.detailOverlay');
    const modalTriggers = document.querySelectorAll('[data-modal-target]');
    const modalCloseBtns = document.querySelectorAll('.modal-close-btn, [data-modal-close]');

    modalTriggers.forEach(trigger => {
        trigger.addEventListener('click', (e) => {
            e.preventDefault();
            const targetId = trigger.getAttribute('data-modal-target');
            const modal = document.getElementById(targetId);
            if (modal) {
                modal.classList.add('active');
                if (targetId === 'deleteModal') {
                    window.cardToDelete = trigger.closest('.property-card');
                }
            }
        });
    });

    // LocalStorage Logic for deleted properties
    const deletedProperties = JSON.parse(localStorage.getItem('deletedProperties') || '[]');

    // Handle Confirm Delete
    const confirmDeleteBtn = document.getElementById('confirmDeleteBtn');
    if (confirmDeleteBtn) {
        confirmDeleteBtn.addEventListener('click', () => {
            if (window.cardToDelete) {
                const titleElement = window.cardToDelete.querySelector('h3');
                if (titleElement) {
                    const title = titleElement.innerText.trim();
                    deletedProperties.push(title);
                    localStorage.setItem('deletedProperties', JSON.stringify(deletedProperties));
                }

                window.cardToDelete.remove();
                showToast('Property deleted successfully!', 'success');
                window.cardToDelete = null;
                if(typeof renderPaginationButtons === 'function') renderPaginationButtons();
            }
            document.getElementById('deleteModal').classList.remove('active');
        });
    }

    // On load, remove any properties that were previously deleted
    const allExistingCards = document.querySelectorAll('.property-card');
    allExistingCards.forEach(card => {
        const titleElement = card.querySelector('h3');
        if (titleElement && deletedProperties.includes(titleElement.innerText.trim())) {
            card.remove();
        }
    });

    modalCloseBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            modals.forEach(modal => modal.classList.remove('active'));
        });
    });

    // Close modal when clicking outside
    modals.forEach(modal => {
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                modal.classList.remove('active');
            }
        });
    });

    // Mock View Button Logic
    const propertyLinks = document.querySelectorAll('.property-card a');
    propertyLinks.forEach(link => {
        if(link.innerHTML.includes('fa-eye')) {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                showToast('Viewing property details...', 'success');
            });
        }
    });

    // Pagination Logic
    window.updatePagination = function() {
        const allCards = document.querySelectorAll('.property-card');
        if (allCards.length === 0) return;

        const pageSize = 3;
        let activePage = 1;
        const activePageBtn = document.querySelector('.pagination .page-btn.active');
        if (activePageBtn && !activePageBtn.querySelector('i')) {
            activePage = parseInt(activePageBtn.innerText.trim(), 10) || 1;
        }

        const startIndex = (activePage - 1) * pageSize;
        const endIndex = startIndex + pageSize;

        allCards.forEach((card, index) => {
            if (index >= startIndex && index < endIndex) {
                card.style.display = 'flex';
            } else {
                card.style.display = 'none';
            }
        });
    };

    window.renderPaginationButtons = function() {
        const pagination = document.querySelector('.pagination');
        if (!pagination) return;

        const allCards = document.querySelectorAll('.property-card');
        const pageSize = 3;
        const totalPages = Math.ceil(allCards.length / pageSize);

        let activePage = 1;
        const currentActiveBtn = pagination.querySelector('.page-btn.active');
        if (currentActiveBtn && !currentActiveBtn.querySelector('i')) {
            activePage = parseInt(currentActiveBtn.innerText.trim(), 10) || 1;
        }

        if (activePage > totalPages) activePage = totalPages || 1;

        pagination.innerHTML = '';
        if (totalPages === 0) return;

        const prevBtn = document.createElement('button');
        prevBtn.className = 'page-btn';
        prevBtn.innerHTML = '<i class="fa-solid fa-chevron-left"></i>';
        pagination.appendChild(prevBtn);

        for (let i = 1; i <= totalPages; i++) {
            const btn = document.createElement('button');
            btn.className = 'page-btn' + (i === activePage ? ' active' : '');
            btn.innerText = i;
            pagination.appendChild(btn);
        }

        const nextBtn = document.createElement('button');
        nextBtn.className = 'page-btn';
        nextBtn.innerHTML = '<i class="fa-solid fa-chevron-right"></i>';
        pagination.appendChild(nextBtn);

        const pageBtns = pagination.querySelectorAll('.page-btn');
        const numericBtns = Array.from(pageBtns).filter(btn => !btn.querySelector('i') && btn.innerText.trim() !== '<' && btn.innerText.trim() !== '>');

        pageBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                let currentActiveIndex = numericBtns.findIndex(b => b.classList.contains('active'));
                if (currentActiveIndex === -1) currentActiveIndex = 0;

                if (this.querySelector('i') || this.innerText.includes('<') || this.innerText.includes('>')) {
                    const isPrev = this.querySelector('.fa-chevron-left') || this.innerText.includes('<');
                    const isNext = this.querySelector('.fa-chevron-right') || this.innerText.includes('>');

                    if (isPrev && currentActiveIndex > 0) {
                        numericBtns.forEach(b => b.classList.remove('active'));
                        numericBtns[currentActiveIndex - 1].classList.add('active');
                        updatePagination();
                    } else if (isNext && currentActiveIndex < numericBtns.length - 1) {
                        numericBtns.forEach(b => b.classList.remove('active'));
                        numericBtns[currentActiveIndex + 1].classList.add('active');
                        updatePagination();
                    }
                } else {
                    numericBtns.forEach(b => b.classList.remove('active'));
                    this.classList.add('active');
                    updatePagination();
                }
            });
        });

        updatePagination();
    };

    renderPaginationButtons();

    // Filter Logic
    const filterBtns = document.querySelectorAll('.filter-btn');
    if (filterBtns.length > 0) {
        filterBtns.forEach(btn => {
            btn.addEventListener('click', function() {
                filterBtns.forEach(b => {
                    b.classList.remove('btn-gold');
                    b.classList.add('text-[#718096]', 'hover:bg-[#F4F6F9]', 'hover:text-[#1E3A5F]');
                });
                this.classList.remove('text-[#718096]', 'hover:bg-[#F4F6F9]', 'hover:text-[#1E3A5F]');
                this.classList.add('btn-gold');

                const filterType = this.getAttribute('data-filter').toLowerCase();
                const allCards = document.querySelectorAll('.property-card');

                allCards.forEach(card => {
                    const tagSpan = card.querySelector('.fa-tag').parentElement.innerText.trim().toLowerCase();
                    if (filterType === 'all' || tagSpan.includes(filterType)) {
                        card.style.display = 'flex';
                    } else {
                        card.style.display = 'none';
                    }
                });

                const pagination = document.querySelector('.pagination');
                if (pagination) {
                    pagination.style.display = filterType === 'all' ? 'flex' : 'none';
                }

                if (filterType === 'all') {
                    const firstPageBtn = document.querySelector('.pagination .page-btn:nth-child(2)');
                    if (firstPageBtn) firstPageBtn.click();
                }
            });
        });
    }
});

// Toast Notification Helper
window.showToast = function(message, type = 'success') {
    let toastContainer = document.getElementById('toastContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastContainer';
        document.body.appendChild(toastContainer);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;

    let icon = type === 'success' ? '<i class="fa-solid fa-check-circle"></i>' : '<i class="fa-solid fa-circle-exclamation"></i>';

    toast.innerHTML = `${icon} <span>${message}</span>`;
    toastContainer.appendChild(toast);

    void toast.offsetWidth;

    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
};

