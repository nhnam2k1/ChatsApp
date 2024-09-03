const Message = ({ msg, callback }) => {
    let whose = msg.sender === 'user' ? 'user' : 'receiver';

    const handleClick = async (e) => {
        e.preventDefault();
        await callback(msg.id, msg.message);
    }

    return (
        <div className={`message-container ${whose}`}>
            <div className={`message ${whose}`} data-testid="message">
                {msg.message}
                {msg.isAttachment && 
                    <button onClick={handleClick}
                            className={`message ${whose}`}
                            style={style}
                    >
                        &#8659;
                    </button>
                }
            </div>
        </div>
    );
};

const style = {
    marginLeft: "5px", 
    padding: "0px",
    height: "32px",
    width: "32px",
    fontSize: "25px", 
    backgroundColor: "lightgray",
};

export default Message;