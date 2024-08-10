var dataTable;
$(document).ready(function () {
    loadDataTable();
    var successMessage = $('#successMessage').val();
    if (successMessage) {
        Swal.fire({
            title: 'Success!',
            text: successMessage,
            icon: 'success',
            confirmButtonText: 'OK'
        });
    }
});

function loadDataTable() {
    dataTable = $('#tableData').DataTable({
        "ajax": { url: '/admin/customer/getall' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "25%" },
            {
                data: 'lockOutEnd',
                render: function (data, type, row) {
                    var lockoutEnd = data ? new Date(data) : null;
                    return `
                        <div class="d-flex justify-content-center w-100">
                            <div class="btn-group" role="group">
                                <a onClick="changeStatus('/admin/customer/changestatus?userId=${row.id}')" class="btn btn-primary mx-2">
                                    ${(!lockoutEnd || lockoutEnd < new Date()) ? '<i class="bi bi-lock"></i> Lock' : '<i class="bi bi-unlock"></i> Unlock'}
                                </a>
                            </div>
                        </div>`;
                },
                width: "20%"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `
                        <div class="d-flex justify-content-center w-100">
                            <div class="btn-group" role="group">
                                <a href="/admin/customer/edit?userId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                            </div>
                        </div>`;
                },
                "width": "30%"
            }
        ]
    });
}

function changeStatus(url) {
    $.ajax({
        url: url,
        type: 'PUT',
        success: function (data) {
            if (data.success) {
                dataTable.ajax.reload();
                toastr.success(data.message);
            } else {
                toastr.error(data.message);
            }
        },
        error: function (xhr) {
            if (xhr.status === 403) {
                window.location.href = '/Admin/User/AccessDenied';
            } else {
                toastr.error('An error occurred while changing the status');
            }
        }
    });
}