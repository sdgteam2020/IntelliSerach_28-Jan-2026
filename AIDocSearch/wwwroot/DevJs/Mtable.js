async function mMsater(sectid = '', ddl, TableId, ParentId) {

    const userdata = new URLSearchParams({
        id: TableId,
        ParentId: ParentId
    });

    try {
        const response = await fetch('/Master/GetAllMMaster', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: userdata
        });

        const data = await response.json();

        if (data != "null") {
            if (data.Code == 500) {
                Swal.fire({
                    text: errormsg
                });
            } else if (data.Code == 200) {


                let listItemddl = "";


                listItemddl += '<option value="0">Please Select</option>';

                for (let i = 0; i < data.Data.length; i++) {
                    listItemddl += `<option value="${data.Data[i].Id}">${data.Data[i].Name}</option>`;
                }

                document.getElementById(ddl).innerHTML = listItemddl;

                if (sectid !== '') {
                    document.getElementById(ddl).value = sectid;
                }
            }
        } else {
            // No data found case (optional alert as in original)
            // Swal.fire({
            //     text: "No data found Offrs"
            // });
        }

    } catch (error) {
        Swal.fire({
            text: errormsg002
        });
    }
}