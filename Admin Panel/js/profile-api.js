// profile-api.js — real API connection for admin profile page
document.addEventListener('DOMContentLoaded', () => {

    // ── Populate fields from stored JWT payload ────────────
    const user = typeof TokenStore !== 'undefined' ? TokenStore.getUser() : null;
    if (user) {
        const nameInput = document.getElementById('profile-full-name');
        const emailInput = document.querySelector('#profile-info-form input[type="email"]');
        if (nameInput  && user.fullName) nameInput.value  = user.fullName;
        if (emailInput && user.email)    emailInput.value = user.email;
        document.querySelectorAll('.sidebar-user-name').forEach(el => { el.textContent = user.fullName || el.textContent; });
        document.querySelectorAll('.sidebar-user-email').forEach(el => { el.textContent = user.email    || el.textContent; });
    }

    // ── Password change ────────────────────────────────────
    const pwForm = document.getElementById('password-change-form');
    if (!pwForm) return;

    // Capture phase — fires before shared.js bubble listener on document
    pwForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();

        const currentPw = document.getElementById('current-password')?.value ?? '';
        const newPw     = document.getElementById('new-password')?.value ?? '';
        const confirmEl = pwForm.querySelector('[data-match="new-password"]');
        const confirmPw = confirmEl?.value ?? '';
        const submitBtn = pwForm.querySelector('[type="submit"]');

        if (!currentPw || !newPw || !confirmPw) {
            window.showToast?.('All password fields are required.', 'error');
            return;
        }
        if (newPw !== confirmPw) {
            window.showToast?.('New passwords do not match.', 'error');
            return;
        }
        if (newPw.length < 6) {
            window.showToast?.('Password must be at least 6 characters.', 'error');
            return;
        }

        const btnLabel = submitBtn?.textContent?.trim() || 'Change Password';
        if (submitBtn) { submitBtn.disabled = true; submitBtn.textContent = 'Saving...'; }

        try {
            await AdminApi.put('/api/admin/profile/change-password', {
                currentPassword: currentPw,
                newPassword:     newPw
            });
            window.showToast?.('Password changed successfully.', 'success');
            pwForm.reset();
        } catch (err) {
            const msg = err?.message || 'Failed to change password. Please check your current password.';
            window.showToast?.(msg, 'error');
        } finally {
            if (submitBtn) { submitBtn.disabled = false; submitBtn.textContent = btnLabel; }
        }
    }, true); // true = capture phase, runs before shared.js document-level handler
});
