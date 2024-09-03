import FileUpload from '../../FileForm';

const ChatInput = ({ currentRecipentID, 
                    inputMessage, 
                    onInputMessageChange, 
                    onKeyPress, 
                    handleMessageSend, 
                    handleFileUpload }) => 
(
    <form className="chat-input" data-testid="chat-input">
        <input
            disabled={!currentRecipentID}
            type="text"
            value={inputMessage}
            placeholder="Type your message..."
            onChange={onInputMessageChange}
            onKeyPress={onKeyPress}
            required
            minLength="1"
            maxLength="255"
        />
        {currentRecipentID && (
            <>
                <button onClick={handleMessageSend}>Send</button>
                <FileUpload callback={handleFileUpload} />
            </>
        )}
    </form>
);

export default ChatInput;