$(() => {
    $("#submit-form").on('submit', function () {
        if ($(".already-contributed").attr('checked')) {
            const val = $(this).val;
            $(`#A${val}`).atrr('value', true);
        }
    });
});