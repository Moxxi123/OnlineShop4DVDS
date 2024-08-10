var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tableData').DataTable({
        "ajax": { url: '/admin/feedback/getall' },
        "columns": [
            { data: 'name', "width": "10%" },
            { data: 'email', "width": "15%" },
            { data: 'phone', "width": "15%" },
            { data: 'subject', "width": "25%" },
            {
                data: 'isSended',
                "render": function (data) {
                    return data ? '<span class="badge bg-success" style="font-size: 1em">Replied</span>' : '<span class="badge bg-danger" style="font-size: 1em">No Reply</span>';
                },
                "width": "15%"
            },
            {
                data: 'id',
                "render": function (data, type, row) {
                    // Check the isSended status
                    let actionButton = row.isSended ? `<a href="/admin/feedback/detail?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-eye"></i> Detail</a>` : `<a href="/admin/feedback/reply?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Reply</a>`;

                    return `<div class="d-flex justify-content-center w-100">
                        <div class="btn-group" role="group">
                            ${actionButton}
                            <a onClick="Delete('/admin/feedback/delete?id=${data}')" class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i> Delete</a>
                        </div>
                    </div>`;
                },
                "width": "20%"
            }
        ]
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
                        toastr.error('An error occurred while deleting the category');
                    }
                }
            });
        }
    });
}