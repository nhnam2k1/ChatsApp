import { useAuth0 } from "@auth0/auth0-react";
import React from "react";


export const SignupButton = () => {
  const { loginWithRedirect } = useAuth0();

  const handleSignUp = async () => {
    await loginWithRedirect({
      appState: {
        returnTo: "/profile",
      },
      authorizationParams: {
        prompt: "login",
        screen_hint: "signup",
      },
    });
  };

  return (
    <button style={style} onClick={handleSignUp}>
      Sign Up
    </button>
  );
};

const style = {
    height: '40px',
    width:  '100px',
    backgroundColor: 'orange',
}