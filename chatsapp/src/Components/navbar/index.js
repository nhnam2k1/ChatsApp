import { useAuth0 } from "@auth0/auth0-react";
import { LoginButton } from "../buttons/login-button";
import { SignupButton } from "../buttons/signup-button";
import { LogoutButton } from "../buttons/logout-button";
import { ChatButton } from "../buttons/chat-button";
import { ProfileButton } from "../buttons/profile-button";
import Logo from "./whatsapp.svg";

const Navbar = () => {
    const { isAuthenticated } = useAuth0();

    const NotSignin = <> <LoginButton/><SignupButton/> </>;
    const Signin = <> <ChatButton/><ProfileButton/><LogoutButton/> </>;
    const showBtns = isAuthenticated ? Signin : NotSignin;
    return(
        <div style={navStyle}>
            <img src={Logo} style={logoStyles} alt="This is a logo"/>
            <div style={btnAreaStyle}>
                {showBtns}
            </div>
        </div>
    )
}

const logoStyles = {
    objectFit: "fill",
    height: "50px",
    width: "auto",
}

const navStyle = {
    height: '60px',
    backgroundColor: 'grey',
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingLeft: "5px",
    paddingRight: "5px",
    marginLeft: "15px",
    marginRight: "15px"
};

const btnAreaStyle = {
    width: '400px',
    display: 'flex',
    flexDirection: 'row',
    justifyContent: 'flex-end',
    alignItems: 'center'
};


export default Navbar;