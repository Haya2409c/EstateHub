// Mock Data Initialization
const MOCK_PROPERTIES = [
    {
        id: 1,
        title: "Downtown Penthouse",
        location: "Downtown Dubai, UAE",
        type: "Penthouse",
        price: 4500000,
        status: "active",
        beds: 4,
        baths: 5,
        area: 4200,
        image: "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80",
        featured: true,
        dateAdded: new Date(Date.now() - 86400000 * 2).toISOString()
    },
    {
        id: 2,
        title: "Luxury Villa Dubai",
        location: "Emirates Hills, Dubai",
        type: "Villa",
        price: 8200000,
        status: "active",
        beds: 6,
        baths: 7,
        area: 8500,
        image: "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80",
        featured: false,
        dateAdded: new Date(Date.now() - 86400000 * 5).toISOString()
    },
    {
        id: 3,
        title: "Palm Jumeirah Apartment",
        location: "Palm Jumeirah, Dubai",
        type: "Apartment",
        price: 1850000,
        status: "sold",
        beds: 3,
        baths: 3,
        area: 2100,
        image: "https://images.unsplash.com/photo-1600607687939-ce8a6c25118c?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80",
        featured: false,
        dateAdded: new Date(Date.now() - 86400000 * 30).toISOString()
    }
];

const MOCK_LEADS = [
    {
        id: 1,
        name: "Eleanor Shellstrop",
        email: "eleanor@example.com",
        phone: "+1 (555) 123-4567",
        propertyId: 1,
        propertyTitle: "Downtown Penthouse",
        date: new Date().toISOString(),
        status: "new",
        type: "Buyer Inquiry"
    },
    {
        id: 2,
        name: "John Doe",
        email: "john.doe@mail.com",
        phone: "+44 20 7946 0958",
        propertyId: 2,
        propertyTitle: "Luxury Villa Dubai",
        date: new Date(Date.now() - 86400000).toISOString(),
        status: "contacted",
        type: "Property Viewing Request"
    },
    {
        id: 3,
        name: "Sarah Jenkins",
        email: "s.jenkins@invest.com",
        phone: "+1 (555) 987-6543",
        propertyId: 3,
        propertyTitle: "Palm Jumeirah Apartment",
        date: new Date(Date.now() - 86400000 * 2).toISOString(),
        status: "negotiating",
        type: "Buyer Inquiry"
    }
];

const MOCK_NOTIFICATIONS = [
    {
        id: 1,
        type: "lead",
        title: "New Lead Received",
        message: "Eleanor Shellstrop sent an inquiry for Downtown Penthouse.",
        date: new Date().toISOString(),
        read: false,
        icon: "fa-user-plus",
        color: "blue"
    },
    {
        id: 2,
        type: "listing",
        title: "Property Approved",
        message: "Your new listing Luxury Villa Dubai has been reviewed and is now live.",
        date: new Date(Date.now() - 7200000).toISOString(),
        read: false,
        icon: "fa-circle-check",
        color: "green"
    },
    {
        id: 3,
        type: "alert",
        title: "Package Expiring Soon",
        message: "Your Agency Elite Premium package will automatically renew on Dec 31, 2023.",
        date: new Date(Date.now() - 86400000 * 3).toISOString(),
        read: true,
        icon: "fa-crown",
        color: "yellow"
    }
];

// Initialize LocalStorage Database
function initDB() {
    if (!localStorage.getItem('properties')) {
        // Future Database Save (Properties)
        localStorage.setItem('properties', JSON.stringify(MOCK_PROPERTIES));
    }
    if (!localStorage.getItem('leads')) {
        // Future Database Save (Leads)
        localStorage.setItem('leads', JSON.stringify(MOCK_LEADS));
    }
    if (!localStorage.getItem('notifications')) {
        // Future Database Save (Notifications)
        localStorage.setItem('notifications', JSON.stringify(MOCK_NOTIFICATIONS));
    }
}

// Helpers
// Future API Call - Get Data
function getDB(key) {
    return JSON.parse(localStorage.getItem(key)) || [];
}

// Future API Call - Save Data
function setDB(key, data) {
    localStorage.setItem(key, JSON.stringify(data));
}

function formatMoney(amount) {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', maximumFractionDigits: 0 }).format(amount);
}

function formatDate(isoString) {
    const options = { month: 'short', day: 'numeric', year: 'numeric' };
    return new Date(isoString).toLocaleDateString('en-US', options);
}

