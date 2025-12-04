<#
.SYNOPSIS
    Writes a Playwright test script to a file.

.DESCRIPTION
    This script creates a Playwright JavaScript test file that tests a website
    by navigating pages and taking screenshots.

.PARAMETER OutputPath
    The path where the test script should be written

.EXAMPLE
    .\Write-PlaywrightTestScript.ps1 -OutputPath "test.js"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$testScript = @"
const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

(async () => {
  const browser = await chromium.launch();
  const context = await browser.newContext({
    ignoreHTTPSErrors: true
  });
  const page = await context.newPage();

  const screenshotsDir = path.join(__dirname, 'screenshots');
  if (!fs.existsSync(screenshotsDir)) {
    fs.mkdirSync(screenshotsDir, { recursive: true });
  }

  const baseUrl = process.env.SITE_URL;
  console.log('Testing site at:', baseUrl);

  let counter = 1;
  try {
    console.log('Navigating to home page...');
    await page.goto(baseUrl, { waitUntil: 'networkidle', timeout: 30000 });

    // Take screenshot of home page
    await page.screenshot({
      path: path.join(screenshotsDir, '01-home.png'),
      fullPage: true,
    });
    console.log('Screenshot saved: 01-home.png');

    // Find all internal links on the page
    const links = await page.evaluate((baseUrl) => {
      const anchors = Array.from(document.querySelectorAll('a[href]'));
      return anchors
        .map((a) => a.href)
        .filter((href) => {
          try {
            const url = new URL(href);
            const baseUrlObj = new URL(baseUrl);
            return (
              url.hostname === baseUrlObj.hostname &&
              !href.includes('#') &&
              !href.includes('javascript:')
            );
          } catch {
            return false;
          }
        })
        .filter((value, index, self) => self.indexOf(value) === index);
    }, baseUrl);

    console.log('Found ' + links.length + ' internal links to test');

    // Visit each discovered link
    counter = 2;
    for (const link of links.slice(0, 25)) {
      try {
        console.log('Navigating to:', link);
        await page.goto(link, { waitUntil: 'networkidle', timeout: 30000 });

        const screenshotName =
          counter.toString().padStart(2, '0') +
          '-' +
          link
            .replace(baseUrl, '')
            .replace(/[^a-z0-9]/gi, '-')
            .substring(0, 50) +
          '.png';

        await page.screenshot({
          path: path.join(screenshotsDir, screenshotName),
          fullPage: true,
        });
        console.log('Screenshot saved:', screenshotName);
        counter++;
      } catch (error) {
        console.error('Error visiting', link, ':', error.message);
      }
    }

    console.log('Testing complete!');
  } catch (error) {
    console.error('Error during testing:', error);
    process.exit(1);
  }

  await browser.close();
})();
"@

$testScript | Out-File -FilePath $OutputPath -Encoding UTF8
Write-Host "Playwright test script written to: $OutputPath" -ForegroundColor Green
