import { useState, useEffect } from 'react';
import { useAuth0 } from '@auth0/auth0-react';

import ChatContainer from './ChatContainer';
import ContactLayout from '../ContactLayout';
import "./index.css";

const signalR = require("@microsoft/signalr");
const signalRMsgPack = require("@microsoft/signalr-protocol-msgpack");

const CONNECTED = signalR.HubConnectionState.Connected;
const DISCONNECTED = signalR.HubConnectionState.Disconnected;
const serverURL = process.env.REACT_APP_API_SERVER_URL;
const chatServer = `${serverURL}/chatHub`;
const initCon = new signalR.HubConnectionBuilder()
                    .withUrl("localhost").build();
const MessagePackProtocol = new signalRMsgPack
                            .MessagePackHubProtocol();
const ChatPage = () => {
    const { user, getAccessTokenSilently } = useAuth0();
    const [connection, setConnection] = useState(initCon);
    const [messages, setMessages] = useState([]);
    const [inputMessage, setInputMessage] = useState('');
    const [currentContact, setCurrentContact] = useState('');
    const [currentRecipentID, setCurrentRecipentID] = useState('');
    const [contact, setContact] = useState([]);

    const handleCurrentContact = (payload) => {
        setCurrentRecipentID(payload.id);
        setCurrentContact(payload.name);
    }

    const handleMessageSend = (e) => {
        e.preventDefault();
        if (currentRecipentID === null) return;
        if (currentRecipentID === "") return;

        if (inputMessage.trim() === '') return;
        setInputMessage('');

        connection.invoke("SendMessage", currentRecipentID, inputMessage)
        .then(() => {
            const newMessage = { 
                id: `${Date.now()}`,
                message: inputMessage, 
                sender: 'user' 
            }
            setMessages(prevMessages => [...prevMessages, newMessage]);
        })
        .catch(err => console.error(err.toString()));
    };

    const onInputMessageChange = (e) => 
        setInputMessage(e.target.value);

    const onKeyPress = (e) => {
        if (e.key === 'Enter') handleMessageSend(e);
    };

    useEffect(() => {
        let params = new URLSearchParams({
            current_user_id: user.sub,
        });
        getAccessTokenSilently()
        .then(token => fetch(`${serverURL}/users?${params}`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            }
        }))
        .then(res => res.json())
        .then(data => setContact(data))
        .catch(err => console.error(err));
    }, [user.sub, getAccessTokenSilently]);

    useEffect(() => {
        if (currentRecipentID === null) return;
        if (currentRecipentID === "") return;

        getAccessTokenSilently()
        .then(token => fetch(`${serverURL}/messages`, {
            method: "POST",
            mode: "cors",
            cache: "no-cache",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({
                "UserId": user.sub,
                "RecipientId": currentRecipentID
            }),
        }))
        .then(res => res.json())
        .then(data => {
            const messages = data.map(message => {
                const fromWhom = (message.userId === user.sub) 
                                ? "user" : "receiver";
                return { 
                    message: message.content, 
                    sender: fromWhom,
                    id: message.id,
                    isAttachment: message.isAttachment,
                }
            });
            setMessages(messages);
        })
        .catch(err => console.error(err));
    }, [user.sub, currentRecipentID, getAccessTokenSilently]);

    useEffect(() => {
        let isMounted = true;

        const receivedMessageHandler = (user, message) => {
            if (user !== currentRecipentID) return;
    
            const newMessage = { 
                message: message.Content, 
                sender: 'receiver',
                id: message.Id,
                isAttachment: message.IsAttachment,
            };
            setMessages(prevMessages => [...prevMessages, newMessage]);
        }

        const handleConnection = async () => {
            const token = await getAccessTokenSilently();
            if (!isMounted) return;

            const newConnection = new signalR.HubConnectionBuilder()
                .withUrl(chatServer,  {
                    accessTokenFactory: () => token,
                    withCredentials: true
                })
                .withHubProtocol(MessagePackProtocol)
                .configureLogging(signalR.LogLevel.Warning)
                .withAutomaticReconnect()
                .build();

            newConnection.on("ReceiveMessage", receivedMessageHandler);

            if (newConnection.state === DISCONNECTED){
                await newConnection.start();
                setConnection(newConnection);
            }
        }
        handleConnection()
            .catch(err => console.error(err.toString()));
        
        return () => {
            isMounted = false;
        };
    }, [getAccessTokenSilently, currentRecipentID]);

    useEffect(() => {
        return() => {
            if (connection == null) return;
            connection.off("ReceiveMessage");
            if (connection.state === CONNECTED) connection.stop();
        }
    }, [connection]);

    const handleFileDownload = async (id, message) => {
        try {
            const token = await getAccessTokenSilently();
            const response = await fetch(`${serverURL}/files/${id}`, {
                method: 'GET',
                headers: {
                    "Authorization": `Bearer ${token}`
                },
            });

            if (!response.ok) {
                alert('Error downloading file.');
                return;
            } 
            const blob = await response.blob();
            getBlob(blob, message);
        } 
        catch (error) 
        {
            alert(`Error downloading file: ${error.message}`);
        }
    };
    
    const handleFileUpload = async (file) => {
        if (!file) {
            alert('Please select a file first.');
            return;
        }

        const formData = new FormData();
        formData.append('file', file);
        formData.append('uploadData', JSON.stringify({
            "UserId": user.sub,
            "RecipientId": currentRecipentID
        }));

        try {
            const token = await getAccessTokenSilently();
            const response = await fetch(`${serverURL}/files`, {
                method: 'POST',
                headers: {
                    "Authorization": `Bearer ${token}`
                },
                body: formData
            });

            if (response.ok) {
                const payload = await response.json();
                var newMessage = { 
                    message: payload.content, 
                    sender: "user",
                    id: payload.id,
                    isAttachment: payload.isAttachment,
                }
                setMessages(prevMessages => [...prevMessages, newMessage]);
                return;
            } 
            alert('Error uploading file.');
        } 
        catch (error) {
            alert(`Error uploading file: ${error.message}`);
        }
    };

    return (
        <div className="main-container">
            <ContactLayout contact={contact} 
                onClick={handleCurrentContact}/>
            <ChatContainer
                currentContact={currentContact}
                messages={messages}
                handleFileDownload={handleFileDownload}
                currentRecipentID={currentRecipentID}
                inputMessage={inputMessage}
                onInputMessageChange={onInputMessageChange}
                onKeyPress={onKeyPress}
                handleMessageSend={handleMessageSend}
                handleFileUpload={handleFileUpload}
            />
        </div>
    );
};

const getBlob = (blob, message) => {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = message;
    document.body.appendChild(a);
    a.click();
    a.remove();
    window.URL.revokeObjectURL(url);
};

export default ChatPage;