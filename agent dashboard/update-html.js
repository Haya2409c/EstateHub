const fs = require('fs');
const path = require('path');

const dir = __dirname;
const htmlFiles = fs.readdirSync(dir).filter(f => f.endsWith('.html'));

// MVC Route mappings
const routes = {
    'index.html': '/AgentDashboard/Index',
    'listings.html': '/AgentDashboard/Listings',
    'add-property.html': '/AgentDashboard/AddProperty',
    'edit-property.html': '/AgentDashboard/EditProperty',
    'property-images.html': '/AgentDashboard/PropertyImages',
    'leads.html': '/AgentDashboard/Leads',
    'packages.html': '/AgentDashboard/Packages',
    'profile.html': '/AgentDashboard/Profile',
    'notifications.html': '/AgentDashboard/Notifications'
};

// JS File mappings
const jsMap = {
    'index.html': 'dashboard.js',
    'listings.html': 'listings.js',
    'add-property.html': 'add-property.js',
    'edit-property.html': 'edit-property.js',
    'property-images.html': 'media.js',
    'leads.html': 'leads.js',
    'packages.html': 'packages.js',
    'profile.html': 'profile.js',
    'notifications.html': 'notifications.js'
};

for (const file of htmlFiles) {
    let content = fs.readFileSync(path.join(dir, file), 'utf8');

    const jsName = jsMap[file];
    const scriptReplacement = '<script src="js/app.js"></script>\n    <script src="js/' + jsName + '"></script>';
    content = content.replace(/<script src="app\.js"><\/script>/g, scriptReplacement);

    for (const [page, route] of Object.entries(routes)) {
        // Regex to find <a href="page.html"...
        const regex = new RegExp('(<li>\\s*)?<a href="' + page + '"', 'g');
        content = content.replace(regex, '$1<!-- Future MVC Route: ' + route + ' -->\n                        <a href="' + page + '"');
    }

    if (file === 'listings.html') {
        content = content.replace(/<table class="w-full text-left"/, '<table id="propertyTable" class="w-full text-left"');
    } else if (file === 'leads.html') {
        content = content.replace(/<table class="w-full text-left"/, '<table id="leadTable" class="w-full text-left"');
    } else if (file === 'profile.html') {
        content = content.replace(/<form class="space-y-6">/, '<form id="profileForm" class="space-y-6">');
    } else if (file === 'notifications.html') {
        content = content.replace(/<div class="space-y-4">/, '<div id="notificationList" class="space-y-4">');
    }

    fs.writeFileSync(path.join(dir, file), content, 'utf8');
    console.log("Updated " + file);
}
