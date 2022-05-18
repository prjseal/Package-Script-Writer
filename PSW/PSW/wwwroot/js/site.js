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
        packageCheckboxes: document.querySelectorAll('#packagelist input[type=checkbox]')
    },
    buttons: {
        clearpackages: document.getElementById('clearpackages'),
        reset: document.getElementById('reset'),
        copy: document.getElementById('copy')
    },
    init: function () {
        psw.addListeners();
    },
    addListeners() {
        psw.buttons.clearpackages.addEventListener('click', function (event) {
            psw.clearAllPackages(event)
        });

        psw.buttons.reset.addEventListener('click', function (event) {
            psw.reset(event)
        });

        psw.controls.search.addEventListener('keyup', psw.filterPackages);

        psw.controls.useUnattendedInstall.addEventListener('change', psw.toggleUnattendedInstallControls);

        psw.controls.installUmbracoTemplate.addEventListener('change', psw.toggleInstallUmbracoTemplateControls);

        psw.controls.includeStarterKit.addEventListener('change', psw.toggleIncludeStarterKitControls);

        psw.controls.createSolutionFile.addEventListener('change', psw.toggleCreateSolutionFileControls);

        psw.buttons.copy.addEventListener('click', function (event) {
            psw.copyCodeBlock(event);
        })

        psw.controls.packageCheckboxes.forEach(function (checkbox) {
            checkbox.addEventListener('change', function () {
                psw.updatePackages(this.id);
            });
        });

        var tab = document.querySelector('#myTab a');
        tab.addEventListener('click', function (event) {
            event.preventDefault()
            $(this).tab('show')
        });

        psw.controls.search.addEventListener('keypress', function () {
            if (event.key === "Enter") {
                event.preventDefault();
            }
        });
    },
    updatePackages: function (checkboxId) {
        var checkbox = document.getElementById(checkboxId);
        var thisVal = checkbox.value;
        var allVals = psw.controls.packages.value;

        var card = checkbox.closest('.card');

        if (checkbox.checked) {
            card.classList.add('selected');
        }
        else {
            card.classList.remove('selected');
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
        psw.controls.packages.value = '';
        psw.controls.installUmbracoTemplate.checked = true;
        psw.controls.umbracoTemplateVersion.value = '';
        psw.controls.includeStarterKit.checked = true;
        psw.controls.starterKitPackage.value = 'Umbraco.TheStarterKit';
        psw.controls.createSolutionFile.checked = true;
        psw.controls.solutionName.value = 'MySolution';
        psw.controls.projectName.value = 'MyProject';
        psw.controls.useUnattendedInstall.checked = true;
        psw.controls.userFriendlyName.value = 'Administrator';
        psw.controls.userEmail.value = 'admin@example.com';
        psw.controls.userPassword.value = '1234567890';
        psw.controls.databaseType.value = 'LocalDb';

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
    }
}; psw.init();





