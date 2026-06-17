document.addEventListener('DOMContentLoaded', () => {
    initListings();
});

let currentPage = 1;
const itemsPerPage = 5;

function initListings() {
    const properties = getDB('properties') || [];
    const tbody = document.getElementById('propertyTableBody');
    const searchInput = document.getElementById('listSearchInput');
    const statusFilter = document.getElementById('listStatusFilter');
    const sortFilter = document.getElementById('listSortFilter');
    
    // Fallback if loading fails or takes too long
    let loaded = false;
    setTimeout(() => {
        if(!loaded && tbody) {
            tbody.innerHTML = '<tr><td colspan="7" class="p-8 text-center text-red-400">Failed to load properties. Please refresh or check connection.</td></tr>';
        }
    }, 5000);

    function updateStats(data) {
        const total = document.getElementById('statTotal');
        const active = document.getElementById('statActive');
        const featured = document.getElementById('statFeatured');
        
        if(total) total.textContent = data.length;
        if(active) active.textContent = data.filter(p => p.status === 'active').length;
        if(featured) featured.textContent = data.filter(p => p.featured).length;
    }

    function renderPagination(totalItems) {
        const controls = document.getElementById('paginationControls');
        const totalPages = Math.ceil(totalItems / itemsPerPage) || 1;
        if(!controls) return;
        
        controls.innerHTML = '';
        
        // Prev
        const prevBtn = document.createElement('button');
        prevBtn.className = `px-3 py-1 rounded-lg border border-white/10 text-sm font-medium transition-theme ${currentPage === 1 ? 'opacity-50 cursor-not-allowed text-gray-500' : 'text-gray-400 hover:text-white hover:bg-white/5'}`;
        prevBtn.textContent = 'Prev';
        if(currentPage === 1) prevBtn.disabled = true;
        prevBtn.onclick = () => { if(currentPage > 1) { currentPage--; filterData(); } };
        controls.appendChild(prevBtn);

        // Numbers
        for(let i=1; i<=totalPages; i++) {
            const pageBtn = document.createElement('button');
            pageBtn.className = `w-8 h-8 rounded-lg flex items-center justify-center text-sm font-medium transition-theme ${currentPage === i ? 'bg-gold-500 text-navy-900 shadow-gold-glow' : 'text-gray-400 hover:text-white hover:bg-white/5'}`;
            pageBtn.textContent = i;
            pageBtn.onclick = () => { currentPage = i; filterData(); };
            controls.appendChild(pageBtn);
        }

        // Next
        const nextBtn = document.createElement('button');
        nextBtn.className = `px-3 py-1 rounded-lg border border-white/10 text-sm font-medium transition-theme ${currentPage === totalPages ? 'opacity-50 cursor-not-allowed text-gray-500' : 'text-gray-400 hover:text-white hover:bg-white/5'}`;
        nextBtn.textContent = 'Next';
        if(currentPage === totalPages) nextBtn.disabled = true;
        nextBtn.onclick = () => { if(currentPage < totalPages) { currentPage++; filterData(); } };
        controls.appendChild(nextBtn);
    }

    function renderTable(data) {
        if(!tbody) return;
        loaded = true;
        
        const startIdx = (currentPage - 1) * itemsPerPage;
        const pageData = data.slice(startIdx, startIdx + itemsPerPage);
        
        tbody.innerHTML = '';
        if(data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="p-8 text-center text-gray-400">No properties found.</td></tr>';
            const pageEl = document.getElementById('paginationText');
            if(pageEl) pageEl.textContent = 'Showing 0 to 0 of 0 entries';
            renderPagination(0);
            return;
        }

        pageData.forEach(prop => {
            const tr = document.createElement('tr');
            tr.setAttribute('data-aos', 'fade-up');
            
            const badgeClass = prop.status === 'active' ? 'bg-green-500/20 text-green-400 border-green-500/20' : 
                             (prop.status === 'pending' ? 'bg-yellow-500/20 text-yellow-400 border-yellow-500/20' : 'bg-red-500/20 text-red-400 border-red-500/20');
            
            tr.innerHTML = `
                <td class="p-4 pl-6 border-b border-white/5">
                    <div class="flex items-center gap-3">
                        <img src="${prop.image}" alt="Property" class="w-12 h-12 rounded-lg object-cover">
                        <div>
                            <p class="text-white font-medium">${prop.title}</p>
                            <p class="text-xs text-gray-400">${prop.location}</p>
                        </div>
                    </div>
                </td>
                <td class="p-4 border-b border-white/5 text-gold-500 font-medium">${prop.price}</td>
                <td class="p-4 border-b border-white/5 text-gray-300">${prop.type}</td>
                <td class="p-4 border-b border-white/5 text-gray-400 text-sm">${prop.dateAdded || '-'}</td>
                <td class="p-4 border-b border-white/5">
                    <span class="px-2.5 py-1 rounded-full text-xs font-medium border ${badgeClass} capitalize">${prop.status}</span>
                </td>
                <td class="p-4 border-b border-white/5">
                    <div class="flex items-center gap-1">
                        <i class="fa-solid fa-eye text-gray-500 text-xs"></i>
                        <span class="text-sm text-gray-300">${prop.views || 0}</span>
                    </div>
                </td>
                <td class="p-4 pr-6 border-b border-white/5 text-right">
                    <div class="flex items-center justify-end gap-2">
                        <a href="edit-property.html?id=${prop.id}" class="w-8 h-8 rounded bg-white/5 hover:bg-gold-500/20 text-gray-400 hover:text-gold-500 transition-theme flex items-center justify-center" title="Edit">
                            <i class="fa-regular fa-pen-to-square"></i>
                        </a>
                        <button class="w-8 h-8 rounded bg-white/5 hover:bg-red-500/20 text-gray-400 hover:text-red-400 transition-theme flex items-center justify-center delete-prop-btn" data-id="${prop.id}" title="Delete">
                            <i class="fa-regular fa-trash-can"></i>
                        </button>
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });

        // Add Delete Listeners
        document.querySelectorAll('.delete-prop-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const id = parseInt(e.currentTarget.getAttribute('data-id'));
                if(confirm('Are you sure you want to delete this property?')) {
                    const newProps = getDB('properties').filter(p => p.id !== id);
                    setDB('properties', newProps);
                    showToast('Property deleted successfully!', 'success');
                    filterData(); // Refresh table safely
                }
            });
        });

        const pageEl = document.getElementById('paginationText');
        if(pageEl) pageEl.textContent = `Showing ${startIdx + 1} to ${Math.min(startIdx + itemsPerPage, data.length)} of ${data.length} entries`;
        renderPagination(data.length);
        
        if (typeof AOS !== 'undefined') AOS.refresh();
    }

    function filterData() {
        let filtered = getDB('properties') || [];
        
        if(searchInput && searchInput.value) {
            const t = searchInput.value.toLowerCase();
            filtered = filtered.filter(p => p.title.toLowerCase().includes(t) || p.location.toLowerCase().includes(t));
        }
        if(statusFilter && statusFilter.value && statusFilter.value !== 'all') {
            filtered = filtered.filter(p => p.status === statusFilter.value);
        }
        if(sortFilter && sortFilter.value) {
            if(sortFilter.value === 'newest') filtered.sort((a,b) => b.id - a.id);
            if(sortFilter.value === 'oldest') filtered.sort((a,b) => a.id - b.id);
            if(sortFilter.value === 'price-high') {
                filtered.sort((a,b) => parseInt(b.price.replace(/\D/g,'')) - parseInt(a.price.replace(/\D/g,'')));
            }
            if(sortFilter.value === 'price-low') {
                filtered.sort((a,b) => parseInt(a.price.replace(/\D/g,'')) - parseInt(b.price.replace(/\D/g,'')));
            }
        }
        
        // Ensure page doesn't exceed new limits
        const totalPages = Math.ceil(filtered.length / itemsPerPage) || 1;
        if(currentPage > totalPages) currentPage = totalPages;
        
        updateStats(getDB('properties')); // Stats usually reflect total universe
        renderTable(filtered);
    }

    if(searchInput) {
        searchInput.addEventListener('input', () => { currentPage = 1; filterData(); });
    }
    if(statusFilter) statusFilter.addEventListener('change', () => { currentPage = 1; filterData(); });
    if(sortFilter) sortFilter.addEventListener('change', () => { currentPage = 1; filterData(); });

    // Initial render
    setTimeout(() => {
        filterData();
    }, 500);
}
