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
        connectionString: document.getElementById('ConnectionString'),
        userFriendlyName: document.getElementById('UserFriendlyName'),
        userEmail: document.getElementById('UserEmail'),
        userPassword: document.getElementById('UserPassword'),
        databaseType: document.getElementById('DatabaseType'),
        search: document.getElementById('search'),
        codeBlock: document.querySelector('pre'),
        packageCheckboxes: document.querySelectorAll('#packagelist input[type=checkbox]'),
        packageVersionDropdowns: document.querySelectorAll('#packagelist select'),
        packageCards: document.querySelectorAll('#packagelist .card'),
        codeNavItem: document.getElementById('code-nav-item')
    },
    buttons: {
        clearpackages: document.getElementById('clearpackages'),
        reset: document.getElementById('reset'),
        copy: document.getElementById('copy'),
        generate: document.getElementById('generate'),
        update: document.getElementById('update'),
        save: document.getElementById('save'),
        deletesave: document.getElementById('deletesave')
    },
    init: function () {
        psw.addListeners();
        psw.setFromLocalStorage();
    },
    addListeners() {
        psw.buttons.save.addEventListener('click', function (event) {
            event.preventDefault();
            window.localStorage.setItem("searchParams", window.location.search);
        });

        psw.buttons.deletesave.addEventListener('click', function (event) {
            event.preventDefault();
            window.localStorage.removeItem("searchParams");
        });

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
                psw.togglePackage(this.id);
                psw.loadDropdown(this.id);
                psw.updatePackages();
                psw.updateOutput();
                psw.updateUrl();
            });
        });

        psw.controls.packageVersionDropdowns.forEach(function (dropdown) {
            dropdown.addEventListener('change', function () {
                psw.updatePackages();
                psw.updateOutput();
                psw.updateUrl();
            });
        });

        psw.controls.packageCards.forEach(function (card) {
            card.addEventListener('click', function (event) {
                if (event.target.nodeName === 'A' || event.target.nodeName == 'INPUT'
                    || event.target.nodeName == 'SELECT' || event.target.nodeName == 'LABEL') return;
                var checkbox = this.querySelector('input[type="checkbox"]');
                checkbox.checked = !checkbox.checked;
                psw.togglePackage(checkbox.id);
                psw.loadDropdown(checkbox.id);
                psw.updatePackages();
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

        psw.controls.connectionString.addEventListener('keyup', psw.debounce(function () {
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
            psw.toggleConnectionString();
            psw.updateOutput();
            psw.updateUrl();
        });
    },
    updatePackages: function () {
        var packageListCheckboxes = document.querySelectorAll('#packagelist input[type="checkbox"]:checked');
        var packageListCheckboxesArr = Array.from(packageListCheckboxes);

        var allValues = [];

        if (packageListCheckboxesArr != null && packageListCheckboxesArr.length > 0) {
            packageListCheckboxesArr.forEach(function (checkbox) {
                var packageId = checkbox.id.split('_')[1];
                var dropdown = document.getElementById('PackageVersion_' + packageId);
                var dropdownVal = dropdown.value;
                var thisVal = checkbox.value + '|' + dropdownVal;
                allValues.push(thisVal);
            });
        }

        var packagesValue = allValues.join();

        psw.controls.packages.value = packagesValue;
    },
    togglePackage: function (checkboxId) {
        var thisCheckbox = document.getElementById(checkboxId);
        var packageIdForDropdown = checkboxId.split('_')[1];
        var thisDropdown = document.getElementById('PackageVersion_' + packageIdForDropdown);
        var card = thisCheckbox.closest('.card');

        if (thisCheckbox.checked) {
            card.classList.add('selected');
            thisDropdown.removeAttribute('disabled');
        }
        else {
            card.classList.remove('selected');
            thisDropdown.setAttribute('disabled', 'disabled');
        }
    },
    loadDropdown: function (checkboxId) {
        var checkbox = document.getElementById(checkboxId);
        var nugetPackageId = checkbox.value;
        var packageIdForDropdown = checkboxId.split('_')[1];
        var thisDropdown = document.getElementById('PackageVersion_' + packageIdForDropdown);

        if (thisDropdown.options.length > 2) return;

        var data = {
            "PackageId": nugetPackageId.toLowerCase()
        }

        var url = "/api/scriptgeneratorapi/getpackageversions";

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
            console.log(data);
            

            var packageVersions = JSON.parse(data);

            var optionPosition;
            var lastOptionPosition = thisDropdown.options.length - 1;
            for (optionPosition = lastOptionPosition; optionPosition > 1; optionPosition--) {
                thisDropdown.remove(optionPosition);
            }

            var versionCount = packageVersions.length;
            for (i = 0; i < versionCount; i++) {
                var version = packageVersions[i];
                var opt = document.createElement("option");
                opt.value = version;
                opt.innerHTML = version;

                thisDropdown.appendChild(opt);
            }

        }).catch((error) => {
            console.log(error);
        });
    },
    deselectPackage: function (element) {
        var packageIdForDropdown = element.id.split('_')[1];
        var thisDropdown = document.getElementById('PackageVersion_' + packageIdForDropdown);
        thisDropdown.value = '';
        thisDropdown.setAttribute('disabled', 'disabled');
        element.checked = false;
        element.val
        var card = element.closest('.card');
        card.classList.remove("selected");
    },
    clearAllPackages: function (event) {
        event.preventDefault();
        var packageListCheckboxes = document.querySelectorAll('#packagelist input[type="checkbox"]:checked');
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
        psw.controls.connectionString.value = 'server=.\SQLEXPRESS;database=myDatabase;user id=myUser;password="myPassword"';
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
    setFromLocalStorage: function () {
        if (window.localStorage.getItem("searchParams") == null) {
            return reset();
        }

        var searchParams = new URLSearchParams(window.localStorage.getItem("searchParams"));

        psw.controls.installUmbracoTemplate.checked = searchParams.get("InstallUmbracoTemplate") === "true";
        psw.controls.umbracoTemplateVersion.value = searchParams.get("UmbracoTemplateVersion");
        psw.controls.includeStarterKit.checked = searchParams.get("IncludeStarterKit") === "true";
        psw.controls.starterKitPackage.value = searchParams.get("StarterKitPackage");
        psw.controls.createSolutionFile.checked = searchParams.get("CreateSolutionFile") === "true";
        psw.controls.solutionName.value = searchParams.get("SolutionName");
        psw.controls.projectName.value = searchParams.get("ProjectName");
        psw.controls.useUnattendedInstall.checked = searchParams.get("UseUnattendedInstall") === "true";
        psw.controls.connectionString.value = searchParams.get("ConnectionString");
        psw.controls.userFriendlyName.value = searchParams.get("UserFriendlyName");
        psw.controls.userEmail.value = searchParams.get("UserEmail");
        psw.controls.userPassword.value = searchParams.get("UserPassword");
        psw.controls.databaseType.value = searchParams.get("DatabaseType");

        psw.controls.umbracoTemplateVersion.disabled = !psw.controls.installUmbracoTemplate.checked;
        psw.controls.starterKitPackage.disabled = !psw.controls.includeStarterKit.checked;
        psw.controls.solutionName.disabled = !psw.controls.createSolutionFile.checked;
        psw.controls.databaseType.disabled = !psw.controls.useUnattendedInstall.checked;
        psw.controls.userFriendlyName.disabled = !psw.controls.useUnattendedInstall.checked;
        psw.controls.userEmail.disabled = !psw.controls.useUnattendedInstall.checked;
        psw.controls.userPassword.disabled = !psw.controls.useUnattendedInstall.checked;

        psw.updateOutput();
        psw.updateUrl();
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
            psw.controls.userPassword.removeAttribute('disabled');
            psw.controls.connectionString.removeAttribute('disabled');
        }
        else {
            psw.controls.databaseType.setAttribute('disabled', 'disabled');
            psw.controls.userFriendlyName.setAttribute('disabled', 'disabled');
            psw.controls.userEmail.setAttribute('disabled', 'disabled');
            psw.controls.userPassword.setAttribute('disabled', 'disabled');
            psw.controls.connectionString.setAttribute('disabled', 'disabled');
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
    toggleConnectionString: function () {
        var dbType = psw.controls.databaseType.value;
        if (dbType === 'SQLServer' || dbType === 'SQLAzure') {
            psw.controls.connectionString.parentNode.classList.add('d-block');
            psw.controls.connectionString.parentNode.classList.remove('d-none');
        }
        else {
            psw.controls.connectionString.parentNode.classList.add('d-none');
            psw.controls.connectionString.parentNode.classList.remove('d-block');
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
            "ConnectionString": psw.controls.connectionString.value,
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
    debounce: function (func, wait, immediate) {
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





