const fs = require('fs');

const files = fs.readdirSync('.').filter(f => f.endsWith('.html'));
files.forEach(f => {
  const content = fs.readFileSync(f, 'utf8');
  if (!content.includes('js/app.js')) {
      console.log(f + ' IS MISSING js/app.js');
  }
  
  const scripts = content.match(/<script.*?src=[\'\"](.*?)[\'\"].*?>/g) || [];
  console.log(f + ' scripts: ' + scripts.map(s => s.match(/src=[\'\"](.*?)[\'\"]/)[1]).join(', '));
  
  // also check if any navigation links were broken by our MVC regex
  // search for <!-- Future MVC Route: undefined -->
  if (content.includes('<!-- Future MVC Route: undefined -->')) {
      console.log(f + ' HAS UNDEFINED MVC ROUTES!');
  }
});
