$(() => {
    $(".deposit").on('click', function () {

        const id = $(this).attr('data-id');
        $("#contributor-id").val(id);

    });


    $(".edit").on('click', function () {
        $("#edit-name").val($(this).attr('data-name'));
        $("#edit-cell").val($(this).attr('data-cell'));
        $("#contributorid").val($(this).attr('data-id'));
        if ($(this).attr('data-alwaysinclude') == "True") {
            $("#edit-alwaysinclude").attr('checked', true);
        } else {
            $("#edit-alwaysinclude").attr('checked', false);
        }

    });
});