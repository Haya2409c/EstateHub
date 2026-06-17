// Future ASP.NET MVC Controller Action: EditProperty
// Future Database Update: SaveChangesAsync
document.addEventListener('DOMContentLoaded', () => {
    initEditProperty();
});

function initEditProperty() {
    const form = document.getElementById('editPropertyForm');
    const coverUploadBtn = document.getElementById('editCoverBtn');
    const coverDeleteBtn = document.getElementById('deleteCoverBtn');
    const coverInput = document.getElementById('coverUploadInput');
    const coverImage = document.getElementById('coverPreviewImage');
    
    // Future API Call: fetchPropertyById(propId)
    const urlParams = new URLSearchParams(window.location.search);
    const propId = urlParams.get('id');
    const properties = getDB('agentProperties') || getDB('properties') || []; // Fallback to properties if agentProperties empty
    const currentProp = properties.find(p => p.id == propId) || properties[0];
    
    if(currentProp) {
        document.title = `Edit ${currentProp.title} - Luxury Realtors Portal`;
    }

    // Dynamic Amenities Rendering
    const amenitiesList = [
        { id: 'amenity_pool', label: 'Swimming Pool', icon: 'fa-water' },
        { id: 'amenity_gym', label: 'Gymnasium', icon: 'fa-dumbbell' },
        { id: 'amenity_security', label: '24/7 Security', icon: 'fa-shield-halved' },
        { id: 'amenity_parking', label: 'Covered Parking', icon: 'fa-car' },
        { id: 'amenity_balcony', label: 'Balcony/Terrace', icon: 'fa-sun' },
        { id: 'amenity_smart', label: 'Smart Home', icon: 'fa-house-signal' },
        { id: 'amenity_spa', label: 'Private Spa', icon: 'fa-spa' },
        { id: 'amenity_maid', label: 'Maid Room', icon: 'fa-broom' }
    ];
    
    const amContainer = document.getElementById('amenitiesContainer');
    if(amContainer) {
        let amHtml = '';
        amenitiesList.forEach((am, index) => {
            const checked = index < 5 ? 'checked' : '';
            amHtml += `
            <label class="relative cursor-pointer group" data-aos="zoom-in" data-aos-delay="${index * 50}">
                <input type="checkbox" id="${am.id}" class="peer sr-only amenity-checkbox" ${checked}>
                <div class="h-full p-4 rounded-xl border border-white/10 bg-navy-900 peer-checked:bg-gold-500/10 peer-checked:border-gold-500/50 transition-theme flex flex-col items-center justify-center gap-2 text-gray-400 peer-checked:text-gold-500 hover:border-white/30">
                    <i class="fa-solid ${am.icon} text-xl mb-1"></i>
                    <span class="text-xs font-medium text-center">${am.label}</span>
                </div>
                <div class="absolute top-2 right-2 w-4 h-4 rounded-full border border-gray-500 peer-checked:border-gold-500 peer-checked:bg-gold-500 transition-theme flex items-center justify-center opacity-0 peer-checked:opacity-100">
                    <i class="fa-solid fa-check text-[10px] text-navy-900"></i>
                </div>
            </label>`;
        });
        amContainer.innerHTML = amHtml;
    }

    // Dirty State Tracker & Browser Unload
    let isDirty = false;
    if(form) {
        form.addEventListener('input', () => { isDirty = true; });
        form.addEventListener('change', () => { isDirty = true; });
    }

    window.addEventListener('beforeunload', (e) => {
        if(isDirty) {
            e.preventDefault();
            e.returnValue = ''; // Shows generic browser dialog
        }
    });

    const cancelBtn = document.getElementById('cancelEditBtn');
    if(cancelBtn) {
        cancelBtn.addEventListener('click', () => {
            if(isDirty) {
                if(confirm('You have unsaved changes. Are you sure you want to leave?')) {
                    isDirty = false;
                    window.location.href = 'listings.html';
                }
            } else {
                window.location.href = 'listings.html';
            }
        });
    }

    const resetBtn = document.getElementById('resetEditBtn');
    if(resetBtn && form) {
        resetBtn.addEventListener('click', () => {
            if(confirm('Are you sure you want to reset the form to its original state?')) {
                form.reset();
                isDirty = false;
                // Hide all error messages
                document.querySelectorAll('.error-msg').forEach(el => el.classList.add('hidden'));
                document.querySelectorAll('.border-red-500').forEach(el => el.classList.remove('border-red-500'));
                showToast('Form reset to original state.', 'info');
            }
        });
    }

    function validateForm() {
        let isValid = true;
        const inputs = form.querySelectorAll('input[required], select[required]');
        
        inputs.forEach(input => {
            const errorMsg = input.nextElementSibling;
            if(!input.checkValidity()) {
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

    if(form) {
        form.addEventListener('submit', (e) => {
            e.preventDefault();
            const submitBtn = document.querySelector('button[type="submit"]');
            
            // GSAP scale animation on submit
            if(typeof gsap !== 'undefined' && submitBtn) {
                gsap.to(submitBtn, { scale: 0.95, duration: 0.1, yoyo: true, repeat: 1 });
            }

            if(validateForm()) {
                isDirty = false;
                showToast('Property Updated Successfully', 'success');
                
                // Update localStorage
                const props = getDB('agentProperties') || [];
                // Mock update logic
                const timestamp = new Date().toISOString();
                setDB('agentProperties', props);

                if(submitBtn) {
                    const originalText = submitBtn.innerHTML;
                    submitBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin mr-2"></i> Saving...';
                    submitBtn.disabled = true;
                    setTimeout(() => {
                        window.location.href = 'listings.html';
                    }, 1200);
                }
            }
        });
        
        // Remove error states on input
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

    if(coverUploadBtn && coverInput) {
        coverUploadBtn.addEventListener('click', () => coverInput.click());
    }
    if(coverInput && coverImage) {
        coverInput.addEventListener('change', (e) => {
            if(e.target.files && e.target.files[0]) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    coverImage.src = e.target.result;
                    isDirty = true;
                    showToast('Image Updated', 'success');
                }
                reader.readAsDataURL(e.target.files[0]);
            }
        });
    }
    if(coverDeleteBtn && coverImage) {
        coverDeleteBtn.addEventListener('click', () => {
            coverImage.src = 'https://via.placeholder.com/1200x400/0f172a/d4af37?text=No+Cover+Image';
            isDirty = true;
            showToast('Image Removed', 'info');
        });
    }
}
