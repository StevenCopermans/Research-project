import React, { Component, useState } from 'react';
import { Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
import { Link } from 'react-router-dom';
import './NavMenu.css';

function NavMenu() {
    return (
        <header>
            <Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
                <Container>
                    <NavbarBrand tag={Link} to="/">CMS_Site</NavbarBrand>
                    <ul className="navbar-nav flex-grow">
                        <NavItem>
                            <NavLink tag={Link} className="text-dark" to="/fetch-data">Login</NavLink>
                        </NavItem>
                    </ul>
                </Container>
            </Navbar>
        </header>

    );
}

export default NavMenu;