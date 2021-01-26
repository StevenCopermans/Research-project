import React, { Component } from 'react';
import { Container } from 'reactstrap';
import NavMenu from './NavMenu';
import Flex from './Flex';
import Toolbar from './Toolbar';

export class Layout extends Component {
    static displayName = Layout.name;

    render() {
        return (
            <div style={{
                height: '100%',
            }}>
                <Flex container
                    justifyContent="space-between"
                    width="100%" height="100%">
                    <Toolbar>
                        <div> hello </div>
                        <div> hello </div>
                        <div> hello </div>
                        <div> hello </div>
                        <div> hello </div>
                    </Toolbar>
                    <div style={{
                        width: '100%',
                    }}>
                        <NavMenu />
                    </div>

                </Flex>

                <Container>
                    {this.props.children}
                </Container>
            </div>
        );
    }
}