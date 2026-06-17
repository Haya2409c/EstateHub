document.addEventListener('DOMContentLoaded', () => {
    initPackages();
});

function initPackages() {
    // Dynamic renewal date
    const dateEl = document.getElementById('renewalDate');
    if(dateEl) {
        const nextYear = new Date();
        nextYear.setFullYear(nextYear.getFullYear() + 1);
        const options = { year: 'numeric', month: 'short', day: 'numeric' };
        dateEl.textContent = \`Renews automatically on \${nextYear.toLocaleDateString('en-US', options)}\`;
    }

    if (typeof AOS !== 'undefined') {
        AOS.init({ duration: 800, once: true, offset: 50 });
    }

    document.querySelectorAll('.glass-card').forEach((el, i) => {
        if(!el.hasAttribute('data-aos')) el.setAttribute('data-aos', 'fade-up');
        el.setAttribute('data-aos-delay', i * 100);
    });

    const buttons = document.querySelectorAll('button');
    buttons.forEach(btn => {
        if(btn.textContent.trim() === 'Downgrade') {
            btn.addEventListener('click', () => {
                showModal('Downgrade Plan', 'Are you sure you want to downgrade to Basic Starter? You will lose access to premium features.', 'Confirm Downgrade', () => {
                    // Future API Call - Change Subscription
                    showToast('Plan downgraded successfully.', 'success');
                });
            });
        }
        if(btn.textContent.trim() === 'Upgrade Now') {
            btn.addEventListener('click', () => {
                showModal('Upgrade Plan', 'Are you sure you want to upgrade to Enterprise Plus? Your account will be charged $399/month.', 'Confirm Upgrade', () => {
                    // Future API Call - Change Subscription
                    showToast('Plan upgraded successfully!', 'success');
                });
            });
        }
    });
}
