document.addEventListener('DOMContentLoaded', () => {
    initMedia();
});

function initMedia() {
    if (typeof AOS !== 'undefined') AOS.init({ duration: 800, once: true, offset: 50 });

    const fileInput = document.getElementById('mediaUploadInput');
    const uploadBtn = document.getElementById('uploadImagesBtn');
    const dropZone = document.getElementById('dropZone');
    
    // Selectors
    const selectAllBtn = document.getElementById('selectAllImagesBtn');
    const deleteSelectedBtn = document.getElementById('deleteSelectedImagesBtn');
    const checkboxes = document.querySelectorAll('input[type="checkbox"]');
    
    // Property Logic
    const propSelect = document.getElementById('propertyGallerySelect');
    const heading = document.getElementById('galleryHeading');
    
    // Stats
    const sCount = document.getElementById('statImagesCount');
    const sUsed = document.getElementById('statStorageUsed');
    const sBar = document.getElementById('statStorageBar');
    const sPercent = document.getElementById('statStoragePercent');

    // 1. File Upload Wireup
    if(uploadBtn && fileInput) {
        uploadBtn.addEventListener('click', () => fileInput.click());
    }
    
    if(dropZone && fileInput) {
        dropZone.addEventListener('click', () => fileInput.click());
        dropZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            dropZone.classList.add('border-gold-500');
        });
        dropZone.addEventListener('dragleave', (e) => {
            e.preventDefault();
            dropZone.classList.remove('border-gold-500');
        });
        dropZone.addEventListener('drop', (e) => {
            e.preventDefault();
            dropZone.classList.remove('border-gold-500');
            if(e.dataTransfer.files.length) {
                showToast(e.dataTransfer.files.length + ' images uploaded successfully!', 'success');
                updateMockStats(e.dataTransfer.files.length);
            }
        });
        
        fileInput.addEventListener('change', (e) => {
            if(e.target.files.length) {
                showToast(e.target.files.length + ' images uploaded successfully!', 'success');
                updateMockStats(e.target.files.length);
            }
        });
    }

    // 2. Selection Bulk Actions
    if(selectAllBtn) {
        let allSelected = false;
        selectAllBtn.addEventListener('click', () => {
            allSelected = !allSelected;
            checkboxes.forEach(cb => cb.checked = allSelected);
            selectAllBtn.textContent = allSelected ? 'Deselect All' : 'Select All';
        });
    }

    if(deleteSelectedBtn) {
        deleteSelectedBtn.addEventListener('click', () => {
            let selectedCount = 0;
            checkboxes.forEach(cb => {
                if(cb.checked) {
                    selectedCount++;
                    // Find the parent card and remove it
                    const card = cb.closest('.group');
                    if(card) card.remove();
                }
            });
            if(selectedCount > 0) {
                showToast(selectedCount + ' images deleted.', 'success');
                updateMockStats(-selectedCount);
            } else {
                showToast('Please select images to delete.', 'info');
            }
        });
    }

    // 3. Property Change
    if(propSelect && heading) {
        propSelect.addEventListener('change', (e) => {
            const title = e.target.options[e.target.selectedIndex].text;
            heading.textContent = title + ' - Gallery';
            showToast('Loaded gallery for ' + title, 'info');
        });
    }

    // 4. Update Stats Logic
    let currentImageCount = 5; // Starting default mock
    function updateMockStats(change = 0) {
        currentImageCount = Math.max(0, currentImageCount + change);
        if(sCount) sCount.textContent = currentImageCount;
        
        const mb = (currentImageCount * 3.5).toFixed(1); // 3.5MB per image avg
        if(sUsed) sUsed.textContent = mb;
        
        const percent = Math.min((mb / 1000) * 100, 100).toFixed(1);
        if(sBar) sBar.style.width = percent + '%';
        if(sPercent) sPercent.textContent = percent + '%';
    }

    updateMockStats(); // Initial call
}
