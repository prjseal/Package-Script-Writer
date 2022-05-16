var psw = psw || {
    init: function () {
        psw.addListeners();
    },
    addListeners() {
        $('#myTab a').on('click', function (event) {
            event.preventDefault()
            $(this).tab('show')
        });

        $('#clearpackages').on('click', function (event) {
            event.preventDefault()
            $('#packagelist input[type="checkbox"]').each(function () {
                var thisId = this.id;
                $(this).prop("checked", false);
                var card = $('#' + thisId).closest('.card');
                $(card).removeClass('selected');
                $('#Packages').val('');
            })
        });

        $('#reset').on('click', function (event) {
            event.preventDefault()
            $('#InstallUmbracoTemplate').prop("checked", true);
            $('#UmbracoTemplateVersion').val('');
            $('#IncludeStarterKit').prop("checked", true);
            $('#StarterKitPackage').val('Umbraco.TheStarterKit');
            $('#CreateSolutionFile').prop("checked", true);
            $('#SolutionName').val('MySolution');
            $('#ProjectName').val('MyProject');
            $('#UseUnattendedInstall').prop("checked", true);
            $('#UserFriendlyName').val('Administrator');
            $('#UserEmail').val('admin@example.com');
            $('#UserPassword').val('1234567890');
            $("#DatabaseType").val("LocalDb");
            $('#DatabaseType').removeAttr('disabled');
            $('#UserFriendlyName').removeAttr('disabled');
            $('#UserEmail').removeAttr('disabled');
            $('#UserPassword').removeAttr('disabled');
            $('#StarterKitPackage').removeAttr('disabled');
            $('#SolutionName').removeAttr('disabled');
        });

        $('#search').on('keyup', function () {
            var input, filter, ul, li, a, i, txtValue;
            input = document.getElementById('search');
            filter = input.value.toUpperCase();
            ul = document.getElementById("packagelist");
            li = ul.getElementsByClassName("packageItem");

            // Loop through all list items, and hide those who don't match the search query
            for (i = 0; i < li.length; i++) {
                txtValue = li[i].getAttribute("data-packageid");
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    li[i].style.display = "";
                } else {
                    li[i].style.display = "none";
                }
            }
        });

        $('#UseUnattendedInstall').on('click', function (event) {
            var thisId = this.id;
            var isChecked = $('#' + thisId).prop("checked");
            if (isChecked) {
                $('#DatabaseType').removeAttr('disabled');
                $('#UserFriendlyName').removeAttr('disabled');
                $('#UserEmail').removeAttr('disabled');
                $('#UserPassword').removeAttr('disabled');
            }
            else {
                $('#DatabaseType').attr('disabled', 'disabled');
                $('#UserFriendlyName').attr('disabled', 'disabled');
                $('#UserEmail').attr('disabled', 'disabled');
                $('#UserPassword').attr('disabled', 'disabled');
            }
        });

        $('#InstallUmbracoTemplate').on('click', function (event) {
            var thisId = this.id;
            var isChecked = $('#' + thisId).prop("checked");
            if (isChecked) {
                $('#UmbracoTemplateVersion').removeAttr('disabled');
            }
            else {
                $('#UmbracoTemplateVersion').attr('disabled', 'disabled');
            }
        });

        $('#IncludeStarterKit').on('click', function (event) {
            var thisId = this.id;
            var isChecked = $('#' + thisId).prop("checked");
            if (isChecked) {
                $('#StarterKitPackage').removeAttr('disabled');
            }
            else {
                $('#StarterKitPackage').attr('disabled', 'disabled');
            }
        });

        $('#CreateSolutionFile').on('click', function (event) {
            var thisId = this.id;
            var isChecked = $('#' + thisId).prop("checked");
            if (isChecked) {
                $('#SolutionName').removeAttr('disabled');
            }
            else {
                $('#SolutionName').attr('disabled', 'disabled');
            }
        });

        $('#copy').on('click', function (event) {
            event.preventDefault();
            var pre = $('pre')[0];
            navigator.clipboard.writeText(pre.innerText);
        });

        var $checkboxes = $("#packagelist input[type=checkbox]");

        $checkboxes.on('change', function () {
            psw.updatePackages(this.id);
        });
    },
    updatePackages: function (thisId) {
        console.log(thisId);
        var thisVal = $('#' + thisId).val();
        var allVals = $('#Packages').val();

        var isChecked = $('#' + thisId).prop("checked");
        var card = $('#' + thisId).closest('.card');

        $(card).toggleClass('selected', isChecked);

        if (!isChecked && allVals != '') {
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
        $('#Packages').val(allVals);
    }
}; psw.init();





