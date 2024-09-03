import { useAuth0 } from "@auth0/auth0-react";
import { Route, Routes } from "react-router-dom";
import Navbar from "./Components/navbar";
import AuthenticationGuard from "./Components/authentication-guard";
import PageLoader from "./Components/page-loader";
import './App.css';

import Profile from "./Profile";
import ChatPage from './Chat';
import Home from './Home';

function App() {
  const { isLoading } = useAuth0();
  
  if (isLoading) return <PageLoader/>;

  const chatPage = <AuthenticationGuard component={ChatPage}/>
  const ProfilePage = <AuthenticationGuard component={Profile}/>
  return (
    <>
      <Navbar/>
      <Routes>
          <Route path="/" element={<Home/>} />
          <Route path="/chat" element={chatPage} />
          <Route path="/profile" element={ProfilePage} />
      </Routes>
    </>
  );
}

export default App;