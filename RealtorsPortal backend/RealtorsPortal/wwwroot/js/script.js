

// Initialize AOS
AOS.init({
    duration: 700,
    easing: 'ease-out-cubic',
    once: true,
    offset: 0
});

// Hero Video Background Cycling
let currentHeroVideoIndex = 0;
const heroVideos = document.querySelectorAll('.hero-bg-video');
function switchHeroBackgroundVideo() {
    if (!heroVideos.length) return;
    heroVideos[currentHeroVideoIndex].classList.remove('active-video');
    currentHeroVideoIndex = (currentHeroVideoIndex + 1) % heroVideos.length;
    heroVideos[currentHeroVideoIndex].classList.add('active-video');
    const v = heroVideos[currentHeroVideoIndex];
    if (v && v.paused) v.play().catch(() => {});
}

window.addEventListener('load', () => {
    if (heroVideos.length > 0) heroVideos[0].play().catch(() => {});
    setInterval(switchHeroBackgroundVideo, 5000);
    
    // Fix for AOS cards appearing shifted down before images load
    if (typeof AOS !== 'undefined') {
        setTimeout(() => AOS.refresh(), 100);
        setTimeout(() => AOS.refresh(), 500);
    }
});

// Initialize Testimonial Swiper
/* Testimonial swiper disabled`nconst testimonialSwiper = new Swiper('.testimonialSwiper', { ... });`n*/

// Toggle Logic
function toggleActive(element) {
    const buttons = document.querySelectorAll('.toggle-btn');
    buttons.forEach(btn => btn.classList.remove('active'));
    element.classList.add('active');
}

// Counter Animation
const animateCounters = () => {
    const counters = document.querySelectorAll('.counter-number');
    counters.forEach(counter => {
        const target = parseInt(counter.getAttribute('data-target'));
        const duration = 2000;
        let startTimestamp = null;
        const suffix = counter.getAttribute('data-suffix') || '';

        const step = (timestamp) => {
            if (!startTimestamp) startTimestamp = timestamp;
            const progress = Math.min((timestamp - startTimestamp) / duration, 1);
            // ease-out cubic
            const easeOut = 1 - Math.pow(1 - progress, 3);
            const current = Math.floor(easeOut * target);
            counter.innerText = current + suffix;
            
            if (progress < 1) {
                window.requestAnimationFrame(step);
            } else {
                counter.innerText = target + suffix;
            }
        };
        window.requestAnimationFrame(step);
    });
};

// Intersection Observer for Counters
const observerOptions = {
    threshold: 0.5
};

const counterObserver = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            animateCounters();
            counterObserver.unobserve(entry.target);
        }
    });
}, observerOptions);

const statsSection = document.querySelector('.stats-grid');
if (statsSection) {
    counterObserver.observe(statsSection);
}

