import { useAuth0 } from "@auth0/auth0-react";
import React from "react";


export const LogoutButton = () => {
  const { logout } = useAuth0();

  const handleLogout = () => {
    logout({
      logoutParams: {
        returnTo: window.location.origin,
      },
    });
  };

  return (
    <button style={style} onClick={handleLogout}>
      Log Out
    </button>
  );
};

const style = {
  height: '40px',
  width:  '100px',
  backgroundColor: 'yellow',
}
