document.addEventListener('DOMContentLoaded', () => {
    initProfile();
});

function initProfile() {
    if (typeof AOS !== 'undefined') AOS.init({ duration: 800, once: true, offset: 50 });

    const profileData = getDB('agentProfile') || {
        firstName: 'Alexander',
        lastName: 'Pierce',
        email: 'alexander@agency.com',
        phone: '+1 (555) 123-4567',
        bio: 'Luxury real estate specialist with over 10 years of experience in the prime residential market. I focus on delivering exceptional service and results for high-net-worth clients.',
        linkedin: 'https://linkedin.com/in/alexpierce',
        instagram: 'https://instagram.com/alexrealty'
    };

    // Hydrate form
    const inputs = document.querySelectorAll('input[type="text"], input[type="tel"]');
    if(inputs.length >= 2) {
        inputs[0].value = profileData.firstName;
        inputs[1].value = profileData.lastName;
    }
    if(inputs.length >= 3) inputs[2].value = profileData.phone;
    
    const emailField = document.getElementById('profileEmail');
    if(emailField) emailField.value = profileData.email;
    
    const bioField = document.querySelector('textarea');
    if(bioField) bioField.value = profileData.bio;
    
    const linkedIn = document.getElementById('linkedinUrl');
    const instagram = document.getElementById('instagramUrl');
    if(linkedIn) linkedIn.value = profileData.linkedin;
    if(instagram) instagram.value = profileData.instagram;

    // Date verified
    const dateEl = document.getElementById('identityDate');
    if(dateEl) {
        const past = new Date();
        past.setFullYear(past.getFullYear() - 2);
        dateEl.textContent = 'Identity verified on ' + past.toLocaleDateString('en-US', {month:'short', year:'numeric'});
    }

    // Avatar Logic
    const avatarInput = document.getElementById('avatarUpload');
    const changeBtn = document.getElementById('changeAvatarBtn');
    const removeBtn = document.getElementById('removeAvatarBtn');
    
    if(changeBtn && avatarInput) {
        changeBtn.addEventListener('click', () => avatarInput.click());
    }
    
    if(avatarInput) {
        avatarInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if(!file) return;
            if(!file.type.startsWith('image/')) {
                showToast('Invalid file type. Please upload an image.', 'error');
                return;
            }
            if(file.size > 5 * 1024 * 1024) {
                showToast('File is too large. Maximum size is 5MB.', 'error');
                return;
            }
            showToast('Profile picture updated!', 'success');
        });
    }

    if(removeBtn) {
        removeBtn.addEventListener('click', () => {
            showToast('Profile picture removed.', 'info');
        });
    }

    // Save & Cancel
    const saveBtn = document.getElementById('saveProfileBtn');
    const cancelBtn = document.getElementById('cancelProfileBtn');

    if(cancelBtn) {
        cancelBtn.addEventListener('click', () => {
            showToast('Changes cancelled.', 'info');
            // reset form visually
            if(inputs.length >= 2) {
                inputs[0].value = profileData.firstName;
                inputs[1].value = profileData.lastName;
            }
        });
    }

    if(saveBtn) {
        saveBtn.addEventListener('click', () => {
            if(linkedIn && linkedIn.value && !linkedIn.value.includes('linkedin.com')) {
                showToast('Invalid LinkedIn URL domain.', 'error');
                return;
            }
            if(instagram && instagram.value && !instagram.value.includes('instagram.com')) {
                showToast('Invalid Instagram URL domain.', 'error');
                return;
            }
            
            showToast('Profile saved successfully!', 'success');
            
            profileData.firstName = inputs[0].value;
            profileData.lastName = inputs[1].value;
            setDB('agentProfile', profileData);
        });
    }
}
