const fs = require('fs');
const path = require('path');

const newSidebar = `    <!-- SIDEBAR -->
    <aside class="sidebar" id="sidebar">

      <!-- Logo -->
      <div class="sidebar-logo">
        <div style="width:36px;height:36px;background:linear-gradient(135deg,#2563EB,#1E3A5F);border-radius:10px;display:flex;align-items:center;justify-content:center;flex-shrink:0;">
          <i class="fas fa-building" style="color:#FFFFFF;font-size:1rem;"></i>
        </div>
        <div>
          <div class="sidebar-logo-text">Estate Hub</div>
          <div class="sidebar-logo-sub">Seller Dashboard</div>
        </div>
      </div>

      <!-- Navigation -->
      <nav class="sidebar-nav">

        <!-- Main -->
        <div class="nav-section-label">Main</div>

        <a href="index.html" class="nav-item" data-page="index.html">
          <i class="fas fa-gauge-high"></i>
          <span>Dashboard</span>
        </a>

        <!-- Properties Section -->
        <div class="nav-section-label">Properties</div>

        <a href="my-properties.html" class="nav-item" data-page="my-properties.html">
          <i class="fas fa-house"></i>
          <span>My Properties</span>
        </a>

        <div class="nav-sub">
          <a href="add-property.html" class="nav-item" data-page="add-property.html">
            <i class="fas fa-plus"></i>
            <span>Add Property</span>
          </a>
        </div>

        <!-- Media -->
        <div class="nav-section-label">Media</div>

        <a href="media.html" class="nav-item" data-page="media.html">
          <i class="fas fa-images"></i>
          <span>Property Images</span>
        </a>

        <!-- Account -->
        <div class="nav-section-label">Account</div>

        <a href="packages.html" class="nav-item" data-page="packages.html">
          <i class="fas fa-credit-card"></i>
          <span>Packages</span>
        </a>

        <a href="profile.html" class="nav-item" data-page="profile.html">
          <i class="fas fa-user-gear"></i>
          <span>Profile Settings</span>
        </a>

      </nav>

      <!-- Sidebar User + Logout -->
      <div class="sidebar-bottom">

        <!-- Logged-in user info -->
        <div class="sidebar-user">
          <div class="sidebar-avatar">AK</div>
          <div class="sidebar-user-info">
            <div class="sidebar-user-name">Ahmed Khan</div>
            <div class="sidebar-user-role">Seller Account</div>
          </div>
        </div>

        <!-- Logout -->
        <a href="logout.html" class="nav-item nav-item-logout" data-page="logout.html">
          <i class="fas fa-right-from-bracket"></i>
          <span>Logout</span>
        </a>

      </div>

    </aside>`;

const files = [
    { file: 'index.html', title: 'Estate Hub | Dashboard' },
    { file: 'my-properties.html', title: 'Estate Hub | My Properties' },
    { file: 'add-property.html', title: 'Estate Hub | Add Property' },
    { file: 'edit-property.html', title: 'Estate Hub | Edit Property' },
    { file: 'packages.html', title: 'Estate Hub | Packages' },
    { file: 'profile.html', title: 'Estate Hub | Profile Settings' },
    { file: 'media.html', title: 'Estate Hub | Property Images' },
    { file: 'logout.html', title: 'Estate Hub | Logout' }
];

files.forEach(({ file, title }) => {
    let content = fs.readFileSync(file, 'utf8');
    
    // Replace title
    content = content.replace(/<title>.*?<\/title>/, `<title>${title}</title>`);
    
    // Replace sidebar
    const sidebarRegex = /<!-- Sidebar -->[\s\S]*?<\/aside>/i;
    content = content.replace(sidebarRegex, newSidebar);
    
    fs.writeFileSync(file, content, 'utf8');
    console.log(`Updated ${file}`);
});
