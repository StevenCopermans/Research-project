import React, { Component } from 'react';
import { Route } from 'react-router';

import Home from './components/Home';

import './custom.css'

function App() {
    return (
        <Route exact path='/' component={Home} />
    );
}

export default App;