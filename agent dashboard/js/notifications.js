document.addEventListener('DOMContentLoaded', () => {
    initNotifications();
});

function initNotifications() {
    if (typeof AOS !== "undefined") AOS.init({ duration: 800, once: true, offset: 50 });

    const list = document.getElementById('notificationList') || document.querySelector('.space-y-4');
    if(!list) return;

    function renderNotifications() {
        const notifications = getDB('notifications');
        list.innerHTML = '';

        if(notifications.length === 0) {
            list.innerHTML = \`<div class="text-center p-8 text-gray-400">No notifications available.</div>\`;
            return;
        }

        notifications.forEach(notif => {
            let colorClasses = '';
            if(notif.color === 'blue') colorClasses = 'bg-blue-500/20 text-blue-400 border-blue-500/20';
            else if(notif.color === 'green') colorClasses = 'bg-green-500/20 text-green-400 border-green-500/20';
            else if(notif.color === 'yellow') colorClasses = 'bg-yellow-500/20 text-yellow-400 border-yellow-500/20';
            
            list.innerHTML += \`
                <div class="glass-card p-6 rounded-xl border border-white/5 \${notif.read ? 'opacity-60' : ''}" data-aos="fade-up">
                    <div class="flex items-start gap-4">
                        <div class="w-12 h-12 rounded-full \${colorClasses} flex items-center justify-center text-xl flex-shrink-0">
                            <i class="fa-solid \${notif.icon}"></i>
                        </div>
                        <div class="flex-1">
                            <div class="flex justify-between items-start">
                                <h4 class="text-lg font-bold text-white">\${notif.title} \${!notif.read ? '<span class="w-2 h-2 rounded-full bg-gold-500 inline-block ml-2 mb-1 shadow-gold-glow"></span>' : ''}</h4>
                                <span class="text-xs text-gray-500">\${formatDate(notif.date)}</span>
                            </div>
                            <p class="text-gray-400 mt-1">\${notif.message}</p>
                            <div class="mt-4 flex gap-3">
                                \${!notif.read ? \`<button class="mark-read-btn text-sm text-gold-500 hover:text-gold-400 font-medium transition-theme" data-id="\${notif.id}">Mark as Read</button>\` : ''}
                                <button class="delete-notif-btn text-sm text-gray-500 hover:text-red-400 transition-theme" data-id="\${notif.id}">Delete</button>
                            </div>
                        </div>
                    </div>
                </div>
            \`;
        });

        document.querySelectorAll('.mark-read-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const id = parseInt(e.currentTarget.getAttribute('data-id'));
                let notifs = getDB('notifications');
                const idx = notifs.findIndex(n => n.id === id);
                if(idx > -1) {
                    notifs[idx].read = true;
                    setDB('notifications', notifs); // Future Database Save
                    updateGlobalNotificationCount();
                    renderNotifications();
                }
            });
        });

        document.querySelectorAll('.delete-notif-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const id = parseInt(e.currentTarget.getAttribute('data-id'));
                let notifs = getDB('notifications');
                notifs = notifs.filter(n => n.id !== id);
                setDB('notifications', notifs); // Future Database Save
                updateGlobalNotificationCount();
                renderNotifications();
            });
        });
    }

    renderNotifications();

    const markAllBtn = document.querySelector('button.bg-white\\\\/5');
    if(markAllBtn) {
        markAllBtn.addEventListener('click', () => {
            let notifs = getDB('notifications');
            notifs.forEach(n => n.read = true);
            setDB('notifications', notifs); // Future Database Save
            updateGlobalNotificationCount();
            renderNotifications();
            showToast('All notifications marked as read.', 'success');
        });
    }
}
