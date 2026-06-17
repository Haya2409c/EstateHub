document.addEventListener('DOMContentLoaded', () => {
    initLeads();
});

function initLeads() {
    if (typeof AOS !== 'undefined') AOS.init({ duration: 800, once: true, offset: 50 });

    const leads = getDB('leads');
    const tbody = document.getElementById('leadTable');
    
    const searchInput = document.getElementById('leadSearch');
    const statusFilter = document.getElementById('leadStatus');
    const propertyFilter = document.getElementById('leadProperty');
    const dateFilter = document.getElementById('leadDate');
    const exportBtn = document.getElementById('exportLeadsBtn');

    if(!tbody) return;

    function renderTable(data) {
        tbody.innerHTML = '';
        if(data.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="p-8 text-center text-gray-400">No leads found.</td></tr>';
            const pageEl = document.getElementById('leadPagination');
            if(pageEl) pageEl.textContent = 'Showing 0 to 0 of 0 entries';
            return;
        }

        data.forEach(lead => {
            const tr = document.createElement('tr');
            tr.setAttribute('data-aos', 'fade-up');
            
            const badgeClass = lead.status === 'New' ? 'bg-blue-500/20 text-blue-400 border-blue-500/20' : 
                             (lead.status === 'Contacted' ? 'bg-yellow-500/20 text-yellow-400 border-yellow-500/20' : 'bg-green-500/20 text-green-400 border-green-500/20');
            
            tr.innerHTML = `
                <td class="p-4 pl-6 border-b border-white/5">
                    <div class="flex items-center gap-3">
                        <div class="w-10 h-10 rounded-full bg-gold-500/10 text-gold-500 flex items-center justify-center font-bold border border-gold-500/20">
                            ${lead.name.charAt(0)}
                        </div>
                        <div>
                            <p class="text-white font-medium">${lead.name}</p>
                            <p class="text-xs text-gray-400">${lead.email}</p>
                        </div>
                    </div>
                </td>
                <td class="p-4 border-b border-white/5 text-gray-300">${lead.phone}</td>
                <td class="p-4 border-b border-white/5 text-gray-300">${lead.property}</td>
                <td class="p-4 border-b border-white/5">
                    <span class="px-2.5 py-1 rounded-full text-xs font-medium border ${badgeClass}">${lead.status}</span>
                </td>
                <td class="p-4 border-b border-white/5 text-gray-400 text-sm">${lead.date}</td>
                <td class="p-4 pr-6 border-b border-white/5 text-right">
                    <div class="flex items-center justify-end gap-2">
                        <button class="w-8 h-8 rounded bg-white/5 hover:bg-white/10 text-gray-400 hover:text-gold-500 transition-theme flex items-center justify-center" title="Contact" onclick="showToast('Initiating contact...', 'info')"><i class="fa-regular fa-envelope"></i></button>
                    </div>
                </td>
            `;
            tbody.appendChild(tr);
        });

        const pageEl = document.getElementById('leadPagination');
        if(pageEl) pageEl.textContent = `Showing 1 to ${data.length} of ${data.length} entries`;
        
        if (typeof AOS !== 'undefined') AOS.refresh();
    }

    function updateStats() {
        const data = getDB('leads');
        const tLeads = document.getElementById('statTotalLeads');
        const nInq = document.getElementById('statNewInquiries');
        const cont = document.getElementById('statContacted');
        const clos = document.getElementById('statClosed');
        
        if(tLeads) tLeads.textContent = data.length;
        if(nInq) nInq.textContent = data.filter(l => l.status === 'New').length;
        if(cont) cont.textContent = data.filter(l => l.status === 'Contacted').length;
        if(clos) clos.textContent = data.filter(l => l.status === 'Qualified').length; // Map closed to qualified for demo
    }

    function filterData() {
        let filtered = getDB('leads');
        
        if(searchInput && searchInput.value) {
            const t = searchInput.value.toLowerCase();
            filtered = filtered.filter(l => l.name.toLowerCase().includes(t) || l.email.toLowerCase().includes(t));
        }
        if(statusFilter && statusFilter.value && statusFilter.value !== 'all') {
            const statusMap = { 'new': 'New', 'contacted': 'Contacted', 'qualified': 'Qualified' };
            filtered = filtered.filter(l => l.status === statusMap[statusFilter.value]);
        }
        // Property filter is pure visual for demo unless specifically mapped
        
        renderTable(filtered);
    }

    if(searchInput) {
        searchInput.addEventListener('input', filterData);
        searchInput.addEventListener('keyup', filterData);
    }
    if(statusFilter) statusFilter.addEventListener('change', filterData);
    if(propertyFilter) propertyFilter.addEventListener('change', filterData);
    if(dateFilter) dateFilter.addEventListener('change', filterData);
    if(exportBtn) exportBtn.addEventListener('click', () => showToast('Leads exported successfully to CSV!', 'success'));

    updateStats();
    setTimeout(() => {
        renderTable(leads);
    }, 500);
}
