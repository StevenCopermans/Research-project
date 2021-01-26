import React, { Component } from 'react';

const Toolbar = (props) => (
    <div
        style={{
            width: '300px',
            minHeight: '100vh',
            backgroundColor: 'grey',
            color: 'white',
            padding: '16px',
        }}
    >
        {props.children}
    </div>
)

export default Toolbar;