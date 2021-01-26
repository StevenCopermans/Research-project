import React from 'react';
import './ErrorOverlay.css';

const ErrorOverlay = (props) => {
    const onClose = () => {
        if (props.onClose != undefined) {
            props.onClose();
        }
    }

    return (
        <div className="c-backdrop__error">
            <div className="c-container__error">
                <div className="c-header__error">
                    <h3>
                        Error: {props.table}
                    </h3>
                    <svg xmlns="http://www.w3.org/2000/svg" className="c-cross__error" onClick={onClose}>
                        <line x1="0" y1="0" x2="20" y2="20" stroke="black" stroke-width="2" />
                        <line x1="20" y1="0" x2="0" y2="20" stroke="black" stroke-width="2" />
                    </svg>
                </div>
                <div className="c-errorContainer__error">
                    {props.errors.map((error) => (
                        <p>{error}</p>
                    )
                    )}
                </div>
                <div className="c-warningContainer__error">
                    {props.warnings.map((warning) => (
                        <p>{warning}</p>
                    )
                    )}
                </div>
            </div>
        </div>
    );
}

ErrorOverlay.defaultProps = {
    onClose: undefined,
    errors: [],
    warnings: [],
    table: ""
}

export default ErrorOverlay;