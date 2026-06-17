document.addEventListener('DOMContentLoaded', () => {
    initDashboard();
});

function initDashboard() {
    // Dynamic Renewal Date
    const nextYear = new Date();
    nextYear.setFullYear(nextYear.getFullYear() + 1);
    const dateEl = document.getElementById('dashRenewalDate');
    if(dateEl) dateEl.textContent = 'Renews ' + nextYear.toLocaleDateString('en-US', {month:'short', day:'numeric', year:'numeric'});

    // Dynamic Quotas
    const props = getDB('properties') || [];
    const activeProps = props.filter(p => p.status === 'active').length;
    const featuredProps = props.filter(p => p.featured).length;
    const qList = document.getElementById('quotaListings');
    const qFeat = document.getElementById('quotaFeatured');
    if(qList) qList.style.width = Math.min((activeProps / 200) * 100, 100) + '%';
    if(qFeat) qFeat.style.width = Math.min((featuredProps / 15) * 100, 100) + '%';

    if (typeof AOS !== "undefined") AOS.init({ duration: 800, once: true, offset: 50 });

    // Future API Call - Get Dashboard Stats & Recent Data
    const properties = getDB('properties');
    const leads = getDB('leads');
    
    // GSAP Count-up animations for stat values
    if (typeof gsap !== 'undefined') {
        const statValues = document.querySelectorAll('.text-3xl.font-bold.text-white');
        
        statValues.forEach((el, index) => {
            const target = parseInt(el.textContent.replace(/,/g, '')) || 0;
            if(target > 0) {
                let obj = { val: 0 };
                gsap.to(obj, {
                    val: target,
                    duration: 2,
                    ease: "power2.out",
                    delay: 0.2 * index,
                    onUpdate: function() {
                        el.textContent = Math.floor(obj.val).toLocaleString();
                    }
                });
            }
        });
    }

    // Render Recent Leads
    const leadsBody = document.getElementById('recentLeadsBody');
    if (leadsBody) {
        leadsBody.innerHTML = '';
        const recentLeads = leads.slice(0, 3);
        if(recentLeads.length === 0) {
            leadsBody.innerHTML = \`<tr><td colspan="5" class="p-4 text-center text-gray-400">No recent leads</td></tr>\`;
        } else {
            recentLeads.forEach(lead => {
                const initials = lead.name.split(' ').map(n=>n[0]).join('').toUpperCase();
                let statusBadge = '';
                if(lead.status === 'new') statusBadge = '<span class="px-2.5 py-1 rounded-full text-xs font-medium bg-blue-500/20 text-blue-400 border border-blue-500/20">New</span>';
                if(lead.status === 'contacted') statusBadge = '<span class="px-2.5 py-1 rounded-full text-xs font-medium bg-yellow-500/20 text-yellow-400 border border-yellow-500/20">Contacted</span>';
                if(lead.status === 'negotiating') statusBadge = '<span class="px-2.5 py-1 rounded-full text-xs font-medium bg-green-500/20 text-green-400 border border-green-500/20">Negotiating</span>';
                
                leadsBody.innerHTML += \`
                    <tr>
                        <td class="p-4 pl-6 border-b border-white/5 whitespace-nowrap">
                            <div class="flex items-center gap-3">
                                <div class="w-8 h-8 rounded-full bg-purple-500/20 text-purple-400 flex items-center justify-center font-bold">\${initials}</div>
                                <div>
                                    <p class="text-white font-medium">\${lead.name}</p>
                                    <p class="text-xs text-gray-500">\${lead.email}</p>
                                </div>
                            </div>
                        </td>
                        <td class="p-4 border-b border-white/5 text-gray-300 whitespace-nowrap">\${lead.propertyTitle}</td>
                        <td class="p-4 border-b border-white/5 text-gray-400 whitespace-nowrap">\${formatDate(lead.date)}</td>
                        <td class="p-4 border-b border-white/5 whitespace-nowrap">\${statusBadge}</td>
                        <td class="p-4 pr-6 border-b border-white/5 whitespace-nowrap">
                            <!-- Future ASP.NET MVC Action / Future API Call -->
                            <button class="text-gray-400 hover:text-gold-500 transition-theme" onclick="showToast('Calling \${lead.phone}', 'info')"><i class="fa-solid fa-phone"></i></button>
                            <button class="text-gray-400 hover:text-gold-500 transition-theme ml-3" onclick="showToast('Emailing \${lead.email}', 'info')"><i class="fa-solid fa-envelope"></i></button>
                        </td>
                    </tr>
                \`;
            });
        }
    }

    // Render Recent Properties
    const propsList = document.getElementById('recentPropertiesList');
    if (propsList) {
        propsList.innerHTML = '';
        const recentProps = properties.slice(0, 4);
        if(recentProps.length === 0) {
            propsList.innerHTML = \`<p class="text-center text-gray-400">No recent properties</p>\`;
        } else {
            recentProps.forEach(prop => {
                const statusColor = prop.status === 'active' ? 'text-green-400' : 'text-gray-400';
                propsList.innerHTML += \`
                    <div class="flex gap-4 relative">
                        <div class="w-12 h-12 rounded-lg overflow-hidden flex-shrink-0 z-10 border border-white/10">
                            <img src="\${prop.image}" alt="\${prop.title}" class="w-full h-full object-cover">
                        </div>
                        <div class="flex-1">
                            <p class="text-sm text-white font-medium truncate">\${prop.title}</p>
                            <p class="text-xs \${statusColor} mt-1 uppercase tracking-wider font-semibold">\${prop.status}</p>
                            <p class="text-xs text-gray-500 mt-1">\${formatMoney(prop.price)}</p>
                        </div>
                    </div>
                \`;
            });
        }
    }
    
    // Animate containers
    document.querySelectorAll('.glass-card').forEach((el) => {
        if(!el.hasAttribute('data-aos')) el.setAttribute('data-aos', 'fade-up');
    });
}
