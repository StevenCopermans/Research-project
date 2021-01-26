import React, { useState } from 'react';
import './FormOverlay.css';

const FormOverlay = (props) => {
    const onClose = () => {
        if (props.onClose != undefined) {
            props.onClose();
        }
    }

    const [errors, setErrors] = useState([]);
    const [inputs, setInputs] = useState(props.item.data || {});

    const onSubmit = async () => {
        const dataToSend = JSON.stringify(inputs);

        if (props.action.toLowerCase() == "create") {
            await fetch(props.api, {
                method: "post",
                headers: { "Content-Type": "application/json" },
                body: dataToSend
            })
                .then(resp => {
                    if (resp.status === 201) {
                        console.log("Item created succesfully");

                        return resp.json();
                    } else if (resp.status === 400) {
                        return resp.json()
                    }
                    else {
                        console.log("Status: " + resp.status)
                        return Promise.reject("server")
                    }
                })
                .then(dataJson => {
                    console.log(dataJson);

                    if (dataJson["errors"] == undefined || dataJson["errors"].length == 0) {
                        if (props.onClose != undefined)
                            if (props.onSucces != undefined)
                                props.onSucces(dataJson["data"][0]);
                        props.onClose();
                    }

                    if (dataJson["errors"])
                        setErrors(dataJson["errors"]);
                })
        } else if (props.action.toLowerCase() == "edit") {
            await fetch(props.api, {
                method: "put",
                headers: { "Content-Type": "application/json" },
                body: dataToSend
            })
                .then(resp => {
                    if (resp.status === 204) {
                        console.log("Item edited succesfully");

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
                    console.log(dataJson);

                    if (dataJson["errors"] == undefined || dataJson["errors"].length == 0) {
                        if (props.onClose != undefined)
                            if (props.onSucces != undefined)
                                props.onSucces(inputs);
                        props.onClose();
                    }

                    if (dataJson["errors"])
                        setErrors(dataJson["errors"]);
                })
        }

        
    }

    const handleInput = (event) => {
        const tempInputs = inputs;

        tempInputs[event.name] = event.value;

        setInputs(tempInputs);
    }

    const getInput = (type, name, required, enabled = true) => {
        switch (type) {
            default:
                return <p>Rip data type</p>;

            /*case "bigint":
                return Int64.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);
    
            case "binary":
            case "varbinary":
            case "image":
            case "rowversion":
            case "timestamp":
                return Encoding.ASCII.GetBytes(variable);
    
            case "bit":
                return Convert.ToBoolean(variable, CultureInfo.InvariantCulture);*/

            case "char":
            case "nchar":
            case "nxtext":
            case "nvarchar":
            case "text":
            case "varchar":
                return <input type="text" required={required && enabled} defaultValue={inputs[name] || ""} name={name} disabled={!enabled} onChange={e => handleInput(e.target)} />

            /*case "date":
            case "datetime":
            case "datetime2":
            case "smalldatetime":
                return Convert.ToDateTime(variable, CultureInfo.InvariantCulture);
    
            case "datetimeoffset":
                return DateTimeOffset.Parse(variable, CultureInfo.InvariantCulture);
    
            case "decimal":
            case "money":
            case "numeric":
            case "smallmoney":
                return decimal.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);*/

            case "float":
                return <input type="number" required={required && enabled} name={name} defaultValue={inputs[name] || ""}  step="0.01" disabled={!enabled} onChange={e => handleInput(e.target)} />

            case "int":
                return <input type="number" required={required && enabled} name={name} defaultValue={inputs[name] || ""}  step="1" disabled={!enabled} onChange={e => handleInput(e.target)} />

            /*case "real":
                return float.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);
    
            case "smallint":
                return short.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);
    
            case "sql_variant":
                return (object)variable;
    
            case "time":
                return TimeSpan.Parse(variable, CultureInfo.InvariantCulture);
    
            case "tinyint":
                return byte.Parse(variable, NumberStyles.Any, CultureInfo.InvariantCulture);*/

            case "uniqueidentifier":
                return <input type="text" defaultValue={inputs[name] || "________-____-____-____-___________"} required={required && enabled} name={name} disabled={!enabled} onChange={e => handleInput(e.target)} />
        }
    }

    const dataType = (data) => {
        const name = data["COLUMN_NAME"];
        const required = data["IS_NULLABLE"] == "NO";
        const enabled = data["CONSTRAINT_TYPE"] != "PRIMARY KEY";
        console.log(enabled);

        return (
            <div className="c-input">
                <label className="c-label" for={name}>{name}</label><br />
                {getInput(data["DATA_TYPE"], name, required, enabled)}
            </div>
        )
    }

    return (
        <div className="c-backdrop">
            <div className="c-container">
                <div className="c-header">
                    <h3>
                        {props.action} entry for {props.table}
                    </h3>
                    <svg xmlns="http://www.w3.org/2000/svg" className="c-cross" onClick={onClose}>
                        <line x1="0" y1="0" x2="20" y2="20" stroke="black" stroke-width="2" />
                        <line x1="20" y1="0" x2="0" y2="20" stroke="black" stroke-width="2" />
                    </svg>
                </div>
                <div className="c-form">
                    {props.structure.map((data) => {
                        return dataType(data)
                    }
                    )}
                </div>
                <div className="c-errorContainer">
                    {errors.map((error) => (
                        <p>{error}</p>
                    )
                    )}
                </div>
                <div className="c-footer">
                    <input type="button" Value="Save" onClick={onSubmit} />
                </div>
            </div>
        </div>
    );
}

FormOverlay.defaultProps = {
    structure: [],
    table: "",
    onClose: undefined,
    onSucces: undefined,
    api: undefined,
    item: {data: undefined}
}

export default FormOverlay;