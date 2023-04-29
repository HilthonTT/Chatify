$('#myModal').on('shown.bs.modal', function () {
    $('#myInput').trigger('focus');
})

window.closeModal = function (modalId) {
    $('#' + modalId).modal('hide');
}