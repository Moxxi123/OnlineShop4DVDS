$(document).ready(function () {
    loadDataTable();

    $('#productTypeFilter li').click(function () {
        var productType = $(this).attr('data-type');
        $('#productTypeFilter li').removeClass('active text-white bg-primary');
        $(this).addClass('active text-white bg-primary');
        loadDataTable(productType);
    });
});

function loadDataTable(productType = '') {
    if ($.fn.DataTable.isDataTable('#tableData')) {
        $('#tableData').DataTable().destroy();
    }

    dataTable = $('#tableData').DataTable({
        "ajax": {
            url: '/admin/order/getall',
            type: 'GET',
            data: { productType: productType },
            dataSrc: function (json) {
                console.log("Data from API:", json.data); // Log the data received from the API
                return json.data;
            }
        },
        "columns": [
            { data: 'orderItem.name', "width": "15%" },
            { data: 'orderItem.orderDate', "width": "10%" },
            { data: 'orderItem.paymentMethod', "width": "10%" },
            { data: 'orderItem.paymentStatus', "width": "15%" },
            { data: 'orderItem.orderStatus', "width": "10%" },
            {
                data: 'orderDetailId',
                "render": function (data, type, row) {
                    console.log("Order Status:", row.orderItem.orderStatus); // Log the order status
                    var editButton = `<a href="/admin/order/edit?id=${row.orderItemId}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>`;
                    if (row.orderItem.orderStatus === "Cancelled") {
                        editButton = ``;
                    }
                    return `<div class="d-flex justify-content-center w-100">
                        <div class="btn-group" role="group">
                            <a href="/admin/order/details?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-eye"></i> Detail</a>
                            ${editButton}
                        </div>
                    </div>`;
                },
                "width": "20%"
            }
        ]
    });
}


