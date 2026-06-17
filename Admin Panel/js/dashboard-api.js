// dashboard-api.js — loads real data into the admin dashboard
document.addEventListener('DOMContentLoaded', async () => {
    await Promise.all([loadSummary(), loadMonthlyRevenue(), loadRecentListings()]);
});

// ── Stat Cards ──────────────────────────────────────────────────
async function loadSummary() {
    try {
        const d = await AdminApi.get('/api/admin/reports/summary');
        if (!d) return;

        set('stat-total-listings',  d.totalListings?.toLocaleString()      ?? '0');
        set('stat-total-sellers',   d.totalSellers?.toLocaleString()       ?? '0');
        set('stat-total-agents',    d.totalAgents?.toLocaleString()        ?? '0');
        set('stat-active-subs',     d.activeSubscriptions?.toLocaleString()?? '0');
        set('stat-pending-listings',d.pendingListings?.toLocaleString()    ?? '0');

        // Sub-labels
        const pending = d.pendingListings ?? 0;
        set('stat-listings-sub',
            d.totalListings === 0 ? 'none yet' : `${d.totalListings} total, ${pending} pending`);
        set('stat-pending-sub',
            pending === 0 ? 'all caught up' : `${pending} need review`);
    } catch (_) {}
}

// ── Revenue Chart ───────────────────────────────────────────────
async function loadMonthlyRevenue() {
    let monthly = new Array(12).fill(0);
    try {
        const d = await AdminApi.get('/api/admin/reports/revenue-monthly');
        if (d?.monthly) monthly = d.monthly;
    } catch (_) {}

    // Wait for skeleton removal before rendering chart (shared.js removes at 800ms)
    setTimeout(() => initChart(monthly), 850);
}

function initChart(monthly) {
    const canvas = document.getElementById('revenueChart');
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    const isDark = document.documentElement.classList.contains('dark');
    const gridColor = isDark ? 'rgba(255,255,255,0.05)' : 'rgba(0,0,0,0.05)';
    const textColor = isDark ? '#9CA3AF' : '#6B7280';

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
            datasets: [{
                label: 'Revenue',
                data: monthly,
                borderColor: '#D4AF37',
                backgroundColor: 'rgba(212,175,55,0.1)',
                borderWidth: 2,
                tension: 0.4,
                fill: true,
                pointBackgroundColor: '#060B19',
                pointBorderColor: '#D4AF37',
                pointBorderWidth: 2,
                pointRadius: 4,
                pointHoverRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: isDark ? '#1C2541' : '#FFFFFF',
                    titleColor:      isDark ? '#FFFFFF' : '#111827',
                    bodyColor:       isDark ? '#D1D5DB' : '#4B5563',
                    borderColor:     isDark ? 'rgba(255,255,255,0.1)' : 'rgba(0,0,0,0.1)',
                    borderWidth: 1, padding: 12, displayColors: false,
                    callbacks: {
                        label: (c) => 'Revenue: ' + new Intl.NumberFormat('en-US', {
                            style: 'currency', currency: 'USD'
                        }).format(c.parsed.y)
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: { color: gridColor, drawBorder: false },
                    ticks: {
                        color: textColor,
                        font: { family: "'JetBrains Mono', monospace" },
                        callback: (v) => v >= 1000 ? '$' + v / 1000 + 'k' : '$' + v
                    }
                },
                x: {
                    grid: { display: false, drawBorder: false },
                    ticks: { color: textColor, font: { family: "'Inter', sans-serif" } }
                }
            }
        }
    });
}

// ── Recent Listings (activity panel) ────────────────────────────
async function loadRecentListings() {
    const list = document.getElementById('activity-list');
    if (!list) return;

    try {
        const d = await AdminApi.get('/api/admin/listings?pageSize=5&page=1');
        const items = d?.items ?? [];

        if (items.length === 0) {
            list.innerHTML = '<p class="text-sm text-gray-500">No listings yet.</p>';
            return;
        }

        list.innerHTML = items.map(p => {
            const status = (p.approvalStatus || 'Pending').toLowerCase();
            const iconMap = {
                active:   { bg: 'bg-emerald-500/10', color: 'text-emerald-500', icon: 'check-circle' },
                pending:  { bg: 'bg-amber-500/10',   color: 'text-amber-400',   icon: 'clock' },
                rejected: { bg: 'bg-red-500/10',     color: 'text-red-500',     icon: 'x-circle' },
            };
            const s = iconMap[status] ?? iconMap.pending;
            const ago = timeAgo(p.listedDate);
            const price = typeof fmtMoney === 'function' ? fmtMoney(p.price) : '$' + p.price;
            return `
            <div class="flex gap-4">
                <div class="w-10 h-10 rounded-full ${s.bg} flex items-center justify-center flex-shrink-0">
                    <i data-lucide="${s.icon}" class="w-5 h-5 ${s.color}"></i>
                </div>
                <div class="min-w-0">
                    <p class="text-sm font-medium text-white truncate">${escHtml(p.title)}</p>
                    <p class="text-xs text-gray-400 mt-0.5">${escHtml(p.approvalStatus || 'Pending')} &middot; ${price}</p>
                    <p class="text-xs text-gray-500 font-mono mt-0.5">${ago}</p>
                </div>
            </div>`;
        }).join('');

        if (window.lucide) lucide.createIcons();
    } catch (_) {
        list.innerHTML = '<p class="text-sm text-gray-500">Could not load activity.</p>';
    }
}

// ── Helpers ─────────────────────────────────────────────────────
function set(id, value) {
    const el = document.getElementById(id);
    if (el) el.textContent = value;
}

function timeAgo(iso) {
    if (!iso) return '—';
    const diff = Date.now() - new Date(iso).getTime();
    const mins = Math.floor(diff / 60000);
    if (mins < 1)   return 'just now';
    if (mins < 60)  return mins + 'm ago';
    const hrs = Math.floor(mins / 60);
    if (hrs < 24)   return hrs + 'h ago';
    const days = Math.floor(hrs / 24);
    return days + 'd ago';
}

function escHtml(str) {
    return String(str ?? '').replace(/[&<>"']/g, c => ({
        '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'
    }[c]));
}
