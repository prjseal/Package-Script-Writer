var psw = psw || {
    controls: {
        packages: document.getElementById('Packages'),
        installUmbracoTemplate: document.getElementById('InstallUmbracoTemplate'),
        umbracoTemplateVersion: document.getElementById('UmbracoTemplateVersion'),
        includeStarterKit: document.getElementById('IncludeStarterKit'),
        starterKitPackage: document.getElementById('StarterKitPackage'),
        createSolutionFile: document.getElementById('CreateSolutionFile'),
        solutionName: document.getElementById('SolutionName'),
        projectName: document.getElementById('ProjectName'),
        useUnattendedInstall: document.getElementById('UseUnattendedInstall'),
        userFriendlyName: document.getElementById('UserFriendlyName'),
        userEmail: document.getElementById('UserEmail'),
        userPassword: document.getElementById('UserPassword'),
        databaseType: document.getElementById('DatabaseType'),
        search: document.getElementById('search'),
        codeBlock: document.querySelector('pre'),
        packageCheckboxes: document.querySelectorAll('#packagelist input[type=checkbox]'),
        packageCards: document.querySelectorAll('#packagelist .card'),
        codeNavItem: document.getElementById('code-nav-item')
    },
    buttons: {
        clearpackages: document.getElementById('clearpackages'),
        reset: document.getElementById('reset'),
        copy: document.getElementById('copy'),
        generate: document.getElementById('generate'),
        update: document.getElementById('update')
    },
    init: function () {
        psw.addListeners();
    },
    addListeners() {
        psw.buttons.clearpackages.addEventListener('click', function (event) {
            psw.clearAllPackages(event);
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.buttons.reset.addEventListener('click', function (event) {
            psw.reset(event);
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.search.addEventListener('keyup', psw.filterPackages);

        psw.controls.useUnattendedInstall.addEventListener('change', function () {
            psw.toggleUnattendedInstallControls();
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.installUmbracoTemplate.addEventListener('change', function () {
            psw.toggleInstallUmbracoTemplateControls();
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.includeStarterKit.addEventListener('change', function () {
            psw.toggleIncludeStarterKitControls();
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.createSolutionFile.addEventListener('change', function () {
            psw.toggleCreateSolutionFileControls();
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.buttons.copy.addEventListener('click', function (event) {
            psw.copyCodeBlock(event);
        });

        psw.buttons.generate.addEventListener('click', function (event) {
            event.preventDefault();
            $('#code-tab').tab('show');
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.buttons.update.addEventListener('click', function (event) {
            event.preventDefault();
            $('#code-tab').tab('show');
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.packageCheckboxes.forEach(function (checkbox) {
            checkbox.addEventListener('change', function () {
                psw.updatePackages(this.id);
                psw.updateOutput();
                psw.updateUrl();
            });
        });

        psw.controls.packageCards.forEach(function (card) {
            card.addEventListener('click', function (event) {
                if (event.target.nodeName === 'A' || event.target.nodeName == 'INPUT') return;
                var checkbox = this.querySelector('input[type="checkbox"]');
                checkbox.checked = !checkbox.checked;
                psw.updatePackages(checkbox.id);
                psw.updateOutput();
                psw.updateUrl();
            });
        });

        var tab = document.querySelector('#myTab a');
        tab.addEventListener('click', function (event) {
            event.preventDefault();
            $(this).tab('show');
        });

        psw.controls.search.addEventListener('keypress', function () {
            if (event.key === "Enter") {
                event.preventDefault();
            }
        });

        psw.controls.solutionName.addEventListener('keyup', psw.debounce(function () {
            psw.updateOutput();
            psw.updateUrl();
        }, 250));

        psw.controls.projectName.addEventListener('keyup', psw.debounce(function () {
            psw.updateOutput();
            psw.updateUrl();
        }, 250));

        psw.controls.userFriendlyName.addEventListener('keyup', psw.debounce(function () {
            psw.updateOutput();
            psw.updateUrl();
        }, 250));

        psw.controls.userEmail.addEventListener('keyup', psw.debounce(function () {
            psw.updateOutput();
            psw.updateUrl();
        }, 250));

        psw.controls.userPassword.addEventListener('keyup', psw.debounce(function () {
            psw.updateOutput();
            psw.updateUrl();
        }, 250));

        psw.controls.installUmbracoTemplate.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.includeStarterKit.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.createSolutionFile.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.useUnattendedInstall.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.umbracoTemplateVersion.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.starterKitPackage.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });

        psw.controls.databaseType.addEventListener('change', function () {
            psw.updateOutput();
            psw.updateUrl();
        });
    },
    updatePackages: function (checkboxId) {
        var checkbox = document.getElementById(checkboxId);
        var thisVal = checkbox.value;
        var allVals = psw.controls.packages.value;

        var card = checkbox.closest('.card');

        if (checkbox.checked) {
            card.classList.add('selected');
            card.classList.add('shadow');
        }
        else {
            card.classList.remove('selected');
            card.classList.remove('shadow');
        }

        if (!checkbox.checked && allVals != '') {
            allVals = allVals.replace(thisVal + ',', '');
            allVals = allVals.replace(',' + thisVal, '');
            allVals = allVals.replace(thisVal, '');
        }
        else {
            if (allVals != '') {
                allVals += ',' + thisVal;
            }
            else {
                allVals = thisVal;
            }
        }
        psw.controls.packages.value = allVals;
    },
    deselectPackage: function (element) {
        element.checked = false;
        element.val
        var card = element.closest('.card');
        card.classList.remove("selected");
    },
    clearAllPackages: function (event) {
        event.preventDefault();
        var packageListCheckboxes = document.querySelectorAll('#packagelist input[type="checkbox"]');
        var packageListCheckboxesArr = Array.from(packageListCheckboxes);
        packageListCheckboxesArr.forEach(psw.deselectPackage);
        psw.controls.packages.value = '';
    },
    reset: function (event) {
        event.preventDefault();
        psw.controls.installUmbracoTemplate.checked = true;
        psw.controls.umbracoTemplateVersion.value = '';
        psw.controls.includeStarterKit.checked = true;
        psw.controls.starterKitPackage.value = 'clean';
        psw.controls.createSolutionFile.checked = true;
        psw.controls.solutionName.value = 'MySolution';
        psw.controls.projectName.value = 'MyProject';
        psw.controls.useUnattendedInstall.checked = true;
        psw.controls.userFriendlyName.value = 'Administrator';
        psw.controls.userEmail.value = 'admin@example.com';
        psw.controls.userPassword.value = '1234567890';
        psw.controls.databaseType.value = 'SQLite';

        psw.controls.umbracoTemplateVersion.removeAttribute('disabled');
        psw.controls.starterKitPackage.removeAttribute('disabled');
        psw.controls.solutionName.removeAttribute('disabled');
        psw.controls.databaseType.removeAttribute('disabled');
        psw.controls.userFriendlyName.removeAttribute('disabled');
        psw.controls.userEmail.removeAttribute('disabled');
        psw.controls.userPassword.removeAttribute('disabled');
    },
    filterPackages: function () {
        var filter, ul, li, a, i, txtValue;
        filter = psw.controls.search.value.toUpperCase();
        ul = document.getElementById("packagelist");
        li = ul.getElementsByClassName("packageItem");

        for (i = 0; i < li.length; i++) {
            txtValue = li[i].getAttribute("data-packageid");
            if (txtValue.toUpperCase().indexOf(filter) > -1) {
                li[i].style.display = "";
            } else {
                li[i].style.display = "none";
            }
        }
    },
    toggleUnattendedInstallControls: function () {
        if (psw.controls.useUnattendedInstall.checked) {
            psw.controls.databaseType.removeAttribute('disabled');
            psw.controls.userFriendlyName.removeAttribute('disabled');
            psw.controls.userEmail.removeAttribute('disabled');
            psw.controls.userPassword.removeAttribute('disabled');
        }
        else {
            psw.controls.databaseType.setAttribute('disabled', 'disabled');
            psw.controls.userFriendlyName.setAttribute('disabled', 'disabled');
            psw.controls.userEmail.setAttribute('disabled', 'disabled');
            psw.controls.userPassword.setAttribute('disabled', 'disabled');
        }
    },
    toggleInstallUmbracoTemplateControls: function () {
        if (psw.controls.installUmbracoTemplate.checked) {
            psw.controls.umbracoTemplateVersion.removeAttribute('disabled');
        }
        else {
            psw.controls.umbracoTemplateVersion.setAttribute('disabled', 'disabled');
        }
    },
    toggleIncludeStarterKitControls: function () {
        if (psw.controls.includeStarterKit.checked) {
            psw.controls.starterKitPackage.removeAttribute('disabled');
        }
        else {
            psw.controls.starterKitPackage.setAttribute('disabled', 'disabled');
        }
    },
    toggleCreateSolutionFileControls: function () {
        if (psw.controls.createSolutionFile.checked) {
            psw.controls.solutionName.removeAttribute('disabled');
        }
        else {
            psw.controls.solutionName.setAttribute('disabled', 'disabled');
        }
    },
    copyCodeBlock: function (event) {
        event.preventDefault();
        navigator.clipboard.writeText(psw.controls.codeBlock.innerText);
    },
    updateOutput: function () {

        var data = {
            "InstallUmbracoTemplate": psw.controls.installUmbracoTemplate.checked,
            "UmbracoTemplateVersion": psw.controls.umbracoTemplateVersion.value,
            "Packages": psw.controls.packages.value,
            "UserEmail": psw.controls.userEmail.value,
            "ProjectName": psw.controls.projectName.value,
            "CreateSolutionFile": psw.controls.createSolutionFile.checked,
            "SolutionName": psw.controls.solutionName.value,
            "UseUnattendedInstall": psw.controls.useUnattendedInstall.checked,
            "DatabaseType": psw.controls.databaseType.value,
            "UserPassword": psw.controls.userPassword.value,
            "UmbracoTemplateVersion": psw.controls.umbracoTemplateVersion.value,
            "UserFriendlyName": psw.controls.userFriendlyName.value,
            "IncludeStarterKit": psw.controls.includeStarterKit.checked,
            "StarterKitPackage": psw.controls.starterKitPackage.value
        }

        var url = "/api/scriptgeneratorapi/generatescript";

        fetch(url, {
            method: 'POST', // *GET, POST, PUT, DELETE, etc.
            mode: 'cors', // no-cors, *cors, same-origin
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data) // body data type must match "Content-Type" header
        }).then((response) => {
            var result = response.text();
            if (response.ok) {
                return result;
            }
        }).then((data) => {
            psw.controls.codeBlock.innerHTML = data;
            psw.controls.codeBlock.classList.remove('prettyprinted');
            PR.prettyPrint();
            psw.controls.codeNavItem.classList.add('glow');
            setTimeout(function () {
                psw.controls.codeNavItem.classList.remove('glow');
            }, 1000)
        }).catch((error) => {
            console.log(error);
        });
    },
    updateUrl: function () {
        if ('URLSearchParams' in window) {
            var searchParams = new URLSearchParams(window.location.search);
            searchParams.set("InstallUmbracoTemplate", psw.controls.installUmbracoTemplate.checked);
            searchParams.set("UmbracoTemplateVersion", psw.controls.umbracoTemplateVersion.value);
            searchParams.set("Packages", psw.controls.packages.value);
            searchParams.set("UserEmail", psw.controls.userEmail.value);
            searchParams.set("ProjectName", psw.controls.projectName.value);
            searchParams.set("CreateSolutionFile", psw.controls.createSolutionFile.checked);
            searchParams.set("SolutionName", psw.controls.solutionName.value);
            searchParams.set("UseUnattendedInstall", psw.controls.useUnattendedInstall.checked);
            searchParams.set("DatabaseType", psw.controls.databaseType.value);
            searchParams.set("UserPassword", psw.controls.userPassword.value);
            searchParams.set("UmbracoTemplateVersion", psw.controls.umbracoTemplateVersion.value);
            searchParams.set("UserFriendlyName", psw.controls.userFriendlyName.value);
            searchParams.set("IncludeStarterKit", psw.controls.includeStarterKit.checked);
            searchParams.set("StarterKitPackage", psw.controls.starterKitPackage.value);
            var newRelativePathQuery = window.location.pathname + '?' + searchParams.toString();
            history.pushState(null, '', newRelativePathQuery);
        }
    },
    debounce: function(func, wait, immediate) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var later = function () {
                timeout = null;
                if (!immediate) func.apply(context, args);
            };
            var callNow = immediate && !timeout;
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
            if (callNow) func.apply(context, args);
        };
    }
}; psw.init();





