import { Auth0Provider } from "@auth0/auth0-react";
import React from "react";
import { useNavigate } from "react-router-dom";

const domain = process.env.REACT_APP_AUTH0_DOMAIN;
const clientId = process.env.REACT_APP_AUTH0_CLIENT_ID;
const redirectUri = process.env.REACT_APP_AUTH0_CALLBACK_URL;

export const Auth0ProviderWithNavigate = ({ children }) => {
  const navigate = useNavigate();

  const onRedirectCallback = (appState) => {
    navigate(appState?.returnTo || window.location.pathname);
  };

  if (!(domain && clientId && redirectUri)) return null;

  const redirect = { 
    redirect_uri: redirectUri, 
    audience: `https://${domain}/api/v2/`,
    scope: "update:current_user_metadata read:current_user openid profile email",
  };

  return (
    <Auth0Provider
        domain={domain} clientId={clientId}
        authorizationParams={redirect}
        onRedirectCallback={onRedirectCallback}
        useRefreshTokens={true}
        cacheLocation="localstorage"
    >
      {children}
    </Auth0Provider>
  );
};
