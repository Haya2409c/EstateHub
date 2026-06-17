// settings-api.js — loads and saves Site Settings via the API
document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('settings-form');
    if (!form) return;

    const fields = {
        currency:     document.getElementById('settings-currency'),
        pageSize:     document.getElementById('settings-page-size'),
        adExpiry:     document.getElementById('settings-ad-expiry'),
        autoApprove:  document.getElementById('settings-auto-approve'),
        paypalClient: document.getElementById('settings-paypal-client'),
        paypalSecret: document.getElementById('settings-paypal-secret'),
    };
    const saveBtn     = document.getElementById('settings-save-btn');
    const discardBtn  = document.getElementById('settings-discard-btn');

    let lastSaved = {};

    // ── Load settings from API ─────────────────────────────
    async function loadSettings() {
        try {
            const data = await AdminApi.get('/api/admin/settings');
            if (!data) return;

            if (fields.currency   && data.currency)     fields.currency.value          = data.currency;
            if (fields.pageSize   && data.page_size)    fields.pageSize.value           = data.page_size;
            if (fields.adExpiry   && data.ad_expiry)    fields.adExpiry.value           = data.ad_expiry;
            if (fields.autoApprove)                     fields.autoApprove.checked      = data.auto_approve === '1';
            if (fields.paypalClient && data.paypal_client) fields.paypalClient.value    = data.paypal_client;
            if (fields.paypalSecret && data.paypal_secret) fields.paypalSecret.value    = data.paypal_secret;

            const paypalMode = data.paypal_mode || 'sandbox';
            const modeRadio = document.querySelector(`input[name="paypal_mode"][value="${paypalMode}"]`);
            if (modeRadio) modeRadio.checked = true;

            // Trigger visual toggle update if the page script defines it
            fields.autoApprove?.dispatchEvent(new Event('change'));

            lastSaved = { ...data };
        } catch (_) { /* toast already shown by api.js */ }
    }

    // ── Save settings to API ───────────────────────────────
    async function saveSettings(e) {
        e.preventDefault();
        e.stopImmediatePropagation(); // prevent shared.js generic form handler from also firing

        const checkedMode = document.querySelector('input[name="paypal_mode"]:checked');
        const payload = {
            currency:      fields.currency?.value     || '',
            page_size:     fields.pageSize?.value     || '20',
            ad_expiry:     fields.adExpiry?.value     || '30',
            auto_approve:  fields.autoApprove?.checked ? '1' : '0',
            paypal_client: fields.paypalClient?.value || '',
            paypal_secret: fields.paypalSecret?.value || '',
            paypal_mode:   checkedMode?.value         || 'sandbox',
        };

        const origHtml = saveBtn.innerHTML;
        saveBtn.disabled = true;
        saveBtn.innerHTML = '<i data-lucide="loader-2" class="w-4 h-4 animate-spin inline mr-1"></i> Saving...';
        if (window.lucide) lucide.createIcons();

        try {
            await AdminApi.put('/api/admin/settings', payload);
            lastSaved = { ...payload };
            window.showToast('Settings saved successfully.', 'success');
        } catch (_) {
            /* toast shown by api.js */
        } finally {
            saveBtn.disabled = false;
            saveBtn.innerHTML = origHtml;
            if (window.lucide) lucide.createIcons();
        }
    }

    // ── Discard changes ───────────────────────────────────
    if (discardBtn) {
        discardBtn.addEventListener('click', (e) => {
            e.preventDefault();
            if (fields.currency   && lastSaved.currency)     fields.currency.value     = lastSaved.currency;
            if (fields.pageSize   && lastSaved.page_size)    fields.pageSize.value     = lastSaved.page_size;
            if (fields.adExpiry   && lastSaved.ad_expiry)    fields.adExpiry.value     = lastSaved.ad_expiry;
            if (fields.autoApprove) fields.autoApprove.checked = lastSaved.auto_approve === '1';
            if (fields.paypalClient && lastSaved.paypal_client) fields.paypalClient.value = lastSaved.paypal_client;
            if (fields.paypalSecret && lastSaved.paypal_secret) fields.paypalSecret.value = lastSaved.paypal_secret;
            fields.autoApprove?.dispatchEvent(new Event('change'));
            window.showToast('Changes discarded.', 'info');
        });
    }

    // Intercept the existing form submit listener by listening in capture phase first
    form.addEventListener('submit', saveSettings, true);

    loadSettings();
});
