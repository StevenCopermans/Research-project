$(function () {
    $.ajax(
        {
            url: "https://localhost:44329/api/rest/GetTables",
            crossDomain: true,
        }).then(function (result) {
            const data = JSON.parse(result);
            HandleTables(data);
    });
});

const HandleTables = (tables) => {
    const tableList = document.querySelector(".js-tables");

    tables.forEach(table => {
        tableList.innerHTML += `
                                <li class="nav-item w-100" style="float: none;">
                                    <a href="#" class="nav-link link-dark">
                                        ${table['TABLE_SCHEMA']}.${table['TABLE_NAME']}
                                    </a>
                                </li>`;
        console.log(table);
    });
};