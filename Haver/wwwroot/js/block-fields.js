//Block fields according to bool decisions
$(document).ready(function () {

    toggleReadOnlyState();

    $("#drawReqUpdatingCheckbox").change(function () {
        toggleReadOnlyState();
    });


    function toggleReadOnlyState() {
        var checkbox = $("#drawReqUpdatingCheckbox");
        var drawingFields = $(".drawing-field");


        drawingFields.prop("readonly", !checkbox.prop("checked"));
    }
});

$(document).ready(function () {
    toggleReadOnlyState();

    $("#custNotificationCheckbox").change(function () {
        toggleReadOnlyState();
    });

    function toggleReadOnlyState() {
        var checkbox = $("#custNotificationCheckbox");
        var textareaField = $("#custIssueMsgField");

        textareaField.prop("readonly", !checkbox.prop("checked"));
    }
});

$(document).ready(function () {
    toggleReadOnlyState();

    $("#carRaisedCheckbox").change(function () {
        toggleReadOnlyState();
    });

    function toggleReadOnlyState() {
        var checkbox = $("#carRaisedCheckbox");
        var carNumField = $("#carNumField");

        carNumField.prop("readonly", !checkbox.prop("checked"));
    }
});


$(document).ready(function () {
    toggleReadOnlyState();

    $("#followUpReqCheckbox").change(function () {
        toggleReadOnlyState();
    });

    function toggleReadOnlyState() {
        var checkbox = $("#followUpReqCheckbox");
        var followUpTypeField = $("#followUpTypeField, #followUpTypeFieldTwo");

        followUpTypeField.prop("readonly", !checkbox.prop("checked"));
    }
});


$(document).ready(function () {
    toggleReadOnlyState();

    $("#suppItemsBackCheckbox").change(function () {
        toggleReadOnlyState();
    });

    function toggleReadOnlyState() {
        var checkbox = $("#suppItemsBackCheckbox");
        var carrierInfoField = $("#carrierInfoField");
        var RMANoField = $("#RMANoField")

        carrierInfoField.prop("readonly", !checkbox.prop("checked"));
        RMANoField.prop("readonly", !checkbox.prop("checked"));
    }
});