import { useNavigate } from "react-router-dom";
import React from "react";

export const ChatButton = () => {
  const navigate = useNavigate();
  const handleLogin = () => navigate("/chat");

  return (
    <button style={style} onClick={handleLogin}>
      Chat
    </button>
  );
};

const style = {
  height: '40px',
  width:  '100px',
  backgroundColor: 'lightgreen',
}