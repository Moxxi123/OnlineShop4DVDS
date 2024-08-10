var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tableData').DataTable({
        "ajax": { url: '/admin/promotion/getall' },
        "columns": [
            { data: 'description', "width": "20%" },
            {
                data: 'discountPercent',
                "render": function (data, type, row) {
                    return data + '%';
                },
                "width": "10%"
            },
            {
                data: 'startDate',
                "render": function (data, type, row) {
                    return moment(data).format('YYYY-MM-DD HH:mm:ss'); // Format the date as needed
                },
                "width": "15%"
            },
            {
                data: 'endDate',
                "render": function (data, type, row) {
                    return moment(data).format('YYYY-MM-DD HH:mm:ss'); // Format the date as needed
                },
                "width": "15%"
            },
            {
                data: 'status',
                "render": function (data, type, row) {
                    let statusButton = '';
                    if (data === 'Active') {
                        statusButton = `<a onClick="changeStatus('/admin/promotion/changestatus?id=${row.id}')" class="btn btn-primary mx-2">
                                            <i class="bi bi-lock"></i> Deactivate
                                        </a>`;
                    } else if (data === 'Deactive') {
                        statusButton = `<a onClick="changeStatus('/admin/promotion/changestatus?id=${row.id}')" class="btn btn-primary mx-2">
                                            <i class="bi bi-unlock"></i> Activate
                                        </a>`;
                    } else {
                        statusButton = `<span class="text-muted mx-2">Expired</span>`;
                    }

                    return `<div class="d-flex justify-content-center w-100">
                                <div class="btn-group" role="group">
                                    ${statusButton}
                                </div>
                            </div>`;
                },
                "width": "20%"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="d-flex justify-content-center w-100">
                                <div class="btn-group" role="group">
                                    <a href="/admin/promotion/edit?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                                    <a onClick="Delete('/admin/promotion/delete?id=${data}')" class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i> Delete</a>
                                </div>
                            </div>`;
                },
                "width": "20%"
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

function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
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
                        toastr.error('An error occurred while deleting the promotion');
                    }
                }
            });
        }
    });
}