// Global Custom Toast System
function showToast(message, type = 'success') {
    const toastContainerId = 'toast-container';
    let container = document.getElementById(toastContainerId);
    
    if (!container) {
        container = document.createElement('div');
        container.id = toastContainerId;
        container.className = 'fixed bottom-4 right-4 z-50 flex flex-col gap-2';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = \`glass-card p-4 rounded-xl flex items-center gap-3 transform translate-y-full opacity-0 transition-all duration-300 min-w-[250px]\`;
    
    const icon = document.createElement('i');
    if (type === 'success') {
        icon.className = 'fa-solid fa-circle-check text-green-400 text-xl';
        toast.style.borderLeft = '4px solid #4ade80';
    } else if (type === 'error') {
        icon.className = 'fa-solid fa-circle-xmark text-red-400 text-xl';
        toast.style.borderLeft = '4px solid #f87171';
    } else {
        icon.className = 'fa-solid fa-circle-info text-blue-400 text-xl';
        toast.style.borderLeft = '4px solid #60a5fa';
    }

    const text = document.createElement('p');
    text.className = 'text-white text-sm font-medium';
    text.textContent = message;

    toast.appendChild(icon);
    toast.appendChild(text);
    container.appendChild(toast);

    // Animate in
    setTimeout(() => {
        toast.classList.remove('translate-y-full', 'opacity-0');
    }, 10);

    // Animate out and remove
    setTimeout(() => {
        toast.classList.add('translate-y-full', 'opacity-0');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// Modal System
function showModal(title, message, confirmText, onConfirm, cancelText = 'Cancel') {
    const overlay = document.createElement('div');
    overlay.className = 'fixed inset-0 bg-black/70 backdrop-blur-sm z-50 flex items-center justify-center opacity-0 transition-opacity duration-300';
    
    const modal = document.createElement('div');
    modal.className = 'glass-card rounded-2xl p-6 md:p-8 max-w-md w-full mx-4 transform scale-95 opacity-0 transition-all duration-300 border-t-4 border-gold-500 shadow-premium';
    
    modal.innerHTML = \`
        <div class="text-center">
            <h3 class="text-2xl font-bold text-white mb-2">\${title}</h3>
            <div class="text-gray-400 text-sm mb-6 modal-message text-left">\${message}</div>
            <div class="flex gap-3 justify-center">
                <button class="modal-cancel px-6 py-2.5 rounded-full border border-white/20 text-white hover:bg-white/5 transition-theme font-medium text-sm">\${cancelText}</button>
                <button class="modal-confirm px-6 py-2.5 rounded-full bg-gold-500 hover:bg-gold-600 text-navy-900 shadow-gold-glow transition-theme font-bold text-sm" style="\${confirmText ? '' : 'display:none;'}">\${confirmText || 'Confirm'}</button>
            </div>
        </div>
    \`;

    overlay.appendChild(modal);
    document.body.appendChild(overlay);

    // Animate in
    setTimeout(() => {
        overlay.classList.remove('opacity-0');
        modal.classList.remove('scale-95', 'opacity-0');
    }, 10);

    // Handlers
    const close = () => {
        overlay.classList.add('opacity-0');
        modal.classList.add('scale-95', 'opacity-0');
        setTimeout(() => overlay.remove(), 300);
    };

    modal.querySelector('.modal-cancel').addEventListener('click', close);
    if(confirmText) {
        modal.querySelector('.modal-confirm').addEventListener('click', () => {
            if(onConfirm) onConfirm();
            close();
        });
    }
}

// Loader Spinner Component
function showLoader() {
    const loaderId = 'global-loader';
    if(document.getElementById(loaderId)) return;
    const loader = document.createElement('div');
    loader.id = loaderId;
    loader.className = 'fixed inset-0 bg-black/80 backdrop-blur-sm z-[100] flex items-center justify-center';
    loader.innerHTML = \`
        <div class="flex flex-col items-center">
            <div class="w-12 h-12 border-4 border-gold-500/30 border-t-gold-500 rounded-full animate-spin mb-4"></div>
            <p class="text-gold-500 font-medium tracking-widest text-sm uppercase">Loading</p>
        </div>
    \`;
    document.body.appendChild(loader);
}

function hideLoader() {
    const loader = document.getElementById('global-loader');
    if(loader) loader.remove();
}

// Global App Initialization
document.addEventListener('DOMContentLoaded', () => {
    // GSAP Animations
    if(typeof gsap !== 'undefined') {
        gsap.from(".glass-card", {
            opacity: 0,
            y: 30,
            duration: 0.8,
            stagger: 0.1,
            ease: "power3.out"
        });
    }

    // Bell Dropdown Logic
    const bellBtn = document.getElementById('navBellBtn');
    const dropdown = document.getElementById('navNotificationDropdown');
    const dropList = document.getElementById('dropdownNotifsList');
    const markDropReadBtn = document.getElementById('markAllReadDropBtn');
    
    if(bellBtn && dropdown) {
        bellBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            const isHidden = dropdown.classList.contains('hidden');
            
            if(isHidden) {
                // Populate first
                const notifs = getDB('notifications') || [];
                if(dropList) {
                    dropList.innerHTML = '';
                    const recent = notifs.slice(0, 3);
                    if(recent.length === 0) {
                        dropList.innerHTML = '<p class="text-xs text-gray-500 text-center py-4">No notifications.</p>';
                    } else {
                        recent.forEach(n => {
                            dropList.innerHTML += `
                            <div class="p-3 rounded-lg ${n.unread ? 'bg-white/5 border border-white/10' : 'opacity-70'} flex items-start gap-3 transition-theme hover:bg-white/10 cursor-pointer">
                                <div class="w-8 h-8 rounded-full flex-shrink-0 flex items-center justify-center ${n.type === 'message' ? 'bg-blue-500/20 text-blue-400' : 'bg-gold-500/20 text-gold-500'}">
                                    <i class="fa-solid ${n.icon} text-sm"></i>
                                </div>
                                <div>
                                    <h5 class="text-sm font-bold text-white leading-tight mb-1">${n.title}</h5>
                                    <p class="text-xs text-gray-400 line-clamp-2">${n.message}</p>
                                </div>
                                ${n.unread ? '<div class="w-2 h-2 rounded-full bg-red-500 mt-1 flex-shrink-0"></div>' : ''}
                            </div>`;
                        });
                    }
                }
                
                // Show
                dropdown.classList.remove('hidden');
                // Small delay to allow block render before transition
                setTimeout(() => {
                    dropdown.classList.remove('opacity-0', 'scale-95');
                    dropdown.classList.add('opacity-100', 'scale-100');
                }, 10);
            } else {
                // Hide
                dropdown.classList.remove('opacity-100', 'scale-100');
                dropdown.classList.add('opacity-0', 'scale-95');
                setTimeout(() => dropdown.classList.add('hidden'), 200);
            }
        });

        // Close when clicking outside
        document.addEventListener('click', (e) => {
            if(!dropdown.contains(e.target) && !bellBtn.contains(e.target)) {
                dropdown.classList.remove('opacity-100', 'scale-100');
                dropdown.classList.add('opacity-0', 'scale-95');
                setTimeout(() => dropdown.classList.add('hidden'), 200);
            }
        });
    }

    if(markDropReadBtn) {
        markDropReadBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            let notifs = getDB('notifications');
            notifs.forEach(n => n.unread = false);
            setDB('notifications', notifs);
            updateGlobalNotificationCount();
            bellBtn.click(); // Close and reopen to refresh
            setTimeout(() => bellBtn.click(), 250);
            showToast('All marked as read', 'success');
        });
    }

    initDB();

    // Initialize AOS
    if (typeof AOS !== 'undefined') {
        AOS.init({
            duration: 800,
            once: true,
            offset: 50
        });
    }
    
    updateGlobalNotificationCount();
});

// Update global notification badge in navbar
function updateGlobalNotificationCount() {
    const notifications = getDB('notifications'); // Note: changed from luxe_notifications
    const unread = notifications.filter(n => !n.read).length;
    const badges = document.querySelectorAll('.fa-bell + span.absolute');
    
    badges.forEach(badge => {
        if (unread > 0) {
            badge.style.display = 'block';
        } else {
            badge.style.display = 'none';
        }
    });
}

// Global Initializers
document.addEventListener('DOMContentLoaded', () => {
    // 1. Dynamic Footer Year
    document.querySelectorAll('.currentYear').forEach(el => {
        el.textContent = new Date().getFullYear();
    });

    // 2. Global AOS Initialization
    if(typeof AOS !== 'undefined') {
        AOS.init({ duration: 800, once: true, offset: 50 });
    }

    // 3. Sync Notification Bell Globally
    function updateGlobalNotificationCount() {
        const notifs = getDB('notifications') || [];
        const unreadCount = notifs.filter(n => n.unread).length;
        const badges = document.querySelectorAll('#navUnreadBadge');
        
        badges.forEach(badge => {
            if(unreadCount > 0) {
                badge.classList.remove('hidden');
            } else {
                badge.classList.add('hidden');
            }
        });
    }
    updateGlobalNotificationCount();
    window.updateGlobalNotificationCount = updateGlobalNotificationCount; // Expose globally
});
