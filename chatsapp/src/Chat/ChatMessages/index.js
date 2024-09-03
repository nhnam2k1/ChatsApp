import Message from '../../Components/Message';

const ChatMessages = ({ messages, handleFileDownload }) => (
    <div className="chat-messages" data-testid="chat-messages">
        {messages.map((msg) => (
            <Message msg={msg} key={msg.id}
                    callback={handleFileDownload} />
        ))}
    </div>
);

export default ChatMessages;
