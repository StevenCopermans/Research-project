import React, { Component, useState, useEffect } from 'react';
import NavMenu from './NavMenu';
import Flex from './Flex';
import Toolbar from './Toolbar';
import DataTable from './DataTable';
import FormOverlay from './FormOverlay';
import ErrorOverlay from './ErrorOverlay';
import './Home.css';

import { Tab, Tabs, TabList, TabPanel } from 'react-tabs';
import 'react-tabs/style/react-tabs.css';

function Home() {

    const [tables, setTables] = useState(['']);
    const [selectedTable, setSelectedTable] = useState(undefined);
    const [selectedTab, setSelectedTab] = useState(0);
    const [showAddEntry, setShowAddEntry] = useState(false);
    const [showEditEntry, setShowEditEntry] = useState(false);
    const [editItem, setEditItem] = useState(undefined);

    const [showErrorOverlay, setShowErrorOverlay] = useState(false);
    const [warnings, setWarnings] = useState([]);
    const [errors, setErrors] = useState([]);

    const setTable = (tableName) => {
        setSelectedTable(tableName);
    }

    const addDataToTable = (data) => {
        const tempData = { ...tableData };

        tempData[selectedTable].push(data);

        setTableData(tempData);
    }

    const deleteFromTable = async (index) => {
        const primaryKey = tableStructure[selectedTable].filter((x) => { return x["CONSTRAINT_TYPE"] == "PRIMARY KEY"; })[0]["COLUMN_NAME"];

        if (tableData[selectedTable][index]) {
            const indexKey = tableData[selectedTable][index][primaryKey];

            let data;

            await fetch("https://localhost:44358/api/Test/" + selectedTable + "/" + indexKey, {
                method: "delete",
                headers: { "Content-Type": "application/json" }
            })
                .then(resp => {
                    if (resp.status === 204 || resp.status === 404) {
                        console.log("Item deleted succesfully");

                        const tempData = { ...tableData };

                        tempData[selectedTable].splice(index, 1);

                        setTableData(tempData);

                        return {};
                    } else if (resp.status === 400) {
                        return resp.json()
                    }
                    else {
                        console.log("Status: " + resp.status)
                        return Promise.reject("server")
                    }
                })
                .then(dataJson => {
                    data = dataJson;
                    if (dataJson["errors"]) {
                        setErrors(dataJson["errors"]);
                    }

                    if (dataJson["warnings"]) {
                        setWarnings(dataJson["warnings"]);
                    }

                    if (dataJson["warnings"] || dataJson["errors"]) {
                        setShowErrorOverlay(true);
                    }
                });

            if (data["warnings"] || data["errors"]) {
                return false;
            }
            return true;
        }
           
        return false;
    }

    const editTableEntry = (index) => {
        setShowEditEntry(true);
        const primaryKey = tableStructure[selectedTable].filter((x) => { return x["CONSTRAINT_TYPE"] == "PRIMARY KEY"; })[0]["COLUMN_NAME"];

        if (tableData[selectedTable][index]) {
            setEditItem({ key: tableData[selectedTable][index][primaryKey], data: tableData[selectedTable][index] });
        }
    }

    const editTableData = (data) => {
        const tempData = { ...tableData };

        console.log(data);
        //tempData[selectedTable].push(data);

        setTableData(tempData);
    }

    async function getTables() {
        const response = await fetch('https://localhost:44358/api/Test');
        const tableData = await response.json();

        tableData.shift();
        setTables(tableData);
    }

    const [tableData, setTableData] = useState([]);
    const [tableStructure, setTableStructure] = useState([]);

    async function getTableData() {
        if (selectedTable && tableData[selectedTable] == undefined) {
            const tempTableData = { ...tableData };

            const response = await fetch('https://localhost:44358/api/Test/' + selectedTable);
            const tableDataJSON = await response.json();

            tempTableData[selectedTable] = tableDataJSON["data"];
            console.log(tempTableData);

            setTableData(tempTableData);
        }
    }

    async function getTableStructure() {
        if (selectedTable && tableStructure[selectedTable] == undefined) {
            const tempStructData = { ...tableStructure };

            const response = await fetch('https://localhost:44358/api/Test/' + selectedTable + "/Structure");
            const tableStructureJSON = await response.json();

            tempStructData[selectedTable] = tableStructureJSON["data"];
            console.log(tempStructData);

            setTableStructure(tempStructData);
        }
    }

    useEffect(() => {
        getTables();
    }, []);

    useEffect(() => {
        switchDataTable(selectedTab)
    }, [selectedTable]);

    async function switchDataTable(tabIndex) {
        setSelectedTab(tabIndex);
        switch (tabIndex) {
            case 0:
                await getTableStructure();
                await getTableData();
                break;
            case 1:
                await getTableStructure();
                break;
        }
    }

    return (
        <div className={showAddEntry || showEditEntry || showErrorOverlay ? "c-modal" : "c-normal"}>
            <Flex container
                justifyContent="space-between"
                width="100%" height="100%">

                <Toolbar>
                    <h1>MCT CMS</h1>

                    <h3>Tables</h3>
                    <ul className="c-tableNameList">
                        {tables.map((table) => (
                            <li className="c-tableName" onClick={() => setTable(table['TABLE_NAME'])}>{table['TABLE_NAME']}</li>
                        ))}

                    </ul>
                </Toolbar>

                <div className="c-Right">
                    <NavMenu />

                    {
                        selectedTable ?
                            <div className="c-content">
                                <h1>{selectedTable}</h1>
                                <Tabs defaultIndex={0} onSelect={index => switchDataTable(index)}>
                                    <TabList>
                                        <Tab>Table Data</Tab>
                                        <Tab>Table Structure</Tab>
                                    </TabList>
                                    <TabPanel>
                                        <button onClick={() => setShowAddEntry(true)}>
                                            Add new entry
                                        </button>
                                        <DataTable key={tableData[selectedTable] ? tableData[selectedTable].length : 1} onEdit={editTableEntry} onDelete={deleteFromTable} dataTable={tableData[selectedTable]} dataHeader={tableStructure[selectedTable]} hasActions={true} />

                                        {
                                            showAddEntry && <FormOverlay action="create" table={selectedTable} api={"https://localhost:44358/api/Test/" + selectedTable} structure={tableStructure[selectedTable]} onSucces={(data) => { addDataToTable(data); }} onClose={() => { setShowAddEntry(false) }} />
                                        }

                                        {
                                            showEditEntry && <FormOverlay action="edit" item={editItem} table={selectedTable} api={"https://localhost:44358/api/Test/" + selectedTable + "/" + editItem["key"]} structure={tableStructure[selectedTable]} onSucces={(data) => { editTableData(data); }} onClose={() => { setShowEditEntry(false) }} />
                                        }

                                    </TabPanel>
                                    <TabPanel>
                                        <DataTable dataTable={tableStructure[selectedTable]} dataHeader={[{ COLUMN_NAME: "COLUMN_NAME" }, { COLUMN_NAME: "DATA_TYPE" }, { COLUMN_NAME: "IS_NULLABLE" }, { COLUMN_NAME: "CONSTRAINT_TYPE" }]} />
                                    </TabPanel>
                                </Tabs>

                                {showErrorOverlay && <ErrorOverlay onClose={() => { setShowErrorOverlay(false) }} table={selectedTable} warnings={warnings} errors={errors} />}
                            </div>
                            : <div className="c-content">
                                Please select a table on the left
                            </div>
                    }
                </div>

            </Flex>
        </div>
    );
}

export default Home;