import ChatHeader from '../ChatHeader';
import ChatMessages from '../ChatMessages';
import ChatInput from '../ChatInput';

const ChatContainer = ({ currentContact, 
                        messages, 
                        handleFileDownload, 
                        currentRecipentID, 
                        inputMessage, 
                        onInputMessageChange, 
                        onKeyPress, 
                        handleMessageSend, 
                        handleFileUpload }) => 
(
    <div className="chat-container">
        <ChatHeader currentContact={currentContact} />
        <ChatMessages messages={messages} 
                    handleFileDownload={handleFileDownload} />
        <ChatInput
            currentRecipentID={currentRecipentID}
            inputMessage={inputMessage}
            onInputMessageChange={onInputMessageChange}
            onKeyPress={onKeyPress}
            handleMessageSend={handleMessageSend}
            handleFileUpload={handleFileUpload}
        />
    </div>
);

export default ChatContainer;