// GSAP Animations
document.addEventListener('DOMContentLoaded', () => {
    // Navbar background change on scroll removed per user request

    // Mouse Movement Parallax for Hero
    const hero = document.querySelector('#hero');
    if (hero) {
        document.addEventListener('mousemove', (e) => {
            if (typeof gsap === 'undefined') return;
            
            const { clientX, clientY } = e;
            const xPos = (clientX / window.innerWidth - 0.5) * 30;
            const yPos = (clientY / window.innerHeight - 0.5) * 30;

            gsap.to('.stat-card', {
                x: xPos,
                y: yPos,
                duration: 1.5,
                ease: 'power2.out',
                stagger: 0.1
            });

            gsap.to('.hero-content', {
                x: xPos * 0.3,
                y: yPos * 0.3,
                duration: 1.5,
                ease: 'power2.out'
            });
        });
    }

    // Button Hover Effects
    const goldButtons = document.querySelectorAll('.btn-gold, .property-card, .category-card, .blog-card');
    goldButtons.forEach(btn => {
        btn.addEventListener('mouseenter', () => {
            if (typeof gsap !== 'undefined') {
                gsap.to(btn, {
                    scale: 1.02,
                    duration: 0.3,
                    ease: 'power2.out'
                });
            }
        });
        btn.addEventListener('mouseleave', () => {
            if (typeof gsap !== 'undefined') {
                gsap.to(btn, {
                    scale: 1,
                    duration: 0.3,
                    ease: 'power2.out'
                });
            }
        });
    });

    // The mobile menu logic will be handled by a dedicated DOMContentLoaded block below

    // Active Link Logic
    const currentUrl = '__SERVER_HANDLES_ACTIVE__';
    
    // Desktop Nav
    const desktopLinks = document.querySelectorAll('.nav-link, .nav-dropdown-link');
    desktopLinks.forEach(link => {
        if (link.getAttribute('href') === currentUrl) {
            link.classList.add('text-gold');
            // If it's a dropdown link, also highlight parent
            const parentDropdown = link.closest('.group');
            if (parentDropdown) {
                const parentBtn = parentDropdown.querySelector('button');
                if (parentBtn) parentBtn.classList.add('text-gold');
            }
        }
    });

    // Mobile Nav
    const mobileAllLinks = document.querySelectorAll('.mobile-nav-link, .mobile-sublink');
    mobileAllLinks.forEach(link => {
        if (link.getAttribute('href') === currentUrl) {
            link.classList.add('text-gold');
            // If it's a sublink, expand parent dropdown and highlight
            const parentDropdown = link.closest('.mobile-dropdown-group');
            if (parentDropdown) {
                const parentBtn = parentDropdown.querySelector('.mobile-dropdown-toggle');
                const content = parentDropdown.querySelector('.mobile-dropdown-content');
                const icon = parentDropdown.querySelector('i');
                if (parentBtn) parentBtn.classList.add('text-gold');
                if (content) {
                    content.classList.remove('hidden');
                    content.classList.add('flex');
                }
                if (icon) icon.style.transform = 'rotate(180deg)';
            }
        }
    });

    // Mobile Dropdown Toggle Logic
    const mobileDropdownToggles = document.querySelectorAll('.mobile-dropdown-toggle');
    mobileDropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', () => {
            const content = toggle.nextElementSibling;
            const icon = toggle.querySelector('i');
            
            // Close others
            mobileDropdownToggles.forEach(otherToggle => {
                if (otherToggle !== toggle) {
                    const otherContent = otherToggle.nextElementSibling;
                    const otherIcon = otherToggle.querySelector('i');
                    if (otherContent) {
                        otherContent.classList.add('hidden');
                        otherContent.classList.remove('flex');
                    }
                    if (otherIcon) otherIcon.style.transform = 'rotate(0deg)';
                }
            });

            if (content.classList.contains('hidden')) {
                content.classList.remove('hidden');
                content.classList.add('flex');
                if (icon) icon.style.transform = 'rotate(180deg)';
            } else {
                content.classList.add('hidden');
                content.classList.remove('flex');
                if (icon) icon.style.transform = 'rotate(0deg)';
            }
        });
    });


    // EFFECT 3: Scroll Reveal Animation (AOS style)
    const revealObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if(entry.isIntersecting) {
                entry.target.style.transform = 'translateY(0)';
                entry.target.style.opacity = '1';
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    const revealElements = document.querySelectorAll('.property-card, .agent-card, .category-card, .section-title, h2, h3');
    revealElements.forEach((el, index) => {
        // Skip already animated elements from AOS
        if (el.hasAttribute('data-aos')) return;
        el.style.opacity = '0';
        el.style.transform = 'translateY(40px)';
        el.style.transition = 'all 0.6s ease-out';
        el.style.transitionDelay = `${(index % 3) * 100}ms`;
        revealObserver.observe(el);
    });


    // Marquee Touch Support
    const track = document.querySelector('.marquee-track');
    if(track) {
        track.addEventListener('touchstart', () => {
            track.style.animationPlayState = 'paused';
        });
        track.addEventListener('touchend', () => {
            track.style.animationPlayState = 'running';
        });
    }



  // Price range formatting
  const minInput = document.getElementById('filter-minprice');
  const maxInput = document.getElementById('filter-maxprice');
  const priceDisplay = document.getElementById('price-display');

  function formatPrice(val) {
    return '$' + parseInt(val || 0).toLocaleString();
  }

  function updatePriceDisplay() {
    if(priceDisplay) {
      const min = minInput?.value || 0;
      const max = maxInput?.value || 'Any';
      priceDisplay.textContent = 
        formatPrice(min) + ' — ' + 
        (maxInput?.value ? formatPrice(max) : 'Any');
    }
  }

  minInput?.addEventListener('input', () => {
    if(maxInput?.value && parseInt(minInput.value) > parseInt(maxInput.value)) {
      minInput.value = maxInput.value;
    }
    updatePriceDisplay();
  });

  maxInput?.addEventListener('input', () => {
    if(minInput?.value && parseInt(maxInput.value) < parseInt(minInput.value)) {
      maxInput.value = minInput.value;
    }
    updatePriceDisplay();
  });

  
// ==========================================
// END OF PRICE FORMATTING LOGIC
// ==========================================

// ==========================================
// DYNAMIC PAGINATION & FILTERING ENGINE
// ==========================================

const properties = window.properties = [
    { id: 1, name: "Skyline Penthouse", city: "Manhattan, New York", price: 12000000, type: "house", status: "buy", beds: 4, baths: 5, sqft: 5500, img: "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600", gallery: ["https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600", "https://images.unsplash.com/photo-1512918728675-ed5a9ecdebfd?w=600", "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=600", "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=600"] },
    { id: 2, name: "Azure Heights", city: "Mayfair, London", price: 5200000, type: "apartment", status: "buy", beds: 3, baths: 4, sqft: 3200, img: "https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=600", gallery: ["https://images.unsplash.com/photo-1560448204-e02f11c3d0e2?w=600", "https://images.unsplash.com/photo-1502005229762-cf1b2da7c5d6?w=600", "https://images.unsplash.com/photo-1497366216548-37526070297c?w=600"] },
    { id: 3, name: "Modern Minimalist", city: "Hollywood Hills, CA", price: 6900000, type: "villa", status: "buy", beds: 4, baths: 5, sqft: 6800, img: "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600", gallery: ["https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600", "https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?w=600", "https://images.unsplash.com/photo-1600607687920-4e2a09cf159d?w=600"] },
    { id: 4, name: "Palm View Estate", city: "Dubai Marina, UAE", price: 8500000, type: "villa", status: "buy", beds: 5, baths: 6, sqft: 8200, img: "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=600", gallery: ["https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=600", "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=600", "https://images.unsplash.com/photo-1600566753086-00f18efc2291?w=600"] },
    { id: 5, name: "The Grand Residency", city: "Paris, France", price: 4750000, type: "apartment", status: "buy", beds: 3, baths: 3, sqft: 2800, img: "https://images.unsplash.com/photo-1549517045-bc93de075e53?w=600", gallery: ["https://images.unsplash.com/photo-1549517045-bc93de075e53?w=600", "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=600", "https://images.unsplash.com/photo-1600566752355-35792bedcfea?w=600"] },
    { id: 6, name: "Oceanfront Retreat", city: "Malibu, CA", price: 15000000, type: "villa", status: "buy", beds: 6, baths: 7, sqft: 9500, img: "https://images.unsplash.com/photo-1600047509807-ba8f99d2cdde?w=600", gallery: ["https://images.unsplash.com/photo-1600047509807-ba8f99d2cdde?w=600", "https://images.unsplash.com/photo-1512915922686-57c11dde9b6b?w=600", "https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=600"] },
    { id: 7, name: "Crystal Tower Suite", city: "Chicago, IL", price: 3200000, type: "house", status: "buy", beds: 3, baths: 3, sqft: 2900, img: "https://images.unsplash.com/photo-1600607687920-4e2a09cf159d?w=600", gallery: ["https://images.unsplash.com/photo-1600607687920-4e2a09cf159d?w=600", "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=600", "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=600"] },
    { id: 8, name: "Hillside Haven", city: "Beverly Hills, CA", price: 9800000, type: "villa", status: "buy", beds: 5, baths: 6, sqft: 7600, img: "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=600", gallery: ["https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=600", "https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=600", "https://images.unsplash.com/photo-1600607687920-4e2a09cf159d?w=600"] },
    { id: 9, name: "Harbour View Residences", city: "Sydney, Australia", price: 6100000, type: "apartment", status: "buy", beds: 4, baths: 4, sqft: 4100, img: "https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=600", gallery: ["https://images.unsplash.com/photo-1580587771525-78b9dba3b914?w=600", "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=600", "https://images.unsplash.com/photo-1600210492486-724fe5c67fb0?w=600"] },
    { id: 10, name: "The Royal Manor", city: "Knightsbridge, London", price: 18500000, type: "villa", status: "buy", beds: 7, baths: 8, sqft: 12000, img: "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=600", gallery: ["https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=600", "https://images.unsplash.com/photo-1600566753086-00f18efc2291?w=600", "https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=600"] },
    { id: 11, name: "Lakefront Luxury", city: "Geneva, Switzerland", price: 7300000, type: "villa", status: "buy", beds: 5, baths: 5, sqft: 6200, img: "https://images.unsplash.com/photo-1600210492493-0946911123ea?w=600", gallery: ["https://images.unsplash.com/photo-1600210492493-0946911123ea?w=600", "https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=600", "https://images.unsplash.com/photo-1512915922686-57c11dde9b6b?w=600"] },
    { id: 12, name: "Sunset Penthouse", city: "Los Angeles, CA", price: 11000000, type: "house", status: "buy", beds: 4, baths: 4, sqft: 5100, img: "https://images.unsplash.com/photo-1571939228382-b2f2b585ce15?w=600", gallery: ["https://images.unsplash.com/photo-1571939228382-b2f2b585ce15?w=600", "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=600", "https://images.unsplash.com/photo-1502005229762-cf1b2da7c5d6?w=600"] }
];

const CARDS_PER_PAGE = 6;
let currentPage = 1;
let filteredProperties = [...properties];

const agents = [
    { id: 1, name: "ASHA BAI", phone: "+92 300 1234567", photo: "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=150" },
    { id: 2, name: "HIFZA SHAHEEN", phone: "+92 301 2345678", photo: "assets/images/agent_hifza.png" },
    { id: 3, name: "TAYYABA ASHRAF", phone: "+92 302 3456789", photo: "assets/images/agent_tayyaba.png" },
    { id: 4, name: "HARMAIN", phone: "+92 303 4567890", photo: "assets/images/agent_harmain.png" }
];

function getAgentForProperty(propertyIndex) {
    const groupSize = 3; // 3 properties per agent
    const agentIndex = Math.floor(propertyIndex / groupSize) % agents.length;
    return agents[agentIndex];
}

const USD_TO_PKR = 280;

function formatCurrency(val) {
    const priceInPKR = parseInt(val) * USD_TO_PKR;
    return 'Rs. ' + priceInPKR.toLocaleString('en-PK');
}

// 1. Rendering engine
function renderProperties(page) {
    const grid = document.getElementById('properties-grid');
    if (!grid) return;
    
    grid.innerHTML = '';
    
    const startIndex = (page - 1) * CARDS_PER_PAGE;
    const endIndex = startIndex + CARDS_PER_PAGE;
    const pageData = filteredProperties.slice(startIndex, endIndex);
    
    if (pageData.length === 0) {
        grid.innerHTML = `<div class="col-span-1 md:col-span-2 xl:col-span-3 text-center py-20"><p class="text-3xl text-gold mb-3">No Properties Found</p><p class="text-[#4a5568] text-lg">Try adjusting your filters</p></div>`;
    }
    
    pageData.forEach(p => {
        const cardHTML = `
            <div class="property-card flex flex-col h-full glass-panel group relative overflow-hidden transition-all duration-300 hover:-translate-y-2 hover:shadow-[0_0_15px_rgba(212,175,55,0.3)]">
                <div class="property-img-container relative h-56 w-full overflow-hidden">
                    <img src="${p.img}" class="w-full h-full object-cover transition-transform duration-700 group-hover:scale-110" alt="${p.name}" onerror="this.onerror=null; this.src='https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=600';">
                    <div class="absolute top-4 left-4 bg-[#1E3A5F] text-white font-bold px-3 py-1 rounded-lg text-sm z-10">${formatCurrency(p.price)}</div>
                    <div class="absolute top-4 right-4 bg-white/90 backdrop-blur-sm border border-gray-200 text-[#1E3A5F] font-bold px-3 py-1 rounded-lg text-xs z-10 capitalize">${p.type}</div>
                </div>
                <div class="p-6 flex-1 flex flex-col">
                    <h3 class="text-xl font-bold text-[#1E3A5F] mb-2 group-hover:text-gold transition-colors">${p.name}</h3>
                    <p class="text-gray-500 text-sm mb-4 flex items-center"><i class="fas fa-map-marker-alt text-gold mr-2 w-4"></i> ${p.city}</p>
                    
                    <div class="flex items-center justify-between border-y border-gray-200/50 py-4 mb-6">
                        <div class="flex flex-col items-center">
                            <i class="fas fa-bed text-gold mb-1"></i>
                            <span class="text-xs text-gray-500 font-medium">${p.beds} Beds</span>
                        </div>
                        <div class="flex flex-col items-center">
                            <i class="fas fa-bath text-gold mb-1"></i>
                            <span class="text-xs text-gray-500 font-medium">${p.baths} Baths</span>
                        </div>
                        <div class="flex flex-col items-center">
                            <i class="fas fa-vector-square text-gold mb-1"></i>
                            <span class="text-xs text-gray-500 font-medium">${p.sqft.toLocaleString()} sqft</span>
                        </div>
                    </div>
                    
                    <button class="w-full py-3 rounded-lg border border-[#1E3A5F] text-[#1E3A5F] bg-transparent font-bold hover:bg-[#1E3A5F] hover:text-white transition-all duration-300 shadow-sm hover:shadow-md mt-auto" onclick="openDetail(${p.id})">
                        View Details
                    </button>
                </div>
            </div>
        `;
        grid.insertAdjacentHTML('beforeend', cardHTML);
    });
    
    // Update count display
    const countEl = document.querySelector('[class*="Showing"]');
    if(countEl && filteredProperties.length > 0) {
        countEl.innerHTML = `Showing <span class="text-white font-bold">${startIndex + 1}-${Math.min(endIndex, filteredProperties.length)}</span> of <span class="text-gold font-bold">${filteredProperties.length}</span> Properties`;
    }
}

// 2. Pagination UI
function renderPaginationUI() {
    const paginationContainer = document.querySelector('.pagination');
    if (!paginationContainer) return;
    
    paginationContainer.innerHTML = '';
    
    const totalPages = Math.ceil(filteredProperties.length / CARDS_PER_PAGE);
    if (totalPages <= 1) return;
    
    // Prev
    const prevBtn = document.createElement('button');
    prevBtn.className = `w-10 h-10 rounded-xl flex justify-center items-center font-bold border transition-all ${currentPage === 1 ? 'opacity-50 cursor-not-allowed bg-gray-100 border-gray-200 text-gray-400' : 'bg-white border-gray-200 text-[#1E3A5F] hover:bg-[#1E3A5F]/10'}`;
    prevBtn.innerHTML = '<i class="fas fa-chevron-left"></i>';
    prevBtn.disabled = currentPage === 1;
    prevBtn.addEventListener('click', () => changePage(currentPage - 1));
    paginationContainer.appendChild(prevBtn);
    
    // Page Numbers
    for (let i = 1; i <= totalPages; i++) {
        const pageBtn = document.createElement('button');
        pageBtn.className = `w-10 h-10 rounded-xl flex justify-center items-center font-bold border transition-all ${i === currentPage ? 'bg-[#1E3A5F] text-white border-[#1E3A5F] shadow-md' : 'bg-white border-gray-200 text-[#1E3A5F] hover:bg-[#1E3A5F]/10'}`;
        pageBtn.innerText = i;
        pageBtn.addEventListener('click', () => changePage(i));
        paginationContainer.appendChild(pageBtn);
    }
    
    // Next
    const nextBtn = document.createElement('button');
    nextBtn.className = `w-10 h-10 rounded-xl flex justify-center items-center font-bold border transition-all ${currentPage === totalPages ? 'opacity-50 cursor-not-allowed bg-gray-100 border-gray-200 text-gray-400' : 'bg-white border-gray-200 text-[#1E3A5F] hover:bg-[#1E3A5F]/10'}`;
    nextBtn.innerHTML = '<i class="fas fa-chevron-right"></i>';
    nextBtn.disabled = currentPage === totalPages;
    nextBtn.addEventListener('click', () => changePage(currentPage + 1));
    paginationContainer.appendChild(nextBtn);
}

function changePage(newPage) {
    currentPage = newPage;
    renderProperties(currentPage);
    renderPaginationUI();
    
    const grid = document.getElementById('properties-grid');
    if (grid) {
        window.scrollTo({
            top: grid.offsetTop - 120,
            behavior: 'smooth'
        });
    }
}

// 3. Filtering
function applyFilters() {
    const keyword = document.getElementById('filter-keyword')?.value.toLowerCase().trim() || '';
    const type = document.getElementById('filter-type')?.value.toLowerCase() || 'all';
    const city = document.getElementById('filter-city')?.value.toLowerCase() || 'all';
    const status = document.getElementById('filter-status')?.value.toLowerCase() || 'all';
    const minPrice = parseFloat(document.getElementById('filter-minprice')?.value) || 0;
    const maxPrice = parseFloat(document.getElementById('filter-maxprice')?.value) || Infinity;
    const beds = document.getElementById('filter-beds')?.value || 'any';
    const baths = document.getElementById('filter-baths')?.value || 'any';

    filteredProperties = properties.filter(p => {
        if(keyword && !p.name.toLowerCase().includes(keyword) && !p.city.toLowerCase().includes(keyword)) return false;
        if(type !== 'all' && p.type !== type) return false;
        if(city !== 'all' && !p.city.toLowerCase().includes(city)) return false;
        if(status !== 'all' && p.status !== status) return false;
        if(p.price < minPrice) return false;
        if(maxPrice !== Infinity && p.price > maxPrice) return false;
        if(beds !== 'any' && p.beds < parseInt(beds)) return false;
        if(baths !== 'any' && p.baths < parseInt(baths)) return false;
        return true;
    });

    currentPage = 1;
    renderProperties(currentPage);
    renderPaginationUI();
}

function resetFilters() {
    ['filter-keyword','filter-type','filter-city',
     'filter-status','filter-minprice',
     'filter-maxprice','filter-beds',
     'filter-baths'].forEach(id => {
      const el = document.getElementById(id);
      if(!el) return;
      if(el.tagName === 'SELECT') 
        el.selectedIndex = 0;
      else el.value = '';
    });

    const minInput = document.getElementById('filter-minprice');
    const maxInput = document.getElementById('filter-maxprice');
    const priceDisplay = document.getElementById('price-display');
    if(priceDisplay) {
        priceDisplay.textContent = '$0 — Any';
    }

    filteredProperties = [...properties];
    currentPage = 1;
    renderProperties(currentPage);
    renderPaginationUI();
}

// 4. Initialization
    if(document.getElementById('properties-grid')) {
        renderProperties(currentPage);
        renderPaginationUI();
    }
    
    const applyBtn = document.getElementById('apply-filters');
    if (applyBtn) applyBtn.addEventListener('click', applyFilters);
    
    const resetBtn = document.getElementById('reset-filters');
    if (resetBtn) resetBtn.addEventListener('click', resetFilters);

// ==========================================
// MOBILE MENU TOGGLE LOGIC
// ==========================================

    // === HAMBURGER MENU TOGGLE (All Pages) ===
    const menuToggle = document.getElementById('mobile-menu-toggle');
    const mobileMenu = document.getElementById('mobile-menu');
    const mobileMenuClose = document.getElementById('mobile-menu-close');

    if (menuToggle && mobileMenu) {
        menuToggle.addEventListener('click', function() {
            mobileMenu.classList.toggle('open');
            document.body.style.overflow = mobileMenu.classList.contains('open') ? 'hidden' : '';
        });
    }

    if (mobileMenuClose && mobileMenu) {
        mobileMenuClose.addEventListener('click', function() {
            mobileMenu.classList.remove('open');
            document.body.style.overflow = '';
        });
    }

    // Close mobile menu when a nav link is clicked
    const mobileMenuLinks = document.querySelectorAll('#mobile-menu a');
    mobileMenuLinks.forEach(link => {
        link.addEventListener('click', function() {
            if (mobileMenu && mobileMenu.classList.contains('open')) {
                mobileMenu.classList.remove('open');
                document.body.style.overflow = '';
            }
        });
    });

// ==========================================
// SPA MODAL LOGIC (Global Scope)
// ==========================================
    window.openDetail = function(id) {
        const p = properties.find(x => x.id === Number(id));
        if (!p) {
            console.error("Property not found for id:", id);
            return;
        }
        
        const overlay = document.getElementById('detailOverlay');
        if (!overlay) return;
        
        document.getElementById('detailTitle').textContent = p.name;
        document.getElementById('detailLocation').querySelector('span').textContent = p.city;
        document.getElementById('detailPrice').textContent = formatCurrency(p.price);
        
        const detailImage = document.getElementById('detailImage');
        detailImage.src = p.img || p.image || 'https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=600';
        detailImage.onerror = function() {
            this.onerror = null;
            this.src = 'https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=600';
        };
        
        // --- Gallery Section (NEW - safe addition) ---
        const galleryContainer = document.getElementById('detailGallery');
        if (galleryContainer) {
            if (p.gallery && p.gallery.length > 0) {
              galleryContainer.innerHTML = p.gallery
                .map((img, index) => `
                  <img src="${img}"
                       class="thumb ${index === 0 ? 'active' : ''}"
                       onclick="document.getElementById('detailImage').src='${img}';
                                document.querySelectorAll('#detailGallery .thumb').forEach(t => t.classList.remove('active'));
                                this.classList.add('active');"
                       onerror="this.style.display='none'">
                `).join('');
              galleryContainer.style.display = 'flex';

              // Main image starts with first gallery image
              document.getElementById('detailImage').src = p.gallery[0];
            } else {
              // No gallery images — hide thumbnail row, keep main image as-is
              galleryContainer.innerHTML = '';
              galleryContainer.style.display = 'none';
            }
        }
        
        // Map
        const mapFrame = document.getElementById('detailMap');
        if (mapFrame) {
            mapFrame.src = `https://www.google.com/maps?q=${encodeURIComponent(p.city)}&output=embed`;
        }
        
        // Extra Info
        document.getElementById('detailType').textContent = p.type ? p.type.charAt(0).toUpperCase() + p.type.slice(1) : 'N/A';
        
        const yearWrapper = document.getElementById('detailYearWrapper');
        if (p.yearBuilt) {
            document.getElementById('detailYear').textContent = p.yearBuilt;
            if (yearWrapper) yearWrapper.style.display = 'inline';
        } else if (yearWrapper) { yearWrapper.style.display = 'none'; }
        
        const parkingWrapper = document.getElementById('detailParkingWrapper');
        if (p.parking) {
            document.getElementById('detailParking').textContent = p.parking;
            if (parkingWrapper) parkingWrapper.style.display = 'inline';
        } else if (parkingWrapper) { parkingWrapper.style.display = 'none'; }
        
        const propIdWrapper = document.getElementById('detailPropIdWrapper');
        if (p.propertyId) {
            document.getElementById('detailPropId').textContent = p.propertyId;
            if (propIdWrapper) propIdWrapper.style.display = 'inline';
        } else if (propIdWrapper) { propIdWrapper.style.display = 'none'; }
        
        document.getElementById('detailBeds').textContent = p.beds;
        document.getElementById('detailBaths').textContent = p.baths;
        document.getElementById('detailSqft').textContent = p.sqft.toLocaleString() + ' sqft';
        
        // Fallback description/features logic
        const defaultDescription = `Experience ultra-luxury living in this exquisite ${p.type || 'property'}. Featuring state-of-the-art amenities, breathtaking views, and unparalleled architectural elegance.`;
        document.getElementById('detailDescription').textContent = p.description || defaultDescription;
        
        const defaultFeatures = ["Premium Location", "24/7 Security", "Smart Home Tech", "Concierge Service"];
        const featuresToUse = (p.features && p.features.length) ? p.features : defaultFeatures;
        document.getElementById('detailFeatures').innerHTML = featuresToUse.map(f => `<li><i class="fas fa-check text-gold mr-2"></i> ${f}</li>`).join('');

        // Agent Info Logic (Option B: Index-based round-robin)
        let agent = p.agentId ? agents.find(a => a.id === p.agentId) : null;
        if (!agent) {
            const index = properties.findIndex(x => x.id === p.id);
            agent = index >= 0 ? getAgentForProperty(index) : null;
        }
        if (!agent) {
            agent = { name: "Property Consultant", phone: "+92 300 0000000", photo: "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=150" };
        }

        const agentName = document.getElementById('agentName');
        const agentPhone = document.getElementById('agentPhone');
        const agentPhoto = document.getElementById('agentPhoto');
        const callAgentBtn = document.getElementById('callAgentBtn');
        
        if (agentName) agentName.textContent = agent.name;
        if (agentPhone) agentPhone.textContent = agent.phone;
        if (agentPhoto) {
            agentPhoto.src = agent.photo;
            agentPhoto.onerror = function() { this.src = "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=150"; };
        }
        
        const agentPhoneRaw = agent.phone;
        if (callAgentBtn) callAgentBtn.href = `tel:${agentPhoneRaw}`;

        // Schedule a Visit Button Redirect
        const scheduleBtn = document.getElementById('scheduleVisitBtn');
        if (scheduleBtn) {
            scheduleBtn.onclick = () => {
                let agentUrl = '/Home/Contact#contact-form'; // Default
                if (agent && agent.name === 'HIFZA SHAHEEN') agentUrl = '/Agents/Profile/hifza#contact-agent';
                else if (agent && agent.name === 'TAYYABA ASHRAF') agentUrl = '/Agents/Profile/tayyaba#contact-agent';
                else if (agent && agent.name === 'HARMAIN') agentUrl = '/Agents/Profile/harmain#contact-agent';
                window.location.href = agentUrl;
            };
        }

        // Dynamic Action Button
        const actionBtn = document.getElementById('detailActionBtn');
        if (actionBtn) {
            if (p.status === "rent") {
                actionBtn.textContent = "Inquire to Rent";
                actionBtn.dataset.action = "rent";
            } else {
                actionBtn.textContent = "Inquire to Purchase";
                actionBtn.dataset.action = "buy";
            }
            
            actionBtn.onclick = function() {
                const msg = `Hi, I'm interested in ${p.name} (${p.status === 'rent' ? 'Rent' : 'Purchase'}) - Property ID: ${p.propertyId || p.id}`;
                window.open(`https://wa.me/${agentPhoneRaw.replace(/\D/g, '')}?text=${encodeURIComponent(msg)}`, '_blank');
            };
        }
        
        // Favorite/Share buttons
        const favBtn = document.getElementById('saveFavoriteBtn');
        if (favBtn) {
            // Initial state
            let wishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
            let isSaved = wishlist.includes(p.id);
            const icon = favBtn.querySelector('i');
            
            if (isSaved) {
                icon.className = 'fas fa-heart text-red-500';
            } else {
                icon.className = 'far fa-heart';
            }

            favBtn.onclick = function() {
                wishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
                isSaved = wishlist.includes(p.id);
                
                if (isSaved) {
                    wishlist = wishlist.filter(id => id !== p.id);
                    icon.className = 'far fa-heart text-gray-400 hover:text-red-500';
                } else {
                    wishlist.push(p.id);
                    icon.className = 'fas fa-heart text-red-500';
                }
                localStorage.setItem('wishlist', JSON.stringify(wishlist));
                
                // Re-render if on wishlist page
                if (typeof renderWishlist === 'function') {
                    renderWishlist();
                }
            };
        }
        
        const shareBtn = document.getElementById('shareBtn');
        if (shareBtn) {
            shareBtn.onclick = function() {
                navigator.clipboard.writeText(window.location.href).then(() => {
                    alert('Link copied to clipboard!');
                });
            };
        }

        overlay.classList.add('open');
        document.body.style.overflow = 'hidden';
    };

    window.closeOverlay = function() {
        const overlay = document.getElementById('detailOverlay');
        if (overlay) {
            overlay.classList.remove('open');
            document.body.style.overflow = '';
        }
    };

    // Close on background click and Escape key
    const overlay = document.getElementById('detailOverlay');
    if (overlay) {
        // Outside click
        overlay.addEventListener('click', function(e) {
            if (e.target === this) {
                closeOverlay();
            }
        });
    }

    document.addEventListener('keydown', function(e) {
        const overlay = document.getElementById('detailOverlay');
        if (e.key === 'Escape' && overlay && overlay.classList.contains('open')) {
            closeOverlay();
        }
    });

    // ==========================================
    // WISHLIST LOGIC
    // ==========================================
    window.renderWishlist = function() {
        const grid = document.getElementById('wishlist-grid');
        if (!grid) return; // Only execute on wishlist page
        
        let wishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
        const items = properties.filter(p => wishlist.includes(p.id));
        
        const countEl = document.getElementById('wishlist-count');
        if (countEl) countEl.textContent = items.length + " Properties Saved";
        
        if (items.length === 0) {
            grid.innerHTML = '<div class="col-span-1 md:col-span-3 text-center py-20"><p class="text-2xl text-[#1E3A5F]">Your wishlist is empty</p><a href="properties.html" class="mt-4 inline-block btn-gold px-6 py-2 rounded-lg text-sm">Browse Properties</a></div>';
            return;
        }
        
        grid.innerHTML = '';
        items.forEach(p => {
            const html = `
                <div class="property-card glass-panel flex flex-col h-full" data-aos="fade-up">
                    <div class="property-img-container relative h-56 w-full overflow-hidden rounded-t-2xl">
                        <img src="${p.img || 'https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=600'}" class="w-full h-full object-cover" alt="${p.name}">
                        <span class="absolute top-4 left-4 bg-gold text-white font-bold px-3 py-1 rounded-lg text-sm z-10">${formatCurrency(p.price)}</span>
                    </div>
                    <div class="p-6 flex flex-col flex-grow">
                        <h3 class="text-xl font-bold mb-2 text-[#1E3A5F]">${p.name}</h3>
                        <p class="text-gray-500 text-sm mb-6"><i class="fas fa-map-marker-alt text-gold mr-2"></i> ${p.city}</p>
                        <div class="mt-auto pt-4 border-t border-gray-200">
                            <button onclick="removeFromWishlist(${p.id})" class="w-full text-center py-[12px] rounded-[8px] border-2 border-red-500 text-red-500 hover:bg-red-500 hover:text-white transition-colors duration-200 font-bold text-sm">
                                <i class="fas fa-trash mr-2"></i> Remove from Wishlist
                            </button>
                        </div>
                    </div>
                </div>
            `;
            grid.insertAdjacentHTML('beforeend', html);
        });
    };

    window.removeFromWishlist = function(id) {
        let wishlist = JSON.parse(localStorage.getItem('wishlist') || '[]');
        wishlist = wishlist.filter(itemId => itemId !== id);
        localStorage.setItem('wishlist', JSON.stringify(wishlist));
        renderWishlist();
    };

    // Render on load if on wishlist page
    renderWishlist();

    // ==========================================
    // AI RECOMMENDATION LOGIC
    // ==========================================
    window.generateAIRecommendation = function() {
        const budget = document.getElementById('ai-budget').value;
        const city = document.getElementById('ai-city').value;
        const type = document.getElementById('ai-type').value;
        const status = document.getElementById('ai-status').value;
        
        const resultCard = document.getElementById('ai-result-card');
        const matchType = document.getElementById('ai-match-type');
        const matchTitle = document.getElementById('ai-match-title');
        const matchDesc = document.getElementById('ai-match-desc');
        const matchLocation = document.getElementById('ai-match-location');
        const matchPrice = document.getElementById('ai-match-price');
        
        // Simulate thinking/loading state on button
        const btn = document.querySelector('button[onclick="generateAIRecommendation()"]');
        const originalBtnHTML = btn.innerHTML;
        btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Analyzing preferences...';
        btn.disabled = true;
        
        // Hide card if visible
        resultCard.classList.remove('opacity-100', 'translate-y-0');
        resultCard.classList.add('opacity-0', 'translate-y-4');
        
        setTimeout(() => {
            let finalCity = city !== 'Any' ? city : 'Karachi';
            let finalType = type !== 'Any' ? type : 'Apartment';
            
            let formattedBudget = budget ? formatCurrency(budget) : 'Flexible';
            
            matchType.textContent = finalType;
            matchTitle.textContent = `Perfect ${finalType}s for ${status === 'rent' ? 'Renting' : 'Buying'}`;
            
            matchDesc.innerHTML = `Based on your budget and preferences, we recommend premium <strong>${finalType}s</strong> in <strong>${finalCity}</strong>. These properties offer excellent value, modern amenities, and convenient access to schools, shopping centers, and public transportation. Perfect for your lifestyle needs.`;
            
            matchLocation.textContent = finalCity;
            matchPrice.textContent = budget ? `Around ${formattedBudget}` : 'Based on selection';
            
            // Show result
            resultCard.classList.remove('hidden');
            
            // Trigger reflow to ensure transition happens
            void resultCard.offsetWidth;
            
            resultCard.classList.remove('opacity-0', 'translate-y-4');
            resultCard.classList.add('opacity-100', 'translate-y-0');
            
            // Restore button
            btn.innerHTML = originalBtnHTML;
            btn.disabled = false;
            
        }, 1500); // 1.5s delay to simulate AI processing
    };

}); // End of the single DOMContentLoaded block

