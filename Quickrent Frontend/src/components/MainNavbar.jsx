import { jwtDecode } from "jwt-decode";
import { Container, Nav, Navbar, Button, NavDropdown } from "react-bootstrap";
import { Link, useLocation } from "react-router-dom";

function MainNavbar() {
  const location = useLocation();

  function handleLogOut() {
    localStorage.removeItem("JwtToken");
    alert("User Logged Out Successfully");
    window.location.href = "/";
  }

  function getUserRole() {
    const token = localStorage.getItem("JwtToken");
    if (!token) return null;
    const decodedToken = jwtDecode(token);
    return decodedToken?.role?.toString().toUpperCase() || null;
  }

  function shouldRenderAccount() {
    return getUserRole() === "CUSTOMER";
  }

  function shouldRenderSellerDashboard() {
    return getUserRole() === "SELLER";
  }

  function shouldRenderAdminDashboard() {
    return getUserRole() === "ADMIN";
  }

  return (
    <Navbar bg="white" expand="lg" className="py-3 shadow-sm">
      <Container fluid="lg">
        <Navbar.Brand as={Link} to="/" className="d-flex align-items-center">
          <img src="/QUICKRENT LOGOS/cv.png" alt="Quick Rent" height="50" />
          <div className="ms-2"></div>
        </Navbar.Brand>
        <Navbar.Toggle aria-controls="basic-navbar-nav" />
        <Navbar.Collapse id="basic-navbar-nav">
          <Nav className="ms-auto align-items-center nav-links">
            {shouldRenderSellerDashboard() && (
              <Nav.Link as={Link} to="/seller" className={location.pathname === "/seller" ? "active" : ""}>
                Dashboard
              </Nav.Link>
            )}

            {shouldRenderAdminDashboard() && (
              <Nav.Link as={Link} to="/admin" className={location.pathname === "/admin" ? "active" : ""}>
                Dashboard
              </Nav.Link>
            )}
            <Nav.Link as={Link} to="/" className={location.pathname === "/" ? "active" : ""}>
              Home
            </Nav.Link>
            <Nav.Link as={Link} to="/categories" className={location.pathname === "/categories" ? "active" : ""}>
              Categories
            </Nav.Link>
            <Nav.Link as={Link} to="/contact" className={location.pathname === "/contact" ? "active" : ""}>
              Contact Us
            </Nav.Link>

            {shouldRenderAccount() && (
              <NavDropdown title="Account" id="basic-nav-dropdown" className="account-nav-btn">
                <NavDropdown.Item>
                  <Nav.Link as={Link} to="/userinfo">
                    Personal Information
                  </Nav.Link>
                </NavDropdown.Item>
                <NavDropdown.Item>
                  <Nav.Link as={Link} to="/userorders">
                    My Orders
                  </Nav.Link>
                </NavDropdown.Item>
                <NavDropdown.Item>
                  <Nav.Link as={Link} to="/" onClick={handleLogOut}>
                    Logout
                  </Nav.Link>
                </NavDropdown.Item>
              </NavDropdown>
            )}
          </Nav>
        </Navbar.Collapse>
      </Container>
    </Navbar>
  );
}

export default MainNavbar;
