var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tableData').DataTable({
        "ajax": { url: '/admin/news/getall' },
        "columns": [
            { data: 'title', "width": "25%" },
            {
                data: 'imageUrl',
                "render": function (data) {
                    return `
                            <div class="d-flex justify-content-center align-items-center w-100">
                                <img src="/${data}" class="img-fluid" style="max-width: 100%; height: auto;" alt="Main Image" />
                            </div>`;
                },
                "width": "25%"
            },
            { data: 'publishDate', "width": "10%" },
            { data: 'summary', "width": "20%" },
            { data: 'newsCategory', "width": "5%" },
            {
                data: 'status',
                "render": function (data, type, row) {
                    return `<div class="d-flex justify-content-center w-100">
                        <div class="btn-group" role="group">
                            <a onClick="changeStatus('/admin/news/changestatus?id=${row.id}')" class="btn btn-primary mx-2">
                                ${data ? '<i class="bi bi-unlock"></i> Unlock' : '<i class="bi bi-lock"></i> Lock'}
                            </a>
                        </div>
                    </div>`;
                },
                "width": "10%"
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="d-flex justify-content-center w-100">
                        <div class="btn-group" role="group">
                            <a href="/admin/news/edit?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                            <a onClick="Delete('/admin/news/delete?id=${data}')" class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i> Delete</a>
                        </div>
                    </div>`;
                },
                "width": "5%"
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
                        toastr.error('An error occurred while deleting the news');
                    }
                }
            });
        }
    });
}


