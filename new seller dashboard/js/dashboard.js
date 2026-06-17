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

    // Load User Name from LocalStorage
    const savedName = localStorage.getItem('userName');
    if (savedName) {
        // Update elements with specific class if they exist
        document.querySelectorAll('.display-user-name, .sidebar-user-name').forEach(el => {
            el.innerText = savedName;
        });
        
        // Update specific input if it exists
        const fullNameInput = document.getElementById('fullNameInput');
        if (fullNameInput) {
            fullNameInput.value = savedName;
        }
        
        // Update avatar initials
        const initials = savedName.split(' ').map(n => n[0]).join('').substring(0, 2).toUpperCase();
        const sidebarAvatar = document.querySelector('.sidebar-avatar');
        if (sidebarAvatar) {
            sidebarAvatar.innerText = initials;
        }

        // Update default ui-avatars image if no custom image is saved
        if (!localStorage.getItem('profileImage')) {
            const defaultUrl = `https://ui-avatars.com/api/?name=${encodeURIComponent(savedName)}&background=1E3A5F&color=fff&size=150`;
            document.querySelectorAll('img[alt="Avatar"], img#profileAvatar, img#previewImage, img#headerAvatar').forEach(img => {
                if (img.src.includes('ui-avatars.com')) {
                    img.src = defaultUrl;
                }
            });
        }
    }

    // Load Custom Profile Image from LocalStorage
    const savedImage = localStorage.getItem('profileImage');
    if (savedImage) {
        document.querySelectorAll('img[alt="Avatar"], img#profileAvatar, img#previewImage, img#headerAvatar').forEach(img => {
            img.src = savedImage;
        });
        
        const removeBtn = document.getElementById('removeImageBtn');
        if (removeBtn) removeBtn.classList.remove('hidden');
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
                // Get the title to save to localStorage
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
                // Update active state
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

                // Hide pagination when a specific filter is applied
                const pagination = document.querySelector('.pagination');
                if (pagination) {
                    pagination.style.display = filterType === 'all' ? 'flex' : 'none';
                }
                
                // Reset pagination to page 1 if 'all' is clicked
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

    // Trigger reflow
    void toast.offsetWidth;

    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

function setActiveNav() {
    const current = window.location.pathname.split('/').pop() || 'index.html';
    document.querySelectorAll('.nav-item[data-page]').forEach(item => {
        item.classList.toggle('active', item.dataset.page === current);
    });
}
document.addEventListener('DOMContentLoaded', setActiveNav);

// Add Property Logic
document.addEventListener('DOMContentLoaded', () => {
    const addPropertyForm = document.getElementById('addPropertyForm');
    if (addPropertyForm) {
        addPropertyForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Get title and price as best effort
            const inputs = this.querySelectorAll('input[type="text"], input[type="number"]');
            let title = 'Newly Published Property';
            let price = '0';
            
            inputs.forEach(input => {
                if (input.placeholder.includes('Villa') || input.placeholder.includes('Title')) title = input.value;
                if (input.placeholder.includes('50000000') || input.type === 'number' && price === '0') price = input.value;
            });
            
            // Save to localStorage
            const newProperty = {
                title: title,
                price: price,
                dateAdded: new Date().getTime()
            };
            
            const addedProperties = JSON.parse(localStorage.getItem('addedProperties') || '[]');
            addedProperties.push(newProperty);
            localStorage.setItem('addedProperties', JSON.stringify(addedProperties));
            
            showToast('Property published successfully!', 'success');
            setTimeout(() => {
                window.location.href = 'my-properties.html';
            }, 1000);
        });
    }
    
    // Render added properties on My Properties page
    const propertyGrid = document.querySelector('.grid.grid-cols-1.md\\:grid-cols-2.lg\\:grid-cols-3');
    if (propertyGrid) {
        const addedProperties = JSON.parse(localStorage.getItem('addedProperties') || '[]');
        
        addedProperties.forEach(prop => {
            const div = document.createElement('div');
            div.className = 'property-card glass-panel overflow-hidden flex flex-col group';
            div.innerHTML = `
                <div class="relative h-[200px] overflow-hidden">
                    <img src="https://images.unsplash.com/photo-1512917774080-9991f1c4c750?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80" alt="Property" class="w-full h-full object-cover group-hover:scale-105 transition-transform duration-500">
                    <div class="absolute top-3 left-3 bg-[#dcfce7] text-[#16a34a] text-xs font-bold px-3 py-1 rounded-full shadow-sm">Active</div>
                </div>
                <div class="p-5 flex-1 flex flex-col">
                    <h3 class="font-outfit font-semibold text-lg text-[#1E3A5F] mb-1 line-clamp-1">${prop.title}</h3>
                    <div class="font-poppins text-sm text-[#718096] mb-3 flex items-center gap-2">
                        <span><i class="fa-solid fa-tag text-xs"></i> Sell</span>
                        <span>•</span>
                        <span><i class="fa-solid fa-location-dot text-xs"></i> Karachi</span>
                    </div>
                    <div class="font-outfit font-bold text-[#1E3A5F] text-xl mt-auto">PKR ${prop.price}</div>
                </div>
                <div class="p-4 border-t border-[#1E3A5F]/10 flex justify-between gap-2 bg-[#F8F7F4]/50">
                    <a href="#" class="btn-outline-navy flex-1 text-sm py-2" onclick="event.preventDefault(); showToast('Viewing property details...', 'success');"><i class="fa-solid fa-eye mr-1"></i> View</a>
                    <a href="#" class="btn-outline-navy flex-1 text-sm py-2" onclick="event.preventDefault()"><i class="fa-solid fa-pen mr-1"></i> Edit</a>
                    <a href="#" class="btn-outline-navy flex-1 text-sm py-2 text-red-500 border-red-200 hover:bg-red-50 hover:text-red-600" data-modal-target="deleteModal"><i class="fa-solid fa-trash mr-1"></i> Delete</a>
                </div>
            `;
            
            // Only add if not deleted
            const deletedProps = JSON.parse(localStorage.getItem('deletedProperties') || '[]');
            if (!deletedProps.includes(prop.title)) {
                // Insert at beginning of grid
                propertyGrid.insertBefore(div, propertyGrid.firstChild);
            }
        });
        
        // Re-bind modal triggers for newly added cards
        const newDeleteBtns = propertyGrid.querySelectorAll('[data-modal-target="deleteModal"]');
        const deleteModal = document.getElementById('deleteModal');
        newDeleteBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.preventDefault();
                deleteModal.classList.add('active');
                window.cardToDelete = btn.closest('.property-card'); // Note: we need cardToDelete to be global, or attach it another way
            });
        });
        
        // update pagination to include newly added cards
        if(typeof renderPaginationButtons === 'function') renderPaginationButtons();
    }
});
