import { useNavigate } from "react-router-dom";
import React from "react";

export const ProfileButton = () => {
  const navigate = useNavigate();
  const handleLogin = () => navigate("/profile");

  return (
    <button style={style} onClick={handleLogin}>
      Profile
    </button>
  );
};

const style = {
  height: '40px',
  width:  '100px',
  backgroundColor: 'lightgreen',
}