function pollBalance() {

    var id = $("#Id").val();
    var currentBalance = parseFloat($("#balance").html());

    $.ajax({
        type: "GET",
        url: "/bankaccount/" + id,
        headers: {
            "Accept": "application/json; charset=utf-8",
            "Content-Type": "application/json; charset=utf-8"
        },
        success: function (data) {
            if (currentBalance !== data.Balance) {
                $("#balance").html(data.Balance);
            }
            setTimeout(pollBalance, 5000);
        }
    });
}

pollBalance();