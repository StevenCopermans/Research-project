import React, { useState } from 'react';
import './Home.css';

const DataTable = (props) => {

    const [tableData, setTableData] = useState(props.dataTable);

    const onEdit = (index) => {
        if (props.onEdit)
            props.onEdit(index);
    }

    const onDelete = async (index) => {
        if (props.onDelete) {
            props.onDelete(index);
        }
    }

    return (
        <div className={props.className || ""}>
            <table className='table table-striped' aria-labelledby="tabelLabel" >
                <thead>

                    {
                        props.dataHeader ?
                            <tr>
                                {props.dataHeader.map((structure) => (
                                    <th key={structure['COLUMN_NAME']}>{structure['COLUMN_NAME']}</th>
                                ))}
                                {
                                    props.hasActions && <th>Actions</th>
                                }
                            </tr> : "No table header found"
                    }

                </thead>
                <tbody>
                    {
                        tableData && props.dataHeader ?
                            tableData.map((data, index) =>
                                <tr key={index}>
                                    {props.dataHeader.map((structure) => (
                                        <td key={structure['COLUMN_NAME']}>{typeof data[structure['COLUMN_NAME']] != typeof {} ? data[structure['COLUMN_NAME']] : "NULL"}</td>
                                    ))}
                                    {
                                        props.hasActions &&
                                        <td>
                                            <button onClick={() => {
                                                onEdit(index);
                                            }}>
                                                Edit entry
                                            </button>
                                            <button onClick={() => {
                                                onDelete(index);
                                            }}>
                                                Delete entry
                                            </button>
                                        </td>
                                    }
                                </tr>
                            ) : <tr><td>Table does not contain anything yet</td></tr>
                    }
                </tbody>
            </table>
        </div>
    );
}

DataTable.defaultProps = {
    dataTable: undefined,
    dataHeader: undefined,
    hasActions: false,
    onEdit: undefined,
    onDelete: undefined
}

export default DataTable;