document.addEventListener('DOMContentLoaded', () => {
    initAddProperty();
});

function initAddProperty() {
    const form = document.getElementById('propertyForm');
    const saveDraftBtn = document.getElementById('saveDraftBtn');
    
    const coverInput = document.getElementById('coverImage');
    const coverPreview = document.getElementById('coverPreviewImage');
    const coverUploadUI = document.getElementById('coverUploadUI');
    const coverInfoUI = document.getElementById('coverInfoUI');
    const removeCoverBtn = document.getElementById('removeCoverBtn');
    
    const descInput = document.getElementById('propDesc');
    const charCounter = document.getElementById('charCounter');
    const priceInput = document.getElementById('propPrice');

    let isDirty = false;

    // Image Preview Logic
    if(coverInput && coverPreview) {
        coverInput.addEventListener('change', function() {
            if(this.files && this.files[0]) {
                const url = URL.createObjectURL(this.files[0]);
                coverPreview.src = url;
                coverPreview.classList.remove('hidden');
                coverUploadUI.classList.add('hidden');
                coverInfoUI.classList.remove('hidden');
                isDirty = true;
                showToast('Image Uploaded', 'success');
            }
        });
    }

    if(removeCoverBtn) {
        removeCoverBtn.addEventListener('click', (e) => {
            e.stopPropagation(); // prevent file dialog
            coverInput.value = '';
            coverPreview.src = '';
            coverPreview.classList.add('hidden');
            coverUploadUI.classList.remove('hidden');
            coverInfoUI.classList.add('hidden');
            isDirty = true;
            showToast('Image Removed', 'info');
        });
    }

    // Character Counter
    if(descInput && charCounter) {
        descInput.addEventListener('input', () => {
            const len = descInput.value.length;
            charCounter.textContent = `${len} / 1000 characters`;
            if(len >= 1000) charCounter.classList.add('text-red-400');
            else charCounter.classList.remove('text-red-400');
            isDirty = true;
        });
    }

    // Auto Price Formatting
    if(priceInput) {
        priceInput.addEventListener('blur', (e) => {
            let val = e.target.value.replace(/[^0-9.]/g, '');
            if(val) {
                const num = parseFloat(val);
                if(!isNaN(num)) {
                    e.target.value = '$' + num.toLocaleString('en-US');
                }
            }
        });
        priceInput.addEventListener('focus', (e) => {
            let val = e.target.value.replace(/[^0-9.]/g, '');
            e.target.value = val;
        });
        priceInput.addEventListener('input', () => isDirty = true);
    }

    // Track dirty state on all inputs
    if(form) {
        form.addEventListener('input', () => { isDirty = true; });
        form.addEventListener('change', () => { isDirty = true; });
    }

    // Unsaved Warning
    window.addEventListener('beforeunload', (e) => {
        if(isDirty) {
            e.preventDefault();
            e.returnValue = '';
        }
    });

    // Validation engine
    function validateForm() {
        let isValid = true;
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        
        inputs.forEach(input => {
            const errorMsg = input.nextElementSibling;
            
            // Special price logic since it's formatted as text
            let isCustomValid = true;
            if(input.id === 'propPrice') {
                const num = parseFloat(input.value.replace(/[^0-9.]/g, ''));
                if(isNaN(num) || num < 1) isCustomValid = false;
            }

            if(!input.checkValidity() || !isCustomValid) {
                isValid = false;
                input.classList.add('border-red-500');
                if(errorMsg && errorMsg.classList.contains('error-msg')) {
                    errorMsg.classList.remove('hidden');
                }
            } else {
                input.classList.remove('border-red-500');
                if(errorMsg && errorMsg.classList.contains('error-msg')) {
                    errorMsg.classList.add('hidden');
                }
            }
        });
        return isValid;
    }

    // Save Draft
    if(saveDraftBtn && form) {
        saveDraftBtn.addEventListener('click', () => {
            const draftData = {
                title: document.getElementById('propTitle')?.value || '',
                type: document.getElementById('propType')?.value || '',
                status: document.getElementById('propStatus')?.value || '',
                price: priceInput?.value || '',
                address: document.getElementById('propAddress')?.value || '',
                country: document.getElementById('propCountry')?.value || '',
                description: descInput?.value || ''
            };
            
            setDB('draftProperty', draftData);
            isDirty = false; // Safe to leave now
            showToast('Draft Saved Successfully', 'success');
        });
    }

    // Form Submit
    if(form) {
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            const submitBtn = document.querySelector('button[type="submit"]');

            if(typeof gsap !== 'undefined' && submitBtn) {
                gsap.to(submitBtn, { scale: 0.95, duration: 0.1, yoyo: true, repeat: 1 });
            }

            if(validateForm()) {
                const newProp = {
                    id: Date.now(),
                    title: document.getElementById('propTitle').value,
                    type: document.getElementById('propType').value,
                    status: document.getElementById('propStatus').value,
                    price: priceInput.value,
                    location: document.getElementById('propAddress').value,
                    image: coverPreview && coverPreview.src ? coverPreview.src : 'https://via.placeholder.com/800x600/0f172a/d4af37?text=New+Property',
                    featured: document.getElementById('badgePremium') && document.getElementById('badgePremium').checked,
                    dateAdded: new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
                };

                const props = getDB('agentProperties') || [];
                props.unshift(newProp);
                setDB('agentProperties', props);

                // Also update the global 'properties' for cross-page mock syncing
                const globalProps = getDB('properties') || [];
                globalProps.unshift(newProp);
                setDB('properties', globalProps);

                isDirty = false;
                showToast('Property Published Successfully', 'success');

                if(submitBtn) {
                    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin mr-2"></i> Publishing...';
                    submitBtn.disabled = true;
                    setTimeout(() => {
                        window.location.href = 'listings.html';
                    }, 1500);
                }
            } else {
                showToast('Please Fill Required Fields', 'error');
            }
        });

        // Clear error on input
        form.addEventListener('input', (e) => {
            if(e.target.classList.contains('border-red-500')) {
                e.target.classList.remove('border-red-500');
                const errorMsg = e.target.nextElementSibling;
                if(errorMsg && errorMsg.classList.contains('error-msg')) {
                    errorMsg.classList.add('hidden');
                }
            }
        });
    }
}
