var dataTable;

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
            url: '/admin/recommendslider/getallProduct',
            data: { productType: productType }
        },
        "columns": [
            { data: 'name', "width": "15%" },
            {
                data: 'imageUrl',
                "render": function (data) {
                    return `
                            <div class="d-flex justify-content-center align-items-center w-100">
                                <img src="/${data}" class="img-fluid" style="max-width: 100%; height: auto;" alt="Main Image" />
                            </div>`;
                },
                "width": "15%"
            },
            {
                data: 'price',
                render: function (data) {
                    return data + '$';
                },
                "width": "10%"
            },
            {
                data: 'promotion.discountPercent',
                render: function (data) {
                    if (data === null || data == 0) {
                        return '';
                    } else {
                        return data + '%';
                    }
                },
                width: "5%"
            },
            {
                data: 'promotionPrice',
                render: function (data) {
                    if (data === null) {
                        return '';
                    } else {
                        return data + '$';
                    }
                },
                "width": "11%"
            },
            { data: 'contentType.type', "width": "10%" },
            { data: 'productType', "width": "10%" },
            {
                data: 'id',
                "render": function (data, type, row) {
                    var buttonLabel = row.isInSlider ? 'Remove from Slider' : 'Add to Slider';
                    var buttonClass = row.isInSlider ? 'btn-danger' : 'btn-primary';
                    return `<div class="d-flex justify-content-center w-100">
        <div class="btn-group" role="group">
            <button onClick="addOrRemoveProductFromSlider(${row.productId}, '${row.productType}')" class="btn ${buttonClass} mx-2">
                <i class="bi bi-pencil-square"></i> ${buttonLabel}
            </button>
        </div>
    </div>`;
                },
                "width": "20%"
            }
        ]
    });
}


function addOrRemoveProductFromSlider(productId, productType) {
    $.ajax({
        url: '/admin/recommendslider/addorremoveproductfromslider',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            productId: productId,
            productType: productType
        }),
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                loadDataTable();
            } else {
                toastr.error('Failed to add or remove product from slider: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            toastr.error('Error: ' + xhr.responseText);
        }
    });
}


